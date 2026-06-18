using System.Text;
using KTALearningHub.API.Data;
using KTALearningHub.API.Middleware;
using KTALearningHub.API.Services;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== AUTHENTICATION =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ===== SERVICES (Dependency Injection) =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ILearningService, LearningService>();
builder.Services.AddScoped<IDiscussionService, DiscussionService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IFileService, FileService>();

// ===== CONTROLLERS =====
builder.Services.AddControllers();

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://yourdomain.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ===== SWAGGER / OpenAPI =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KTA Learning Hub API",
        Version = "v1",
        Description = "Complete Learning Management System API for KTA Learning Hub",
        Contact = new OpenApiContact
        {
            Name = "KTA Learning Hub",
            Email = "support@ktalearninghub.com"
        }
    });

    // JWT Authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KTA Learning Hub API v1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// FIX: Serve files from wwwroot/uploads at /uploads URL path
app.UseStaticFiles(); // Keep this for default wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// ===== DATABASE MIGRATION & SEEDING =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await DbInitializer.SeedAsync(context);
}

app.Run();
