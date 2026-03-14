using API.Data;
using API.DTOs.QuizGame;
using API.Entities.QuizGame;
using API.Interfaces.QuizGame;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace API.Services.QuizGame;

public class QuizService : IQuizService
{
    private readonly DataContext _context;
    private readonly IWebHostEnvironment _environment;
    private static readonly HashSet<string> AllowedCoverExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };
    private const long MaxCoverSizeBytes = 5 * 1024 * 1024;

    public QuizService(DataContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<QuizResponseDto> CreateAsync(QuizCreateUpdateDto dto, int? userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("Quiz title is required.");
        }

        var quiz = new Quiz
        {
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Mode = dto.Mode,
            DurationMinutes = dto.DurationMinutes,
            IsPublished = false,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Set<Quiz>().Add(quiz);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(quiz.Id) ?? throw new InvalidOperationException("Quiz was not created.");
    }

    public async Task<QuizResponseDto?> UpdateAsync(int id, QuizCreateUpdateDto dto)
    {
        var quiz = await _context.Set<Quiz>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (quiz is null)
        {
            return null;
        }

        quiz.Title = dto.Title.Trim();
        quiz.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        quiz.Mode = dto.Mode;
        quiz.DurationMinutes = dto.DurationMinutes;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quiz = await _context.Set<Quiz>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (quiz is null)
        {
            return false;
        }

        quiz.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<QuizResponseDto?> GetByIdAsync(int id)
    {
        var quiz = await _context.Set<Quiz>()
            .AsNoTracking()
            .Include(x => x.QuizQuestions.OrderBy(q => q.Order))
            .ThenInclude(x => x.Question)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (quiz is null)
        {
            return null;
        }

        return new QuizResponseDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            CoverImageUrl = GetCoverImageUrl(quiz.Id),
            Mode = quiz.Mode,
            DurationMinutes = quiz.DurationMinutes,
            IsPublished = quiz.IsPublished,
            CreatedBy = quiz.CreatedBy,
            CreatedAt = quiz.CreatedAt,
            QuestionsCount = quiz.QuizQuestions.Count,
            Questions = quiz.QuizQuestions
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Order)
                .Select(x => new QuizQuestionResponseDto
                {
                    Id = x.Id,
                    QuestionId = x.QuestionId,
                    QuestionTitle = x.Question.Title,
                    Order = x.Order,
                    PointsOverride = x.PointsOverride,
                    AnswerSeconds = x.AnswerSeconds
                }).ToList()
        };
    }

    public async Task<PagedResultDto<QuizResponseDto>> GetAllAsync(QuizQueryDto query)
    {
        var page = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var size = query.PageSize <= 0 ? 10 : query.PageSize;

        var dbQuery = _context.Set<Quiz>()
            .AsNoTracking()
            .Include(x => x.QuizQuestions)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(x => x.Title.ToLower().Contains(search) || (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        if (query.Mode.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Mode == query.Mode.Value);
        }

        if (query.IsPublished.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsPublished == query.IsPublished.Value);
        }

        var total = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResultDto<QuizResponseDto>
        {
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            Items = items.Select(x => new QuizResponseDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                CoverImageUrl = GetCoverImageUrl(x.Id),
                Mode = x.Mode,
                DurationMinutes = x.DurationMinutes,
                IsPublished = x.IsPublished,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                QuestionsCount = x.QuizQuestions.Count(q => !q.IsDeleted)
            }).ToList()
        };
    }

    public async Task<string?> UploadCoverImageAsync(int quizId, IFormFile file)
    {
        var quizExists = await _context.Set<Quiz>()
            .AsNoTracking()
            .AnyAsync(x => x.Id == quizId && !x.IsDeleted);

        if (!quizExists)
        {
            return null;
        }

        if (file.Length <= 0)
        {
            throw new ArgumentException("Image file is empty.");
        }

        if (file.Length > MaxCoverSizeBytes)
        {
            throw new ArgumentException("Image size must be 5 MB or less.");
        }

        var extension = Path.GetExtension(file.FileName)?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AllowedCoverExtensions.Contains(extension))
        {
            throw new ArgumentException("Only JPG, JPEG, PNG, or WEBP images are allowed.");
        }

        var uploadsDirectory = EnsureQuizCoverDirectory();
        DeleteExistingCoverImages(quizId, uploadsDirectory);

        var fileName = $"quiz-{quizId}{extension}";
        var fullPath = Path.Combine(uploadsDirectory, fileName);

        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/quizzes/{fileName}";
        return AddVersion(relativePath, File.GetLastWriteTimeUtc(fullPath));
    }

    public async Task<bool> AddQuestionsAsync(int quizId, List<QuizQuestionAddDto> questions)
    {
        var quiz = await _context.Set<Quiz>()
            .Include(x => x.QuizQuestions)
            .FirstOrDefaultAsync(x => x.Id == quizId && !x.IsDeleted);

        if (quiz is null)
        {
            return false;
        }

        foreach (var item in questions)
        {
            var question = await _context.Set<Question>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == item.QuestionId && !x.IsDeleted);
            if (question is null)
            {
                throw new ArgumentException($"Question {item.QuestionId} does not exist.");
            }

            var current = quiz.QuizQuestions.FirstOrDefault(x => x.QuestionId == item.QuestionId && !x.IsDeleted);
            var effectiveAnswerSeconds = NormalizeAnswerSeconds(item.AnswerSeconds ?? question.AnswerSeconds);
            if (current is null)
            {
                quiz.QuizQuestions.Add(new QuizQuestion
                {
                    QuestionId = item.QuestionId,
                    Order = item.Order,
                    PointsOverride = item.PointsOverride,
                    AnswerSeconds = effectiveAnswerSeconds
                });
            }
            else
            {
                current.Order = item.Order;
                current.PointsOverride = item.PointsOverride;
                current.AnswerSeconds = effectiveAnswerSeconds;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderAsync(int quizId, List<QuizQuestionReorderItemDto> items)
    {
        var quizQuestions = await _context.Set<QuizQuestion>()
            .Where(x => x.QuizId == quizId && !x.IsDeleted)
            .ToListAsync();

        if (quizQuestions.Count == 0)
        {
            return false;
        }

        foreach (var item in items)
        {
            var match = quizQuestions.FirstOrDefault(x => x.Id == item.QuizQuestionId);
            if (match is not null)
            {
                match.Order = item.Order;
                if (item.AnswerSeconds.HasValue)
                {
                    match.AnswerSeconds = NormalizeAnswerSeconds(item.AnswerSeconds);
                }
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PublishAsync(int quizId, bool isPublished)
    {
        var quiz = await _context.Set<Quiz>().FirstOrDefaultAsync(x => x.Id == quizId && !x.IsDeleted);
        if (quiz is null)
        {
            return false;
        }

        quiz.IsPublished = isPublished;
        await _context.SaveChangesAsync();
        return true;
    }

    private static int NormalizeAnswerSeconds(int? rawValue)
    {
        var value = rawValue ?? 30;
        if (value < 5) return 5;
        if (value > 300) return 300;
        return value;
    }

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
        return AddVersion(relativePath, File.GetLastWriteTimeUtc(filePath));
    }

    private string EnsureQuizCoverDirectory()
    {
        var path = GetQuizCoverDirectoryPath();
        Directory.CreateDirectory(path);
        return path;
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

    private static string AddVersion(string relativePath, DateTime lastWriteUtc)
    {
        var version = new DateTimeOffset(lastWriteUtc).ToUnixTimeSeconds();
        return $"{relativePath}?v={version}";
    }

    private static void DeleteExistingCoverImages(int quizId, string uploadsDirectory)
    {
        foreach (var path in Directory.EnumerateFiles(uploadsDirectory, $"quiz-{quizId}.*", SearchOption.TopDirectoryOnly))
        {
            var extension = Path.GetExtension(path);
            if (!AllowedCoverExtensions.Contains(extension))
            {
                continue;
            }

            File.Delete(path);
        }
    }
}
