using System.ComponentModel.DataAnnotations;

namespace KTALearningHub.API.DTOs;

// ===== AUTH REQUEST DTOs =====
public class RegisterRequest
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(300)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    [MaxLength(200)]
    public string? FullName { get; set; }

    [EmailAddress, MaxLength(300)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }
}

// ===== AUTH RESPONSE DTOs =====
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserResponse User { get; set; } = null!;
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime? LastLogin { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
