using API.DTOs.QuizGame;
using API.Extensions;
using API.Interfaces.QuizGame;
using Microsoft.AspNetCore.Authorization;
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
}
