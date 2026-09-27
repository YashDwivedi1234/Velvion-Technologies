using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace velvion_API.Entities;

[Table("tbl_users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RoleId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Mobile { get; set; }

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RoleId))]
    public virtual Role? Role { get; set; }
}

[Table("tbl_role_menu_permission")]
public class RoleMenuPermission
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RoleId { get; set; }

    [Required]
    public int MenuId { get; set; }

    public bool CanView { get; set; } = false;
    public bool CanAdd { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RoleId))]
    public virtual Role? Role { get; set; }

    [ForeignKey(nameof(MenuId))]
    public virtual Menu? Menu { get; set; }
}

[Table("tbl_blogs")]
public class Blog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public int? AuthorId { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(AuthorId))]
    public virtual User? Author { get; set; }
}

[Table("tbl_portfolio")]
public class Portfolio
{
    [Key]
    public int Id { get; set; }

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
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("tbl_team_members")]
public class TeamMember
{
    [Key]
    public int Id { get; set; }

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
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("tbl_testimonials")]
public class Testimonial
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CompanyName { get; set; }

    [Required]
    public string Review { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public int? Rating { get; set; } = 5;

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("tbl_contact_inquiries")]
public class ContactInquiry
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public string? Message { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("tbl_audit_logs")]
public class AuditLog
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [MaxLength(50)]
    public string? ActionName { get; set; }

    [MaxLength(50)]
    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public string? LogDetails { get; set; }

    [MaxLength(50)]
    public string? IPAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
