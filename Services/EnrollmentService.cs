using KTALearningHub.API.Data;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Models;
using KTALearningHub.API.Models.Enums;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KTALearningHub.API.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;

    public EnrollmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<EnrollmentResponse>> EnrollInCourseAsync(Guid userId, EnrollCourseRequest request)
    {
        var course = await _context.Courses.FindAsync(request.CourseId);
        if (course == null)
            return ApiResponse<EnrollmentResponse>.Fail("Course not found.");

        var existingEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == request.CourseId);

        if (existingEnrollment != null)
            return ApiResponse<EnrollmentResponse>.Fail("You are already enrolled in this course.");

        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = request.CourseId,
            Status = course.Price > 0 ? EnrollmentStatus.Locked : EnrollmentStatus.Active,
            ProgressPercentage = 0
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        // If course is free, no payment needed
        if (course.Price == 0)
        {
            enrollment.Status = EnrollmentStatus.Active;
            await _context.SaveChangesAsync();
        }

        return ApiResponse<EnrollmentResponse>.Ok(await MapToEnrollmentResponse(enrollment),
            course.Price > 0 ? "Enrolled successfully. Complete payment to unlock." : "Enrolled successfully.");
    }

    public async Task<ApiResponse<PaymentResponse>> ProcessPaymentAsync(Guid userId, ProcessPaymentRequest request)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == request.EnrollmentId && e.UserId == userId);

        if (enrollment == null)
            return ApiResponse<PaymentResponse>.Fail("Enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Active)
            return ApiResponse<PaymentResponse>.Fail("Course is already unlocked.");

        // Create payment record
        var payment = new Payment
        {
            UserId = userId,
            CourseId = enrollment.CourseId,
            EnrollmentId = enrollment.Id,
            Amount = enrollment.Course.Price,
            PaymentMethod = request.PaymentMethod,
            PaymentReference = request.PaymentReference ?? Guid.NewGuid().ToString("N"),
            TransactionId = request.TransactionId,
            Status = PaymentStatus.Completed,
            PaidAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        // Activate enrollment
        enrollment.Status = EnrollmentStatus.Active;

        await _context.SaveChangesAsync();

        var response = new PaymentResponse
        {
            Id = payment.Id,
            UserId = payment.UserId,
            StudentName = enrollment.User.FullName,
            CourseId = payment.CourseId,
            CourseTitle = enrollment.Course.Title,
            EnrollmentId = payment.EnrollmentId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            PaymentReference = payment.PaymentReference,
            PaymentMethod = payment.PaymentMethod,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        };

        return ApiResponse<PaymentResponse>.Ok(response, "Payment successful. Course unlocked.");
    }

    public async Task<ApiResponse<List<EnrollmentResponse>>> GetUserEnrollmentsAsync(Guid userId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.User)
            .Include(e => e.Payment)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        var responses = new List<EnrollmentResponse>();
        foreach (var enrollment in enrollments)
        {
            responses.Add(await MapToEnrollmentResponse(enrollment));
        }

        return ApiResponse<List<EnrollmentResponse>>.Ok(responses);
    }

    public async Task<ApiResponse<EnrollmentResponse>> GetEnrollmentAsync(Guid userId, Guid courseId)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.User)
            .Include(e => e.Payment)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (enrollment == null)
            return ApiResponse<EnrollmentResponse>.Fail("Enrollment not found.");

        return ApiResponse<EnrollmentResponse>.Ok(await MapToEnrollmentResponse(enrollment));
    }

    private async Task<EnrollmentResponse> MapToEnrollmentResponse(Enrollment enrollment)
    {
        // Calculate progress
        var course = enrollment.Course ?? await _context.Courses.FindAsync(enrollment.CourseId);
        var user = enrollment.User ?? await _context.Users.FindAsync(enrollment.UserId);

        var totalLessons = await _context.Lessons
            .CountAsync(l => _context.Modules
                .Where(m => m.CourseId == enrollment.CourseId)
                .Select(m => m.Id)
                .Contains(l.ModuleId));

        var completedLessons = 0;
        if (totalLessons > 0)
        {
            var lessonIds = await _context.Lessons
                .Where(l => _context.Modules
                    .Where(m => m.CourseId == enrollment.CourseId)
                    .Select(m => m.Id)
                    .Contains(l.ModuleId))
                .Select(l => l.Id)
                .ToListAsync();

            completedLessons = await _context.LessonProgresses
                .CountAsync(p => p.UserId == enrollment.UserId && lessonIds.Contains(p.LessonId) && p.IsFullyCompleted);
        }

        var progress = totalLessons > 0 ? Math.Round((decimal)completedLessons / totalLessons * 100, 2) : 0;

        // Update progress in enrollment
        enrollment.ProgressPercentage = progress;
        if (progress >= 100 && enrollment.Status == EnrollmentStatus.Active)
        {
            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.CompletedAt = DateTime.UtcNow;
        }

        return new EnrollmentResponse
        {
            Id = enrollment.Id,
            UserId = enrollment.UserId,
            StudentName = user?.FullName ?? string.Empty,
            CourseId = enrollment.CourseId,
            CourseTitle = course?.Title ?? string.Empty,
            Status = enrollment.Status.ToString(),
            ProgressPercentage = progress,
            CoursePrice = course?.Price ?? 0,
            EnrolledAt = enrollment.EnrolledAt,
            CompletedAt = enrollment.CompletedAt,
            IsPaid = enrollment.Payment != null && enrollment.Payment.Status == PaymentStatus.Completed
        };
    }
}
