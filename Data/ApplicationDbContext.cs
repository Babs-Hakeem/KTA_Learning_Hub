using KTALearningHub.API.Models;
using Microsoft.EntityFrameworkCore;

namespace KTALearningHub.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
    public DbSet<ReflectionSubmission> ReflectionSubmissions => Set<ReflectionSubmission>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<LessonRating> LessonRatings => Set<LessonRating>();
    public DbSet<PlatformSettings> PlatformSettings => Set<PlatformSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== USER =====
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(300);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        });

        // ===== COURSE =====
        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("courses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.Level).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Instructor)
                  .WithMany(u => u.InstructedCourses)
                  .HasForeignKey(e => e.InstructorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ===== MODULE =====
        modelBuilder.Entity<Module>(entity =>
        {
            entity.ToTable("modules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Modules)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CourseId, e.OrderIndex });
        });

        // ===== LESSON =====
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("lessons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.VideoUrl).HasMaxLength(500);
            entity.Property(e => e.AudioUrl).HasMaxLength(500);
            entity.Property(e => e.AssignmentTitle).HasMaxLength(300);
            entity.Property(e => e.AssignmentSubmissionType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Module)
                  .WithMany(m => m.Lessons)
                  .HasForeignKey(e => e.ModuleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ModuleId, e.OrderIndex });
        });

        // ===== ENROLLMENT =====
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("enrollments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.ProgressPercentage).HasPrecision(5, 2);
            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Enrollments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
        });

        // ===== PAYMENT =====
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.PaymentReference).HasMaxLength(200);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Payments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Course)
                  .WithMany()
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Enrollment)
                  .WithOne(e => e.Payment)
                  .HasForeignKey<Payment>(e => e.EnrollmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PaymentReference);
        });

        // ===== LESSON PROGRESS =====
        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.ToTable("lesson_progresses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.LessonProgresses)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.LessonProgresses)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.LessonId }).IsUnique();
        });

        // ===== ASSIGNMENT SUBMISSION =====
        modelBuilder.Entity<AssignmentSubmission>(entity =>
        {
            entity.ToTable("assignment_submissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DocumentUrl).HasMaxLength(500);
            entity.Property(e => e.DocumentFileName).HasMaxLength(300);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.AssignmentSubmissions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.AssignmentSubmissions)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReviewedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedById)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.UserId, e.LessonId });
        });

        // ===== REFLECTION SUBMISSION =====
        modelBuilder.Entity<ReflectionSubmission>(entity =>
        {
            entity.ToTable("reflection_submissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ReflectionType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.FileName).HasMaxLength(300);
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.ReflectionSubmissions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.ReflectionSubmissions)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReviewedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedById)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.UserId, e.LessonId });
        });

        // ===== COMMENT =====
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Content).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.Comments)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentComment)
                  .WithMany(c => c.Replies)
                  .HasForeignKey(e => e.ParentCommentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.LessonId);
            entity.HasIndex(e => e.ParentCommentId);
        });

        // ===== COMMENT LIKE =====
        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.ToTable("comment_likes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.CommentLikes)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Comment)
                  .WithMany(c => c.Likes)
                  .HasForeignKey(e => e.CommentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.CommentId }).IsUnique();
        });

        // ===== LESSON RATING =====
        modelBuilder.Entity<LessonRating>(entity =>
        {
            entity.ToTable("lesson_ratings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Feedback).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.LessonRatings)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.LessonRatings)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.LessonId }).IsUnique();
        });

        // ===== PLATFORM SETTINGS =====
        modelBuilder.Entity<PlatformSettings>(entity =>
        {
            entity.ToTable("platform_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PlatformName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var updatedAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
            if (updatedAtProp != null)
            {
                updatedAtProp.CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
