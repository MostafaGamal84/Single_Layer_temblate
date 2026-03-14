using API.DTOs.QuizGame;
using Microsoft.AspNetCore.Http;

namespace API.Interfaces.QuizGame;

public interface IQuizService
{
    Task<QuizResponseDto> CreateAsync(QuizCreateUpdateDto dto, int? userId);
    Task<QuizResponseDto?> UpdateAsync(int id, QuizCreateUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<QuizResponseDto?> GetByIdAsync(int id);
    Task<PagedResultDto<QuizResponseDto>> GetAllAsync(QuizQueryDto query);
    Task<string?> UploadCoverImageAsync(int quizId, IFormFile file);
    Task<bool> AddQuestionsAsync(int quizId, List<QuizQuestionAddDto> questions);
    Task<bool> ReorderAsync(int quizId, List<QuizQuestionReorderItemDto> items);
    Task<bool> PublishAsync(int quizId, bool isPublished);
}
