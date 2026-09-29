using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using velvion_API.Data;
using velvion_API.DTOs.App;
using velvion_API.DTOs.Common;
using velvion_API.Entities;
using velvion_API.Services.Interfaces;

namespace velvion_API.Services.Implementations;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "_VelvionSalt_2026"));
        return Convert.ToBase64String(hashedBytes);
    }
}

// ==========================================
// 1. USER SERVICE
// ==========================================
public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<UserDto>>> GetUsersAsync(bool activeOnly = false)
    {
        var query = _context.Users.Include(u => u.Role).AsNoTracking();
        if (activeOnly) query = query.Where(u => u.IsActive);

        var list = await query.OrderBy(u => u.FullName).Select(u => new UserDto
        {
            Id = u.Id, RoleId = u.RoleId,
            RoleName = u.Role != null ? u.Role.RoleName : string.Empty,
            FullName = u.FullName, Email = u.Email, Mobile = u.Mobile,
            IsActive = u.IsActive, CreatedAt = u.CreatedAt, UpdatedAt = u.UpdatedAt
        }).ToListAsync();

        return ApiResponse<List<UserDto>>.SuccessResult(list, "Users fetched successfully.");
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return ApiResponse<UserDto>.FailResult("User not found.", statusCode: 404);

        return ApiResponse<UserDto>.SuccessResult(new UserDto
        {
            Id = user.Id, RoleId = user.RoleId,
            RoleName = user.Role != null ? user.Role.RoleName : string.Empty,
            FullName = user.FullName, Email = user.Email, Mobile = user.Mobile,
            IsActive = user.IsActive, CreatedAt = user.CreatedAt, UpdatedAt = user.UpdatedAt
        });
    }

    public async Task<ApiResponse<UserDto>> SaveUserAsync(SaveUserDto dto)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId);
        if (role == null) return ApiResponse<UserDto>.FailResult("Specified Role does not exist.", statusCode: 400);

        if (dto.Id <= 0)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                return ApiResponse<UserDto>.FailResult("Password is required for new users.", statusCode: 400);

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
                return ApiResponse<UserDto>.FailResult("User with this email already exists.", statusCode: 409);

            var user = new User
            {
                RoleId = dto.RoleId, FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLower(), Mobile = dto.Mobile?.Trim(),
                PasswordHash = PasswordHelper.HashPassword(dto.Password), IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return ApiResponse<UserDto>.SuccessResult(new UserDto
            {
                Id = user.Id, RoleId = user.RoleId, RoleName = role.RoleName,
                FullName = user.FullName, Email = user.Email, Mobile = user.Mobile,
                IsActive = user.IsActive, CreatedAt = user.CreatedAt, UpdatedAt = user.UpdatedAt
            }, "User created successfully.", statusCode: 201);
        }
        else
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (user == null) return ApiResponse<UserDto>.FailResult("User not found.", statusCode: 404);

            if (await _context.Users.AnyAsync(u => u.Id != dto.Id && u.Email.ToLower() == dto.Email.ToLower()))
                return ApiResponse<UserDto>.FailResult("Another user with this email already exists.", statusCode: 409);

            user.RoleId = dto.RoleId; user.FullName = dto.FullName.Trim();
            user.Email = dto.Email.Trim().ToLower(); user.Mobile = dto.Mobile?.Trim();
            user.IsActive = dto.IsActive; user.UpdatedBy = dto.UpdatedBy; user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Password)) user.PasswordHash = PasswordHelper.HashPassword(dto.Password);

            await _context.SaveChangesAsync();

            return ApiResponse<UserDto>.SuccessResult(new UserDto
            {
                Id = user.Id, RoleId = user.RoleId, RoleName = role.RoleName,
                FullName = user.FullName, Email = user.Email, Mobile = user.Mobile,
                IsActive = user.IsActive, CreatedAt = user.CreatedAt, UpdatedAt = user.UpdatedAt
            }, "User updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(int id, int? updatedBy = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return ApiResponse<bool>.FailResult("User not found.", statusCode: 404);

        user.IsDeleted = true; user.UpdatedBy = updatedBy; user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "User deleted successfully.");
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.Trim().ToLower() && u.IsActive);

        if (user == null || user.PasswordHash != PasswordHelper.HashPassword(dto.Password))
            return ApiResponse<LoginResponseDto>.FailResult("Invalid email or password.", statusCode: 401);

        return ApiResponse<LoginResponseDto>.SuccessResult(new LoginResponseDto
        {
            UserId = user.Id, FullName = user.FullName, Email = user.Email,
            RoleId = user.RoleId, RoleName = user.Role != null ? user.Role.RoleName : string.Empty
        }, "Login successful.");
    }
}

