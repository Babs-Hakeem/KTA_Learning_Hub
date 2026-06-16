using KTALearningHub.API.Models.Enums;

namespace KTALearningHub.API.Models;

public class Enrollment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Locked;
    public decimal ProgressPercentage { get; set; } = 0;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public Payment? Payment { get; set; }
}
