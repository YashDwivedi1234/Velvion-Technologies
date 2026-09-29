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

// ==========================================
// BLOG — Enhanced with slug, summary, category, tags, publish status
// ==========================================
[Table("tbl_blogs")]
public class Blog
{
    [Key]
    public int Id { get; set; }

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
    public DateTime? PublishedAt { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(AuthorId))]
    public virtual User? Author { get; set; }
}

// ==========================================
// PORTFOLIO — Enhanced with category, tech stack, project URL
// ==========================================
[Table("tbl_portfolio")]
public class Portfolio
{
    [Key]
    public int Id { get; set; }

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
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// ==========================================
// TEAM MEMBER — Enhanced with department, bio, email, github
// ==========================================
[Table("tbl_team_members")]
public class TeamMember
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Designation { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? ProfileImage { get; set; }

    public string? Bio { get; set; }

    [MaxLength(255)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(255)]
    public string? GithubUrl { get; set; }

    public int? UserId { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}

// ==========================================
// TESTIMONIAL — Enhanced with designation, feedbackText, avatarUrl
// ==========================================
[Table("tbl_testimonials")]
public class Testimonial
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ClientDesignation { get; set; }

    [MaxLength(100)]
    public string? CompanyName { get; set; }

    [MaxLength(255)]
    public string? AvatarUrl { get; set; }

    public int Rating { get; set; } = 5;

    [Required]
    public string FeedbackText { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// ==========================================
// CONTACT INQUIRY — Enhanced with fullName, subject, isResolved
// ==========================================
[Table("tbl_contact_inquiries")]
public class ContactInquiry
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Subject { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsResolved { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// ==========================================
// AUDIT LOG
// ==========================================
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

// ==========================================
// JOB POSTING — New entity for Careers page
// ==========================================
[Table("tbl_job_postings")]
public class JobPosting
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string JobTitle { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string? JobType { get; set; } // Full-time, Part-time, Remote, Contract

    [MaxLength(50)]
    public string? ExperienceLevel { get; set; } // Fresher, 1-3 yrs, 3-6 yrs, 6+

    public string? Description { get; set; }

    public string? RequiredSkills { get; set; }

    [MaxLength(100)]
    public string? SalaryRange { get; set; }

    public DateTime? LastDateToApply { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}

// ==========================================
// JOB APPLICATION — New entity for ATS
// ==========================================
[Table("tbl_job_applications")]
public class JobApplication
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int JobPostingId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? ResumeUrl { get; set; }

    public string? CoverLetter { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Applied"; // Applied, Shortlisted, Interview, Rejected, Hired

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(JobPostingId))]
    public virtual JobPosting? JobPosting { get; set; }
}
