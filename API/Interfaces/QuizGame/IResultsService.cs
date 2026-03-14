using API.DTOs.QuizGame;

namespace API.Interfaces.QuizGame;

public interface IResultsService
{
    Task<GameSessionResponseDto?> GetSessionResultAsync(int sessionId);
    Task<IEnumerable<ParticipantResultDto>> GetSessionParticipantsAsync(int sessionId);
    Task<IEnumerable<SessionQuestionAnalysisDto>> GetSessionQuestionsAnalysisAsync(int sessionId);
    Task<TestResultDto?> GetTestAttemptResultAsync(int attemptId);
    Task<QuizSummaryDto?> GetQuizSummaryAsync(int quizId);
    Task<IEnumerable<PlayerSessionHistoryItemDto>> GetPlayerSessionHistoryAsync(int userId);
}
