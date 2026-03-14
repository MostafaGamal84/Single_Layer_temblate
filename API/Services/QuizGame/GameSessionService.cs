using API.Data;
using API.DTOs.QuizGame;
using API.Entities.QuizGame;
using API.Interfaces.QuizGame;
using API.SignalR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Services.QuizGame;

public class GameSessionService : IGameSessionService
{
    private readonly DataContext _context;
    private readonly IHubContext<LiveGameHub> _hubContext;
    private readonly IWebHostEnvironment _environment;
    private static readonly HashSet<string> AllowedCoverExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public GameSessionService(DataContext context, IHubContext<LiveGameHub> hubContext, IWebHostEnvironment environment)
    {
        _context = context;
        _hubContext = hubContext;
        _environment = environment;
    }

    public async Task<GameSessionResponseDto> CreateAsync(CreateGameSessionDto dto, int? hostId, string baseUrl)
    {
        var quiz = await _context.Set<Quiz>().FirstOrDefaultAsync(x => x.Id == dto.QuizId && !x.IsDeleted);
        if (quiz is null)
        {
            throw new ArgumentException("Quiz not found.");
        }

        var joinCode = await GenerateUniqueJoinCodeAsync();
        var session = new GameSession
        {
            QuizId = dto.QuizId,
            HostId = hostId,
            JoinCode = joinCode,
            JoinLink = $"{baseUrl.TrimEnd('/')}/player/join/{joinCode}",
            Status = GameSessionStatus.Waiting,
            QuestionFlowMode = NormalizeFlowMode(dto.QuestionFlowMode),
            CreatedAt = DateTime.UtcNow,
            CurrentQuestionIndex = 0,
            IsDeleted = false
        };

        _context.Set<GameSession>().Add(session);
        await _context.SaveChangesAsync();

        await BroadcastSessionUpdatedAsync(session.Id);
        return await GetByIdAsync(session.Id) ?? throw new InvalidOperationException("Session was not created.");
    }

    public async Task<IEnumerable<GameSessionResponseDto>> GetAllAsync()
    {
        var sessions = await _context.Set<GameSession>()
            .AsNoTracking()
            .Include(x => x.Quiz)
            .Include(x => x.Participants)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return sessions.Select(MapSession);
    }

    public async Task<GameSessionResponseDto?> GetByIdAsync(int id)
    {
        var session = await LoadSession(id);
        return session is null ? null : MapSession(session);
    }

    public async Task<GameSessionResponseDto?> GetByCodeAsync(string joinCode)
    {
        var session = await _context.Set<GameSession>()
            .AsNoTracking()
            .Include(x => x.Quiz)
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.JoinCode == joinCode && !x.IsDeleted);

