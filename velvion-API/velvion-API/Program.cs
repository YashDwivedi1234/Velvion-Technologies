using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using velvion_API.Data;
using velvion_API.Services.Implementations;
using velvion_API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection (MySQL via Pomelo EF Core)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// 2. Dependency Injection — Core App Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<ITeamMemberService, TeamMemberService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IContactInquiryService, ContactInquiryService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// 3. Dependency Injection — New Modules
builder.Services.AddScoped<IJobPostingService, JobPostingService>();
builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// 4. Master Service
builder.Services.AddScoped<IMasterService, MasterService>();

// 5. CORS Configuration (Angular frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 6. Controllers & OpenAPI / Scalar Documentation
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 7. HTTP Pipeline Configuration
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();