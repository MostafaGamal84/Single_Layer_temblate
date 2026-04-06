using API.Data;
using API.DTOs.QuizGame;
using API.Entities.QuizGame;
using API.Interfaces.QuizGame;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace API.Services.QuizGame;

public class QuestionService : IQuestionService
{
    private static readonly Regex HtmlTagRegex = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private readonly DataContext _context;
    private readonly IWebHostEnvironment _environment;

    public QuestionService(DataContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<QuestionResponseDto> CreateAsync(QuestionCreateUpdateDto dto, int? userId)
    {
        ValidateQuestion(dto);

        var question = new Question
        {
            Title = dto.Title.Trim(),
            Text = dto.Text.Trim(),
            Type = dto.Type,
            SelectionMode = NormalizeSelectionMode(dto),
            Difficulty = string.IsNullOrWhiteSpace(dto.Difficulty) ? null : dto.Difficulty.Trim(),
            Explanation = string.IsNullOrWhiteSpace(dto.Explanation) ? null : dto.Explanation.Trim(),
            Points = dto.Points,
            AnswerSeconds = NormalizeAnswerSeconds(dto.AnswerSeconds),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            CategoryId = dto.CategoryId,
            QuizId = dto.QuizId,
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

        if (dto.QuizId.HasValue)
        {
            var maxOrder = await _context.QuizQuestions
                .Where(xq => xq.QuizId == dto.QuizId.Value && !xq.IsDeleted)
                .Select(xq => (int?)xq.Order)
                .MaxAsync() ?? 0;

            _context.QuizQuestions.Add(new QuizQuestion
            {
                QuizId = dto.QuizId.Value,
                QuestionId = question.Id,
                Order = maxOrder + 1,
                PointsOverride = null,
                AnswerSeconds = dto.AnswerSeconds
            });

            await _context.SaveChangesAsync();
        }

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
        question.SelectionMode = NormalizeSelectionMode(dto);
        question.Difficulty = string.IsNullOrWhiteSpace(dto.Difficulty) ? null : dto.Difficulty.Trim();
        question.Explanation = string.IsNullOrWhiteSpace(dto.Explanation) ? null : dto.Explanation.Trim();
        question.Points = dto.Points;
        question.AnswerSeconds = NormalizeAnswerSeconds(dto.AnswerSeconds);
        question.CategoryId = dto.CategoryId;

        var existingChoices = question.Choices
            .Where(c => !c.IsDeleted)
            .ToDictionary(c => c.Id);

        var incomingIds = dto.Choices
            .Where(c => c.Id > 0)
            .Select(c => c.Id)
            .ToHashSet();

        if (incomingIds.Any(idValue => !existingChoices.ContainsKey(idValue)))
        {
            throw new ArgumentException("One or more choices are invalid for this question.");
        }

        var choicesToRemove = existingChoices.Values
            .Where(choice => !incomingIds.Contains(choice.Id))
            .ToList();

        foreach (var choice in choicesToRemove)
        {
            DeleteExistingImages(choice.Id, EnsureImageDirectory("question-choices"), "choice");
            _context.Set<QuestionChoice>().Remove(choice);
        }

        foreach (var dtoChoice in dto.Choices)
        {
            if (dtoChoice.Id > 0 && existingChoices.TryGetValue(dtoChoice.Id, out var existingChoice))
            {
                existingChoice.ChoiceText = dtoChoice.ChoiceText.Trim();
                existingChoice.IsCorrect = dtoChoice.IsCorrect;
                existingChoice.Order = dtoChoice.Order;
                continue;
            }

            question.Choices.Add(new QuestionChoice
            {
                QuestionId = question.Id,
                ChoiceText = dtoChoice.ChoiceText.Trim(),
                IsCorrect = dtoChoice.IsCorrect,
                Order = dtoChoice.Order
            });
        }

        await _context.SaveChangesAsync();

        var updatedQuestion = await _context.Set<Question>()
            .AsNoTracking()
            .Include(x => x.Choices.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == question.Id && !x.IsDeleted);

        return updatedQuestion is null
            ? ToQuestionResponse(question)
            : ToQuestionResponse(updatedQuestion);
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
            .Include(x => x.Category)
            .Include(x => x.Quiz)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(x =>
                x.Title.ToLower().Contains(search) ||
                x.Text.ToLower().Contains(search) ||
                (x.Explanation != null && x.Explanation.ToLower().Contains(search)));
        }

        if (query.Type.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Type == query.Type.Value);
        }

        if (query.SelectionMode.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.SelectionMode == query.SelectionMode.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Difficulty))
        {
            var difficulty = query.Difficulty.Trim().ToLower();
            dbQuery = dbQuery.Where(x => x.Difficulty != null && x.Difficulty.ToLower() == difficulty);
        }

