using API.Data;
using API.DTOs.QuizGame;
using API.Entities.QuizGame;
using API.Interfaces.QuizGame;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace API.Services.QuizGame;

public class QuestionService : IQuestionService
{
    private static readonly Regex HtmlTagRegex = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private readonly DataContext _context;

    public QuestionService(DataContext context)
    {
        _context = context;
    }

    public async Task<QuestionResponseDto> CreateAsync(QuestionCreateUpdateDto dto, int? userId)
    {
        ValidateQuestion(dto);

        var question = new Question
        {
            Title = dto.Title.Trim(),
            Text = dto.Text.Trim(),
            Type = dto.Type,
            Difficulty = string.IsNullOrWhiteSpace(dto.Difficulty) ? null : dto.Difficulty.Trim(),
            Points = dto.Points,
            AnswerSeconds = NormalizeAnswerSeconds(dto.AnswerSeconds),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            Choices = dto.Choices.Select(c => new QuestionChoice
            {
                ChoiceText = c.ChoiceText.Trim(),
                IsCorrect = c.IsCorrect,
                Order = c.Order
            }).ToList()
        };

        _context.Set<Question>().Add(question);
        await _context.SaveChangesAsync();

        return ToQuestionResponse(question);
    }

    public async Task<QuestionResponseDto?> UpdateAsync(int id, QuestionCreateUpdateDto dto)
    {
        ValidateQuestion(dto);

        var question = await _context.Set<Question>()
            .Include(x => x.Choices)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (question is null)
        {
            return null;
        }

        question.Title = dto.Title.Trim();
        question.Text = dto.Text.Trim();
        question.Type = dto.Type;
        question.Difficulty = string.IsNullOrWhiteSpace(dto.Difficulty) ? null : dto.Difficulty.Trim();
        question.Points = dto.Points;
        question.AnswerSeconds = NormalizeAnswerSeconds(dto.AnswerSeconds);

        var existing = question.Choices.ToList();
        if (existing.Count > 0)
        {
            _context.Set<QuestionChoice>().RemoveRange(existing);
        }

        question.Choices = dto.Choices.Select(c => new QuestionChoice
        {
            QuestionId = question.Id,
            ChoiceText = c.ChoiceText.Trim(),
            IsCorrect = c.IsCorrect,
            Order = c.Order
        }).ToList();

        await _context.SaveChangesAsync();
        return ToQuestionResponse(question);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var question = await _context.Set<Question>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (question is null)
        {
            return false;
        }

        question.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<QuestionResponseDto?> GetByIdAsync(int id)
    {
        var question = await _context.Set<Question>()
            .AsNoTracking()
            .Include(x => x.Choices.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        return question is null ? null : ToQuestionResponse(question);
    }

    public async Task<PagedResultDto<QuestionResponseDto>> GetAllAsync(QuestionQueryDto query)
    {
        var normalizedPage = query.PageNumber <= 0 ? 1 : query.PageNumber;
        var normalizedSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var dbQuery = _context.Set<Question>()
            .AsNoTracking()
            .Include(x => x.Choices.Where(c => !c.IsDeleted))
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(x => x.Title.ToLower().Contains(search) || x.Text.ToLower().Contains(search));
        }

        if (query.Type.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Type == query.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Difficulty))
        {
            var difficulty = query.Difficulty.Trim().ToLower();
            dbQuery = dbQuery.Where(x => x.Difficulty != null && x.Difficulty.ToLower() == difficulty);
        }

        var total = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedSize)
            .Take(normalizedSize)
            .ToListAsync();

        return new PagedResultDto<QuestionResponseDto>
        {
            PageNumber = normalizedPage,
            PageSize = normalizedSize,
            TotalCount = total,
            Items = items.Select(ToQuestionResponse)
        };
    }

    private static QuestionResponseDto ToQuestionResponse(Question question)
    {
        return new QuestionResponseDto
        {
            Id = question.Id,
            Title = question.Title,
            Text = question.Text,
            Type = question.Type,
            Difficulty = question.Difficulty,
            Points = question.Points,
            AnswerSeconds = question.AnswerSeconds,
            CreatedBy = question.CreatedBy,
            CreatedAt = question.CreatedAt,
            Choices = question.Choices
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Order)
                .Select(c => new QuestionChoiceDto
                {
                    Id = c.Id,
                    ChoiceText = c.ChoiceText,
                    IsCorrect = c.IsCorrect,
                    Order = c.Order
                }).ToList()
        };
    }

    private static void ValidateQuestion(QuestionCreateUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ArgumentException("Question title is required.");
        }

        if (string.IsNullOrWhiteSpace(ExtractPlainText(dto.Text)))
        {
            throw new ArgumentException("Question text is required.");
        }

        if (dto.Points <= 0)
        {
            throw new ArgumentException("Points must be greater than 0.");
        }

        if (dto.AnswerSeconds < 5 || dto.AnswerSeconds > 300)
        {
            throw new ArgumentException("Answer time must be between 5 and 300 seconds.");
        }

        foreach (var choice in dto.Choices)
        {
            if (string.IsNullOrWhiteSpace(choice.ChoiceText))
            {
                throw new ArgumentException("Choice text cannot be empty.");
            }
        }

        if (dto.Type == QuestionType.MultipleChoice || dto.Type == QuestionType.TrueFalse)
        {
            if (dto.Choices.Count < 2)
            {
                throw new ArgumentException("At least 2 choices are required for this question type.");
            }

            if (dto.Choices.Count(c => c.IsCorrect) == 0)
            {
                throw new ArgumentException("At least one correct choice is required.");
            }
        }

        if (dto.Type == QuestionType.ShortAnswer)
        {
            if (dto.Choices.Count != 1)
            {
                throw new ArgumentException("Short answer requires exactly one answer key.");
            }

            if (!dto.Choices[0].IsCorrect)
            {
                throw new ArgumentException("Short answer key must be marked as correct.");
            }
        }
    }

    private static int NormalizeAnswerSeconds(int value)
    {
        if (value < 5) return 5;
        if (value > 300) return 300;
        return value;
    }

    private static string ExtractPlainText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var withoutTags = HtmlTagRegex.Replace(value, " ");
        withoutTags = withoutTags
            .Replace("&nbsp;", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("&#160;", " ", StringComparison.OrdinalIgnoreCase);

        return WhitespaceRegex.Replace(withoutTags, " ").Trim();
    }
}


