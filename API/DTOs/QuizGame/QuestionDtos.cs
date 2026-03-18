using API.Entities.QuizGame;

namespace API.DTOs.QuizGame;

public class PagedRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}

public class PagedResultDto<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}

public class QuestionChoiceDto
{
    public int Id { get; set; }
    public string ChoiceText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool HasImage { get; set; }
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
}

public class QuestionCreateUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public QuestionSelectionMode SelectionMode { get; set; } = QuestionSelectionMode.Single;
    public string? Difficulty { get; set; }
    public string? Explanation { get; set; }
    public int Points { get; set; }
    public int AnswerSeconds { get; set; } = 30;
    public List<QuestionChoiceDto> Choices { get; set; } = new();
}

public class QuestionResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public QuestionSelectionMode SelectionMode { get; set; }
    public string? Difficulty { get; set; }
    public string? ImageUrl { get; set; }
    public string? Explanation { get; set; }
    public int Points { get; set; }
    public int AnswerSeconds { get; set; } = 30;
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<QuestionChoiceDto> Choices { get; set; } = new();
}

public class QuestionQueryDto : PagedRequestDto
{
    public QuestionType? Type { get; set; }
    public QuestionSelectionMode? SelectionMode { get; set; }
    public string? Difficulty { get; set; }
}