        if (query.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.CategoryId == query.CategoryId.Value);
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

    public async Task<string?> UploadImageAsync(int questionId, IFormFile file)
    {
        var exists = await _context.Set<Question>()
            .AsNoTracking()
            .AnyAsync(x => x.Id == questionId && !x.IsDeleted);

        if (!exists)
        {
            return null;
        }

        ValidateImageFile(file);

        var uploadsDirectory = EnsureImageDirectory("questions");
        DeleteExistingImages(questionId, uploadsDirectory, "question");

        var extension = Path.GetExtension(file.FileName)?.Trim().ToLowerInvariant() ?? ".png";
        var fileName = $"question-{questionId}{extension}";
        var fullPath = Path.Combine(uploadsDirectory, fileName);

        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/questions/{fileName}";
        return AddVersion(relativePath, File.GetLastWriteTimeUtc(fullPath));
    }

    public async Task<string?> UploadChoiceImageAsync(int questionId, int choiceId, IFormFile file)
    {
        var exists = await _context.Set<QuestionChoice>()
            .AsNoTracking()
            .AnyAsync(x => x.Id == choiceId && x.QuestionId == questionId && !x.IsDeleted && !x.Question.IsDeleted);

        if (!exists)
        {
            return null;
        }

        ValidateImageFile(file);

        var uploadsDirectory = EnsureImageDirectory("question-choices");
        DeleteExistingImages(choiceId, uploadsDirectory, "choice");

        var extension = Path.GetExtension(file.FileName)?.Trim().ToLowerInvariant() ?? ".png";
        var fileName = $"choice-{choiceId}{extension}";
        var fullPath = Path.Combine(uploadsDirectory, fileName);

        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/question-choices/{fileName}";
        return AddVersion(relativePath, File.GetLastWriteTimeUtc(fullPath));
    }

    private QuestionResponseDto ToQuestionResponse(Question question)
    {
        return new QuestionResponseDto
        {
            Id = question.Id,
            Title = question.Title,
            Text = question.Text,
            Type = question.Type,
            SelectionMode = question.SelectionMode,
            Difficulty = question.Difficulty,
            ImageUrl = GetQuestionImageUrl(question.Id),
            Explanation = question.Explanation,
            Points = question.Points,
            AnswerSeconds = question.AnswerSeconds,
            CreatedBy = question.CreatedBy,
            CreatedAt = question.CreatedAt,
            CategoryId = question.CategoryId,
            CategoryName = question.Category?.Name,
            QuizId = question.QuizId,
            QuizTitle = question.Quiz?.Title,
            Choices = question.Choices
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Order)
                .Select(ToChoiceDto)
                .ToList()
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
            if (string.IsNullOrWhiteSpace(choice.ChoiceText) && !choice.HasImage)
            {
                throw new ArgumentException("Each choice must include text or an image.");
            }
        }

        if (dto.Type == QuestionType.MultipleChoice || dto.Type == QuestionType.TrueFalse)
        {
            if (dto.Choices.Count < 2)
            {
                throw new ArgumentException("At least 2 choices are required for this question type.");
            }

            var correctChoices = dto.Choices.Count(c => c.IsCorrect);
            if (correctChoices == 0)
            {
                throw new ArgumentException("At least one correct choice is required.");
            }

            var requiresSingleAnswer = dto.Type == QuestionType.TrueFalse || dto.SelectionMode != QuestionSelectionMode.Multiple;
            if (requiresSingleAnswer && correctChoices != 1)
            {
                throw new ArgumentException("Single-answer questions must have exactly one correct choice.");
            }
        }

