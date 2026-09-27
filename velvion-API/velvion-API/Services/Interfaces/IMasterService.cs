using velvion_API.DTOs.Common;
using velvion_API.DTOs.Master;

namespace velvion_API.Services.Interfaces;

public interface IMasterService
{
    // Unified overview (GET)
    Task<ApiResponse<AllMastersDto>> GetAllMastersAsync();

    // 1. Roles (GET & POST)
    Task<ApiResponse<List<RoleDto>>> GetRolesAsync(bool activeOnly = false);
    Task<ApiResponse<RoleDto>> GetRoleByIdAsync(int id);
    Task<ApiResponse<RoleDto>> SaveRoleAsync(SaveRoleDto dto);
    Task<ApiResponse<bool>> DeleteRoleAsync(int id, int? updatedBy = null);

    // 2. Menus (GET & POST)
    Task<ApiResponse<List<MenuDto>>> GetMenusAsync(bool activeOnly = false);
    Task<ApiResponse<List<MenuDto>>> GetMenuTreeAsync();
    Task<ApiResponse<MenuDto>> GetMenuByIdAsync(int id);
    Task<ApiResponse<MenuDto>> SaveMenuAsync(SaveMenuDto dto);
    Task<ApiResponse<bool>> DeleteMenuAsync(int id, int? updatedBy = null);

    // 3. Services (GET & POST)
    Task<ApiResponse<List<ServiceMasterDto>>> GetServicesAsync(bool activeOnly = false);
    Task<ApiResponse<ServiceMasterDto>> GetServiceByIdAsync(int id);
    Task<ApiResponse<ServiceMasterDto>> SaveServiceAsync(SaveServiceMasterDto dto);
    Task<ApiResponse<bool>> DeleteServiceAsync(int id, int? updatedBy = null);

    // 4. Settings (GET & POST)
    Task<ApiResponse<List<SettingDto>>> GetSettingsAsync(bool activeOnly = false);
    Task<ApiResponse<SettingDto>> GetSettingByIdAsync(int id);
    Task<ApiResponse<SettingDto>> GetSettingByKeyAsync(string key);
    Task<ApiResponse<SettingDto>> SaveSettingAsync(SaveSettingDto dto);
    Task<ApiResponse<bool>> DeleteSettingAsync(int id, int? updatedBy = null);
}