// ==========================================
// 2. PERMISSION SERVICE
// ==========================================
public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    public PermissionService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<RoleMenuPermissionDto>>> GetPermissionsByRoleIdAsync(int roleId)
    {
        var list = await _context.RoleMenuPermissions.Include(p => p.Role).Include(p => p.Menu)
            .AsNoTracking().Where(p => p.RoleId == roleId)
            .Select(p => new RoleMenuPermissionDto
            {
                Id = p.Id, RoleId = p.RoleId, RoleName = p.Role != null ? p.Role.RoleName : string.Empty,
                MenuId = p.MenuId, MenuName = p.Menu != null ? p.Menu.MenuName : string.Empty,
                RouteUrl = p.Menu != null ? p.Menu.RouteUrl : null,
                CanView = p.CanView, CanAdd = p.CanAdd, CanEdit = p.CanEdit, CanDelete = p.CanDelete,
                IsActive = p.IsActive, CreatedAt = p.CreatedAt
            }).ToListAsync();
        return ApiResponse<List<RoleMenuPermissionDto>>.SuccessResult(list, "Permissions fetched successfully.");
    }

    public async Task<ApiResponse<bool>> SaveRolePermissionsAsync(BulkSaveRolePermissionDto dto)
    {
        var existing = await _context.RoleMenuPermissions.Where(p => p.RoleId == dto.RoleId).ToListAsync();
        foreach (var item in dto.Permissions)
        {
            var perm = existing.FirstOrDefault(p => p.MenuId == item.MenuId);
            if (perm != null)
            {
                perm.CanView = item.CanView; perm.CanAdd = item.CanAdd;
                perm.CanEdit = item.CanEdit; perm.CanDelete = item.CanDelete;
                perm.UpdatedBy = dto.UpdatedBy; perm.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.RoleMenuPermissions.Add(new RoleMenuPermission
                {
                    RoleId = dto.RoleId, MenuId = item.MenuId,
                    CanView = item.CanView, CanAdd = item.CanAdd, CanEdit = item.CanEdit, CanDelete = item.CanDelete,
                    IsActive = true, UpdatedBy = dto.UpdatedBy, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                });
            }
        }
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Permissions saved successfully.");
    }

    public async Task<ApiResponse<List<RoleMenuPermissionDto>>> GetAllPermissionsAsync()
    {
        var list = await _context.RoleMenuPermissions.Include(p => p.Role).Include(p => p.Menu).AsNoTracking()
            .Select(p => new RoleMenuPermissionDto
            {
                Id = p.Id, RoleId = p.RoleId, RoleName = p.Role != null ? p.Role.RoleName : string.Empty,
                MenuId = p.MenuId, MenuName = p.Menu != null ? p.Menu.MenuName : string.Empty,
                RouteUrl = p.Menu != null ? p.Menu.RouteUrl : null,
                CanView = p.CanView, CanAdd = p.CanAdd, CanEdit = p.CanEdit, CanDelete = p.CanDelete,
                IsActive = p.IsActive, CreatedAt = p.CreatedAt
            }).ToListAsync();
        return ApiResponse<List<RoleMenuPermissionDto>>.SuccessResult(list, "All permissions fetched.");
    }
}

// ==========================================
// 3. BLOG SERVICE — Updated to match new entity
// ==========================================
public class BlogService : IBlogService
{
    private readonly AppDbContext _context;
    public BlogService(AppDbContext context) => _context = context;

    private static string GenerateSlug(string title) =>
        System.Text.RegularExpressions.Regex.Replace(title.ToLower().Trim(), @"[^a-z0-9\s-]", "")
            .Replace(" ", "-").Trim('-');

    public async Task<ApiResponse<List<BlogDto>>> GetBlogsAsync(bool activeOnly = false)
    {
        var query = _context.Blogs.Include(b => b.Author).AsNoTracking();
        if (activeOnly) query = query.Where(b => b.IsActive);

        var list = await query.OrderByDescending(b => b.CreatedAt).Select(b => new BlogDto
        {
            Id = b.Id, Title = b.Title, Slug = b.Slug, Summary = b.Summary, Content = b.Content,
            AuthorId = b.AuthorId, AuthorName = b.Author != null ? b.Author.FullName : null,
            Category = b.Category, Tags = b.Tags, BannerImage = b.BannerImage,
            IsPublished = b.IsPublished, PublishedAt = b.PublishedAt,
            IsActive = b.IsActive, CreatedAt = b.CreatedAt, UpdatedAt = b.UpdatedAt
        }).ToListAsync();

        return ApiResponse<List<BlogDto>>.SuccessResult(list, "Blogs fetched successfully.");
    }

    public async Task<ApiResponse<BlogDto>> GetBlogByIdAsync(int id)
    {
        var blog = await _context.Blogs.Include(b => b.Author).AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (blog == null) return ApiResponse<BlogDto>.FailResult("Blog not found.", statusCode: 404);

        return ApiResponse<BlogDto>.SuccessResult(new BlogDto
        {
            Id = blog.Id, Title = blog.Title, Slug = blog.Slug, Summary = blog.Summary, Content = blog.Content,
            AuthorId = blog.AuthorId, AuthorName = blog.Author != null ? blog.Author.FullName : null,
            Category = blog.Category, Tags = blog.Tags, BannerImage = blog.BannerImage,
            IsPublished = blog.IsPublished, PublishedAt = blog.PublishedAt,
            IsActive = blog.IsActive, CreatedAt = blog.CreatedAt, UpdatedAt = blog.UpdatedAt
        });
    }

    public async Task<ApiResponse<BlogDto>> SaveBlogAsync(SaveBlogDto dto)
    {
        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Title) : dto.Slug.Trim().ToLower();

        if (dto.Id <= 0)
        {
            // Check slug uniqueness
            if (await _context.Blogs.AnyAsync(b => b.Slug == slug))
                slug = slug + "-" + DateTime.UtcNow.Ticks.ToString()[^4..];

            var blog = new Blog
            {
                Title = dto.Title.Trim(), Slug = slug, Summary = dto.Summary?.Trim(), Content = dto.Content,
                Category = dto.Category?.Trim(), Tags = dto.Tags?.Trim(), BannerImage = dto.BannerImage,
                AuthorId = dto.AuthorId, IsPublished = dto.IsPublished,
                PublishedAt = dto.IsPublished ? DateTime.UtcNow : null,
                IsActive = dto.IsActive, UpdatedBy = dto.UpdatedBy, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            return ApiResponse<BlogDto>.SuccessResult(new BlogDto
            {
                Id = blog.Id, Title = blog.Title, Slug = blog.Slug, Summary = blog.Summary, Content = blog.Content,
                Category = blog.Category, Tags = blog.Tags, BannerImage = blog.BannerImage,
                IsPublished = blog.IsPublished, PublishedAt = blog.PublishedAt,
                IsActive = blog.IsActive, CreatedAt = blog.CreatedAt, UpdatedAt = blog.UpdatedAt
            }, "Blog created successfully.", statusCode: 201);
        }
        else
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == dto.Id);
            if (blog == null) return ApiResponse<BlogDto>.FailResult("Blog not found.", statusCode: 404);

            if (await _context.Blogs.AnyAsync(b => b.Id != dto.Id && b.Slug == slug))
                slug = slug + "-" + DateTime.UtcNow.Ticks.ToString()[^4..];

