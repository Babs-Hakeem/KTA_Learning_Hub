namespace KTALearningHub.API.Models.Enums;

public enum UserRole
{
    Student = 0,
    Admin = 1
}

public enum UserStatus
{
    Active = 0,
    Inactive = 1
}

public enum CourseStatus
{
    Draft = 0,
    Published = 1
}

public enum CourseLevel
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2
}

public enum EnrollmentStatus
{
    Locked = 0,
    Active = 1,
    Completed = 2
}

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}

public enum SubmissionType
{
    Text = 0,
    Document = 1,
    Both = 2
}

public enum AssignmentReviewStatus
{
    Pending = 0,
    Reviewed = 1,
    Approved = 2,
    NeedsRevision = 3
}

public enum ReflectionType
{
    Text = 0,
    Voice = 1,
    Document = 2
}

public enum LessonStatus
{
    Draft = 0,
    Preview = 1,
    Published = 2
}
