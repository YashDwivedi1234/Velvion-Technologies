using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace velvion_API.Entities;

[Table("md_roles")]
public class Role
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("md_menu")]
public class Menu
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string MenuName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? RouteUrl { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    public int? ParentMenuId { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("md_services")]
public class ServiceMaster
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(255)]
    public string? IconUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("md_settings")]
public class Setting
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string SettingKey { get; set; } = string.Empty;

    [Required]
    public string SettingValue { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// ==========================================
// NEW MASTER ENTITIES
// ==========================================

[Table("md_categories")]
public class CategoryMaster
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Slug { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; } = "folder";

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("md_departments")]
public class DepartmentMaster
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Designations { get; set; } // Overview or legacy summary

    [MaxLength(100)]
    public string? HeadOfDepartment { get; set; }

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<DesignationMaster> DesignationList { get; set; } = new List<DesignationMaster>();
}

[Table("md_designations")]
public class DesignationMaster
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string DesignationName { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(DepartmentId))]
    public virtual DepartmentMaster? Department { get; set; }
}

[Table("md_clients")]
public class ClientMaster
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(255)]
    public string? WebsiteUrl { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }

    [MaxLength(50)]
    public string? PartnerTier { get; set; } = "Enterprise Client"; // Enterprise Client, Strategic Partner, Technology Partner

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("md_faqs")]
public class FaqMaster
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(300)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; } = "General"; // Cloud & AI, Security, SLA & Billing, Support

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