            blog.Title = dto.Title.Trim(); blog.Slug = slug;
            blog.Summary = dto.Summary?.Trim(); blog.Content = dto.Content;
            blog.Category = dto.Category?.Trim(); blog.Tags = dto.Tags?.Trim(); blog.BannerImage = dto.BannerImage;
            blog.AuthorId = dto.AuthorId;
            if (dto.IsPublished && !blog.IsPublished) blog.PublishedAt = DateTime.UtcNow;
            blog.IsPublished = dto.IsPublished;
            blog.IsActive = dto.IsActive; blog.UpdatedBy = dto.UpdatedBy; blog.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<BlogDto>.SuccessResult(new BlogDto
            {
                Id = blog.Id, Title = blog.Title, Slug = blog.Slug, Summary = blog.Summary, Content = blog.Content,
                Category = blog.Category, Tags = blog.Tags, BannerImage = blog.BannerImage,
                IsPublished = blog.IsPublished, PublishedAt = blog.PublishedAt,
                IsActive = blog.IsActive, CreatedAt = blog.CreatedAt, UpdatedAt = blog.UpdatedAt
            }, "Blog updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteBlogAsync(int id, int? updatedBy = null)
    {
        var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
        if (blog == null) return ApiResponse<bool>.FailResult("Blog not found.", statusCode: 404);

        blog.IsDeleted = true; blog.UpdatedBy = updatedBy; blog.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Blog deleted successfully.");
    }
}

// ==========================================
// 4. PORTFOLIO SERVICE — Updated to match new entity
// ==========================================
public class PortfolioService : IPortfolioService
{
    private readonly AppDbContext _context;
    public PortfolioService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<PortfolioDto>>> GetPortfoliosAsync(bool activeOnly = false)
    {
        var query = _context.Portfolios.AsNoTracking();
        if (activeOnly) query = query.Where(p => p.IsActive);

        var list = await query.OrderByDescending(p => p.CreatedAt).Select(p => new PortfolioDto
        {
            Id = p.Id, Title = p.Title, ClientName = p.ClientName, Category = p.Category,
            ProjectUrl = p.ProjectUrl, ThumbnailUrl = p.ThumbnailUrl, Description = p.Description,
            Technologies = p.Technologies, CompletionDate = p.CompletionDate,
            IsActive = p.IsActive, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
        }).ToListAsync();

        return ApiResponse<List<PortfolioDto>>.SuccessResult(list, "Portfolios fetched successfully.");
    }

    public async Task<ApiResponse<PortfolioDto>> GetPortfolioByIdAsync(int id)
    {
        var p = await _context.Portfolios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return ApiResponse<PortfolioDto>.FailResult("Portfolio not found.", statusCode: 404);

        return ApiResponse<PortfolioDto>.SuccessResult(new PortfolioDto
        {
            Id = p.Id, Title = p.Title, ClientName = p.ClientName, Category = p.Category,
            ProjectUrl = p.ProjectUrl, ThumbnailUrl = p.ThumbnailUrl, Description = p.Description,
            Technologies = p.Technologies, CompletionDate = p.CompletionDate,
            IsActive = p.IsActive, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
        });
    }

