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

    // 5. Categories & Industries (GET & POST)
    Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync(bool activeOnly = false);
    Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
    Task<ApiResponse<CategoryDto>> SaveCategoryAsync(SaveCategoryDto dto);
    Task<ApiResponse<bool>> DeleteCategoryAsync(int id, int? updatedBy = null);

    // 6. Departments (GET & POST)
    Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync(bool activeOnly = false);
    Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id);
    Task<ApiResponse<DepartmentDto>> SaveDepartmentAsync(SaveDepartmentDto dto);
    Task<ApiResponse<bool>> DeleteDepartmentAsync(int id, int? updatedBy = null);

    // 6B. Designations (GET & POST)
    Task<ApiResponse<List<DesignationDto>>> GetDesignationsAsync(bool activeOnly = false, int? departmentId = null);
    Task<ApiResponse<DesignationDto>> GetDesignationByIdAsync(int id);
    Task<ApiResponse<DesignationDto>> SaveDesignationAsync(SaveDesignationDto dto);
    Task<ApiResponse<bool>> DeleteDesignationAsync(int id, int? updatedBy = null);

    // 7. Clients & Partners (GET & POST)
    Task<ApiResponse<List<ClientDto>>> GetClientsAsync(bool activeOnly = false);
    Task<ApiResponse<ClientDto>> GetClientByIdAsync(int id);
    Task<ApiResponse<ClientDto>> SaveClientAsync(SaveClientDto dto);
    Task<ApiResponse<bool>> DeleteClientAsync(int id, int? updatedBy = null);

    // 8. FAQs & Knowledge Base (GET & POST)
    Task<ApiResponse<List<FaqDto>>> GetFaqsAsync(bool activeOnly = false, string? category = null);
    Task<ApiResponse<FaqDto>> GetFaqByIdAsync(int id);
    Task<ApiResponse<FaqDto>> SaveFaqAsync(SaveFaqDto dto);
    Task<ApiResponse<bool>> DeleteFaqAsync(int id, int? updatedBy = null);
}