        if (dto.Type == QuestionType.ShortAnswer)
        {
            if (dto.Choices.Count != 1)
            {
                throw new ArgumentException("Short answer requires exactly one answer key.");
            }

            if (string.IsNullOrWhiteSpace(dto.Choices[0].ChoiceText))
            {
                throw new ArgumentException("Short answer key cannot be empty.");
            }

            if (!dto.Choices[0].IsCorrect)
            {
                throw new ArgumentException("Short answer key must be marked as correct.");
            }
        }
    }

    private static QuestionSelectionMode NormalizeSelectionMode(QuestionCreateUpdateDto dto)
    {
        if (dto.Type != QuestionType.MultipleChoice)
        {
            return QuestionSelectionMode.Single;
        }

        return dto.SelectionMode == QuestionSelectionMode.Multiple
            ? QuestionSelectionMode.Multiple
            : QuestionSelectionMode.Single;
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

    private static void ValidateImageFile(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new ArgumentException("Image file is empty.");
        }

        if (file.Length > MaxImageSizeBytes)
        {
            throw new ArgumentException("Image size must be 5 MB or less.");
        }

        var extension = Path.GetExtension(file.FileName)?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
        {
            throw new ArgumentException("Only JPG, JPEG, PNG, or WEBP images are allowed.");
        }
    }

    private QuestionChoiceDto ToChoiceDto(QuestionChoice choice)
    {
        var imageUrl = GetChoiceImageUrl(choice.Id);
        return new QuestionChoiceDto
        {
            Id = choice.Id,
            ChoiceText = choice.ChoiceText,
            ImageUrl = imageUrl,
            HasImage = !string.IsNullOrWhiteSpace(imageUrl),
            IsCorrect = choice.IsCorrect,
            Order = choice.Order
        };
    }

    private string GetQuestionImageUrl(int questionId)
    {
        return GetImageUrl(questionId, "questions", "question");
    }

    private string GetChoiceImageUrl(int choiceId)
    {
        return GetImageUrl(choiceId, "question-choices", "choice");
    }

    private string GetImageUrl(int entityId, string folderName, string filePrefix)
    {
        var uploadsDirectory = GetImageDirectoryPath(folderName);
        if (!Directory.Exists(uploadsDirectory))
        {
            return string.Empty;
        }

        var filePath = Directory
            .EnumerateFiles(uploadsDirectory, $"{filePrefix}-{entityId}.*", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(path => AllowedImageExtensions.Contains(Path.GetExtension(path)));

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        var fileName = Path.GetFileName(filePath);
        var relativePath = $"/uploads/{folderName}/{fileName}";
        return AddVersion(relativePath, File.GetLastWriteTimeUtc(filePath));
    }

    private string EnsureImageDirectory(string folderName)
    {
        var path = GetImageDirectoryPath(folderName);
        Directory.CreateDirectory(path);
        return path;
    }

    private string GetImageDirectoryPath(string folderName)
    {
        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        return Path.Combine(webRoot, "uploads", folderName);
    }

    private static string AddVersion(string relativePath, DateTime lastWriteUtc)
    {
        var version = new DateTimeOffset(lastWriteUtc).ToUnixTimeSeconds();
        return $"{relativePath}?v={version}";
    }

    private static void DeleteExistingImages(int entityId, string uploadsDirectory, string filePrefix)
    {
        foreach (var path in Directory.EnumerateFiles(uploadsDirectory, $"{filePrefix}-{entityId}.*", SearchOption.TopDirectoryOnly))
        {
            var extension = Path.GetExtension(path);
            if (!AllowedImageExtensions.Contains(extension))
            {
                continue;
            }

            File.Delete(path);
        }
    }

    public async Task<List<RandomQuestionResultDto>> GetRandomQuestionsByCategoryAsync(RandomQuestionSelectionRequest request)
    {
        var results = new List<RandomQuestionResultDto>();

        foreach (var selection in request.CategorySelections)
        {
            var category = await _context.Set<QuestionCategory>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == selection.CategoryId);

            if (category == null) continue;

            var allQuestions = await _context.Set<Question>()
                .AsNoTracking()
                .Include(x => x.Choices.Where(c => !c.IsDeleted))
                .Where(x => !x.IsDeleted && x.CategoryId == selection.CategoryId)
                .ToListAsync();

            var shuffled = allQuestions.OrderBy(_ => Guid.NewGuid()).Take(selection.Count).ToList();
            var availableCount = allQuestions.Count;

            results.Add(new RandomQuestionResultDto
            {
                CategoryId = selection.CategoryId,
                CategoryName = category.Name,
                RequestedCount = selection.Count,
                AvailableCount = availableCount,
                Questions = shuffled.Select(ToQuestionResponse).ToList()
            });
        }

        return results;
    }

    public async Task<List<QuestionCategory>> GetCategoriesWithQuestionCountsAsync()
    {
        return await _context.Set<QuestionCategory>()
            .AsNoTracking()
            .Include(c => c.Questions.Where(q => !q.IsDeleted))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<QuestionResponseDto?> DuplicateAsync(int questionId, int userId)
    {
        var original = await _context.Set<Question>()
            .Include(x => x.Choices.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == questionId && !x.IsDeleted);

        if (original is null)
        {
            return null;
        }

        var newQuestion = new Question
        {
            Title = $"{original.Title} (Copy)",
            Text = original.Text,
            Type = original.Type,
            SelectionMode = original.SelectionMode,
            Difficulty = original.Difficulty,
            Explanation = original.Explanation,
            Points = original.Points,
            AnswerSeconds = original.AnswerSeconds,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            CategoryId = original.CategoryId,
            QuizId = original.QuizId
        };

        foreach (var choice in original.Choices.OrderBy(c => c.Order))
        {
            newQuestion.Choices.Add(new QuestionChoice
            {
                ChoiceText = choice.ChoiceText,
                IsCorrect = choice.IsCorrect,
                Order = choice.Order,
                IsDeleted = false
            });
        }

        _context.Set<Question>().Add(newQuestion);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(newQuestion.Id);
    }

    public async Task<int> ImportFromExcelAsync(IFormFile file)
    {
        var questions = new List<Question>();
        var categories = await _context.Set<QuestionCategory>().ToListAsync();

        using var stream = file.OpenReadStream();
        using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            var title = row.Cell(1).GetString();
            if (string.IsNullOrWhiteSpace(title)) continue;

            var categoryName = row.Cell(2).GetString();
            var category = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            var question = new Question
            {
                Title = title,
                Text = row.Cell(3).GetString(),
                Type = (QuestionType)GetQuestionType(row.Cell(4).GetString()),
                SelectionMode = QuestionSelectionMode.Single,
                Difficulty = row.Cell(5).GetString(),
                Points = GetCellValueOrDefault(row.Cell(6), 1),
                AnswerSeconds = 30,
                CategoryId = category?.Id,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var correctIndex = row.Cell(7).GetString().Trim().ToUpper();
            var choiceTexts = new[] {
                row.Cell(8).GetString(),
                row.Cell(9).GetString(),
                row.Cell(10).GetString(),
                row.Cell(11).GetString()
            };

            for (int i = 0; i < choiceTexts.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(choiceTexts[i]))
                {
                    question.Choices.Add(new QuestionChoice
                    {
                        ChoiceText = choiceTexts[i],
                        IsCorrect = correctIndex == ((char)('A' + i)).ToString(),
                        Order = i + 1,
                        IsDeleted = false
                    });
                }
            }

            if (question.Choices.Count == 0)
            {
                question.Type = QuestionType.ShortAnswer;
            }

            questions.Add(question);
        }

        _context.Set<Question>().AddRange(questions);
        await _context.SaveChangesAsync();

        return questions.Count;
    }

    private static int GetCellValueOrDefault(ClosedXML.Excel.IXLCell cell, int defaultValue)
    {
        if (cell.Value.IsNumber)
            return (int)cell.Value.GetNumber();
        if (cell.Value.IsText && int.TryParse(cell.Value.GetText(), out int result))
            return result;
        return defaultValue;
    }

    public async Task<Stream> ExportToExcelAsync(string? search = null, int? type = null, string? difficulty = null)
    {
        var query = _context.Set<Question>().Include(q => q.Category).Include(q => q.Choices.Where(c => !c.IsDeleted)).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(q => q.Title.Contains(search) || q.Text.Contains(search));
        }
        if (type.HasValue)
        {
            query = query.Where(q => q.Type == (QuestionType)type.Value);
        }
        if (!string.IsNullOrWhiteSpace(difficulty))
        {
            query = query.Where(q => q.Difficulty == difficulty);
        }

        var questions = await query.Where(q => !q.IsDeleted).OrderBy(q => q.Id).ToListAsync();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Questions");

        worksheet.Cell(1, 1).Value = "Title";
        worksheet.Cell(1, 2).Value = "Category";
        worksheet.Cell(1, 3).Value = "Text";
        worksheet.Cell(1, 4).Value = "Type";
        worksheet.Cell(1, 5).Value = "Difficulty";
        worksheet.Cell(1, 6).Value = "Points";
        worksheet.Cell(1, 7).Value = "Correct Answer";
        worksheet.Cell(1, 8).Value = "Choice A";
        worksheet.Cell(1, 9).Value = "Choice B";
        worksheet.Cell(1, 10).Value = "Choice C";
        worksheet.Cell(1, 11).Value = "Choice D";

        var headerRange = worksheet.Range(1, 1, 1, 11);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

        int row = 2;
        foreach (var q in questions)
        {
            worksheet.Cell(row, 1).Value = q.Title;
            worksheet.Cell(row, 2).Value = q.Category?.Name ?? "";
            worksheet.Cell(row, 3).Value = q.Text;
            worksheet.Cell(row, 4).Value = GetTypeName(q.Type);
            worksheet.Cell(row, 5).Value = q.Difficulty ?? "";
            worksheet.Cell(row, 6).Value = q.Points;

            var correctChoice = q.Choices.OrderBy(c => c.Order).FirstOrDefault(c => c.IsCorrect);
            worksheet.Cell(row, 7).Value = correctChoice != null ? GetChoiceLetter(correctChoice.Order) : "";

            var orderedChoices = q.Choices.OrderBy(c => c.Order).ToList();
            for (int i = 0; i < 4 && i < orderedChoices.Count; i++)
            {
                worksheet.Cell(row, 8 + i).Value = orderedChoices[i].ChoiceText;
            }

            row++;
        }

        worksheet.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream;
    }

    private static QuestionType GetQuestionType(string typeStr)
    {
        return typeStr.ToLower() switch
        {
            "multiple choice" or "mcq" => QuestionType.MultipleChoice,
            "true/false" or "tf" => QuestionType.TrueFalse,
            "short answer" => QuestionType.ShortAnswer,
            _ => QuestionType.MultipleChoice
        };
    }

    private static string GetTypeName(QuestionType type)
    {
        return type switch
        {
            QuestionType.MultipleChoice => "Multiple Choice",
            QuestionType.TrueFalse => "True/False",
            QuestionType.ShortAnswer => "Short Answer",
            _ => "Multiple Choice"
        };
    }

    private static string GetChoiceLetter(int order)
    {
        return order switch
        {
            1 => "A",
            2 => "B",
            3 => "C",
            4 => "D",
            _ => ""
        };
    }

    public async Task<Stream> ExportSelectedToExcelAsync(List<int> ids)
    {
        var questions = await _context.Set<Question>()
            .Include(q => q.Category)
            .Include(q => q.Choices.Where(c => !c.IsDeleted))
            .Where(q => !q.IsDeleted && ids.Contains(q.Id))
            .OrderBy(q => q.Id)
            .ToListAsync();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Questions");

        worksheet.Cell(1, 1).Value = "Title";
        worksheet.Cell(1, 2).Value = "Category";
        worksheet.Cell(1, 3).Value = "Text";
        worksheet.Cell(1, 4).Value = "Type";
        worksheet.Cell(1, 5).Value = "Difficulty";
        worksheet.Cell(1, 6).Value = "Points";
        worksheet.Cell(1, 7).Value = "Correct Answer";
        worksheet.Cell(1, 8).Value = "Choice A";
        worksheet.Cell(1, 9).Value = "Choice B";
        worksheet.Cell(1, 10).Value = "Choice C";
        worksheet.Cell(1, 11).Value = "Choice D";

        var headerRange = worksheet.Range(1, 1, 1, 11);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

        int row = 2;
        foreach (var q in questions)
        {
            worksheet.Cell(row, 1).Value = q.Title;
            worksheet.Cell(row, 2).Value = q.Category?.Name ?? "";
            worksheet.Cell(row, 3).Value = q.Text;
            worksheet.Cell(row, 4).Value = GetTypeName(q.Type);
            worksheet.Cell(row, 5).Value = q.Difficulty ?? "";
            worksheet.Cell(row, 6).Value = q.Points;

            var correctChoice = q.Choices.OrderBy(c => c.Order).FirstOrDefault(c => c.IsCorrect);
            worksheet.Cell(row, 7).Value = correctChoice != null ? GetChoiceLetter(correctChoice.Order) : "";

            var orderedChoices = q.Choices.OrderBy(c => c.Order).ToList();
            for (int i = 0; i < 4 && i < orderedChoices.Count; i++)
            {
                worksheet.Cell(row, 8 + i).Value = orderedChoices[i].ChoiceText;
            }

            row++;
        }

        worksheet.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream;
    }
}
