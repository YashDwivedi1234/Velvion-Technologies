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
        await EnsureDefaultCategoriesSeededAsync();
        await EnsureDefaultDepartmentsSeededAsync();
        await EnsureDefaultDesignationsSeededAsync();
        await EnsureDefaultClientsSeededAsync();
        await EnsureDefaultFaqsSeededAsync();

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

        var categories = await _context.Categories.AsNoTracking()
            .Select(c => new CategoryDto { Id = c.Id, CategoryName = c.CategoryName, Slug = c.Slug, Description = c.Description, Icon = c.Icon, DisplayOrder = c.DisplayOrder, IsActive = c.IsActive, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt })
            .ToListAsync();

        var departments = await _context.Departments.AsNoTracking()
            .Select(d => new DepartmentDto { Id = d.Id, DepartmentName = d.DepartmentName, Description = d.Description, Designations = d.Designations, HeadOfDepartment = d.HeadOfDepartment, DisplayOrder = d.DisplayOrder, IsActive = d.IsActive, CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt })
            .ToListAsync();

        var designations = await _context.Designations.Include(ds => ds.Department).AsNoTracking()
            .Select(ds => new DesignationDto { Id = ds.Id, DesignationName = ds.DesignationName, DepartmentId = ds.DepartmentId, DepartmentName = ds.Department != null ? ds.Department.DepartmentName : null, Description = ds.Description, DisplayOrder = ds.DisplayOrder, IsActive = ds.IsActive, CreatedAt = ds.CreatedAt, UpdatedAt = ds.UpdatedAt })
            .ToListAsync();

        var clients = await _context.Clients.AsNoTracking()
            .Select(cl => new ClientDto { Id = cl.Id, ClientName = cl.ClientName, LogoUrl = cl.LogoUrl, WebsiteUrl = cl.WebsiteUrl, Industry = cl.Industry, PartnerTier = cl.PartnerTier, DisplayOrder = cl.DisplayOrder, IsActive = cl.IsActive, CreatedAt = cl.CreatedAt, UpdatedAt = cl.UpdatedAt })
            .ToListAsync();

        var faqs = await _context.Faqs.AsNoTracking()
            .Select(f => new FaqDto { Id = f.Id, Question = f.Question, Answer = f.Answer, Category = f.Category, DisplayOrder = f.DisplayOrder, IsActive = f.IsActive, CreatedAt = f.CreatedAt, UpdatedAt = f.UpdatedAt })
            .ToListAsync();

        var result = new AllMastersDto
        {
            Roles = roles,
            Menus = menus,
            Services = services,
            Settings = settings,
            Categories = categories,
            Departments = departments,
            Designations = designations,
            Clients = clients,
            Faqs = faqs
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
        ("Menus", "/admin/masters/menus", "menu"),
        ("Services", "/admin/masters/services", "layers"),
        ("Categories & Industries", "/admin/masters/categories", "folder"),
        ("Departments & Roles", "/admin/masters/departments", "briefcase"),
        ("Clients & Partners", "/admin/masters/clients", "globe"),
        ("FAQs & Knowledge", "/admin/masters/faqs", "help-circle"),
        ("Settings", "/admin/masters/settings", "settings"),
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
            // Fallback gracefully
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

    // ==========================================
    // 5. CATEGORIES & INDUSTRIES (GET & POST)
    // ==========================================
    private async Task EnsureDefaultCategoriesSeededAsync()
    {
        try
        {
            if (!await _context.Categories.AnyAsync())
            {
                var defaults = new List<CategoryMaster>
                {
                    new() { CategoryName = "FinTech & Banking", Slug = "fintech-banking", Description = "Financial software, decentralized ledgers & payment gateways", Icon = "credit-card", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { CategoryName = "Healthcare & MedTech", Slug = "healthcare-medtech", Description = "HIPAA compliant medical records, diagnostics & telemedicine", Icon = "activity", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { CategoryName = "E-Commerce & Retail", Slug = "ecommerce-retail", Description = "High throughput marketplace & omnichannel platforms", Icon = "shopping-cart", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { CategoryName = "Cybersecurity & Cloud", Slug = "cybersecurity-cloud", Description = "Zero-trust architecture, cloud infra & compliance systems", Icon = "shield", DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { CategoryName = "AI & Machine Learning", Slug = "ai-machine-learning", Description = "LLM integrations, computer vision & predictive analytics", Icon = "cpu", DisplayOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { CategoryName = "EdTech & Learning", Slug = "edtech-learning", Description = "Interactive learning portals, LMS and live cohort classrooms", Icon = "book-open", DisplayOrder = 6, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };

                _context.Categories.AddRange(defaults);
                await _context.SaveChangesAsync();
            }
        }
        catch
        {
            // Fallback gracefully
        }
    }

    public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync(bool activeOnly = false)
    {
        await EnsureDefaultCategoriesSeededAsync();

        var query = _context.Categories.AsNoTracking();
        if (activeOnly)
            query = query.Where(c => c.IsActive);

        var list = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.CategoryName)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                CategoryName = c.CategoryName,
                Slug = c.Slug,
                Description = c.Description,
                Icon = c.Icon,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<CategoryDto>>.SuccessResult(list, "Categories fetched successfully.");
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        var c = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
            return ApiResponse<CategoryDto>.FailResult("Category not found.", statusCode: 404);

        var dto = new CategoryDto
        {
            Id = c.Id,
            CategoryName = c.CategoryName,
            Slug = c.Slug,
            Description = c.Description,
            Icon = c.Icon,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
        return ApiResponse<CategoryDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<CategoryDto>> SaveCategoryAsync(SaveCategoryDto dto)
    {
        if (dto.Id <= 0)
        {
            var exists = await _context.Categories.AnyAsync(x => x.CategoryName.ToLower() == dto.CategoryName.ToLower());
            if (exists)
                return ApiResponse<CategoryDto>.FailResult("A category with this name already exists.", statusCode: 409);

            var category = new CategoryMaster
            {
                CategoryName = dto.CategoryName.Trim(),
                Slug = string.IsNullOrWhiteSpace(dto.Slug) ? dto.CategoryName.Trim().ToLower().Replace(" ", "-") : dto.Slug.Trim(),
                Description = dto.Description,
                Icon = string.IsNullOrWhiteSpace(dto.Icon) ? "folder" : dto.Icon.Trim(),
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Slug = category.Slug,
                Description = category.Description,
                Icon = category.Icon,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
            return ApiResponse<CategoryDto>.SuccessResult(result, "Category created successfully.", statusCode: 201);
        }
        else
        {
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (category == null)
                return ApiResponse<CategoryDto>.FailResult("Category not found.", statusCode: 404);

            var nameExists = await _context.Categories.AnyAsync(x => x.Id != dto.Id && x.CategoryName.ToLower() == dto.CategoryName.ToLower());
            if (nameExists)
                return ApiResponse<CategoryDto>.FailResult("Another category with this name already exists.", statusCode: 409);

            category.CategoryName = dto.CategoryName.Trim();
            category.Slug = string.IsNullOrWhiteSpace(dto.Slug) ? dto.CategoryName.Trim().ToLower().Replace(" ", "-") : dto.Slug.Trim();
            category.Description = dto.Description;
            category.Icon = string.IsNullOrWhiteSpace(dto.Icon) ? "folder" : dto.Icon.Trim();
            category.DisplayOrder = dto.DisplayOrder;
            category.IsActive = dto.IsActive;
            category.UpdatedBy = dto.UpdatedBy;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Slug = category.Slug,
                Description = category.Description,
                Icon = category.Icon,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
            return ApiResponse<CategoryDto>.SuccessResult(result, "Category updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id, int? updatedBy = null)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (category == null)
            return ApiResponse<bool>.FailResult("Category not found.", statusCode: 404);

        category.IsDeleted = true;
        category.UpdatedBy = updatedBy;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Category deleted successfully.");
    }

    // ==========================================
    // 6. DEPARTMENTS (GET & POST)
    // ==========================================
    private async Task EnsureDefaultDepartmentsSeededAsync()
    {
        try
        {
            if (!await _context.Departments.AnyAsync())
            {
                var defaults = new List<DepartmentMaster>
                {
                    new() { DepartmentName = "Engineering & Architecture", Description = "Software engineering, DevOps, cloud infrastructure & system architecture", Designations = "Chief Architect, Lead Software Engineer, Senior Full-Stack Dev, DevOps Engineer, QA Automation Lead", HeadOfDepartment = "Alex Vance", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DepartmentName = "UI/UX & Product Design", Description = "User research, UI design, prototyping, UX architecture & brand systems", Designations = "Head of Design, Senior Product Designer, Motion Graphics Specialist, Design System Architect", HeadOfDepartment = "Elena Rostova", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DepartmentName = "AI & Data Science", Description = "Machine learning models, GenAI solutions, data engineering & NLP analytics", Designations = "Principal AI Scientist, ML Ops Engineer, NLP Researcher, Data Pipeline Engineer", HeadOfDepartment = "Dr. Marcus Reed", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DepartmentName = "Project Management & Delivery", Description = "Agile sprint management, technical delivery, client communication & QA", Designations = "VP of Delivery, Senior Scrum Master, Agile Coach, Technical Project Manager", HeadOfDepartment = "Sarah Jenkins", DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DepartmentName = "Sales & Strategic Growth", Description = "Enterprise sales, business development, client partnerships & account expansion", Designations = "Chief Revenue Officer, Enterprise Account Exec, Solutions Consultant", HeadOfDepartment = "David Sterling", DisplayOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DepartmentName = "Human Resources & Talent", Description = "Talent acquisition, employee engagement, HR policy & organizational development", Designations = "Director of People Ops, Technical Talent Scout, HR Business Partner", HeadOfDepartment = "Priya Sharma", DisplayOrder = 6, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };

                _context.Departments.AddRange(defaults);
                await _context.SaveChangesAsync();
            }
        }
        catch
        {
            // Fallback gracefully
        }
    }

    public async Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync(bool activeOnly = false)
    {
        await EnsureDefaultDepartmentsSeededAsync();

        var query = _context.Departments.AsNoTracking();
        if (activeOnly)
            query = query.Where(d => d.IsActive);

        var list = await query
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.DepartmentName)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                Designations = d.Designations,
                HeadOfDepartment = d.HeadOfDepartment,
                DisplayOrder = d.DisplayOrder,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<DepartmentDto>>.SuccessResult(list, "Departments fetched successfully.");
    }

    public async Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id)
    {
        var d = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (d == null)
            return ApiResponse<DepartmentDto>.FailResult("Department not found.", statusCode: 404);

        var dto = new DepartmentDto
        {
            Id = d.Id,
            DepartmentName = d.DepartmentName,
            Description = d.Description,
            Designations = d.Designations,
            HeadOfDepartment = d.HeadOfDepartment,
            DisplayOrder = d.DisplayOrder,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
        return ApiResponse<DepartmentDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<DepartmentDto>> SaveDepartmentAsync(SaveDepartmentDto dto)
    {
        if (dto.Id <= 0)
        {
            var exists = await _context.Departments.AnyAsync(x => x.DepartmentName.ToLower() == dto.DepartmentName.ToLower());
            if (exists)
                return ApiResponse<DepartmentDto>.FailResult("A department with this name already exists.", statusCode: 409);

            var dept = new DepartmentMaster
            {
                DepartmentName = dto.DepartmentName.Trim(),
                Description = dto.Description?.Trim(),
                Designations = dto.Designations?.Trim(),
                HeadOfDepartment = dto.HeadOfDepartment?.Trim(),
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            var result = new DepartmentDto
            {
                Id = dept.Id,
                DepartmentName = dept.DepartmentName,
                Description = dept.Description,
                Designations = dept.Designations,
                HeadOfDepartment = dept.HeadOfDepartment,
                DisplayOrder = dept.DisplayOrder,
                IsActive = dept.IsActive,
                CreatedAt = dept.CreatedAt,
                UpdatedAt = dept.UpdatedAt
            };
            return ApiResponse<DepartmentDto>.SuccessResult(result, "Department created successfully.", statusCode: 201);
        }
        else
        {
            var dept = await _context.Departments.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (dept == null)
                return ApiResponse<DepartmentDto>.FailResult("Department not found.", statusCode: 404);

            var nameExists = await _context.Departments.AnyAsync(x => x.Id != dto.Id && x.DepartmentName.ToLower() == dto.DepartmentName.ToLower());
            if (nameExists)
                return ApiResponse<DepartmentDto>.FailResult("Another department with this name already exists.", statusCode: 409);

            dept.DepartmentName = dto.DepartmentName.Trim();
            dept.Description = dto.Description?.Trim();
            dept.Designations = dto.Designations?.Trim();
            dept.HeadOfDepartment = dto.HeadOfDepartment?.Trim();
            dept.DisplayOrder = dto.DisplayOrder;
            dept.IsActive = dto.IsActive;
            dept.UpdatedBy = dto.UpdatedBy;
            dept.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new DepartmentDto
            {
                Id = dept.Id,
                DepartmentName = dept.DepartmentName,
                Description = dept.Description,
                Designations = dept.Designations,
                HeadOfDepartment = dept.HeadOfDepartment,
                DisplayOrder = dept.DisplayOrder,
                IsActive = dept.IsActive,
                CreatedAt = dept.CreatedAt,
                UpdatedAt = dept.UpdatedAt
            };
            return ApiResponse<DepartmentDto>.SuccessResult(result, "Department updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteDepartmentAsync(int id, int? updatedBy = null)
    {
        var dept = await _context.Departments.FirstOrDefaultAsync(x => x.Id == id);
        if (dept == null)
            return ApiResponse<bool>.FailResult("Department not found.", statusCode: 404);

        dept.IsDeleted = true;
        dept.UpdatedBy = updatedBy;
        dept.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Department deleted successfully.");
    }

    // ==========================================
    // 6B. DESIGNATIONS (GET & POST)
    // ==========================================
    private async Task EnsureDefaultDesignationsSeededAsync()
    {
        try
        {
            await EnsureDefaultDepartmentsSeededAsync();

            if (!await _context.Designations.AnyAsync())
            {
                var depts = await _context.Departments.ToListAsync();
                var engDept = depts.FirstOrDefault(d => d.DepartmentName.Contains("Engineering"));
                var designDept = depts.FirstOrDefault(d => d.DepartmentName.Contains("Design") || d.DepartmentName.Contains("UI/UX"));
                var aiDept = depts.FirstOrDefault(d => d.DepartmentName.Contains("AI") || d.DepartmentName.Contains("Data"));
                var pmDept = depts.FirstOrDefault(d => d.DepartmentName.Contains("Project") || d.DepartmentName.Contains("Delivery"));
                var salesDept = depts.FirstOrDefault(d => d.DepartmentName.Contains("Sales") || d.DepartmentName.Contains("Growth"));
                var hrDept = depts.FirstOrDefault(d => d.DepartmentName.Contains("Human") || d.DepartmentName.Contains("Talent"));

                var defaults = new List<DesignationMaster>
                {
                    // Engineering
                    new() { DesignationName = "Chief Technical Architect", DepartmentId = engDept?.Id, Description = "High-level software architecture and tech vision", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "Lead Software Engineer", DepartmentId = engDept?.Id, Description = "Technical team leadership and code quality", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "Senior Full-Stack Developer", DepartmentId = engDept?.Id, Description = "End-to-end full stack web application development", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "Frontend Specialist (Angular)", DepartmentId = engDept?.Id, Description = "Modern Angular UI architecture and reactive patterns", DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "Backend .NET Core Engineer", DepartmentId = engDept?.Id, Description = "High-performance microservices and REST APIs", DisplayOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "DevOps & Cloud Architect", DepartmentId = engDept?.Id, Description = "CI/CD pipelines, Docker, Kubernetes and Cloud ops", DisplayOrder = 6, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "QA Automation Lead", DepartmentId = engDept?.Id, Description = "Automated testing, quality assurance and security audits", DisplayOrder = 7, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // UI/UX & Design
                    new() { DesignationName = "Head of Product Design", DepartmentId = designDept?.Id, Description = "Design vision, user experience strategy & design ops", DisplayOrder = 8, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "Senior UI/UX Designer", DepartmentId = designDept?.Id, Description = "User journey mapping, wireframing & high-fidelity UI", DisplayOrder = 9, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "Design System Architect", DepartmentId = designDept?.Id, Description = "Reusable design tokens, component libraries & style guides", DisplayOrder = 10, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // AI & Data Science
                    new() { DesignationName = "Principal AI Scientist", DepartmentId = aiDept?.Id, Description = "Advanced machine learning, LLM fine-tuning & research", DisplayOrder = 11, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "MLOps & Data Engineer", DepartmentId = aiDept?.Id, Description = "Data pipelines, model deployment and vector databases", DisplayOrder = 12, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // Project Management
                    new() { DesignationName = "VP of Delivery & Operations", DepartmentId = pmDept?.Id, Description = "Client delivery management and organizational execution", DisplayOrder = 13, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "Senior Scrum Master", DepartmentId = pmDept?.Id, Description = "Agile sprint facilitation and delivery management", DisplayOrder = 14, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // Sales
                    new() { DesignationName = "Chief Revenue Officer", DepartmentId = salesDept?.Id, Description = "Strategic revenue growth and corporate expansion", DisplayOrder = 15, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "Enterprise Solutions Consultant", DepartmentId = salesDept?.Id, Description = "Client discovery, solution design & proposal closure", DisplayOrder = 16, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // HR
                    new() { DesignationName = "Director of People Operations", DepartmentId = hrDept?.Id, Description = "HR strategy, organizational culture and talent growth", DisplayOrder = 17, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { DesignationName = "Technical Talent Scout", DepartmentId = hrDept?.Id, Description = "Top engineering talent recruitment and onboarding", DisplayOrder = 18, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };

                _context.Designations.AddRange(defaults);
                await _context.SaveChangesAsync();
            }
        }
        catch
        {
            // Fallback gracefully
        }
    }

    public async Task<ApiResponse<List<DesignationDto>>> GetDesignationsAsync(bool activeOnly = false, int? departmentId = null)
    {
        await EnsureDefaultDesignationsSeededAsync();

        var query = _context.Designations.Include(ds => ds.Department).AsNoTracking();
        if (activeOnly)
            query = query.Where(ds => ds.IsActive);
        if (departmentId.HasValue && departmentId.Value > 0)
            query = query.Where(ds => ds.DepartmentId == departmentId.Value);

        var list = await query
            .OrderBy(ds => ds.DisplayOrder)
            .ThenBy(ds => ds.DesignationName)
            .Select(ds => new DesignationDto
            {
                Id = ds.Id,
                DesignationName = ds.DesignationName,
                DepartmentId = ds.DepartmentId,
                DepartmentName = ds.Department != null ? ds.Department.DepartmentName : null,
                Description = ds.Description,
                DisplayOrder = ds.DisplayOrder,
                IsActive = ds.IsActive,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<DesignationDto>>.SuccessResult(list, "Designations fetched successfully.");
    }

    public async Task<ApiResponse<DesignationDto>> GetDesignationByIdAsync(int id)
    {
        var ds = await _context.Designations.Include(d => d.Department).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (ds == null)
            return ApiResponse<DesignationDto>.FailResult("Designation not found.", statusCode: 404);

        var dto = new DesignationDto
        {
            Id = ds.Id,
            DesignationName = ds.DesignationName,
            DepartmentId = ds.DepartmentId,
            DepartmentName = ds.Department != null ? ds.Department.DepartmentName : null,
            Description = ds.Description,
            DisplayOrder = ds.DisplayOrder,
            IsActive = ds.IsActive,
            CreatedAt = ds.CreatedAt,
            UpdatedAt = ds.UpdatedAt
        };
        return ApiResponse<DesignationDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<DesignationDto>> SaveDesignationAsync(SaveDesignationDto dto)
    {
        if (dto.Id <= 0)
        {
            var exists = await _context.Designations.AnyAsync(x => x.DesignationName.ToLower() == dto.DesignationName.ToLower() && x.DepartmentId == dto.DepartmentId);
            if (exists)
                return ApiResponse<DesignationDto>.FailResult("A designation with this name already exists in this department.", statusCode: 409);

            var desig = new DesignationMaster
            {
                DesignationName = dto.DesignationName.Trim(),
                DepartmentId = dto.DepartmentId > 0 ? dto.DepartmentId : null,
                Description = dto.Description?.Trim(),
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Designations.Add(desig);
            await _context.SaveChangesAsync();

            string? deptName = null;
            if (desig.DepartmentId.HasValue)
            {
                deptName = await _context.Departments.Where(d => d.Id == desig.DepartmentId.Value).Select(d => d.DepartmentName).FirstOrDefaultAsync();
            }

            var result = new DesignationDto
            {
                Id = desig.Id,
                DesignationName = desig.DesignationName,
                DepartmentId = desig.DepartmentId,
                DepartmentName = deptName,
                Description = desig.Description,
                DisplayOrder = desig.DisplayOrder,
                IsActive = desig.IsActive,
                CreatedAt = desig.CreatedAt,
                UpdatedAt = desig.UpdatedAt
            };
            return ApiResponse<DesignationDto>.SuccessResult(result, "Designation created successfully.", statusCode: 201);
        }
        else
        {
            var desig = await _context.Designations.Include(d => d.Department).FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (desig == null)
                return ApiResponse<DesignationDto>.FailResult("Designation not found.", statusCode: 404);

            var nameExists = await _context.Designations.AnyAsync(x => x.Id != dto.Id && x.DesignationName.ToLower() == dto.DesignationName.ToLower() && x.DepartmentId == dto.DepartmentId);
            if (nameExists)
                return ApiResponse<DesignationDto>.FailResult("Another designation with this name already exists in this department.", statusCode: 409);

            desig.DesignationName = dto.DesignationName.Trim();
            desig.DepartmentId = dto.DepartmentId > 0 ? dto.DepartmentId : null;
            desig.Description = dto.Description?.Trim();
            desig.DisplayOrder = dto.DisplayOrder;
            desig.IsActive = dto.IsActive;
            desig.UpdatedBy = dto.UpdatedBy;
            desig.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            string? deptName = null;
            if (desig.DepartmentId.HasValue)
            {
                deptName = await _context.Departments.Where(d => d.Id == desig.DepartmentId.Value).Select(d => d.DepartmentName).FirstOrDefaultAsync();
            }

            var result = new DesignationDto
            {
                Id = desig.Id,
                DesignationName = desig.DesignationName,
                DepartmentId = desig.DepartmentId,
                DepartmentName = deptName,
                Description = desig.Description,
                DisplayOrder = desig.DisplayOrder,
                IsActive = desig.IsActive,
                CreatedAt = desig.CreatedAt,
                UpdatedAt = desig.UpdatedAt
            };
            return ApiResponse<DesignationDto>.SuccessResult(result, "Designation updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteDesignationAsync(int id, int? updatedBy = null)
    {
        var desig = await _context.Designations.FirstOrDefaultAsync(x => x.Id == id);
        if (desig == null)
            return ApiResponse<bool>.FailResult("Designation not found.", statusCode: 404);

        desig.IsDeleted = true;
        desig.UpdatedBy = updatedBy;
        desig.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Designation deleted successfully.");
    }

    // ==========================================
    // 7. CLIENTS & PARTNERS (GET & POST)
    // ==========================================
    private async Task EnsureDefaultClientsSeededAsync()
    {
        try
        {
            if (!await _context.Clients.AnyAsync())
            {
                var defaults = new List<ClientMaster>
                {
                    new() { ClientName = "Apex Nexus Global", Industry = "FinTech", PartnerTier = "Strategic Tier 1", WebsiteUrl = "https://apexnexus.example.com", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { ClientName = "Cyberdyne Systems AI", Industry = "Artificial Intelligence", PartnerTier = "Global Tech Partner", WebsiteUrl = "https://cyberdyne.example.com", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { ClientName = "BioVanguard Therapeutics", Industry = "HealthTech", PartnerTier = "Enterprise Client", WebsiteUrl = "https://biovanguard.example.com", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { ClientName = "Krypton Cloud Solutions", Industry = "Cloud Infrastructure", PartnerTier = "Alliance Partner", WebsiteUrl = "https://kryptoncloud.example.com", DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { ClientName = "OmniStore Retail Network", Industry = "E-Commerce", PartnerTier = "Enterprise Client", WebsiteUrl = "https://omnistore.example.com", DisplayOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };

                _context.Clients.AddRange(defaults);
                await _context.SaveChangesAsync();
            }
        }
        catch
        {
            // Fallback gracefully
        }
    }

    public async Task<ApiResponse<List<ClientDto>>> GetClientsAsync(bool activeOnly = false)
    {
        await EnsureDefaultClientsSeededAsync();

        var query = _context.Clients.AsNoTracking();
        if (activeOnly)
            query = query.Where(c => c.IsActive);

        var list = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.ClientName)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                ClientName = c.ClientName,
                LogoUrl = c.LogoUrl,
                WebsiteUrl = c.WebsiteUrl,
                Industry = c.Industry,
                PartnerTier = c.PartnerTier,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<ClientDto>>.SuccessResult(list, "Clients fetched successfully.");
    }

    public async Task<ApiResponse<ClientDto>> GetClientByIdAsync(int id)
    {
        var c = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
            return ApiResponse<ClientDto>.FailResult("Client not found.", statusCode: 404);

        var dto = new ClientDto
        {
            Id = c.Id,
            ClientName = c.ClientName,
            LogoUrl = c.LogoUrl,
            WebsiteUrl = c.WebsiteUrl,
            Industry = c.Industry,
            PartnerTier = c.PartnerTier,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
        return ApiResponse<ClientDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<ClientDto>> SaveClientAsync(SaveClientDto dto)
    {
        if (dto.Id <= 0)
        {
            var exists = await _context.Clients.AnyAsync(x => x.ClientName.ToLower() == dto.ClientName.ToLower());
            if (exists)
                return ApiResponse<ClientDto>.FailResult("A client with this name already exists.", statusCode: 409);

            var client = new ClientMaster
            {
                ClientName = dto.ClientName.Trim(),
                LogoUrl = dto.LogoUrl,
                WebsiteUrl = dto.WebsiteUrl,
                Industry = dto.Industry,
                PartnerTier = string.IsNullOrWhiteSpace(dto.PartnerTier) ? "Enterprise Client" : dto.PartnerTier.Trim(),
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            var result = new ClientDto
            {
                Id = client.Id,
                ClientName = client.ClientName,
                LogoUrl = client.LogoUrl,
                WebsiteUrl = client.WebsiteUrl,
                Industry = client.Industry,
                PartnerTier = client.PartnerTier,
                DisplayOrder = client.DisplayOrder,
                IsActive = client.IsActive,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt
            };
            return ApiResponse<ClientDto>.SuccessResult(result, "Client created successfully.", statusCode: 201);
        }
        else
        {
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (client == null)
                return ApiResponse<ClientDto>.FailResult("Client not found.", statusCode: 404);

            var nameExists = await _context.Clients.AnyAsync(x => x.Id != dto.Id && x.ClientName.ToLower() == dto.ClientName.ToLower());
            if (nameExists)
                return ApiResponse<ClientDto>.FailResult("Another client with this name already exists.", statusCode: 409);

            client.ClientName = dto.ClientName.Trim();
            client.LogoUrl = dto.LogoUrl;
            client.WebsiteUrl = dto.WebsiteUrl;
            client.Industry = dto.Industry;
            client.PartnerTier = string.IsNullOrWhiteSpace(dto.PartnerTier) ? "Enterprise Client" : dto.PartnerTier.Trim();
            client.DisplayOrder = dto.DisplayOrder;
            client.IsActive = dto.IsActive;
            client.UpdatedBy = dto.UpdatedBy;
            client.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new ClientDto
            {
                Id = client.Id,
                ClientName = client.ClientName,
                LogoUrl = client.LogoUrl,
                WebsiteUrl = client.WebsiteUrl,
                Industry = client.Industry,
                PartnerTier = client.PartnerTier,
                DisplayOrder = client.DisplayOrder,
                IsActive = client.IsActive,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt
            };
            return ApiResponse<ClientDto>.SuccessResult(result, "Client updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteClientAsync(int id, int? updatedBy = null)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == id);
        if (client == null)
            return ApiResponse<bool>.FailResult("Client not found.", statusCode: 404);

        client.IsDeleted = true;
        client.UpdatedBy = updatedBy;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Client deleted successfully.");
    }

    // ==========================================
    // 8. FAQS & KNOWLEDGE BASE (GET & POST)
    // ==========================================
    private async Task EnsureDefaultFaqsSeededAsync()
    {
        try
        {
            if (!await _context.Faqs.AnyAsync())
            {
                var defaults = new List<FaqMaster>
                {
                    new() { Question = "What modern technologies does Velvion use for enterprise web apps?", Answer = "We specialize in ASP.NET Core Web API, Angular 19+, Next.js, MySQL/PostgreSQL, Docker containers, and cloud native architectures with strict security and high concurrency.", Category = "Technology", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Question = "How does Velvion ensure high-speed API performance?", Answer = "Through optimized Entity Framework Core query pipelines, compiled queries, Redis distributed caching, connection pooling, and asynchronous non-blocking I/O execution.", Category = "Architecture", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Question = "What role & permission model is implemented in the Admin Portal?", Answer = "Velvion provides granular Role-Based Access Control (RBAC) with full CRUD permission matrices mapped directly to hierarchical menus and API action filters.", Category = "Security", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new() { Question = "Can custom masters and workflows be configured dynamically?", Answer = "Yes, Velvion's Master Management suite allows dynamic maintenance of services, industry verticals, departments, client tiers, system settings, and knowledge bases without downtime.", Category = "General", DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };

                _context.Faqs.AddRange(defaults);
                await _context.SaveChangesAsync();
            }
        }
        catch
        {
            // Fallback gracefully
        }
    }

    public async Task<ApiResponse<List<FaqDto>>> GetFaqsAsync(bool activeOnly = false, string? category = null)
    {
        await EnsureDefaultFaqsSeededAsync();

        var query = _context.Faqs.AsNoTracking();
        if (activeOnly)
            query = query.Where(f => f.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(f => f.Category != null && f.Category.ToLower() == category.ToLower());

        var list = await query
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.Id)
            .Select(f => new FaqDto
            {
                Id = f.Id,
                Question = f.Question,
                Answer = f.Answer,
                Category = f.Category,
                DisplayOrder = f.DisplayOrder,
                IsActive = f.IsActive,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<FaqDto>>.SuccessResult(list, "FAQs fetched successfully.");
    }

    public async Task<ApiResponse<FaqDto>> GetFaqByIdAsync(int id)
    {
        var f = await _context.Faqs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (f == null)
            return ApiResponse<FaqDto>.FailResult("FAQ not found.", statusCode: 404);

        var dto = new FaqDto
        {
            Id = f.Id,
            Question = f.Question,
            Answer = f.Answer,
            Category = f.Category,
            DisplayOrder = f.DisplayOrder,
            IsActive = f.IsActive,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        };
        return ApiResponse<FaqDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<FaqDto>> SaveFaqAsync(SaveFaqDto dto)
    {
        if (dto.Id <= 0)
        {
            var faq = new FaqMaster
            {
                Question = dto.Question.Trim(),
                Answer = dto.Answer.Trim(),
                Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category.Trim(),
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Faqs.Add(faq);
            await _context.SaveChangesAsync();

            var result = new FaqDto
            {
                Id = faq.Id,
                Question = faq.Question,
                Answer = faq.Answer,
                Category = faq.Category,
                DisplayOrder = faq.DisplayOrder,
                IsActive = faq.IsActive,
                CreatedAt = faq.CreatedAt,
                UpdatedAt = faq.UpdatedAt
            };
            return ApiResponse<FaqDto>.SuccessResult(result, "FAQ created successfully.", statusCode: 201);
        }
        else
        {
            var faq = await _context.Faqs.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (faq == null)
                return ApiResponse<FaqDto>.FailResult("FAQ not found.", statusCode: 404);

            faq.Question = dto.Question.Trim();
            faq.Answer = dto.Answer.Trim();
            faq.Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category.Trim();
            faq.DisplayOrder = dto.DisplayOrder;
            faq.IsActive = dto.IsActive;
            faq.UpdatedBy = dto.UpdatedBy;
            faq.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new FaqDto
            {
                Id = faq.Id,
                Question = faq.Question,
                Answer = faq.Answer,
                Category = faq.Category,
                DisplayOrder = faq.DisplayOrder,
                IsActive = faq.IsActive,
                CreatedAt = faq.CreatedAt,
                UpdatedAt = faq.UpdatedAt
            };
            return ApiResponse<FaqDto>.SuccessResult(result, "FAQ updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteFaqAsync(int id, int? updatedBy = null)
    {
        var faq = await _context.Faqs.FirstOrDefaultAsync(x => x.Id == id);
        if (faq == null)
            return ApiResponse<bool>.FailResult("FAQ not found.", statusCode: 404);

        faq.IsDeleted = true;
        faq.UpdatedBy = updatedBy;
        faq.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "FAQ deleted successfully.");
    }
}
