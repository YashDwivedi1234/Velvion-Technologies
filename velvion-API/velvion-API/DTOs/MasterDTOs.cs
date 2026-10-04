using System.ComponentModel.DataAnnotations;

namespace velvion_API.DTOs.Master;

// ==========================================
// 1. ROLE DTOs
// ==========================================
public class RoleDto
{
    public int Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveRoleDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required(ErrorMessage = "Role name is required.")]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 2. MENU DTOs
// ==========================================
public class MenuDto
{
    public int Id { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public string? RouteUrl { get; set; }
    public string? Icon { get; set; }
    public int? ParentMenuId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<MenuDto> SubMenus { get; set; } = new();
}

public class SaveMenuDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required(ErrorMessage = "Menu name is required.")]
    [MaxLength(100)]
    public string MenuName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? RouteUrl { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    public int? ParentMenuId { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 3. SERVICE MASTER DTOs
// ==========================================
public class ServiceMasterDto
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveServiceMasterDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required(ErrorMessage = "Service name is required.")]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(255)]
    public string? IconUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 4. SETTING MASTER DTOs
// ==========================================
public class SettingDto
{
    public int Id { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveSettingDto
{
    public int Id { get; set; } = 0; // 0 = Insert, > 0 = Update

    [Required(ErrorMessage = "Setting key is required.")]
    [MaxLength(100)]
    public string SettingKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Setting value is required.")]
    public string SettingValue { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 5. CATEGORY & INDUSTRY DTOs
// ==========================================
public class CategoryDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveCategoryDto
{
    public int Id { get; set; } = 0;

    [Required(ErrorMessage = "Category name is required.")]
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
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 6. DEPARTMENT DTOs
// ==========================================
public class DepartmentDto
{
    public int Id { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Designations { get; set; }
    public string? HeadOfDepartment { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveDepartmentDto
{
    public int Id { get; set; } = 0;

    [Required(ErrorMessage = "Department name is required.")]
    [MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Designations { get; set; }

    [MaxLength(100)]
    public string? HeadOfDepartment { get; set; }

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 6B. DESIGNATION DTOs
// ==========================================
public class DesignationDto
{
    public int Id { get; set; }
    public string DesignationName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveDesignationDto
{
    public int Id { get; set; } = 0;

    [Required(ErrorMessage = "Designation name is required.")]
    [MaxLength(100)]
    public string DesignationName { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 7. CLIENT & PARTNER DTOs
// ==========================================
public class ClientDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Industry { get; set; }
    public string? PartnerTier { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveClientDto
{
    public int Id { get; set; } = 0;

    [Required(ErrorMessage = "Client name is required.")]
    [MaxLength(150)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(255)]
    public string? WebsiteUrl { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }

    [MaxLength(50)]
    public string? PartnerTier { get; set; } = "Enterprise Client";

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 8. FAQ & KNOWLEDGE BASE DTOs
// ==========================================
public class FaqDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveFaqDto
{
    public int Id { get; set; } = 0;

    [Required(ErrorMessage = "Question is required.")]
    [MaxLength(300)]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "Answer is required.")]
    public string Answer { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; } = "General";

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int? UpdatedBy { get; set; }
}

// ==========================================
// 9. UNIFIED ALL-MASTERS SUMMARY DTO
// ==========================================
public class AllMastersDto
{
    public List<RoleDto> Roles { get; set; } = new();
    public List<MenuDto> Menus { get; set; } = new();
    public List<ServiceMasterDto> Services { get; set; } = new();
    public List<SettingDto> Settings { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<DepartmentDto> Departments { get; set; } = new();
    public List<DesignationDto> Designations { get; set; } = new();
    public List<ClientDto> Clients { get; set; } = new();
    public List<FaqDto> Faqs { get; set; } = new();
}

public class DeleteRequestDto
{
    public int Id { get; set; }
    public int? UpdatedBy { get; set; }
}
