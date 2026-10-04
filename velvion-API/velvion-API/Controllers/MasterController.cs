using Microsoft.AspNetCore.Mvc;
using velvion_API.DTOs.Common;
using velvion_API.DTOs.Master;
using velvion_API.Services.Interfaces;

namespace velvion_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MasterController : ControllerBase
{
    private readonly IMasterService _masterService;

    public MasterController(IMasterService masterService)
    {
        _masterService = masterService;
    }

    // ==========================================
    // 0. UNIFIED OVERVIEW (GET)
    // ==========================================
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<AllMastersDto>>> GetAllMasters()
    {
        var result = await _masterService.GetAllMastersAsync();
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 1. ROLES (GET & POST ONLY)
    // ==========================================
    [HttpGet("roles")]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetRoles([FromQuery] bool activeOnly = false)
    {
        var result = await _masterService.GetRolesAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("roles/{id:int}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleById(int id)
    {
        var result = await _masterService.GetRoleByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("roles/save")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> SaveRole([FromBody] SaveRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<RoleDto>.FailResult("Invalid model state."));

        var result = await _masterService.SaveRoleAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("roles/delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRole(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _masterService.DeleteRoleAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 2. MENUS (GET & POST ONLY)
    // ==========================================
    [HttpGet("menus")]
    public async Task<ActionResult<ApiResponse<List<MenuDto>>>> GetMenus([FromQuery] bool activeOnly = false)
    {
        var result = await _masterService.GetMenusAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("menus/tree")]
    public async Task<ActionResult<ApiResponse<List<MenuDto>>>> GetMenuTree()
    {
        var result = await _masterService.GetMenuTreeAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("menus/{id:int}")]
    public async Task<ActionResult<ApiResponse<MenuDto>>> GetMenuById(int id)
    {
        var result = await _masterService.GetMenuByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("menus/save")]
    public async Task<ActionResult<ApiResponse<MenuDto>>> SaveMenu([FromBody] SaveMenuDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<MenuDto>.FailResult("Invalid model state."));

        var result = await _masterService.SaveMenuAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("menus/delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMenu(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _masterService.DeleteMenuAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 3. SERVICES (GET & POST ONLY)
    // ==========================================
    [HttpGet("services")]
    public async Task<ActionResult<ApiResponse<List<ServiceMasterDto>>>> GetServices([FromQuery] bool activeOnly = false)
    {
        var result = await _masterService.GetServicesAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("services/{id:int}")]
    public async Task<ActionResult<ApiResponse<ServiceMasterDto>>> GetServiceById(int id)
    {
        var result = await _masterService.GetServiceByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("services/save")]
    public async Task<ActionResult<ApiResponse<ServiceMasterDto>>> SaveService([FromBody] SaveServiceMasterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ServiceMasterDto>.FailResult("Invalid model state."));

        var result = await _masterService.SaveServiceAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("services/delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteService(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _masterService.DeleteServiceAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 4. SETTINGS (GET & POST ONLY)
    // ==========================================
    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<List<SettingDto>>>> GetSettings([FromQuery] bool activeOnly = false)
    {
        var result = await _masterService.GetSettingsAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("settings/{id:int}")]
    public async Task<ActionResult<ApiResponse<SettingDto>>> GetSettingById(int id)
    {
        var result = await _masterService.GetSettingByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("settings/key/{key}")]
    public async Task<ActionResult<ApiResponse<SettingDto>>> GetSettingByKey(string key)
    {
        var result = await _masterService.GetSettingByKeyAsync(key);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("settings/save")]
    public async Task<ActionResult<ApiResponse<SettingDto>>> SaveSetting([FromBody] SaveSettingDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<SettingDto>.FailResult("Invalid model state."));

        var result = await _masterService.SaveSettingAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("settings/delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSetting(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _masterService.DeleteSettingAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 5. CATEGORIES & INDUSTRIES (GET & POST ONLY)
    // ==========================================
    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories([FromQuery] bool activeOnly = false)
    {
        var result = await _masterService.GetCategoriesAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("categories/{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategoryById(int id)
    {
        var result = await _masterService.GetCategoryByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("categories/save")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> SaveCategory([FromBody] SaveCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CategoryDto>.FailResult("Invalid model state."));

        var result = await _masterService.SaveCategoryAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("categories/delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _masterService.DeleteCategoryAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 6. DEPARTMENTS (GET & POST ONLY)
    // ==========================================
    [HttpGet("departments")]
    public async Task<ActionResult<ApiResponse<List<DepartmentDto>>>> GetDepartments([FromQuery] bool activeOnly = false)
    {
        var result = await _masterService.GetDepartmentsAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("departments/{id:int}")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetDepartmentById(int id)
    {
        var result = await _masterService.GetDepartmentByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("departments/save")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> SaveDepartment([FromBody] SaveDepartmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<DepartmentDto>.FailResult("Invalid model state."));

        var result = await _masterService.SaveDepartmentAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("departments/delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDepartment(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _masterService.DeleteDepartmentAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 6B. DESIGNATIONS (GET & POST ONLY)
    // ==========================================
    [HttpGet("designations")]
    public async Task<ActionResult<ApiResponse<List<DesignationDto>>>> GetDesignations([FromQuery] bool activeOnly = false, [FromQuery] int? departmentId = null)
    {
        var result = await _masterService.GetDesignationsAsync(activeOnly, departmentId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("designations/{id:int}")]
    public async Task<ActionResult<ApiResponse<DesignationDto>>> GetDesignationById(int id)
    {
        var result = await _masterService.GetDesignationByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("designations/save")]
    public async Task<ActionResult<ApiResponse<DesignationDto>>> SaveDesignation([FromBody] SaveDesignationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<DesignationDto>.FailResult("Invalid model state."));

        var result = await _masterService.SaveDesignationAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("designations/delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDesignation(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _masterService.DeleteDesignationAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 7. CLIENTS & PARTNERS (GET & POST ONLY)
    // ==========================================
    [HttpGet("clients")]
    public async Task<ActionResult<ApiResponse<List<ClientDto>>>> GetClients([FromQuery] bool activeOnly = false)
    {
        var result = await _masterService.GetClientsAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("clients/{id:int}")]
    public async Task<ActionResult<ApiResponse<ClientDto>>> GetClientById(int id)
    {
        var result = await _masterService.GetClientByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("clients/save")]
    public async Task<ActionResult<ApiResponse<ClientDto>>> SaveClient([FromBody] SaveClientDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ClientDto>.FailResult("Invalid model state."));

        var result = await _masterService.SaveClientAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("clients/delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteClient(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _masterService.DeleteClientAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 8. FAQS & KNOWLEDGE BASE (GET & POST ONLY)
    // ==========================================
    [HttpGet("faqs")]
    public async Task<ActionResult<ApiResponse<List<FaqDto>>>> GetFaqs([FromQuery] bool activeOnly = false, [FromQuery] string? category = null)
    {
        var result = await _masterService.GetFaqsAsync(activeOnly, category);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("faqs/{id:int}")]
    public async Task<ActionResult<ApiResponse<FaqDto>>> GetFaqById(int id)
    {
        var result = await _masterService.GetFaqByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("faqs/save")]
    public async Task<ActionResult<ApiResponse<FaqDto>>> SaveFaq([FromBody] SaveFaqDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<FaqDto>.FailResult("Invalid model state."));

        var result = await _masterService.SaveFaqAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("faqs/delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFaq(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _masterService.DeleteFaqAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }
}
