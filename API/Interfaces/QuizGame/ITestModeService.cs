using API.DTOs.QuizGame;

namespace API.Interfaces.QuizGame;

public interface ITestModeService
{
    Task<TestAttemptResponseDto?> StartAsync(int quizId, int? userId, StartTestAttemptDto dto);
    Task<TestAttemptOverviewDto?> GetAttemptOverviewAsync(int attemptId);
    Task<List<TestModeQuestionDto>> GetQuestionsAsync(int attemptId);
    Task<TestModeQuestionDto?> GetCurrentQuestionAsync(int attemptId, int? questionIndex = null);
    Task<TestAnswerSubmitResponseDto> SubmitAnswerAsync(int attemptId, SubmitTestAnswerDto dto);
    Task<TestResultDto?> FinishAsync(int attemptId, FinishTestAttemptDto dto);
    Task<TestResultDto?> GetResultAsync(int attemptId);
    Task<IEnumerable<PlayerTestHistoryItemDto>> GetMyHistoryAsync(int userId);
}
