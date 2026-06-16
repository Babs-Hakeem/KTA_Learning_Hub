using KTALearningHub.API.DTOs;

namespace KTALearningHub.API.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<UserResponse>> GetProfileAsync(Guid userId);
    Task<ApiResponse<UserResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}

public interface ICourseService
{
    Task<ApiResponse<CourseResponse>> CreateCourseAsync(CreateCourseRequest request);
    Task<ApiResponse<CourseResponse>> UpdateCourseAsync(Guid courseId, UpdateCourseRequest request);
    Task<ApiResponse<bool>> DeleteCourseAsync(Guid courseId);
    Task<ApiResponse<CourseDetailResponse>> GetCourseByIdAsync(Guid courseId);
    Task<ApiResponse<CourseListResponse>> GetCoursesAsync(PaginationParams paginationParams);
    Task<ApiResponse<List<CourseResponse>>> GetPublishedCoursesAsync();

    // Modules
    Task<ApiResponse<ModuleResponse>> CreateModuleAsync(CreateModuleRequest request);
    Task<ApiResponse<ModuleResponse>> UpdateModuleAsync(Guid moduleId, UpdateModuleRequest request);
    Task<ApiResponse<bool>> DeleteModuleAsync(Guid moduleId);
    Task<ApiResponse<List<ModuleResponse>>> GetModulesByCourseAsync(Guid courseId);
    Task<ApiResponse<bool>> ReorderModulesAsync(ReorderModulesRequest request);
}

public interface ILessonService
{
    Task<ApiResponse<LessonDetailResponse>> CreateLessonAsync(CreateLessonRequest request);
    Task<ApiResponse<LessonDetailResponse>> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request);
    Task<ApiResponse<bool>> DeleteLessonAsync(Guid lessonId);
    Task<ApiResponse<LessonDetailResponse>> GetLessonByIdAsync(Guid lessonId);
    Task<ApiResponse<StudentLessonResponse>> GetStudentLessonAsync(Guid lessonId, Guid userId);
    Task<ApiResponse<List<LessonSummaryResponse>>> GetLessonsByModuleAsync(Guid moduleId);

    // Content management
    Task<ApiResponse<LessonDetailResponse>> UpdateLessonContentAsync(Guid lessonId, UpdateLessonContentRequest request);
    Task<ApiResponse<LessonDetailResponse>> UpdateLessonAudioAsync(Guid lessonId, UpdateLessonAudioRequest request);
    Task<ApiResponse<LessonDetailResponse>> UpdateLessonAssignmentAsync(Guid lessonId, UpdateLessonAssignmentRequest request);
    Task<ApiResponse<LessonDetailResponse>> UpdateLessonReflectionAsync(Guid lessonId, UpdateLessonReflectionRequest request);
    Task<ApiResponse<LessonDetailResponse>> UpdateLessonCommunityAsync(Guid lessonId, UpdateLessonCommunityRequest request);
    Task<ApiResponse<LessonDetailResponse>> UpdateLessonRatingSettingsAsync(Guid lessonId, UpdateLessonRatingSettingsRequest request);
    Task<ApiResponse<LessonDetailResponse>> PublishLessonAsync(Guid lessonId, PublishLessonRequest request);
    Task<ApiResponse<bool>> ReorderLessonsAsync(ReorderLessonsRequest request);
}

public interface IEnrollmentService
{
    Task<ApiResponse<EnrollmentResponse>> EnrollInCourseAsync(Guid userId, EnrollCourseRequest request);
    Task<ApiResponse<PaymentResponse>> ProcessPaymentAsync(Guid userId, ProcessPaymentRequest request);
    Task<ApiResponse<List<EnrollmentResponse>>> GetUserEnrollmentsAsync(Guid userId);
    Task<ApiResponse<EnrollmentResponse>> GetEnrollmentAsync(Guid userId, Guid courseId);
}

