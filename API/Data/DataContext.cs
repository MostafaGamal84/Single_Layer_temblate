using API.Entities;
using API.Entities.QuizGame;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : IdentityDbContext<AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
    IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionChoice> QuestionChoices => Set<QuestionChoice>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<GameParticipant> GameParticipants => Set<GameParticipant>();
    public DbSet<PlayerAnswer> PlayerAnswers => Set<PlayerAnswer>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.Entity<AppRole>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        builder.Entity<Question>()
            .HasMany(x => x.Choices)
            .WithOne(x => x.Question)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Quiz>()
            .HasMany(x => x.QuizQuestions)
            .WithOne(x => x.Quiz)
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Question>()
            .HasMany(x => x.QuizQuestions)
            .WithOne(x => x.Question)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GameSession>()
            .HasOne(x => x.Quiz)
            .WithMany()
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GameSession>()
            .HasMany(x => x.Participants)
            .WithOne(x => x.GameSession)
            .HasForeignKey(x => x.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GameSession>()
            .HasMany(x => x.Answers)
            .WithOne(x => x.GameSession)
            .HasForeignKey(x => x.GameSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GameParticipant>()
            .HasMany(x => x.Answers)
            .WithOne(x => x.Participant)
            .HasForeignKey(x => x.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PlayerAnswer>()
            .HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PlayerAnswer>()
            .HasOne(x => x.SelectedChoice)
            .WithMany()
            .HasForeignKey(x => x.SelectedChoiceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<QuizAttempt>()
            .HasOne(x => x.Quiz)
            .WithMany()
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuizAttempt>()
            .HasMany(x => x.Answers)
            .WithOne(x => x.QuizAttempt)
            .HasForeignKey(x => x.QuizAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuizAttemptAnswer>()
            .HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuizAttemptAnswer>()
            .HasOne(x => x.SelectedChoice)
            .WithMany()
            .HasForeignKey(x => x.SelectedChoiceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<GameSession>()
            .HasIndex(x => x.JoinCode)
            .IsUnique();

        builder.Entity<GameParticipant>()
            .HasIndex(x => new { x.GameSessionId, x.DisplayName })
            .IsUnique();
    }
}
