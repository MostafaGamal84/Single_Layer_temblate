using Entities;

namespace API.Entities.QuizGame;

public class Quiz : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public QuizMode Mode { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsPublished { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}

public class QuizQuestion : BaseEntity
{
    public int QuizId { get; set; }
    public int QuestionId { get; set; }
    public int Order { get; set; }
    public int? PointsOverride { get; set; }
    public int AnswerSeconds { get; set; } = 30;
    public virtual Quiz Quiz { get; set; } = null!;
    public virtual Question Question { get; set; } = null!;
}