public interface ILearningService
{
    // Progress
    Task<ApiResponse<LessonProgressResponse>> GetLessonProgressAsync(Guid userId, Guid lessonId);
    Task<ApiResponse<LessonProgressResponse>> MarkStepCompleteAsync(Guid userId, Guid lessonId, MarkStepCompleteRequest request);

    // Assignments
    Task<ApiResponse<AssignmentSubmissionResponse>> SubmitAssignmentAsync(Guid userId, SubmitAssignmentRequest request);
    Task<ApiResponse<AssignmentSubmissionResponse>> ReviewAssignmentAsync(Guid submissionId, Guid reviewerId, ReviewAssignmentRequest request);
    Task<ApiResponse<AssignmentListResponse>> GetAssignmentsAsync(PaginationParams paginationParams, Guid? courseId = null, Guid? lessonId = null, string? status = null);
    Task<ApiResponse<AssignmentSubmissionResponse>> GetAssignmentByIdAsync(Guid submissionId);

    // Reflections
    Task<ApiResponse<ReflectionSubmissionResponse>> SubmitReflectionAsync(Guid userId, SubmitReflectionRequest request);
    Task<ApiResponse<ReflectionSubmissionResponse>> ReviewReflectionAsync(Guid submissionId, Guid reviewerId, ReviewReflectionRequest request);
    Task<ApiResponse<ReflectionListResponse>> GetReflectionsAsync(PaginationParams paginationParams, Guid? courseId = null, Guid? lessonId = null, string? reflectionType = null);
    Task<ApiResponse<ReflectionSubmissionResponse>> GetReflectionByIdAsync(Guid submissionId);

    // Ratings
    Task<ApiResponse<LessonRatingResponse>> SubmitRatingAsync(Guid userId, SubmitRatingRequest request);
    Task<ApiResponse<List<LessonRatingResponse>>> GetLessonRatingsAsync(Guid lessonId);
}

public interface IDiscussionService
{
    Task<ApiResponse<CommentResponse>> CreateCommentAsync(Guid userId, CreateCommentRequest request);
    Task<ApiResponse<CommentResponse>> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentRequest request);
    Task<ApiResponse<bool>> DeleteCommentAsync(Guid commentId, Guid userId, bool isAdmin);
    Task<ApiResponse<bool>> ToggleHideCommentAsync(Guid commentId);
    Task<ApiResponse<bool>> TogglePinCommentAsync(Guid commentId);
    Task<ApiResponse<bool>> LikeCommentAsync(Guid commentId, Guid userId);
    Task<ApiResponse<CommentListResponse>> GetLessonCommentsAsync(Guid lessonId, Guid? currentUserId = null);
    Task<ApiResponse<CommentListResponse>> GetAllCommentsAsync(PaginationParams paginationParams, Guid? courseId = null, Guid? lessonId = null);
}

public interface IAdminService
{
    Task<ApiResponse<AdminDashboardResponse>> GetDashboardAsync();
    Task<ApiResponse<StudentListResponse>> GetStudentsAsync(PaginationParams paginationParams);
    Task<ApiResponse<StudentDetailResponse>> GetStudentDetailAsync(Guid studentId);
    Task<ApiResponse<bool>> UpdateStudentStatusAsync(Guid studentId, UpdateStudentStatusRequest request);
    Task<ApiResponse<UserResponse>> CreateStudentAsync(CreateStudentRequest request);
    Task<ApiResponse<CourseAnalyticsResponse>> GetCourseAnalyticsAsync(Guid courseId);
    Task<ApiResponse<PlatformAnalyticsResponse>> GetPlatformAnalyticsAsync();
    Task<ApiResponse<PlatformSettingsResponse>> GetPlatformSettingsAsync();
    Task<ApiResponse<PlatformSettingsResponse>> UpdatePlatformSettingsAsync(UpdatePlatformSettingsRequest request);
}

public interface IFileService
{
    Task<FileUploadResponse> UploadFileAsync(IFormFile file, string subFolder);
    bool DeleteFile(string fileUrl);
}
