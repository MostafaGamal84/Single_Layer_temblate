using API.DTOs.QuizGame;
using API.Extensions;
using API.Interfaces.QuizGame;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.QuizGame;

[ApiController]
[Route("api/quizzes")]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _service;

    public QuizzesController(IQuizService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] QuizCreateUpdateDto dto)
    {
        var result = await _service.CreateAsync(dto, User.GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] QuizCreateUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result is null ? NotFound(new { message = "Quiz not found" }) : Ok(result);
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPost("{id:int}/cover-image")]
    public async Task<IActionResult> UploadCoverImage(int id, [FromForm] IFormFile? file)
    {
        if (file is null)
        {
            return BadRequest(new { message = "Image file is required." });
        }

        try
        {
            var coverImageUrl = await _service.UploadCoverImageAsync(id, file);
            return coverImageUrl is null
                ? NotFound(new { message = "Quiz not found" })
                : Ok(new { coverImageUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? Ok(new { message = "Quiz deleted" }) : NotFound(new { message = "Quiz not found" });
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QuizQueryDto query)
    {
        return Ok(await _service.GetAllAsync(query));
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        return Ok(await _service.GetCategoriesAsync());
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var quiz = await _service.GetByIdAsync(id);
        return quiz is null ? NotFound(new { message = "Quiz not found" }) : Ok(quiz);
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPost("{id:int}/questions")]
    public async Task<IActionResult> AddQuestions(int id, [FromBody] List<QuizQuestionAddDto> dto)
    {
        try
        {
            var ok = await _service.AddQuestionsAsync(id, dto);
            return ok ? Ok(new { message = "Questions added" }) : NotFound(new { message = "Quiz not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpDelete("{id:int}/questions/{quizQuestionId:int}")]
    public async Task<IActionResult> RemoveQuestion(int id, int quizQuestionId)
    {
        var ok = await _service.RemoveQuestionAsync(id, quizQuestionId);
        return ok ? Ok(new { message = "Quiz question removed" }) : NotFound(new { message = "Quiz question not found" });
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPut("{id:int}/questions/reorder")]
    public async Task<IActionResult> ReorderQuestions(int id, [FromBody] List<QuizQuestionReorderItemDto> dto)
    {
        var ok = await _service.ReorderAsync(id, dto);
        return ok ? Ok(new { message = "Quiz questions reordered" }) : NotFound(new { message = "Quiz questions not found" });
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPut("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, [FromBody] QuizPublishDto dto)
    {
        var ok = await _service.PublishAsync(id, dto.IsPublished);
        return ok ? Ok(new { message = dto.IsPublished ? "Quiz published" : "Quiz unpublished" }) : NotFound(new { message = "Quiz not found" });
    }
}