        return session is null ? null : MapSession(session);
    }

    public async Task<SessionStateDto?> StartAsync(int id)
    {
        var session = await LoadSession(id);
        if (session is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        session.Status = GameSessionStatus.Live;
        session.StartedAt = now;
        session.EndedAt = null;
        await ConfigureTimerForCurrentQuestionAsync(session, now);

        await _context.SaveChangesAsync();

        var state = await GetStateAsync(id);
        await _hubContext.Clients.Group(GetGroupName(id)).SendAsync("sessionStarted", state);
        await BroadcastSessionUpdatedAsync(id);
        return state;
    }

    public async Task<SessionStateDto?> PauseAsync(int id)
    {
        var session = await LoadSession(id);
        if (session is null)
        {
            return null;
        }

        if (session.QuestionFlowMode == SessionQuestionFlowMode.TimedByQuestion &&
            session.CurrentQuestionEndsAt.HasValue)
        {
            var remaining = (int)Math.Ceiling((session.CurrentQuestionEndsAt.Value - DateTime.UtcNow).TotalSeconds);
            session.CurrentQuestionRemainingSeconds = remaining > 0 ? remaining : 1;
            session.CurrentQuestionEndsAt = null;
        }

        session.Status = GameSessionStatus.Paused;
        await _context.SaveChangesAsync();

        var state = await GetStateAsync(id);
        await _hubContext.Clients.Group(GetGroupName(id)).SendAsync("sessionPaused", state);
        await BroadcastSessionUpdatedAsync(id);
        return state;
    }

    public async Task<SessionStateDto?> ResumeAsync(int id)
    {
        var session = await LoadSession(id);
        if (session is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        session.Status = GameSessionStatus.Live;
        if (session.QuestionFlowMode == SessionQuestionFlowMode.TimedByQuestion)
        {
            var remaining = session.CurrentQuestionRemainingSeconds;
            await ConfigureTimerForCurrentQuestionAsync(session, now, remaining);
        }
        else
        {
            ClearQuestionTimer(session);
        }

        await _context.SaveChangesAsync();

        var state = await GetStateAsync(id);
        await _hubContext.Clients.Group(GetGroupName(id)).SendAsync("sessionResumed", state);
        await BroadcastSessionUpdatedAsync(id);
        return state;
    }

    public async Task<SessionStateDto?> EndAsync(int id)
    {
        var session = await LoadSession(id);
        if (session is null)
        {
            return null;
        }

        session.Status = GameSessionStatus.Ended;
        session.EndedAt = DateTime.UtcNow;
        ClearQuestionTimer(session);

        await RecalculateRanksAsync(id);
        await _context.SaveChangesAsync();

        var state = await GetStateAsync(id);
        await _hubContext.Clients.Group(GetGroupName(id)).SendAsync("sessionEnded", state);
        await BroadcastSessionUpdatedAsync(id);
        return state;
    }

    public async Task<SessionStateDto?> NextQuestionAsync(int id)
    {
        var session = await LoadSession(id);
        if (session is null)
        {
            return null;
        }

        var totalQuestions = await _context.Set<QuizQuestion>().CountAsync(x => x.QuizId == session.QuizId && !x.IsDeleted);
        if (totalQuestions == 0)
        {
            throw new ArgumentException("Session quiz has no questions.");
        }

        if (session.CurrentQuestionIndex < totalQuestions - 1)
        {
            session.CurrentQuestionIndex += 1;
            await ConfigureTimerForCurrentQuestionAsync(session, DateTime.UtcNow);
        }
        else
        {
            session.Status = GameSessionStatus.Ended;
            session.EndedAt = DateTime.UtcNow;
            ClearQuestionTimer(session);
        }

        await _context.SaveChangesAsync();

        var state = await GetStateAsync(id);
        await _hubContext.Clients.Group(GetGroupName(id)).SendAsync("nextQuestion", state);

        var leaderboard = await GetLeaderboardAsync(id);
        await _hubContext.Clients.Group(GetGroupName(id)).SendAsync("leaderboardUpdated", leaderboard);
        await BroadcastSessionUpdatedAsync(id);

        return state;
    }

    public async Task<List<LeaderboardItemDto>> GetLeaderboardAsync(int id)
    {
        await RecalculateRanksAsync(id);
        await _context.SaveChangesAsync();

        var participants = await _context.Set<GameParticipant>()
            .AsNoTracking()
            .Where(x => x.GameSessionId == id && !x.IsDeleted && x.JoinStatus == ParticipantJoinStatus.Approved)
            .OrderBy(x => x.Rank)
            .ThenByDescending(x => x.TotalScore)
            .ToListAsync();

        return participants.Select(x => new LeaderboardItemDto
        {
            ParticipantId = x.Id,
            DisplayName = x.DisplayName,
            TotalScore = x.TotalScore,
            Rank = x.Rank ?? 0
        }).ToList();
    }

    public async Task<SessionStateDto?> GetStateAsync(int id)
    {
        var session = await LoadSession(id);
        if (session is null)
        {
            return null;
        }

        var currentQuestion = await GetQuestionByIndexInternalAsync(session, session.CurrentQuestionIndex);
        var nextQuestion = await GetQuestionByIndexInternalAsync(session, session.CurrentQuestionIndex + 1);

        return new SessionStateDto
        {
            SessionId = session.Id,
            QuizId = session.QuizId,
            QuizTitle = session.Quiz.Title,
            QuizCoverImageUrl = GetCoverImageUrl(session.QuizId),
            Status = session.Status,
            QuestionFlowMode = session.QuestionFlowMode,
            CurrentQuestionIndex = session.CurrentQuestionIndex,
            CurrentQuestion = currentQuestion,
            NextQuestion = nextQuestion,
            CurrentQuestionEndsAtUtc = ToUtc(session.CurrentQuestionEndsAt),
            CurrentQuestionDurationSeconds = currentQuestion?.AnswerSeconds,
            ParticipantsCount = session.Participants.Count(IsApprovedParticipant)
        };
    }

    public async Task<List<JoinRequestItemDto>> GetJoinRequestsAsync(int sessionId)
    {
        var sessionExists = await _context.Set<GameSession>()
            .AsNoTracking()
            .AnyAsync(x => x.Id == sessionId && !x.IsDeleted);
        if (!sessionExists)
        {
            return new List<JoinRequestItemDto>();
        }

        var items = await _context.Set<GameParticipant>()
            .AsNoTracking()
            .Where(x => x.GameSessionId == sessionId && !x.IsDeleted && x.JoinStatus == ParticipantJoinStatus.Pending)
            .OrderBy(x => x.RequestedAt)
            .ToListAsync();

        return items.Select(MapJoinRequest).ToList();
    }

    public async Task<JoinRequestItemDto?> ApproveJoinRequestAsync(int sessionId, int participantId, int? hostId)
    {
        var participant = await _context.Set<GameParticipant>()
            .FirstOrDefaultAsync(x =>
                x.Id == participantId &&
                x.GameSessionId == sessionId &&
                !x.IsDeleted &&
                x.JoinStatus == ParticipantJoinStatus.Pending);

        if (participant is null)
        {
            return null;
        }

        participant.JoinStatus = ParticipantJoinStatus.Approved;
        participant.IsConnected = true;
        participant.ApprovedAt = DateTime.UtcNow;
        participant.DecisionByHostId = hostId;
        participant.DecisionNote = null;
        participant.JoinedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var item = MapJoinRequest(participant);
        await _hubContext.Clients.Group(GetGroupName(sessionId)).SendAsync("joinRequestApproved", item);
        await _hubContext.Clients.Group(GetGroupName(sessionId)).SendAsync("playerJoined", new
        {
            sessionId,
            participantId = participant.Id,
            displayName = participant.DisplayName
        });

        await BroadcastWaitingRoomUpdatedAsync(sessionId);
        var leaderboard = await GetLeaderboardAsync(sessionId);
        await _hubContext.Clients.Group(GetGroupName(sessionId)).SendAsync("leaderboardUpdated", leaderboard);
        await BroadcastSessionUpdatedAsync(sessionId);

        return item;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var session = await _context.Set<GameSession>()
            .Include(x => x.Participants)
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (session is null)
        {
            return false;
        }

        session.IsDeleted = true;
        session.Status = GameSessionStatus.Ended;
        session.EndedAt ??= DateTime.UtcNow;
        ClearQuestionTimer(session);

        foreach (var participant in session.Participants.Where(x => !x.IsDeleted))
        {
            participant.IsDeleted = true;
            participant.IsConnected = false;
            participant.LeftAt ??= DateTime.UtcNow;
            participant.JoinStatus = participant.JoinStatus == ParticipantJoinStatus.Approved
                ? ParticipantJoinStatus.Left
                : participant.JoinStatus;
        }

        foreach (var answer in session.Answers.Where(x => !x.IsDeleted))
        {
            answer.IsDeleted = true;
        }

        await _context.SaveChangesAsync();

        var payload = new { id = session.Id, sessionId = session.Id };
        await _hubContext.Clients.Group(GetGroupName(id)).SendAsync("sessionDeleted", payload);
        await _hubContext.Clients.Group(GetGlobalGroupName()).SendAsync("sessionDeleted", payload);

        return true;
    }

    public async Task<JoinRequestItemDto?> RejectJoinRequestAsync(int sessionId, int participantId, int? hostId, string? note)
    {
        var participant = await _context.Set<GameParticipant>()
            .FirstOrDefaultAsync(x =>
                x.Id == participantId &&
                x.GameSessionId == sessionId &&
                !x.IsDeleted &&
                x.JoinStatus == ParticipantJoinStatus.Pending);

        if (participant is null)
        {
            return null;
        }

        participant.JoinStatus = ParticipantJoinStatus.Rejected;
        participant.IsConnected = false;
        participant.RejectedAt = DateTime.UtcNow;
        participant.DecisionByHostId = hostId;
        participant.DecisionNote = string.IsNullOrWhiteSpace(note) ? "Join request rejected by host" : note.Trim();

        await _context.SaveChangesAsync();

        var item = MapJoinRequest(participant);
        await _hubContext.Clients.Group(GetGroupName(sessionId)).SendAsync("joinRequestRejected", item);
        await BroadcastWaitingRoomUpdatedAsync(sessionId);
        await BroadcastSessionUpdatedAsync(sessionId);

        return item;
    }

    public async Task AutoAdvanceTimedSessionsAsync(DateTime? nowUtc = null)
    {
        var now = nowUtc ?? DateTime.UtcNow;
        var expiredSessionIds = await _context.Set<GameSession>()
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Status == GameSessionStatus.Live &&
                x.QuestionFlowMode == SessionQuestionFlowMode.TimedByQuestion &&
                x.CurrentQuestionEndsAt.HasValue &&
                x.CurrentQuestionEndsAt <= now)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var sessionId in expiredSessionIds)
        {
            var session = await LoadSession(sessionId);
            if (session is null ||
                session.IsDeleted ||
                session.Status != GameSessionStatus.Live ||
                session.QuestionFlowMode != SessionQuestionFlowMode.TimedByQuestion ||
                !session.CurrentQuestionEndsAt.HasValue ||
                session.CurrentQuestionEndsAt > now)
            {
                continue;
            }

            var totalQuestions = await _context.Set<QuizQuestion>()
                .CountAsync(x => x.QuizId == session.QuizId && !x.IsDeleted);

            var ended = false;
            if (totalQuestions == 0 || session.CurrentQuestionIndex >= totalQuestions - 1)
            {
                session.Status = GameSessionStatus.Ended;
                session.EndedAt = now;
                ClearQuestionTimer(session);
                ended = true;
            }
            else
            {
                session.CurrentQuestionIndex += 1;
                await ConfigureTimerForCurrentQuestionAsync(session, now);
            }

            await _context.SaveChangesAsync();

            var state = await GetStateAsync(sessionId);
            if (state is null)
            {
                continue;
            }

            if (ended)
            {
                await _hubContext.Clients.Group(GetGroupName(sessionId)).SendAsync("sessionEnded", state);
            }
            else
            {
                await _hubContext.Clients.Group(GetGroupName(sessionId)).SendAsync("nextQuestion", state);
            }

            var leaderboard = await GetLeaderboardAsync(sessionId);
            await _hubContext.Clients.Group(GetGroupName(sessionId)).SendAsync("leaderboardUpdated", leaderboard);
            await BroadcastSessionUpdatedAsync(sessionId);
        }
    }

    private async Task RecalculateRanksAsync(int sessionId)
    {
        var participants = await _context.Set<GameParticipant>()
            .Where(x => x.GameSessionId == sessionId && !x.IsDeleted && x.JoinStatus == ParticipantJoinStatus.Approved)
            .OrderByDescending(x => x.TotalScore)
            .ThenBy(x => x.JoinedAt)
            .ToListAsync();

        var rank = 1;
        foreach (var item in participants)
        {
            item.Rank = rank++;
        }
    }

    private async Task<QuestionResponseDto?> GetQuestionByIndexInternalAsync(GameSession session, int questionIndex)
    {
        if (questionIndex < 0)
        {
            return null;
        }

        var qq = await _context.Set<QuizQuestion>()
            .AsNoTracking()
            .Where(x => x.QuizId == session.QuizId && !x.IsDeleted)
            .OrderBy(x => x.Order)
            .Skip(questionIndex)
            .Take(1)
            .Include(x => x.Question)
            .ThenInclude(x => x.Choices.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync();

        if (qq is null)
        {
            return null;
        }

        return new QuestionResponseDto
        {
            Id = qq.Question.Id,
            Title = qq.Question.Title,
            Text = qq.Question.Text,
            Type = qq.Question.Type,
            Difficulty = qq.Question.Difficulty,
            Points = qq.PointsOverride ?? qq.Question.Points,
            AnswerSeconds = qq.AnswerSeconds,
            CreatedBy = qq.Question.CreatedBy,
            CreatedAt = qq.Question.CreatedAt,
            Choices = qq.Question.Type == QuestionType.ShortAnswer
                ? new List<QuestionChoiceDto>()
                : qq.Question.Choices
                    .OrderBy(c => c.Order)
                    .Select(c => new QuestionChoiceDto
                    {
                        Id = c.Id,
                        ChoiceText = c.ChoiceText,
                        IsCorrect = false,
                        Order = c.Order
                    }).ToList()
        };
    }

    private async Task BroadcastSessionUpdatedAsync(int sessionId)
    {
        var session = await LoadSession(sessionId);
        if (session is null)
        {
            return;
        }

        var payload = MapSession(session);
        await _hubContext.Clients.Group(GetGroupName(sessionId)).SendAsync("sessionUpdated", payload);
        await _hubContext.Clients.Group(GetGlobalGroupName()).SendAsync("sessionsUpdated", payload);
    }

    private async Task BroadcastWaitingRoomUpdatedAsync(int sessionId)
    {
        var session = await _context.Set<GameSession>()
            .AsNoTracking()
            .Include(x => x.Quiz)
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == sessionId && !x.IsDeleted);

        if (session is null)
        {
            return;
        }

        var participants = session.Participants
            .Where(IsApprovedParticipant)
            .OrderBy(x => x.JoinedAt)
            .Select(x => x.DisplayName)
            .ToList();

        var payload = new WaitingRoomDto
        {
            SessionId = session.Id,
            SessionStatus = session.Status.ToString(),
            QuizTitle = session.Quiz.Title,
            ParticipantsCount = participants.Count,
            Participants = participants
        };

        await _hubContext.Clients.Group(GetGroupName(sessionId)).SendAsync("waitingRoomUpdated", payload);
    }

    private async Task<GameSession?> LoadSession(int id)
    {
        return await _context.Set<GameSession>()
            .Include(x => x.Quiz)
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    private async Task ConfigureTimerForCurrentQuestionAsync(GameSession session, DateTime nowUtc, int? overrideDurationSeconds = null)
    {
        if (session.QuestionFlowMode != SessionQuestionFlowMode.TimedByQuestion || session.Status != GameSessionStatus.Live)
        {
            ClearQuestionTimer(session);
            return;
        }

        var seconds = overrideDurationSeconds ?? await GetQuestionAnswerSecondsAsync(session.QuizId, session.CurrentQuestionIndex);
        if (seconds <= 0)
        {
            ClearQuestionTimer(session);
            return;
        }

        session.CurrentQuestionStartedAt = nowUtc;
        session.CurrentQuestionEndsAt = nowUtc.AddSeconds(seconds);
        session.CurrentQuestionRemainingSeconds = null;
    }

    private async Task<int> GetQuestionAnswerSecondsAsync(int quizId, int questionIndex)
    {
        var seconds = await _context.Set<QuizQuestion>()
            .AsNoTracking()
            .Where(x => x.QuizId == quizId && !x.IsDeleted)
            .OrderBy(x => x.Order)
            .Select(x => x.AnswerSeconds)
            .Skip(questionIndex)
            .Take(1)
            .FirstOrDefaultAsync();

        if (seconds < 5) return 5;
        if (seconds > 300) return 300;
        return seconds == 0 ? 30 : seconds;
    }

    private static void ClearQuestionTimer(GameSession session)
    {
        session.CurrentQuestionStartedAt = null;
        session.CurrentQuestionEndsAt = null;
        session.CurrentQuestionRemainingSeconds = null;
    }

    private static SessionQuestionFlowMode NormalizeFlowMode(SessionQuestionFlowMode mode)
    {
        return mode == SessionQuestionFlowMode.TimedByQuestion
            ? SessionQuestionFlowMode.TimedByQuestion
            : SessionQuestionFlowMode.HostControlled;
    }

    private static DateTime? ToUtc(DateTime? value)
    {
        if (!value.HasValue) return null;
        return DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
    }

    private GameSessionResponseDto MapSession(GameSession session)
    {
        return new GameSessionResponseDto
        {
            Id = session.Id,
            QuizId = session.QuizId,
            QuizTitle = session.Quiz.Title,
            QuizCoverImageUrl = GetCoverImageUrl(session.QuizId),
            HostId = session.HostId,
            JoinCode = session.JoinCode,
            JoinLink = session.JoinLink,
            Status = session.Status,
            QuestionFlowMode = session.QuestionFlowMode,
            CurrentQuestionIndex = session.CurrentQuestionIndex,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            CreatedAt = session.CreatedAt,
            ParticipantsCount = session.Participants.Count(IsApprovedParticipant)
        };
    }

    private static JoinRequestItemDto MapJoinRequest(GameParticipant participant)
    {
        return new JoinRequestItemDto
        {
            ParticipantId = participant.Id,
            DisplayName = participant.DisplayName,
            JoinStatus = participant.JoinStatus,
            RequestedAt = participant.RequestedAt,
            DecisionNote = participant.DecisionNote
        };
    }

    private static bool IsApprovedParticipant(GameParticipant participant)
    {
        return !participant.IsDeleted && participant.JoinStatus == ParticipantJoinStatus.Approved;
    }

    private async Task<string> GenerateUniqueJoinCodeAsync()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();

        while (true)
        {
            var code = new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
            var exists = await _context.Set<GameSession>().AnyAsync(x => x.JoinCode == code && !x.IsDeleted);
            if (!exists)
            {
                return code;
            }
        }
    }

    private static string GetGlobalGroupName() => "sessions";
    private static string GetGroupName(int sessionId) => $"session-{sessionId}";

    private string GetCoverImageUrl(int quizId)
    {
        var uploadsDirectory = GetQuizCoverDirectoryPath();
        if (!Directory.Exists(uploadsDirectory))
        {
            return string.Empty;
        }

        var filePath = Directory
            .EnumerateFiles(uploadsDirectory, $"quiz-{quizId}.*", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(path => AllowedCoverExtensions.Contains(Path.GetExtension(path)));

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        var fileName = Path.GetFileName(filePath);
        var relativePath = $"/uploads/quizzes/{fileName}";
        var version = new DateTimeOffset(File.GetLastWriteTimeUtc(filePath)).ToUnixTimeSeconds();
        return $"{relativePath}?v={version}";
    }

    private string GetQuizCoverDirectoryPath()
    {
        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        return Path.Combine(webRoot, "uploads", "quizzes");
    }
}
