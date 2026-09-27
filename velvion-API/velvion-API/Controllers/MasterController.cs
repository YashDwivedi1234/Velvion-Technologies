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
}
