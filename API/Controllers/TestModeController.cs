using API.DTOs.QuizGame;
using API.Entities.QuizGame;
using API.Extensions;
using API.Interfaces.QuizGame;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.QuizGame;

[ApiController]
[Route("api/test-mode")]
public class TestModeController : ControllerBase
{
    private readonly ITestModeService _service;
    private readonly IQuizService _quizService;

    public TestModeController(ITestModeService service, IQuizService quizService)
    {
        _service = service;
        _quizService = quizService;
    }

    [AllowAnonymous]
    [HttpGet("quizzes")]
    public async Task<IActionResult> Quizzes([FromQuery] QuizQueryDto query)
    {
        query.Mode = QuizMode.Test;
        query.IsPublished = true;
        query.PageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        query.PageSize = query.PageSize <= 0 ? 200 : query.PageSize;

        return Ok(await _quizService.GetAllAsync(query));
    }

    [AllowAnonymous]
    [HttpPost("quizzes/{quizId:int}/start")]
    public async Task<IActionResult> Start(int quizId, [FromBody] StartTestAttemptDto dto)
    {
        var attempt = await _service.StartAsync(quizId, User.Identity?.IsAuthenticated == true ? User.GetUserId() : null, dto);
        return attempt is null ? NotFound(new { message = "Quiz not found or not published for test mode" }) : Ok(attempt);
    }

    [AllowAnonymous]
    [HttpGet("attempts/{attemptId:int}/overview")]
    public async Task<IActionResult> Overview(int attemptId)
    {
        var overview = await _service.GetAttemptOverviewAsync(attemptId);
        return overview is null ? NotFound(new { message = "Attempt not found" }) : Ok(overview);
    }

    [AllowAnonymous]
    [HttpGet("attempts/{attemptId:int}/questions")]
    public async Task<IActionResult> Questions(int attemptId)
    {
        return Ok(await _service.GetQuestionsAsync(attemptId));
    }

    [AllowAnonymous]
    [HttpGet("attempts/{attemptId:int}/current-question")]
    public async Task<IActionResult> CurrentQuestion(int attemptId, [FromQuery] int? questionIndex)
    {
        var question = await _service.GetCurrentQuestionAsync(attemptId, questionIndex);
        return question is null ? NotFound(new { message = "No active question" }) : Ok(question);
    }

    [AllowAnonymous]
    [HttpPost("attempts/{attemptId:int}/submit-answer")]
    public async Task<IActionResult> SubmitAnswer(int attemptId, [FromBody] SubmitTestAnswerDto dto)
    {
        var result = await _service.SubmitAnswerAsync(attemptId, dto);
        return result.Accepted
            ? Ok(result)
            : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("attempts/{attemptId:int}/finish")]
    public async Task<IActionResult> Finish(int attemptId, [FromBody] FinishTestAttemptDto? dto)
    {
        var result = await _service.FinishAsync(attemptId, dto ?? new FinishTestAttemptDto());
        return result is null ? NotFound(new { message = "Attempt not found" }) : Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("attempts/{attemptId:int}/result")]
    public async Task<IActionResult> Result(int attemptId)
    {
        var result = await _service.GetResultAsync(attemptId);
        return result is null ? NotFound(new { message = "Attempt not found" }) : Ok(result);
    }

    [Authorize(Roles = "Player,Admin,Host")]
    [HttpGet("history/me")]
    public async Task<IActionResult> MyHistory()
    {
        var userId = User.GetUserId();
        return Ok(await _service.GetMyHistoryAsync(userId));
    }
}


