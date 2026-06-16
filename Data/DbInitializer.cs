using KTALearningHub.API.Models;
using KTALearningHub.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KTALearningHub.API.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Only seed if the database is empty
        if (await context.Users.AnyAsync())
            return;

        // Create Admin User
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Admin User",
            Email = "admin@ktalearninghub.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create Sample Student
        var studentUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "John Doe",
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
            Role = UserRole.Student,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(adminUser, studentUser);

        // Create Sample Course
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Social Media Management Masterclass",
            Description = "Master content strategy, audience growth, engagement techniques, and social media performance tracking.",
            Price = 25000,
            Category = "Digital Marketing",
            InstructorId = adminUser.Id,
            Duration = "6 weeks",
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Courses.Add(course);

        // Create Modules
        var module1 = new Module
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = "Introduction to Social Media",
            Description = "Understanding the social media landscape and its importance for businesses.",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var module2 = new Module
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = "Content Strategy",
            Description = "Learn how to create compelling content strategies for different platforms.",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var module3 = new Module
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = "Audience Growth",
            Description = "Strategies for growing and engaging your audience organically.",
            OrderIndex = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Modules.AddRange(module1, module2, module3);

        // Create Lessons for Module 1
        var lesson1 = new Lesson
        {
            Id = Guid.NewGuid(),
            ModuleId = module1.Id,
            Title = "Creating A Winning Social Media Strategy",
            Description = "Learn how to develop a comprehensive social media strategy that drives results.",
            EstimatedDurationMinutes = 45,
            OrderIndex = 1,
            HasAssignment = true,
            AssignmentTitle = "Create a Content Strategy",
            AssignmentInstructions = "Create a content strategy for a fictional business. Include target audience, content pillars, posting schedule, and KPIs.",
            AssignmentSubmissionType = SubmissionType.Both,
            EnableReflection = true,
            AllowTextReflection = true,
            AllowVoiceReflection = true,
            AllowDocumentReflection = true,
            EnableDiscussion = true,
            AllowReplies = true,
            AllowLikes = true,
            EnableRating = true,
            Status = LessonStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var lesson2 = new Lesson
        {
            Id = Guid.NewGuid(),
            ModuleId = module1.Id,
            Title = "Understanding Your Target Audience",
            Description = "Deep dive into audience research and persona development.",
            EstimatedDurationMinutes = 30,
            OrderIndex = 2,
            HasAssignment = true,
            AssignmentTitle = "Audience Persona",
            AssignmentInstructions = "Create a detailed audience persona for a brand of your choice.",
            AssignmentSubmissionType = SubmissionType.Both,
            EnableReflection = true,
            EnableDiscussion = true,
            EnableRating = true,
            Status = LessonStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Lessons.AddRange(lesson1, lesson2);

        // Create Lessons for Module 2
        var lesson3 = new Lesson
        {
            Id = Guid.NewGuid(),
            ModuleId = module2.Id,
            Title = "Content Pillars & Themes",
            Description = "How to establish consistent content pillars for your brand.",
            EstimatedDurationMinutes = 35,
            OrderIndex = 1,
            HasAssignment = true,
            AssignmentTitle = "Content Pillars Framework",
            AssignmentInstructions = "Define 5 content pillars for a business and explain why each is important.",
            AssignmentSubmissionType = SubmissionType.Text,
            EnableReflection = true,
            EnableDiscussion = true,
            EnableRating = true,
            Status = LessonStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var lesson4 = new Lesson
        {
            Id = Guid.NewGuid(),
            ModuleId = module2.Id,
            Title = "Visual Content Creation",
            Description = "Tools and techniques for creating eye-catching social media visuals.",
            EstimatedDurationMinutes = 40,
            OrderIndex = 2,
            HasAssignment = true,
            AssignmentTitle = "Visual Content Pack",
            AssignmentInstructions = "Create 3 social media graphics using free design tools.",
            AssignmentSubmissionType = SubmissionType.Document,
            EnableReflection = true,
            EnableDiscussion = true,
            EnableRating = true,
            Status = LessonStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Lessons.AddRange(lesson3, lesson4);

        // Create Lessons for Module 3
        var lesson5 = new Lesson
        {
            Id = Guid.NewGuid(),
            ModuleId = module3.Id,
            Title = "Organic Growth Strategies",
            Description = "Proven methods to grow your social media following organically.",
            EstimatedDurationMinutes = 35,
            OrderIndex = 1,
            HasAssignment = true,
            AssignmentTitle = "Growth Plan",
            AssignmentInstructions = "Create a 30-day organic growth plan for an Instagram account.",
            AssignmentSubmissionType = SubmissionType.Both,
            EnableReflection = true,
            EnableDiscussion = true,
            EnableRating = true,
            Status = LessonStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var lesson6 = new Lesson
        {
            Id = Guid.NewGuid(),
            ModuleId = module3.Id,
            Title = "Analytics & Performance Tracking",
            Description = "How to measure and analyze your social media performance.",
            EstimatedDurationMinutes = 40,
            OrderIndex = 2,
            HasAssignment = true,
            AssignmentTitle = "Analytics Report",
            AssignmentInstructions = "Create a sample analytics report template for a social media campaign.",
            AssignmentSubmissionType = SubmissionType.Both,
            EnableReflection = true,
            EnableDiscussion = true,
            EnableRating = true,
            Status = LessonStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Lessons.AddRange(lesson5, lesson6);

        // Create Platform Settings
        var settings = new PlatformSettings
        {
            Id = Guid.NewGuid(),
            PlatformName = "KTA Learning Hub",
            PrimaryColor = "#4F46E5",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.PlatformSettings.Add(settings);

        await context.SaveChangesAsync();
    }
}
