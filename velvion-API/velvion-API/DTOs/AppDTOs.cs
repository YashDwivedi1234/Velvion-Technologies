using System.ComponentModel.DataAnnotations;

namespace velvion_API.DTOs.App;

// ==========================================
// 1. USER DTOs
// ==========================================
public class UserDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveUserDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    public int RoleId { get; set; }

    public int? DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Mobile { get; set; }

    public string? Password { get; set; } // Required on insert, optional on update

    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public string? Token { get; set; }
}

// ==========================================
// 2. ROLE MENU PERMISSION DTOs
// ==========================================
public class RoleMenuPermissionDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int MenuId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public string? RouteUrl { get; set; }
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BulkSaveRolePermissionDto
{
    [Required]
    public int RoleId { get; set; }
    public List<MenuPermissionItemDto> Permissions { get; set; } = new();
    public int? UpdatedBy { get; set; }
}

public class MenuPermissionItemDto
{
    public int MenuId { get; set; }
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

// ==========================================
// 3. BLOG DTOs — Synced with Angular UI model
// ==========================================
public class BlogDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public string? BannerImage { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveBlogDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Slug { get; set; }

    [MaxLength(500)]
    public string? Summary { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    [MaxLength(500)]
    public string? BannerImage { get; set; }

    public int? AuthorId { get; set; }
    public bool IsPublished { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 4. PORTFOLIO DTOs — Synced with Angular UI model
// ==========================================
public class PortfolioDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string? Category { get; set; }
    public string? ProjectUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Description { get; set; }
    public string? Technologies { get; set; }
    public DateTime? CompletionDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SavePortfolioDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ClientName { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(255)]
    public string? ProjectUrl { get; set; }

    [MaxLength(255)]
    public string? ThumbnailUrl { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Technologies { get; set; }

    public DateTime? CompletionDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 5. TEAM MEMBER DTOs — Synced with Angular UI model & User Login
// ==========================================
public class TeamMemberDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? ProfileImage { get; set; }
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GithubUrl { get; set; }
    public int? UserId { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public bool HasLoginAccess { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveTeamMemberDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Designation { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Mobile { get; set; }

    [MaxLength(255)]
    public string? ProfileImage { get; set; }

    public string? Bio { get; set; }

    [MaxLength(255)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(255)]
    public string? GithubUrl { get; set; }

    public bool EnableLoginAccess { get; set; } = true;
    public int? RoleId { get; set; }
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 6. TESTIMONIAL DTOs — Synced with Angular UI model
// ==========================================
public class TestimonialDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? ClientDesignation { get; set; }
    public string? CompanyName { get; set; }
    public string? AvatarUrl { get; set; }
    public int Rating { get; set; }
    public string FeedbackText { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveTestimonialDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    [MaxLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ClientDesignation { get; set; }

    [MaxLength(100)]
    public string? CompanyName { get; set; }

    [MaxLength(255)]
    public string? AvatarUrl { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    [Required]
    public string FeedbackText { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 7. CONTACT INQUIRY DTOs — Synced with Angular UI model
// ==========================================
public class ContactInquiryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveContactInquiryDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Subject { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    public bool IsResolved { get; set; } = false;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 8. AUDIT LOG DTOs
// ==========================================
public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ActionName { get; set; }
    public string? TableName { get; set; }
    public int? RecordId { get; set; }
    public string? LogDetails { get; set; }
    public string? IPAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAuditLogDto
{
    public int? UserId { get; set; }
    public string? ActionName { get; set; }
    public string? TableName { get; set; }
    public int? RecordId { get; set; }
    public string? LogDetails { get; set; }
    public string? IPAddress { get; set; }
}

// ==========================================
// 9. JOB POSTING DTOs — New
// ==========================================
public class JobPostingDto
{
    public int Id { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Location { get; set; }
    public string? JobType { get; set; }
    public string? ExperienceLevel { get; set; }
    public string? Description { get; set; }
    public string? RequiredSkills { get; set; }
    public string? SalaryRange { get; set; }
    public DateTime? LastDateToApply { get; set; }
    public int ApplicationCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveJobPostingDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    [MaxLength(150)]
    public string JobTitle { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string? JobType { get; set; }

    [MaxLength(50)]
    public string? ExperienceLevel { get; set; }

    public string? Description { get; set; }
    public string? RequiredSkills { get; set; }

    [MaxLength(100)]
    public string? SalaryRange { get; set; }

    public DateTime? LastDateToApply { get; set; }
    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 10. JOB APPLICATION DTOs — New
// ==========================================
public class JobApplicationDto
{
    public int Id { get; set; }
    public int JobPostingId { get; set; }
    public string? JobTitle { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ResumeUrl { get; set; }
    public string? CoverLetter { get; set; }
    public string Status { get; set; } = "Applied";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveJobApplicationDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    public int JobPostingId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? ResumeUrl { get; set; }

    public string? CoverLetter { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Applied";

    public string? Notes { get; set; }
    public int? UpdatedBy { get; set; }
}

// ==========================================
// COMMON DTOs
// ==========================================
public class CommonDeleteDto
{
    public int Id { get; set; }
    public int? UpdatedBy { get; set; }
}

// Dashboard Stats DTO
public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalBlogs { get; set; }
    public int PublishedBlogs { get; set; }
    public int TotalPortfolios { get; set; }
    public int TotalTeamMembers { get; set; }
    public int TotalTestimonials { get; set; }
    public int TotalInquiries { get; set; }
    public int UnresolvedInquiries { get; set; }
    public int ActiveJobPostings { get; set; }
    public int TotalJobApplications { get; set; }
    public List<ContactInquiryDto> RecentInquiries { get; set; } = new();
    public List<AuditLogDto> RecentAuditLogs { get; set; } = new();
}
