namespace KTALearningHub.API.Models;

public class CommentLike
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CommentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Comment Comment { get; set; } = null!;
}
