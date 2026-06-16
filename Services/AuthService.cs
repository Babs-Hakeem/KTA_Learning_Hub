using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KTALearningHub.API.Data;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Models;
using KTALearningHub.API.Models.Enums;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KTALearningHub.API.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            return ApiResponse<AuthResponse>.Fail("A user with this email already exists.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Student,
            Status = UserStatus.Active
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var response = new AuthResponse
        {
            Token = token,
            User = MapToUserResponse(user)
        };

        return ApiResponse<AuthResponse>.Ok(response, "Registration successful.");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");

        if (user.Status == UserStatus.Inactive)
            return ApiResponse<AuthResponse>.Fail("Your account has been deactivated. Please contact support.");

        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var response = new AuthResponse
        {
            Token = token,
            User = MapToUserResponse(user)
        };

        return ApiResponse<AuthResponse>.Ok(response, "Login successful.");
    }

    public async Task<ApiResponse<UserResponse>> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return ApiResponse<UserResponse>.Fail("User not found.");

        return ApiResponse<UserResponse>.Ok(MapToUserResponse(user));
    }

    public async Task<ApiResponse<UserResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return ApiResponse<UserResponse>.Fail("User not found.");

        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Email))
        {
            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Id != userId);
            if (emailExists)
                return ApiResponse<UserResponse>.Fail("Email is already in use.");
            user.Email = request.Email.ToLower();
        }

        if (request.ProfileImageUrl != null)
            user.ProfileImageUrl = request.ProfileImageUrl;

        await _context.SaveChangesAsync();
        return ApiResponse<UserResponse>.Ok(MapToUserResponse(user), "Profile updated successfully.");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return ApiResponse<bool>.Fail("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return ApiResponse<bool>.Fail("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Password changed successfully.");
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("userId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationInMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserResponse MapToUserResponse(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString(),
        ProfileImageUrl = user.ProfileImageUrl,
        LastLogin = user.LastLogin,
        Status = user.Status.ToString(),
        CreatedAt = user.CreatedAt
    };
}
