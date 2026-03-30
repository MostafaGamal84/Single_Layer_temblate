using API.DTOs.QuizGame;
using API.Extensions;
using API.Interfaces.QuizGame;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.QuizGame;

[ApiController]
[Route("api/questions")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _service;

    public QuestionsController(IQuestionService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] QuestionCreateUpdateDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto, User.Identity?.IsAuthenticated == true ? User.GetUserId() : null);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] QuestionCreateUpdateDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);
            return result is null ? NotFound(new { message = "Question not found" }) : Ok(result);
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
        var deleted = await _service.SoftDeleteAsync(id);
        return deleted ? Ok(new { message = "Question deleted" }) : NotFound(new { message = "Question not found" });
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPost("{id:int}/image")]
    public async Task<IActionResult> UploadImage(int id, [FromForm] IFormFile? file)
    {
        if (file is null)
        {
            return BadRequest(new { message = "Image file is required." });
        }

        try
        {
            var imageUrl = await _service.UploadImageAsync(id, file);
            return imageUrl is null
                ? NotFound(new { message = "Question not found" })
                : Ok(new { imageUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPost("{questionId:int}/choices/{choiceId:int}/image")]
    public async Task<IActionResult> UploadChoiceImage(int questionId, int choiceId, [FromForm] IFormFile? file)
    {
        if (file is null)
        {
            return BadRequest(new { message = "Image file is required." });
        }

        try
        {
            var imageUrl = await _service.UploadChoiceImageAsync(questionId, choiceId, file);
            return imageUrl is null
                ? NotFound(new { message = "Choice not found for this question" })
                : Ok(new { imageUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QuestionQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound(new { message = "Question not found" }) : Ok(result);
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpPost("random-by-category")]
    public async Task<IActionResult> GetRandomQuestionsByCategory([FromBody] RandomQuestionSelectionRequest request)
    {
        if (request.CategorySelections == null || !request.CategorySelections.Any())
        {
            return BadRequest(new { message = "At least one category selection is required." });
        }

        var result = await _service.GetRandomQuestionsByCategoryAsync(request);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,Host")]
    [HttpGet("categories-with-counts")]
    public async Task<IActionResult> GetCategoriesWithCounts()
    {
        var result = await _service.GetCategoriesWithQuestionCountsAsync();
        return Ok(result.Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            QuestionCount = c.Questions.Count(q => !q.IsDeleted)
        }));
    }
}
