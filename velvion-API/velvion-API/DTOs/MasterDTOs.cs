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
// 5. UNIFIED ALL-MASTERS SUMMARY DTO
// ==========================================
public class AllMastersDto
{
    public List<RoleDto> Roles { get; set; } = new();
    public List<MenuDto> Menus { get; set; } = new();
    public List<ServiceMasterDto> Services { get; set; } = new();
    public List<SettingDto> Settings { get; set; } = new();
}

public class DeleteRequestDto
{
    public int Id { get; set; }
    public int? UpdatedBy { get; set; }
}
