using Microsoft.EntityFrameworkCore;
using velvion_API.Data;
using velvion_API.DTOs.Common;
using velvion_API.DTOs.Master;
using velvion_API.Entities;
using velvion_API.Services.Interfaces;

namespace velvion_API.Services.Implementations;

public class MasterService : IMasterService
{
    private readonly AppDbContext _context;

    public MasterService(AppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // UNIFIED OVERVIEW (GET)
    // ==========================================
    public async Task<ApiResponse<AllMastersDto>> GetAllMastersAsync()
    {
        var roles = await _context.Roles.AsNoTracking()
            .Select(r => new RoleDto { Id = r.Id, RoleName = r.RoleName, IsActive = r.IsActive, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt })
            .ToListAsync();

        var menus = await _context.Menus.AsNoTracking()
            .Select(m => new MenuDto { Id = m.Id, MenuName = m.MenuName, RouteUrl = m.RouteUrl, Icon = m.Icon, ParentMenuId = m.ParentMenuId, IsActive = m.IsActive, CreatedAt = m.CreatedAt, UpdatedAt = m.UpdatedAt })
            .ToListAsync();

        var services = await _context.Services.AsNoTracking()
            .Select(s => new ServiceMasterDto { Id = s.Id, ServiceName = s.ServiceName, Description = s.Description, IconUrl = s.IconUrl, IsActive = s.IsActive, CreatedAt = s.CreatedAt, UpdatedAt = s.UpdatedAt })
            .ToListAsync();

        var settings = await _context.Settings.AsNoTracking()
            .Select(st => new SettingDto { Id = st.Id, SettingKey = st.SettingKey, SettingValue = st.SettingValue, Description = st.Description, IsActive = st.IsActive, CreatedAt = st.CreatedAt, UpdatedAt = st.UpdatedAt })
            .ToListAsync();

        var result = new AllMastersDto
        {
            Roles = roles,
            Menus = menus,
            Services = services,
            Settings = settings
        };

        return ApiResponse<AllMastersDto>.SuccessResult(result, "All master records fetched successfully.");
    }

    // ==========================================
    // 1. ROLES (GET & POST)
    // ==========================================
    public async Task<ApiResponse<List<RoleDto>>> GetRolesAsync(bool activeOnly = false)
    {
        var query = _context.Roles.AsNoTracking();
        if (activeOnly)
            query = query.Where(r => r.IsActive);

        var list = await query
            .OrderBy(r => r.RoleName)
            .Select(r => new RoleDto { Id = r.Id, RoleName = r.RoleName, IsActive = r.IsActive, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt })
            .ToListAsync();

        return ApiResponse<List<RoleDto>>.SuccessResult(list, "Roles fetched successfully.");
    }

    public async Task<ApiResponse<RoleDto>> GetRoleByIdAsync(int id)
    {
        var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
            return ApiResponse<RoleDto>.FailResult("Role not found.", statusCode: 404);

        var dto = new RoleDto { Id = role.Id, RoleName = role.RoleName, IsActive = role.IsActive, CreatedAt = role.CreatedAt, UpdatedAt = role.UpdatedAt };
        return ApiResponse<RoleDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<RoleDto>> SaveRoleAsync(SaveRoleDto dto)
    {
        // 1. INSERT (Id == 0)
        if (dto.Id <= 0)
        {
            var exists = await _context.Roles.AnyAsync(r => r.RoleName.ToLower() == dto.RoleName.ToLower());
            if (exists)
                return ApiResponse<RoleDto>.FailResult("A role with this name already exists.", statusCode: 409);

            var role = new Role
            {
                RoleName = dto.RoleName.Trim(),
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var result = new RoleDto { Id = role.Id, RoleName = role.RoleName, IsActive = role.IsActive, CreatedAt = role.CreatedAt, UpdatedAt = role.UpdatedAt };
            return ApiResponse<RoleDto>.SuccessResult(result, "Role created successfully.", statusCode: 201);
        }
        // 2. UPDATE (Id > 0)
        else
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.Id);
            if (role == null)
                return ApiResponse<RoleDto>.FailResult("Role not found.", statusCode: 404);

            var nameExists = await _context.Roles.AnyAsync(r => r.Id != dto.Id && r.RoleName.ToLower() == dto.RoleName.ToLower());
            if (nameExists)
                return ApiResponse<RoleDto>.FailResult("Another role with this name already exists.", statusCode: 409);

            role.RoleName = dto.RoleName.Trim();
            role.IsActive = dto.IsActive;
            role.UpdatedBy = dto.UpdatedBy;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new RoleDto { Id = role.Id, RoleName = role.RoleName, IsActive = role.IsActive, CreatedAt = role.CreatedAt, UpdatedAt = role.UpdatedAt };
            return ApiResponse<RoleDto>.SuccessResult(result, "Role updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteRoleAsync(int id, int? updatedBy = null)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
            return ApiResponse<bool>.FailResult("Role not found.", statusCode: 404);

        role.IsDeleted = true;
        role.UpdatedBy = updatedBy;
        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Role deleted successfully.");
    }

    // ==========================================
    // 2. MENUS (GET & POST)
    // ==========================================
    private static readonly (string Name, string Route, string Icon)[] DefaultPlatformMenus =
    {
        ("Dashboard", "/admin/dashboard", "grid"),
        ("Users", "/admin/users", "users"),
        ("Roles", "/admin/roles", "shield"),
        ("Role Menu Permissions", "/admin/permissions", "check-circle"),
        ("Menus Master", "/admin/masters/menus", "menu"),
        ("Services Master", "/admin/masters/services", "layers"),
        ("Settings Master", "/admin/masters/settings", "settings"),
        ("Blogs & Articles", "/admin/blogs", "edit"),
        ("Portfolios", "/admin/portfolios", "folder"),
        ("Team Members", "/admin/team", "user"),
        ("Testimonials", "/admin/testimonials", "star"),
        ("Inquiries & Leads", "/admin/inquiries", "mail"),
        ("Audit Trail", "/admin/audit-logs", "activity")
    };

    private async Task EnsureDefaultMenusSeededAsync()
    {
        try
        {
            var existingNames = await _context.Menus.Select(m => m.MenuName.ToLower()).ToListAsync();
            bool changed = false;

            foreach (var (name, route, icon) in DefaultPlatformMenus)
            {
                if (!existingNames.Contains(name.ToLower()))
                {
                    _context.Menus.Add(new Menu
                    {
                        MenuName = name,
                        RouteUrl = route,
                        Icon = icon,
                        ParentMenuId = 0,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    changed = true;
                }
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }
        }
        catch
        {
            // Fallback gracefully if database table is read-only
        }
    }

    public async Task<ApiResponse<List<MenuDto>>> GetMenusAsync(bool activeOnly = false)
    {
        await EnsureDefaultMenusSeededAsync();

        var query = _context.Menus.AsNoTracking();
        if (activeOnly)
            query = query.Where(m => m.IsActive);

        var list = await query
            .OrderBy(m => m.Id)
            .Select(m => new MenuDto { Id = m.Id, MenuName = m.MenuName, RouteUrl = m.RouteUrl, Icon = m.Icon, ParentMenuId = m.ParentMenuId, IsActive = m.IsActive, CreatedAt = m.CreatedAt, UpdatedAt = m.UpdatedAt })
            .ToListAsync();

        return ApiResponse<List<MenuDto>>.SuccessResult(list, "Menus fetched successfully.");
    }

    public async Task<ApiResponse<List<MenuDto>>> GetMenuTreeAsync()
    {
        await EnsureDefaultMenusSeededAsync();

        var allMenus = await _context.Menus.AsNoTracking()
            .Where(m => m.IsActive)
            .Select(m => new MenuDto { Id = m.Id, MenuName = m.MenuName, RouteUrl = m.RouteUrl, Icon = m.Icon, ParentMenuId = m.ParentMenuId, IsActive = m.IsActive, CreatedAt = m.CreatedAt, UpdatedAt = m.UpdatedAt })
            .ToListAsync();

        var parents = allMenus.Where(m => m.ParentMenuId == null || m.ParentMenuId == 0).ToList();
        foreach (var parent in parents)
        {
            parent.SubMenus = allMenus.Where(m => m.ParentMenuId == parent.Id).ToList();
        }

        return ApiResponse<List<MenuDto>>.SuccessResult(parents, "Menu hierarchy fetched successfully.");
    }

    public async Task<ApiResponse<MenuDto>> GetMenuByIdAsync(int id)
    {
        var menu = await _context.Menus.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (menu == null)
            return ApiResponse<MenuDto>.FailResult("Menu not found.", statusCode: 404);

        var dto = new MenuDto { Id = menu.Id, MenuName = menu.MenuName, RouteUrl = menu.RouteUrl, Icon = menu.Icon, ParentMenuId = menu.ParentMenuId, IsActive = menu.IsActive, CreatedAt = menu.CreatedAt, UpdatedAt = menu.UpdatedAt };
        return ApiResponse<MenuDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<MenuDto>> SaveMenuAsync(SaveMenuDto dto)
    {
        if (dto.Id <= 0)
        {
            var menu = new Menu
            {
                MenuName = dto.MenuName.Trim(),
                RouteUrl = dto.RouteUrl?.Trim(),
                Icon = dto.Icon?.Trim(),
                ParentMenuId = dto.ParentMenuId ?? 0,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            var result = new MenuDto { Id = menu.Id, MenuName = menu.MenuName, RouteUrl = menu.RouteUrl, Icon = menu.Icon, ParentMenuId = menu.ParentMenuId, IsActive = menu.IsActive, CreatedAt = menu.CreatedAt, UpdatedAt = menu.UpdatedAt };
            return ApiResponse<MenuDto>.SuccessResult(result, "Menu created successfully.", statusCode: 201);
        }
        else
        {
            var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == dto.Id);
            if (menu == null)
                return ApiResponse<MenuDto>.FailResult("Menu not found.", statusCode: 404);

            menu.MenuName = dto.MenuName.Trim();
            menu.RouteUrl = dto.RouteUrl?.Trim();
            menu.Icon = dto.Icon?.Trim();
            menu.ParentMenuId = dto.ParentMenuId ?? 0;
            menu.IsActive = dto.IsActive;
            menu.UpdatedBy = dto.UpdatedBy;
            menu.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new MenuDto { Id = menu.Id, MenuName = menu.MenuName, RouteUrl = menu.RouteUrl, Icon = menu.Icon, ParentMenuId = menu.ParentMenuId, IsActive = menu.IsActive, CreatedAt = menu.CreatedAt, UpdatedAt = menu.UpdatedAt };
            return ApiResponse<MenuDto>.SuccessResult(result, "Menu updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteMenuAsync(int id, int? updatedBy = null)
    {
        var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == id);
        if (menu == null)
            return ApiResponse<bool>.FailResult("Menu not found.", statusCode: 404);

        menu.IsDeleted = true;
        menu.UpdatedBy = updatedBy;
        menu.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Menu deleted successfully.");
    }

    // ==========================================
    // 3. SERVICES (GET & POST)
    // ==========================================
    public async Task<ApiResponse<List<ServiceMasterDto>>> GetServicesAsync(bool activeOnly = false)
    {
        var query = _context.Services.AsNoTracking();
        if (activeOnly)
            query = query.Where(s => s.IsActive);

        var list = await query
            .OrderBy(s => s.ServiceName)
            .Select(s => new ServiceMasterDto { Id = s.Id, ServiceName = s.ServiceName, Description = s.Description, IconUrl = s.IconUrl, IsActive = s.IsActive, CreatedAt = s.CreatedAt, UpdatedAt = s.UpdatedAt })
            .ToListAsync();

        return ApiResponse<List<ServiceMasterDto>>.SuccessResult(list, "Services fetched successfully.");
    }

    public async Task<ApiResponse<ServiceMasterDto>> GetServiceByIdAsync(int id)
    {
        var s = await _context.Services.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (s == null)
            return ApiResponse<ServiceMasterDto>.FailResult("Service not found.", statusCode: 404);

        var dto = new ServiceMasterDto { Id = s.Id, ServiceName = s.ServiceName, Description = s.Description, IconUrl = s.IconUrl, IsActive = s.IsActive, CreatedAt = s.CreatedAt, UpdatedAt = s.UpdatedAt };
        return ApiResponse<ServiceMasterDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<ServiceMasterDto>> SaveServiceAsync(SaveServiceMasterDto dto)
    {
        if (dto.Id <= 0)
        {
            var service = new ServiceMaster
            {
                ServiceName = dto.ServiceName.Trim(),
                Description = dto.Description,
                IconUrl = dto.IconUrl,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            var result = new ServiceMasterDto { Id = service.Id, ServiceName = service.ServiceName, Description = service.Description, IconUrl = service.IconUrl, IsActive = service.IsActive, CreatedAt = service.CreatedAt, UpdatedAt = service.UpdatedAt };
            return ApiResponse<ServiceMasterDto>.SuccessResult(result, "Service created successfully.", statusCode: 201);
        }
        else
        {
            var s = await _context.Services.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (s == null)
                return ApiResponse<ServiceMasterDto>.FailResult("Service not found.", statusCode: 404);

            s.ServiceName = dto.ServiceName.Trim();
            s.Description = dto.Description;
            s.IconUrl = dto.IconUrl;
            s.IsActive = dto.IsActive;
            s.UpdatedBy = dto.UpdatedBy;
            s.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new ServiceMasterDto { Id = s.Id, ServiceName = s.ServiceName, Description = s.Description, IconUrl = s.IconUrl, IsActive = s.IsActive, CreatedAt = s.CreatedAt, UpdatedAt = s.UpdatedAt };
            return ApiResponse<ServiceMasterDto>.SuccessResult(result, "Service updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteServiceAsync(int id, int? updatedBy = null)
    {
        var s = await _context.Services.FirstOrDefaultAsync(x => x.Id == id);
        if (s == null)
            return ApiResponse<bool>.FailResult("Service not found.", statusCode: 404);

        s.IsDeleted = true;
        s.UpdatedBy = updatedBy;
        s.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Service deleted successfully.");
    }

    // ==========================================
    // 4. SETTINGS (GET & POST)
    // ==========================================
    public async Task<ApiResponse<List<SettingDto>>> GetSettingsAsync(bool activeOnly = false)
    {
        var query = _context.Settings.AsNoTracking();
        if (activeOnly)
            query = query.Where(st => st.IsActive);

        var list = await query
            .OrderBy(st => st.SettingKey)
            .Select(st => new SettingDto { Id = st.Id, SettingKey = st.SettingKey, SettingValue = st.SettingValue, Description = st.Description, IsActive = st.IsActive, CreatedAt = st.CreatedAt, UpdatedAt = st.UpdatedAt })
            .ToListAsync();

        return ApiResponse<List<SettingDto>>.SuccessResult(list, "Settings fetched successfully.");
    }

    public async Task<ApiResponse<SettingDto>> GetSettingByIdAsync(int id)
    {
        var st = await _context.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (st == null)
            return ApiResponse<SettingDto>.FailResult("Setting not found.", statusCode: 404);

        var dto = new SettingDto { Id = st.Id, SettingKey = st.SettingKey, SettingValue = st.SettingValue, Description = st.Description, IsActive = st.IsActive, CreatedAt = st.CreatedAt, UpdatedAt = st.UpdatedAt };
        return ApiResponse<SettingDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<SettingDto>> GetSettingByKeyAsync(string key)
    {
        var st = await _context.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.SettingKey.ToLower() == key.ToLower());
        if (st == null)
            return ApiResponse<SettingDto>.FailResult($"Setting with key '{key}' not found.", statusCode: 404);

        var dto = new SettingDto { Id = st.Id, SettingKey = st.SettingKey, SettingValue = st.SettingValue, Description = st.Description, IsActive = st.IsActive, CreatedAt = st.CreatedAt, UpdatedAt = st.UpdatedAt };
        return ApiResponse<SettingDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<SettingDto>> SaveSettingAsync(SaveSettingDto dto)
    {
        if (dto.Id <= 0)
        {
            var exists = await _context.Settings.AnyAsync(x => x.SettingKey.ToLower() == dto.SettingKey.ToLower());
            if (exists)
                return ApiResponse<SettingDto>.FailResult("Setting key already exists.", statusCode: 409);

            var setting = new Setting
            {
                SettingKey = dto.SettingKey.Trim(),
                SettingValue = dto.SettingValue,
                Description = dto.Description,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Settings.Add(setting);
            await _context.SaveChangesAsync();

            var result = new SettingDto { Id = setting.Id, SettingKey = setting.SettingKey, SettingValue = setting.SettingValue, Description = setting.Description, IsActive = setting.IsActive, CreatedAt = setting.CreatedAt, UpdatedAt = setting.UpdatedAt };
            return ApiResponse<SettingDto>.SuccessResult(result, "Setting created successfully.", statusCode: 201);
        }
        else
        {
            var st = await _context.Settings.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (st == null)
                return ApiResponse<SettingDto>.FailResult("Setting not found.", statusCode: 404);

            var keyExists = await _context.Settings.AnyAsync(x => x.Id != dto.Id && x.SettingKey.ToLower() == dto.SettingKey.ToLower());
            if (keyExists)
                return ApiResponse<SettingDto>.FailResult("Another setting with this key already exists.", statusCode: 409);

            st.SettingKey = dto.SettingKey.Trim();
            st.SettingValue = dto.SettingValue;
            st.Description = dto.Description;
            st.IsActive = dto.IsActive;
            st.UpdatedBy = dto.UpdatedBy;
            st.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new SettingDto { Id = st.Id, SettingKey = st.SettingKey, SettingValue = st.SettingValue, Description = st.Description, IsActive = st.IsActive, CreatedAt = st.CreatedAt, UpdatedAt = st.UpdatedAt };
            return ApiResponse<SettingDto>.SuccessResult(result, "Setting updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteSettingAsync(int id, int? updatedBy = null)
    {
        var st = await _context.Settings.FirstOrDefaultAsync(x => x.Id == id);
        if (st == null)
            return ApiResponse<bool>.FailResult("Setting not found.", statusCode: 404);

        st.IsDeleted = true;
        st.UpdatedBy = updatedBy;
        st.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Setting deleted successfully.");
    }
}