    public async Task<ApiResponse<PortfolioDto>> SavePortfolioAsync(SavePortfolioDto dto)
    {
        if (dto.Id <= 0)
        {
            var portfolio = new Portfolio
            {
                Title = dto.Title.Trim(), ClientName = dto.ClientName?.Trim(), Category = dto.Category?.Trim(),
                ProjectUrl = dto.ProjectUrl?.Trim(), ThumbnailUrl = dto.ThumbnailUrl?.Trim(),
                Description = dto.Description, Technologies = dto.Technologies?.Trim(),
                CompletionDate = dto.CompletionDate, IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _context.Portfolios.Add(portfolio);
            await _context.SaveChangesAsync();

            return ApiResponse<PortfolioDto>.SuccessResult(new PortfolioDto
            {
                Id = portfolio.Id, Title = portfolio.Title, ClientName = portfolio.ClientName,
                Category = portfolio.Category, ProjectUrl = portfolio.ProjectUrl, ThumbnailUrl = portfolio.ThumbnailUrl,
                Description = portfolio.Description, Technologies = portfolio.Technologies,
                CompletionDate = portfolio.CompletionDate, IsActive = portfolio.IsActive,
                CreatedAt = portfolio.CreatedAt, UpdatedAt = portfolio.UpdatedAt
            }, "Portfolio created successfully.", statusCode: 201);
        }
        else
        {
            var portfolio = await _context.Portfolios.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (portfolio == null) return ApiResponse<PortfolioDto>.FailResult("Portfolio not found.", statusCode: 404);

            portfolio.Title = dto.Title.Trim(); portfolio.ClientName = dto.ClientName?.Trim();
            portfolio.Category = dto.Category?.Trim(); portfolio.ProjectUrl = dto.ProjectUrl?.Trim();
            portfolio.ThumbnailUrl = dto.ThumbnailUrl?.Trim(); portfolio.Description = dto.Description;
            portfolio.Technologies = dto.Technologies?.Trim(); portfolio.CompletionDate = dto.CompletionDate;
            portfolio.IsActive = dto.IsActive; portfolio.UpdatedBy = dto.UpdatedBy; portfolio.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<PortfolioDto>.SuccessResult(new PortfolioDto
            {
                Id = portfolio.Id, Title = portfolio.Title, ClientName = portfolio.ClientName,
                Category = portfolio.Category, ProjectUrl = portfolio.ProjectUrl, ThumbnailUrl = portfolio.ThumbnailUrl,
                Description = portfolio.Description, Technologies = portfolio.Technologies,
                CompletionDate = portfolio.CompletionDate, IsActive = portfolio.IsActive,
                CreatedAt = portfolio.CreatedAt, UpdatedAt = portfolio.UpdatedAt
            }, "Portfolio updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeletePortfolioAsync(int id, int? updatedBy = null)
    {
        var p = await _context.Portfolios.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return ApiResponse<bool>.FailResult("Portfolio not found.", statusCode: 404);

        p.IsDeleted = true; p.UpdatedBy = updatedBy; p.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Portfolio deleted successfully.");
    }
}

// ==========================================
// 5. TEAM MEMBER SERVICE — Updated to match new entity
// ==========================================
public class TeamMemberService : ITeamMemberService
{
    private readonly AppDbContext _context;
    public TeamMemberService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<TeamMemberDto>>> GetTeamMembersAsync(bool activeOnly = false)
    {
        var query = _context.TeamMembers.Include(t => t.User).ThenInclude(u => u!.Role).AsNoTracking();
        if (activeOnly) query = query.Where(t => t.IsActive);

        var list = await query.OrderBy(t => t.FullName).Select(t => new TeamMemberDto
        {
            Id = t.Id,
            FullName = t.FullName,
            Designation = t.Designation,
            Department = t.Department,
            Email = t.Email,
            Mobile = t.User != null ? t.User.Mobile : null,
            ProfileImage = t.ProfileImage,
            Bio = t.Bio,
            LinkedInUrl = t.LinkedInUrl,
            GithubUrl = t.GithubUrl,
            UserId = t.UserId,
            RoleId = t.User != null ? t.User.RoleId : null,
            RoleName = t.User != null && t.User.Role != null ? t.User.Role.RoleName : null,
            HasLoginAccess = t.UserId != null && t.User != null && t.User.IsActive,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToListAsync();

        return ApiResponse<List<TeamMemberDto>>.SuccessResult(list, "Team members fetched successfully.");
    }

    public async Task<ApiResponse<TeamMemberDto>> GetTeamMemberByIdAsync(int id)
    {
        var t = await _context.TeamMembers.Include(x => x.User).ThenInclude(u => u!.Role).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return ApiResponse<TeamMemberDto>.FailResult("Team member not found.", statusCode: 404);

        return ApiResponse<TeamMemberDto>.SuccessResult(new TeamMemberDto
        {
            Id = t.Id,
            FullName = t.FullName,
            Designation = t.Designation,
            Department = t.Department,
            Email = t.Email,
            Mobile = t.User != null ? t.User.Mobile : null,
            ProfileImage = t.ProfileImage,
            Bio = t.Bio,
            LinkedInUrl = t.LinkedInUrl,
            GithubUrl = t.GithubUrl,
            UserId = t.UserId,
            RoleId = t.User != null ? t.User.RoleId : null,
            RoleName = t.User != null && t.User.Role != null ? t.User.Role.RoleName : null,
            HasLoginAccess = t.UserId != null && t.User != null && t.User.IsActive,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        });
    }

    public async Task<ApiResponse<TeamMemberDto>> SaveTeamMemberAsync(SaveTeamMemberDto dto)
    {
        int? linkedUserId = null;
        string? resolvedRoleName = null;

        // Sync or Create User in tbl_users if email is present and login access is requested
        if (dto.EnableLoginAccess && !string.IsNullOrWhiteSpace(dto.Email))
        {
            var cleanEmail = dto.Email.Trim().ToLower();
            var existingUser = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail);

            // Determine Role
            int targetRoleId;
            if (dto.RoleId.HasValue && dto.RoleId.Value > 0)
            {
                targetRoleId = dto.RoleId.Value;
            }
            else if (existingUser != null)
            {
                targetRoleId = existingUser.RoleId;
            }
            else
            {
                // Default to first non-admin role or any role
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == "employee" || r.RoleName.ToLower() == "team member" || r.RoleName.ToLower() == "staff" || r.RoleName.ToLower() == "user")
                                  ?? await _context.Roles.FirstOrDefaultAsync();
                targetRoleId = defaultRole != null ? defaultRole.Id : 1;
            }

            var roleObj = await _context.Roles.FirstOrDefaultAsync(r => r.Id == targetRoleId);
            resolvedRoleName = roleObj?.RoleName;

            if (existingUser != null)
            {
                existingUser.FullName = dto.FullName.Trim();
                existingUser.RoleId = targetRoleId;
                if (!string.IsNullOrWhiteSpace(dto.Mobile)) existingUser.Mobile = dto.Mobile.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Password)) existingUser.PasswordHash = PasswordHelper.HashPassword(dto.Password);
                existingUser.IsActive = dto.IsActive;
                existingUser.UpdatedAt = DateTime.UtcNow;
                linkedUserId = existingUser.Id;
            }
            else
            {
                var initialPassword = string.IsNullOrWhiteSpace(dto.Password) ? "Velvion@2026" : dto.Password;
                var newUser = new User
                {
                    FullName = dto.FullName.Trim(),
                    Email = cleanEmail,
                    Mobile = dto.Mobile?.Trim(),
                    RoleId = targetRoleId,
                    PasswordHash = PasswordHelper.HashPassword(initialPassword),
                    IsActive = dto.IsActive,
                    UpdatedBy = dto.UpdatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                linkedUserId = newUser.Id;
            }
        }

        if (dto.Id <= 0)
        {
            var member = new TeamMember
            {
                FullName = dto.FullName.Trim(),
                Designation = dto.Designation.Trim(),
                Department = dto.Department?.Trim(),
                Email = dto.Email?.Trim().ToLower(),
                ProfileImage = dto.ProfileImage?.Trim(),
                Bio = dto.Bio,
                LinkedInUrl = dto.LinkedInUrl?.Trim(),
                GithubUrl = dto.GithubUrl?.Trim(),
                UserId = linkedUserId,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.TeamMembers.Add(member);
            await _context.SaveChangesAsync();

            return ApiResponse<TeamMemberDto>.SuccessResult(new TeamMemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                Designation = member.Designation,
                Department = member.Department,
                Email = member.Email,
                Mobile = dto.Mobile,
                ProfileImage = member.ProfileImage,
                Bio = member.Bio,
                LinkedInUrl = member.LinkedInUrl,
                GithubUrl = member.GithubUrl,
                UserId = member.UserId,
                RoleId = dto.RoleId,
                RoleName = resolvedRoleName,
                HasLoginAccess = linkedUserId.HasValue,
                IsActive = member.IsActive,
                CreatedAt = member.CreatedAt,
                UpdatedAt = member.UpdatedAt
            }, "Team member added and user login synced successfully.", statusCode: 201);
        }
        else
        {
            var member = await _context.TeamMembers.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (member == null) return ApiResponse<TeamMemberDto>.FailResult("Team member not found.", statusCode: 404);

            member.FullName = dto.FullName.Trim();
            member.Designation = dto.Designation.Trim();
            member.Department = dto.Department?.Trim();
            member.Email = dto.Email?.Trim().ToLower();
            member.ProfileImage = dto.ProfileImage?.Trim();
            member.Bio = dto.Bio;
            member.LinkedInUrl = dto.LinkedInUrl?.Trim();
            member.GithubUrl = dto.GithubUrl?.Trim();
            if (linkedUserId.HasValue) member.UserId = linkedUserId;
            member.IsActive = dto.IsActive;
            member.UpdatedBy = dto.UpdatedBy;
            member.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<TeamMemberDto>.SuccessResult(new TeamMemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                Designation = member.Designation,
                Department = member.Department,
                Email = member.Email,
                Mobile = dto.Mobile,
                ProfileImage = member.ProfileImage,
                Bio = member.Bio,
                LinkedInUrl = member.LinkedInUrl,
                GithubUrl = member.GithubUrl,
                UserId = member.UserId,
                RoleId = dto.RoleId,
                RoleName = resolvedRoleName,
                HasLoginAccess = member.UserId.HasValue,
                IsActive = member.IsActive,
                CreatedAt = member.CreatedAt,
                UpdatedAt = member.UpdatedAt
            }, "Team member updated and user account synced successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteTeamMemberAsync(int id, int? updatedBy = null)
    {
        var member = await _context.TeamMembers.FirstOrDefaultAsync(x => x.Id == id);
        if (member == null) return ApiResponse<bool>.FailResult("Team member not found.", statusCode: 404);

        member.IsDeleted = true;
        member.UpdatedBy = updatedBy;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Team member deleted successfully.");
    }
}

// ==========================================
// 6. TESTIMONIAL SERVICE — Updated to match new entity
// ==========================================
public class TestimonialService : ITestimonialService
{
    private readonly AppDbContext _context;
    public TestimonialService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<TestimonialDto>>> GetTestimonialsAsync(bool activeOnly = false)
    {
        var query = _context.Testimonials.AsNoTracking();
        if (activeOnly) query = query.Where(t => t.IsActive);

        var list = await query.OrderByDescending(t => t.CreatedAt).Select(t => new TestimonialDto
        {
            Id = t.Id, ClientName = t.ClientName, ClientDesignation = t.ClientDesignation,
            CompanyName = t.CompanyName, AvatarUrl = t.AvatarUrl, Rating = t.Rating,
            FeedbackText = t.FeedbackText, IsActive = t.IsActive, CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
        }).ToListAsync();

        return ApiResponse<List<TestimonialDto>>.SuccessResult(list, "Testimonials fetched successfully.");
    }

    public async Task<ApiResponse<TestimonialDto>> GetTestimonialByIdAsync(int id)
    {
        var t = await _context.Testimonials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return ApiResponse<TestimonialDto>.FailResult("Testimonial not found.", statusCode: 404);

        return ApiResponse<TestimonialDto>.SuccessResult(new TestimonialDto
        {
            Id = t.Id, ClientName = t.ClientName, ClientDesignation = t.ClientDesignation,
            CompanyName = t.CompanyName, AvatarUrl = t.AvatarUrl, Rating = t.Rating,
            FeedbackText = t.FeedbackText, IsActive = t.IsActive, CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
        });
    }

    public async Task<ApiResponse<TestimonialDto>> SaveTestimonialAsync(SaveTestimonialDto dto)
    {
        if (dto.Id <= 0)
        {
            var t = new Testimonial
            {
                ClientName = dto.ClientName.Trim(), ClientDesignation = dto.ClientDesignation?.Trim(),
                CompanyName = dto.CompanyName?.Trim(), AvatarUrl = dto.AvatarUrl?.Trim(),
                Rating = dto.Rating, FeedbackText = dto.FeedbackText.Trim(),
                IsActive = dto.IsActive, UpdatedBy = dto.UpdatedBy, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _context.Testimonials.Add(t);
            await _context.SaveChangesAsync();

            return ApiResponse<TestimonialDto>.SuccessResult(new TestimonialDto
            {
                Id = t.Id, ClientName = t.ClientName, ClientDesignation = t.ClientDesignation,
                CompanyName = t.CompanyName, AvatarUrl = t.AvatarUrl, Rating = t.Rating,
                FeedbackText = t.FeedbackText, IsActive = t.IsActive, CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
            }, "Testimonial added successfully.", statusCode: 201);
        }
        else
        {
            var t = await _context.Testimonials.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (t == null) return ApiResponse<TestimonialDto>.FailResult("Testimonial not found.", statusCode: 404);

            t.ClientName = dto.ClientName.Trim(); t.ClientDesignation = dto.ClientDesignation?.Trim();
            t.CompanyName = dto.CompanyName?.Trim(); t.AvatarUrl = dto.AvatarUrl?.Trim();
            t.Rating = dto.Rating; t.FeedbackText = dto.FeedbackText.Trim();
            t.IsActive = dto.IsActive; t.UpdatedBy = dto.UpdatedBy; t.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<TestimonialDto>.SuccessResult(new TestimonialDto
            {
                Id = t.Id, ClientName = t.ClientName, ClientDesignation = t.ClientDesignation,
                CompanyName = t.CompanyName, AvatarUrl = t.AvatarUrl, Rating = t.Rating,
                FeedbackText = t.FeedbackText, IsActive = t.IsActive, CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
            }, "Testimonial updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteTestimonialAsync(int id, int? updatedBy = null)
    {
        var t = await _context.Testimonials.FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return ApiResponse<bool>.FailResult("Testimonial not found.", statusCode: 404);

        t.IsDeleted = true; t.UpdatedBy = updatedBy; t.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Testimonial deleted successfully.");
    }
}

// ==========================================
// 7. CONTACT INQUIRY SERVICE — Updated to match new entity
// ==========================================
public class ContactInquiryService : IContactInquiryService
{
    private readonly AppDbContext _context;
    public ContactInquiryService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<ContactInquiryDto>>> GetInquiriesAsync(bool activeOnly = false)
    {
        var query = _context.ContactInquiries.AsNoTracking();
        if (activeOnly) query = query.Where(c => !c.IsResolved);

        var list = await query.OrderByDescending(c => c.CreatedAt).Select(c => new ContactInquiryDto
        {
            Id = c.Id, FullName = c.FullName, Email = c.Email, Phone = c.Phone,
            Subject = c.Subject, Message = c.Message, IsResolved = c.IsResolved,
            CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
        }).ToListAsync();

        return ApiResponse<List<ContactInquiryDto>>.SuccessResult(list, "Inquiries fetched successfully.");
    }

    public async Task<ApiResponse<ContactInquiryDto>> GetInquiryByIdAsync(int id)
    {
        var c = await _context.ContactInquiries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return ApiResponse<ContactInquiryDto>.FailResult("Inquiry not found.", statusCode: 404);

        return ApiResponse<ContactInquiryDto>.SuccessResult(new ContactInquiryDto
        {
            Id = c.Id, FullName = c.FullName, Email = c.Email, Phone = c.Phone,
            Subject = c.Subject, Message = c.Message, IsResolved = c.IsResolved,
            CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
        });
    }

    public async Task<ApiResponse<ContactInquiryDto>> SaveInquiryAsync(SaveContactInquiryDto dto)
    {
        if (dto.Id <= 0)
        {
            var inquiry = new ContactInquiry
            {
                FullName = dto.FullName.Trim(), Email = dto.Email.Trim().ToLower(),
                Phone = dto.Phone?.Trim(), Subject = dto.Subject?.Trim(), Message = dto.Message.Trim(),
                IsResolved = dto.IsResolved, UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _context.ContactInquiries.Add(inquiry);
            await _context.SaveChangesAsync();

            return ApiResponse<ContactInquiryDto>.SuccessResult(new ContactInquiryDto
            {
                Id = inquiry.Id, FullName = inquiry.FullName, Email = inquiry.Email, Phone = inquiry.Phone,
                Subject = inquiry.Subject, Message = inquiry.Message, IsResolved = inquiry.IsResolved,
                CreatedAt = inquiry.CreatedAt, UpdatedAt = inquiry.UpdatedAt
            }, "Inquiry submitted successfully.", statusCode: 201);
        }
        else
        {
            var inquiry = await _context.ContactInquiries.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (inquiry == null) return ApiResponse<ContactInquiryDto>.FailResult("Inquiry not found.", statusCode: 404);

            inquiry.FullName = dto.FullName.Trim(); inquiry.Email = dto.Email.Trim().ToLower();
            inquiry.Phone = dto.Phone?.Trim(); inquiry.Subject = dto.Subject?.Trim();
            inquiry.Message = dto.Message.Trim(); inquiry.IsResolved = dto.IsResolved;
            inquiry.UpdatedBy = dto.UpdatedBy; inquiry.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<ContactInquiryDto>.SuccessResult(new ContactInquiryDto
            {
                Id = inquiry.Id, FullName = inquiry.FullName, Email = inquiry.Email, Phone = inquiry.Phone,
                Subject = inquiry.Subject, Message = inquiry.Message, IsResolved = inquiry.IsResolved,
                CreatedAt = inquiry.CreatedAt, UpdatedAt = inquiry.UpdatedAt
            }, "Inquiry updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> MarkResolvedAsync(int id, int? updatedBy = null)
    {
        var inquiry = await _context.ContactInquiries.FirstOrDefaultAsync(x => x.Id == id);
        if (inquiry == null) return ApiResponse<bool>.FailResult("Inquiry not found.", statusCode: 404);

        inquiry.IsResolved = true; inquiry.UpdatedBy = updatedBy; inquiry.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Inquiry marked as resolved.");
    }

    public async Task<ApiResponse<bool>> DeleteInquiryAsync(int id, int? updatedBy = null)
    {
        var inquiry = await _context.ContactInquiries.FirstOrDefaultAsync(x => x.Id == id);
        if (inquiry == null) return ApiResponse<bool>.FailResult("Inquiry not found.", statusCode: 404);

        inquiry.IsDeleted = true; inquiry.UpdatedBy = updatedBy; inquiry.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Inquiry deleted successfully.");
    }
}

// ==========================================
// 8. AUDIT LOG SERVICE
// ==========================================
public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;
    public AuditLogService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<AuditLogDto>>> GetLogsAsync(int limit = 100)
    {
        var list = await _context.AuditLogs.Include(l => l.User).AsNoTracking()
            .OrderByDescending(l => l.CreatedAt).Take(limit)
            .Select(l => new AuditLogDto
            {
                Id = l.Id, UserId = l.UserId,
                UserName = l.User != null ? l.User.FullName : null,
                ActionName = l.ActionName, TableName = l.TableName,
                RecordId = l.RecordId, LogDetails = l.LogDetails,
                IPAddress = l.IPAddress, CreatedAt = l.CreatedAt
            }).ToListAsync();

        return ApiResponse<List<AuditLogDto>>.SuccessResult(list, "Audit logs fetched.");
    }

    public async Task<ApiResponse<bool>> CreateLogAsync(CreateAuditLogDto dto)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = dto.UserId, ActionName = dto.ActionName, TableName = dto.TableName,
            RecordId = dto.RecordId, LogDetails = dto.LogDetails,
            IPAddress = dto.IPAddress, CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Log created.");
    }
}

// ==========================================
// 9. JOB POSTING SERVICE — New
// ==========================================
public class JobPostingService : IJobPostingService
{
    private readonly AppDbContext _context;
    public JobPostingService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<JobPostingDto>>> GetJobPostingsAsync(bool activeOnly = false)
    {
        var query = _context.JobPostings.AsNoTracking();
        if (activeOnly) query = query.Where(j => j.IsActive);

        var list = await query.OrderByDescending(j => j.CreatedAt).Select(j => new JobPostingDto
        {
            Id = j.Id, JobTitle = j.JobTitle, Department = j.Department, Location = j.Location,
            JobType = j.JobType, ExperienceLevel = j.ExperienceLevel, Description = j.Description,
            RequiredSkills = j.RequiredSkills, SalaryRange = j.SalaryRange, LastDateToApply = j.LastDateToApply,
            ApplicationCount = j.Applications.Count(a => !a.IsDeleted),
            IsActive = j.IsActive, CreatedAt = j.CreatedAt, UpdatedAt = j.UpdatedAt
        }).ToListAsync();

        return ApiResponse<List<JobPostingDto>>.SuccessResult(list, "Job postings fetched successfully.");
    }

    public async Task<ApiResponse<JobPostingDto>> GetJobPostingByIdAsync(int id)
    {
        var j = await _context.JobPostings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (j == null) return ApiResponse<JobPostingDto>.FailResult("Job posting not found.", statusCode: 404);

        var appCount = await _context.JobApplications.CountAsync(a => a.JobPostingId == id && !a.IsDeleted);

        return ApiResponse<JobPostingDto>.SuccessResult(new JobPostingDto
        {
            Id = j.Id, JobTitle = j.JobTitle, Department = j.Department, Location = j.Location,
            JobType = j.JobType, ExperienceLevel = j.ExperienceLevel, Description = j.Description,
            RequiredSkills = j.RequiredSkills, SalaryRange = j.SalaryRange, LastDateToApply = j.LastDateToApply,
            ApplicationCount = appCount, IsActive = j.IsActive, CreatedAt = j.CreatedAt, UpdatedAt = j.UpdatedAt
        });
    }

    public async Task<ApiResponse<JobPostingDto>> SaveJobPostingAsync(SaveJobPostingDto dto)
    {
        if (dto.Id <= 0)
        {
            var job = new JobPosting
            {
                JobTitle = dto.JobTitle.Trim(), Department = dto.Department?.Trim(), Location = dto.Location?.Trim(),
                JobType = dto.JobType, ExperienceLevel = dto.ExperienceLevel, Description = dto.Description,
                RequiredSkills = dto.RequiredSkills, SalaryRange = dto.SalaryRange?.Trim(),
                LastDateToApply = dto.LastDateToApply, IsActive = dto.IsActive, UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _context.JobPostings.Add(job);
            await _context.SaveChangesAsync();

            return ApiResponse<JobPostingDto>.SuccessResult(new JobPostingDto
            {
                Id = job.Id, JobTitle = job.JobTitle, Department = job.Department, Location = job.Location,
                JobType = job.JobType, ExperienceLevel = job.ExperienceLevel, Description = job.Description,
                RequiredSkills = job.RequiredSkills, SalaryRange = job.SalaryRange, LastDateToApply = job.LastDateToApply,
                ApplicationCount = 0, IsActive = job.IsActive, CreatedAt = job.CreatedAt, UpdatedAt = job.UpdatedAt
            }, "Job posting created successfully.", statusCode: 201);
        }
        else
        {
            var job = await _context.JobPostings.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (job == null) return ApiResponse<JobPostingDto>.FailResult("Job posting not found.", statusCode: 404);

            job.JobTitle = dto.JobTitle.Trim(); job.Department = dto.Department?.Trim(); job.Location = dto.Location?.Trim();
            job.JobType = dto.JobType; job.ExperienceLevel = dto.ExperienceLevel; job.Description = dto.Description;
            job.RequiredSkills = dto.RequiredSkills; job.SalaryRange = dto.SalaryRange?.Trim();
            job.LastDateToApply = dto.LastDateToApply; job.IsActive = dto.IsActive;
            job.UpdatedBy = dto.UpdatedBy; job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var appCount = await _context.JobApplications.CountAsync(a => a.JobPostingId == job.Id && !a.IsDeleted);

            return ApiResponse<JobPostingDto>.SuccessResult(new JobPostingDto
            {
                Id = job.Id, JobTitle = job.JobTitle, Department = job.Department, Location = job.Location,
                JobType = job.JobType, ExperienceLevel = job.ExperienceLevel, Description = job.Description,
                RequiredSkills = job.RequiredSkills, SalaryRange = job.SalaryRange, LastDateToApply = job.LastDateToApply,
                ApplicationCount = appCount, IsActive = job.IsActive, CreatedAt = job.CreatedAt, UpdatedAt = job.UpdatedAt
            }, "Job posting updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteJobPostingAsync(int id, int? updatedBy = null)
    {
        var job = await _context.JobPostings.FirstOrDefaultAsync(x => x.Id == id);
        if (job == null) return ApiResponse<bool>.FailResult("Job posting not found.", statusCode: 404);

        job.IsDeleted = true; job.UpdatedBy = updatedBy; job.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Job posting deleted successfully.");
    }
}

// ==========================================
// 10. JOB APPLICATION SERVICE — New
// ==========================================
public class JobApplicationService : IJobApplicationService
{
    private readonly AppDbContext _context;
    public JobApplicationService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<JobApplicationDto>>> GetApplicationsAsync(int? jobPostingId = null)
    {
        var query = _context.JobApplications.Include(a => a.JobPosting).AsNoTracking();
        if (jobPostingId.HasValue) query = query.Where(a => a.JobPostingId == jobPostingId.Value);

        var list = await query.OrderByDescending(a => a.CreatedAt).Select(a => new JobApplicationDto
        {
            Id = a.Id, JobPostingId = a.JobPostingId,
            JobTitle = a.JobPosting != null ? a.JobPosting.JobTitle : null,
            ApplicantName = a.ApplicantName, Email = a.Email, Phone = a.Phone,
            ResumeUrl = a.ResumeUrl, CoverLetter = a.CoverLetter, Status = a.Status,
            Notes = a.Notes, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt
        }).ToListAsync();

        return ApiResponse<List<JobApplicationDto>>.SuccessResult(list, "Applications fetched successfully.");
    }

    public async Task<ApiResponse<JobApplicationDto>> GetApplicationByIdAsync(int id)
    {
        var a = await _context.JobApplications.Include(x => x.JobPosting).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return ApiResponse<JobApplicationDto>.FailResult("Application not found.", statusCode: 404);

        return ApiResponse<JobApplicationDto>.SuccessResult(new JobApplicationDto
        {
            Id = a.Id, JobPostingId = a.JobPostingId,
            JobTitle = a.JobPosting != null ? a.JobPosting.JobTitle : null,
            ApplicantName = a.ApplicantName, Email = a.Email, Phone = a.Phone,
            ResumeUrl = a.ResumeUrl, CoverLetter = a.CoverLetter, Status = a.Status,
            Notes = a.Notes, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt
        });
    }

    public async Task<ApiResponse<JobApplicationDto>> SaveApplicationAsync(SaveJobApplicationDto dto)
    {
        var job = await _context.JobPostings.FirstOrDefaultAsync(j => j.Id == dto.JobPostingId);
        if (job == null) return ApiResponse<JobApplicationDto>.FailResult("Job posting not found.", statusCode: 400);

        if (dto.Id <= 0)
        {
            var app = new JobApplication
            {
                JobPostingId = dto.JobPostingId, ApplicantName = dto.ApplicantName.Trim(),
                Email = dto.Email.Trim().ToLower(), Phone = dto.Phone?.Trim(),
                ResumeUrl = dto.ResumeUrl?.Trim(), CoverLetter = dto.CoverLetter, Status = dto.Status,
                Notes = dto.Notes, UpdatedBy = dto.UpdatedBy, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _context.JobApplications.Add(app);
            await _context.SaveChangesAsync();

            return ApiResponse<JobApplicationDto>.SuccessResult(new JobApplicationDto
            {
                Id = app.Id, JobPostingId = app.JobPostingId, JobTitle = job.JobTitle,
                ApplicantName = app.ApplicantName, Email = app.Email, Phone = app.Phone,
                ResumeUrl = app.ResumeUrl, CoverLetter = app.CoverLetter, Status = app.Status,
                Notes = app.Notes, CreatedAt = app.CreatedAt, UpdatedAt = app.UpdatedAt
            }, "Application submitted successfully.", statusCode: 201);
        }
        else
        {
            var app = await _context.JobApplications.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (app == null) return ApiResponse<JobApplicationDto>.FailResult("Application not found.", statusCode: 404);

            app.ApplicantName = dto.ApplicantName.Trim(); app.Email = dto.Email.Trim().ToLower();
            app.Phone = dto.Phone?.Trim(); app.ResumeUrl = dto.ResumeUrl?.Trim(); app.CoverLetter = dto.CoverLetter;
            app.Status = dto.Status; app.Notes = dto.Notes;
            app.UpdatedBy = dto.UpdatedBy; app.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<JobApplicationDto>.SuccessResult(new JobApplicationDto
            {
                Id = app.Id, JobPostingId = app.JobPostingId, JobTitle = job.JobTitle,
                ApplicantName = app.ApplicantName, Email = app.Email, Phone = app.Phone,
                ResumeUrl = app.ResumeUrl, CoverLetter = app.CoverLetter, Status = app.Status,
                Notes = app.Notes, CreatedAt = app.CreatedAt, UpdatedAt = app.UpdatedAt
            }, "Application updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> UpdateApplicationStatusAsync(int id, string status, string? notes, int? updatedBy = null)
    {
        var app = await _context.JobApplications.FirstOrDefaultAsync(x => x.Id == id);
        if (app == null) return ApiResponse<bool>.FailResult("Application not found.", statusCode: 404);

        app.Status = status;
        if (notes != null) app.Notes = notes;
        app.UpdatedBy = updatedBy; app.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, $"Status updated to '{status}'.");
    }

    public async Task<ApiResponse<bool>> DeleteApplicationAsync(int id, int? updatedBy = null)
    {
        var app = await _context.JobApplications.FirstOrDefaultAsync(x => x.Id == id);
        if (app == null) return ApiResponse<bool>.FailResult("Application not found.", statusCode: 404);

        app.IsDeleted = true; app.UpdatedBy = updatedBy; app.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Application deleted successfully.");
    }
}

// ==========================================
// 11. DASHBOARD SERVICE — New (Sprint 5)
// ==========================================
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    public DashboardService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync()
    {
        var stats = new DashboardStatsDto
        {
            TotalUsers = await _context.Users.CountAsync(),
            TotalRoles = await _context.Roles.CountAsync(),
            TotalBlogs = await _context.Blogs.CountAsync(),
            PublishedBlogs = await _context.Blogs.CountAsync(b => b.IsPublished),
            TotalPortfolios = await _context.Portfolios.CountAsync(),
            TotalTeamMembers = await _context.TeamMembers.CountAsync(),
            TotalTestimonials = await _context.Testimonials.CountAsync(),
            TotalInquiries = await _context.ContactInquiries.CountAsync(),
            UnresolvedInquiries = await _context.ContactInquiries.CountAsync(c => !c.IsResolved),
            ActiveJobPostings = await _context.JobPostings.CountAsync(j => j.IsActive),
            TotalJobApplications = await _context.JobApplications.CountAsync(),

            RecentInquiries = await _context.ContactInquiries.AsNoTracking()
                .OrderByDescending(c => c.CreatedAt).Take(5)
                .Select(c => new ContactInquiryDto
                {
                    Id = c.Id, FullName = c.FullName, Email = c.Email,
                    Subject = c.Subject, Message = c.Message, IsResolved = c.IsResolved,
                    CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
                }).ToListAsync(),

            RecentAuditLogs = await _context.AuditLogs.Include(l => l.User).AsNoTracking()
                .OrderByDescending(l => l.CreatedAt).Take(10)
                .Select(l => new AuditLogDto
                {
                    Id = l.Id, UserId = l.UserId,
                    UserName = l.User != null ? l.User.FullName : null,
                    ActionName = l.ActionName, TableName = l.TableName,
                    RecordId = l.RecordId, LogDetails = l.LogDetails,
                    IPAddress = l.IPAddress, CreatedAt = l.CreatedAt
                }).ToListAsync()
        };

        return ApiResponse<DashboardStatsDto>.SuccessResult(stats, "Dashboard stats fetched successfully.");
    }
}
