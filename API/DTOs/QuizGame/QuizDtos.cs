using API.Entities.QuizGame;

namespace API.DTOs.QuizGame;

public class QuizCreateUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QuizMode Mode { get; set; }
    public int DurationMinutes { get; set; }
    public int? TotalMarks { get; set; }
    public bool IsPublished { get; set; }
    public List<string> Categories { get; set; } = new();
}

public class QuizQuestionAddDto
{
    public int QuestionId { get; set; }
    public int Order { get; set; }
    public int? PointsOverride { get; set; }
    public int? AnswerSeconds { get; set; }
}

public class QuizQuestionReorderItemDto
{
    public int QuizQuestionId { get; set; }
    public int Order { get; set; }
    public int? AnswerSeconds { get; set; }
}

public class QuizQuestionResponseDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public int Order { get; set; }
    public int? PointsOverride { get; set; }
    public int AnswerSeconds { get; set; }
    public QuestionResponseDto? Question { get; set; }
}

public class QuizCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class QuizResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public QuizMode Mode { get; set; }
    public int DurationMinutes { get; set; }
    public int? TotalMarks { get; set; }
    public int EffectiveTotalMarks { get; set; }
    public bool IsPublished { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int QuestionsCount { get; set; }
    public List<QuizCategoryDto> Categories { get; set; } = new();
    public List<QuizQuestionResponseDto> Questions { get; set; } = new();
}

public class QuizPublishDto
{
    public bool IsPublished { get; set; }
}

public class QuizQueryDto : PagedRequestDto
{
    public QuizMode? Mode { get; set; }
    public bool? IsPublished { get; set; }
    public string? Category { get; set; }
}
