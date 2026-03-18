using API.DTOs.QuizGame;
using Microsoft.AspNetCore.Http;

namespace API.Interfaces.QuizGame;

public interface IQuestionService
{
    Task<QuestionResponseDto> CreateAsync(QuestionCreateUpdateDto dto, int? userId);
    Task<QuestionResponseDto?> UpdateAsync(int id, QuestionCreateUpdateDto dto);
    Task<bool> SoftDeleteAsync(int id);
    Task<QuestionResponseDto?> GetByIdAsync(int id);
    Task<PagedResultDto<QuestionResponseDto>> GetAllAsync(QuestionQueryDto query);
    Task<string?> UploadImageAsync(int questionId, IFormFile file);
    Task<string?> UploadChoiceImageAsync(int questionId, int choiceId, IFormFile file);
}
