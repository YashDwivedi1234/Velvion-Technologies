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

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Mobile { get; set; }

    public string? Password { get; set; } // Required on insert (checked in service), optional on update

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
// 3. BLOG DTOs
// ==========================================
public class BlogDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? AuthorId { get; set; }
    public string? AuthorName { get; set; }
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

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public int? AuthorId { get; set; }
    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 4. PORTFOLIO DTOs
// ==========================================
public class PortfolioDto
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string? TechStack { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SavePortfolioDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    [MaxLength(150)]
    public string ProjectName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ClientName { get; set; }

    [MaxLength(255)]
    public string? TechStack { get; set; }

    public string? Description { get; set; }

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 5. TEAM MEMBER DTOs
// ==========================================
public class TeamMemberDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveTeamMemberDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Designation { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    [MaxLength(255)]
    public string? LinkedInUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 6. TESTIMONIAL DTOs
// ==========================================
public class TestimonialDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Review { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? Rating { get; set; }
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
    public string? CompanyName { get; set; }

    [Required]
    public string Review { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    [Range(1, 5)]
    public int? Rating { get; set; } = 5;

    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 7. CONTACT INQUIRY DTOs
// ==========================================
public class ContactInquiryDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Message { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveContactInquiryDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public string? Message { get; set; }
    public bool IsActive { get; set; } = true;
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

public class CommonDeleteDto
{
    public int Id { get; set; }
    public int? UpdatedBy { get; set; }
}
