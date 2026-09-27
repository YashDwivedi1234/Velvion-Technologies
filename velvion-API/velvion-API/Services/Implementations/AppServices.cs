using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using velvion_API.Data;
using velvion_API.DTOs.App;
using velvion_API.DTOs.Common;
using velvion_API.Entities;
using velvion_API.Services.Interfaces;

namespace velvion_API.Services.Implementations;

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

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "_VelvionSalt_2026"));
        return Convert.ToBase64String(hashedBytes);
    }

    public async Task<ApiResponse<List<UserDto>>> GetUsersAsync(bool activeOnly = false)
    {
        var query = _context.Users.Include(u => u.Role).AsNoTracking();
        if (activeOnly)
            query = query.Where(u => u.IsActive);

        var list = await query
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                RoleId = u.RoleId,
                RoleName = u.Role != null ? u.Role.RoleName : string.Empty,
                FullName = u.FullName,
                Email = u.Email,
                Mobile = u.Mobile,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<UserDto>>.SuccessResult(list, "Users fetched successfully.");
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return ApiResponse<UserDto>.FailResult("User not found.", statusCode: 404);

        var dto = new UserDto
        {
            Id = user.Id,
            RoleId = user.RoleId,
            RoleName = user.Role != null ? user.Role.RoleName : string.Empty,
            FullName = user.FullName,
            Email = user.Email,
            Mobile = user.Mobile,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return ApiResponse<UserDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<UserDto>> SaveUserAsync(SaveUserDto dto)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId);
        if (role == null)
            return ApiResponse<UserDto>.FailResult("Specified Role does not exist.", statusCode: 400);

        // 1. INSERT
        if (dto.Id <= 0)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                return ApiResponse<UserDto>.FailResult("Password is required for new users.", statusCode: 400);

            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (emailExists)
                return ApiResponse<UserDto>.FailResult("User with this email already exists.", statusCode: 409);

            var user = new User
            {
                RoleId = dto.RoleId,
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Mobile = dto.Mobile?.Trim(),
                PasswordHash = HashPassword(dto.Password),
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserDto
            {
                Id = user.Id,
                RoleId = user.RoleId,
                RoleName = role.RoleName,
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return ApiResponse<UserDto>.SuccessResult(result, "User created successfully.", statusCode: 201);
        }
        // 2. UPDATE
        else
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (user == null)
                return ApiResponse<UserDto>.FailResult("User not found.", statusCode: 404);

            var emailExists = await _context.Users.AnyAsync(u => u.Id != dto.Id && u.Email.ToLower() == dto.Email.ToLower());
            if (emailExists)
                return ApiResponse<UserDto>.FailResult("Another user with this email already exists.", statusCode: 409);

            user.RoleId = dto.RoleId;
            user.FullName = dto.FullName.Trim();
            user.Email = dto.Email.Trim().ToLower();
            user.Mobile = dto.Mobile?.Trim();
            user.IsActive = dto.IsActive;
            user.UpdatedBy = dto.UpdatedBy;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            var result = new UserDto
            {
                Id = user.Id,
                RoleId = user.RoleId,
                RoleName = role.RoleName,
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return ApiResponse<UserDto>.SuccessResult(result, "User updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(int id, int? updatedBy = null)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return ApiResponse<bool>.FailResult("User not found.", statusCode: 404);

        user.IsDeleted = true;
        user.UpdatedBy = updatedBy;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "User deleted successfully.");
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.Trim().ToLower() && u.IsActive);

        if (user == null || user.PasswordHash != HashPassword(dto.Password))
            return ApiResponse<LoginResponseDto>.FailResult("Invalid email or password.", statusCode: 401);

        var response = new LoginResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role != null ? user.Role.RoleName : string.Empty
        };

        return ApiResponse<LoginResponseDto>.SuccessResult(response, "Login successful.");
    }
}

// ==========================================
// 2. PERMISSION SERVICE
// ==========================================
public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<RoleMenuPermissionDto>>> GetPermissionsByRoleIdAsync(int roleId)
    {
        var list = await _context.RoleMenuPermissions
            .Include(p => p.Role)
            .Include(p => p.Menu)
            .AsNoTracking()
            .Where(p => p.RoleId == roleId)
            .Select(p => new RoleMenuPermissionDto
            {
                Id = p.Id,
                RoleId = p.RoleId,
                RoleName = p.Role != null ? p.Role.RoleName : string.Empty,
                MenuId = p.MenuId,
                MenuName = p.Menu != null ? p.Menu.MenuName : string.Empty,
                RouteUrl = p.Menu != null ? p.Menu.RouteUrl : null,
                CanView = p.CanView,
                CanAdd = p.CanAdd,
                CanEdit = p.CanEdit,
                CanDelete = p.CanDelete,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

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
                perm.CanView = item.CanView;
                perm.CanAdd = item.CanAdd;
                perm.CanEdit = item.CanEdit;
                perm.CanDelete = item.CanDelete;
                perm.UpdatedBy = dto.UpdatedBy;
                perm.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.RoleMenuPermissions.Add(new RoleMenuPermission
                {
                    RoleId = dto.RoleId,
                    MenuId = item.MenuId,
                    CanView = item.CanView,
                    CanAdd = item.CanAdd,
                    CanEdit = item.CanEdit,
                    CanDelete = item.CanDelete,
                    IsActive = true,
                    UpdatedBy = dto.UpdatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Permissions saved successfully.");
    }

    public async Task<ApiResponse<List<RoleMenuPermissionDto>>> GetAllPermissionsAsync()
    {
        var list = await _context.RoleMenuPermissions
            .Include(p => p.Role)
            .Include(p => p.Menu)
            .AsNoTracking()
            .Select(p => new RoleMenuPermissionDto
            {
                Id = p.Id,
                RoleId = p.RoleId,
                RoleName = p.Role != null ? p.Role.RoleName : string.Empty,
                MenuId = p.MenuId,
                MenuName = p.Menu != null ? p.Menu.MenuName : string.Empty,
                RouteUrl = p.Menu != null ? p.Menu.RouteUrl : null,
                CanView = p.CanView,
                CanAdd = p.CanAdd,
                CanEdit = p.CanEdit,
                CanDelete = p.CanDelete,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<RoleMenuPermissionDto>>.SuccessResult(list, "All permissions fetched successfully.");
    }
}

// ==========================================
// 3. BLOG SERVICE
// ==========================================
public class BlogService : IBlogService
{
    private readonly AppDbContext _context;

    public BlogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<BlogDto>>> GetBlogsAsync(bool activeOnly = false)
    {
        var query = _context.Blogs.Include(b => b.Author).AsNoTracking();
        if (activeOnly)
            query = query.Where(b => b.IsActive);

        var list = await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BlogDto
            {
                Id = b.Id,
                Title = b.Title,
                Content = b.Content,
                ImageUrl = b.ImageUrl,
                AuthorId = b.AuthorId,
                AuthorName = b.Author != null ? b.Author.FullName : null,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<BlogDto>>.SuccessResult(list, "Blogs fetched successfully.");
    }

    public async Task<ApiResponse<BlogDto>> GetBlogByIdAsync(int id)
    {
        var blog = await _context.Blogs.Include(b => b.Author).AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (blog == null)
            return ApiResponse<BlogDto>.FailResult("Blog not found.", statusCode: 404);

        var dto = new BlogDto
        {
            Id = blog.Id,
            Title = blog.Title,
            Content = blog.Content,
            ImageUrl = blog.ImageUrl,
            AuthorId = blog.AuthorId,
            AuthorName = blog.Author != null ? blog.Author.FullName : null,
            IsActive = blog.IsActive,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt
        };

        return ApiResponse<BlogDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<BlogDto>> SaveBlogAsync(SaveBlogDto dto)
    {
        if (dto.Id <= 0)
        {
            var blog = new Blog
            {
                Title = dto.Title.Trim(),
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                AuthorId = dto.AuthorId,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            var result = new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                ImageUrl = blog.ImageUrl,
                AuthorId = blog.AuthorId,
                IsActive = blog.IsActive,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt
            };

            return ApiResponse<BlogDto>.SuccessResult(result, "Blog created successfully.", statusCode: 201);
        }
        else
        {
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == dto.Id);
            if (blog == null)
                return ApiResponse<BlogDto>.FailResult("Blog not found.", statusCode: 404);

            blog.Title = dto.Title.Trim();
            blog.Content = dto.Content;
            blog.ImageUrl = dto.ImageUrl;
            blog.AuthorId = dto.AuthorId;
            blog.IsActive = dto.IsActive;
            blog.UpdatedBy = dto.UpdatedBy;
            blog.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                ImageUrl = blog.ImageUrl,
                AuthorId = blog.AuthorId,
                IsActive = blog.IsActive,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt
            };

            return ApiResponse<BlogDto>.SuccessResult(result, "Blog updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteBlogAsync(int id, int? updatedBy = null)
    {
        var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
        if (blog == null)
            return ApiResponse<bool>.FailResult("Blog not found.", statusCode: 404);

        blog.IsDeleted = true;
        blog.UpdatedBy = updatedBy;
        blog.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Blog deleted successfully.");
    }
}

// ==========================================
// 4. PORTFOLIO SERVICE
// ==========================================
public class PortfolioService : IPortfolioService
{
    private readonly AppDbContext _context;

    public PortfolioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<PortfolioDto>>> GetPortfoliosAsync(bool activeOnly = false)
    {
        var query = _context.Portfolios.AsNoTracking();
        if (activeOnly)
            query = query.Where(p => p.IsActive);

        var list = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PortfolioDto
            {
                Id = p.Id,
                ProjectName = p.ProjectName,
                ClientName = p.ClientName,
                TechStack = p.TechStack,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<PortfolioDto>>.SuccessResult(list, "Portfolios fetched successfully.");
    }

    public async Task<ApiResponse<PortfolioDto>> GetPortfolioByIdAsync(int id)
    {
        var p = await _context.Portfolios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (p == null)
            return ApiResponse<PortfolioDto>.FailResult("Portfolio item not found.", statusCode: 404);

        var dto = new PortfolioDto
        {
            Id = p.Id,
            ProjectName = p.ProjectName,
            ClientName = p.ClientName,
            TechStack = p.TechStack,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };

        return ApiResponse<PortfolioDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<PortfolioDto>> SavePortfolioAsync(SavePortfolioDto dto)
    {
        if (dto.Id <= 0)
        {
            var p = new Portfolio
            {
                ProjectName = dto.ProjectName.Trim(),
                ClientName = dto.ClientName,
                TechStack = dto.TechStack,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Portfolios.Add(p);
            await _context.SaveChangesAsync();

            var result = new PortfolioDto
            {
                Id = p.Id,
                ProjectName = p.ProjectName,
                ClientName = p.ClientName,
                TechStack = p.TechStack,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };

            return ApiResponse<PortfolioDto>.SuccessResult(result, "Portfolio created successfully.", statusCode: 201);
        }
        else
        {
            var p = await _context.Portfolios.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (p == null)
                return ApiResponse<PortfolioDto>.FailResult("Portfolio item not found.", statusCode: 404);

            p.ProjectName = dto.ProjectName.Trim();
            p.ClientName = dto.ClientName;
            p.TechStack = dto.TechStack;
            p.Description = dto.Description;
            p.ImageUrl = dto.ImageUrl;
            p.IsActive = dto.IsActive;
            p.UpdatedBy = dto.UpdatedBy;
            p.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new PortfolioDto
            {
                Id = p.Id,
                ProjectName = p.ProjectName,
                ClientName = p.ClientName,
                TechStack = p.TechStack,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };

            return ApiResponse<PortfolioDto>.SuccessResult(result, "Portfolio updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeletePortfolioAsync(int id, int? updatedBy = null)
    {
        var p = await _context.Portfolios.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null)
            return ApiResponse<bool>.FailResult("Portfolio item not found.", statusCode: 404);

        p.IsDeleted = true;
        p.UpdatedBy = updatedBy;
        p.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Portfolio deleted successfully.");
    }
}

// ==========================================
// 5. TEAM MEMBER SERVICE
// ==========================================
public class TeamMemberService : ITeamMemberService
{
    private readonly AppDbContext _context;

    public TeamMemberService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<TeamMemberDto>>> GetTeamMembersAsync(bool activeOnly = false)
    {
        var query = _context.TeamMembers.AsNoTracking();
        if (activeOnly)
            query = query.Where(t => t.IsActive);

        var list = await query
            .OrderBy(t => t.Name)
            .Select(t => new TeamMemberDto
            {
                Id = t.Id,
                Name = t.Name,
                Designation = t.Designation,
                ImageUrl = t.ImageUrl,
                LinkedInUrl = t.LinkedInUrl,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<TeamMemberDto>>.SuccessResult(list, "Team members fetched successfully.");
    }

    public async Task<ApiResponse<TeamMemberDto>> GetTeamMemberByIdAsync(int id)
    {
        var t = await _context.TeamMembers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (t == null)
            return ApiResponse<TeamMemberDto>.FailResult("Team member not found.", statusCode: 404);

        var dto = new TeamMemberDto
        {
            Id = t.Id,
            Name = t.Name,
            Designation = t.Designation,
            ImageUrl = t.ImageUrl,
            LinkedInUrl = t.LinkedInUrl,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };

        return ApiResponse<TeamMemberDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<TeamMemberDto>> SaveTeamMemberAsync(SaveTeamMemberDto dto)
    {
        if (dto.Id <= 0)
        {
            var t = new TeamMember
            {
                Name = dto.Name.Trim(),
                Designation = dto.Designation.Trim(),
                ImageUrl = dto.ImageUrl,
                LinkedInUrl = dto.LinkedInUrl,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TeamMembers.Add(t);
            await _context.SaveChangesAsync();

            var result = new TeamMemberDto
            {
                Id = t.Id,
                Name = t.Name,
                Designation = t.Designation,
                ImageUrl = t.ImageUrl,
                LinkedInUrl = t.LinkedInUrl,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };

            return ApiResponse<TeamMemberDto>.SuccessResult(result, "Team member created successfully.", statusCode: 201);
        }
        else
        {
            var t = await _context.TeamMembers.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (t == null)
                return ApiResponse<TeamMemberDto>.FailResult("Team member not found.", statusCode: 404);

            t.Name = dto.Name.Trim();
            t.Designation = dto.Designation.Trim();
            t.ImageUrl = dto.ImageUrl;
            t.LinkedInUrl = dto.LinkedInUrl;
            t.IsActive = dto.IsActive;
            t.UpdatedBy = dto.UpdatedBy;
            t.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new TeamMemberDto
            {
                Id = t.Id,
                Name = t.Name,
                Designation = t.Designation,
                ImageUrl = t.ImageUrl,
                LinkedInUrl = t.LinkedInUrl,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };

            return ApiResponse<TeamMemberDto>.SuccessResult(result, "Team member updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteTeamMemberAsync(int id, int? updatedBy = null)
    {
        var t = await _context.TeamMembers.FirstOrDefaultAsync(x => x.Id == id);
        if (t == null)
            return ApiResponse<bool>.FailResult("Team member not found.", statusCode: 404);

        t.IsDeleted = true;
        t.UpdatedBy = updatedBy;
        t.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Team member deleted successfully.");
    }
}

// ==========================================
// 6. TESTIMONIAL SERVICE
// ==========================================
public class TestimonialService : ITestimonialService
{
    private readonly AppDbContext _context;

    public TestimonialService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<TestimonialDto>>> GetTestimonialsAsync(bool activeOnly = false)
    {
        var query = _context.Testimonials.AsNoTracking();
        if (activeOnly)
            query = query.Where(t => t.IsActive);

        var list = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TestimonialDto
            {
                Id = t.Id,
                ClientName = t.ClientName,
                CompanyName = t.CompanyName,
                Review = t.Review,
                ImageUrl = t.ImageUrl,
                Rating = t.Rating,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<TestimonialDto>>.SuccessResult(list, "Testimonials fetched successfully.");
    }

    public async Task<ApiResponse<TestimonialDto>> GetTestimonialByIdAsync(int id)
    {
        var t = await _context.Testimonials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (t == null)
            return ApiResponse<TestimonialDto>.FailResult("Testimonial not found.", statusCode: 404);

        var dto = new TestimonialDto
        {
            Id = t.Id,
            ClientName = t.ClientName,
            CompanyName = t.CompanyName,
            Review = t.Review,
            ImageUrl = t.ImageUrl,
            Rating = t.Rating,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };

        return ApiResponse<TestimonialDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<TestimonialDto>> SaveTestimonialAsync(SaveTestimonialDto dto)
    {
        if (dto.Id <= 0)
        {
            var t = new Testimonial
            {
                ClientName = dto.ClientName.Trim(),
                CompanyName = dto.CompanyName,
                Review = dto.Review.Trim(),
                ImageUrl = dto.ImageUrl,
                Rating = dto.Rating ?? 5,
                IsActive = dto.IsActive,
                UpdatedBy = dto.UpdatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Testimonials.Add(t);
            await _context.SaveChangesAsync();

            var result = new TestimonialDto
            {
                Id = t.Id,
                ClientName = t.ClientName,
                CompanyName = t.CompanyName,
                Review = t.Review,
                ImageUrl = t.ImageUrl,
                Rating = t.Rating,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };

            return ApiResponse<TestimonialDto>.SuccessResult(result, "Testimonial created successfully.", statusCode: 201);
        }
        else
        {
            var t = await _context.Testimonials.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (t == null)
                return ApiResponse<TestimonialDto>.FailResult("Testimonial not found.", statusCode: 404);

            t.ClientName = dto.ClientName.Trim();
            t.CompanyName = dto.CompanyName;
            t.Review = dto.Review.Trim();
            t.ImageUrl = dto.ImageUrl;
            t.Rating = dto.Rating ?? 5;
            t.IsActive = dto.IsActive;
            t.UpdatedBy = dto.UpdatedBy;
            t.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new TestimonialDto
            {
                Id = t.Id,
                ClientName = t.ClientName,
                CompanyName = t.CompanyName,
                Review = t.Review,
                ImageUrl = t.ImageUrl,
                Rating = t.Rating,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };

            return ApiResponse<TestimonialDto>.SuccessResult(result, "Testimonial updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteTestimonialAsync(int id, int? updatedBy = null)
    {
        var t = await _context.Testimonials.FirstOrDefaultAsync(x => x.Id == id);
        if (t == null)
            return ApiResponse<bool>.FailResult("Testimonial not found.", statusCode: 404);

        t.IsDeleted = true;
        t.UpdatedBy = updatedBy;
        t.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Testimonial deleted successfully.");
    }
}

// ==========================================
// 7. CONTACT INQUIRY SERVICE
// ==========================================
public class ContactInquiryService : IContactInquiryService
{
    private readonly AppDbContext _context;

    public ContactInquiryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ContactInquiryDto>>> GetInquiriesAsync(bool activeOnly = false)
    {
        var query = _context.ContactInquiries.AsNoTracking();
        if (activeOnly)
            query = query.Where(c => c.IsActive);

        var list = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ContactInquiryDto
            {
                Id = c.Id,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Phone = c.Phone,
                Message = c.Message,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return ApiResponse<List<ContactInquiryDto>>.SuccessResult(list, "Inquiries fetched successfully.");
    }

    public async Task<ApiResponse<ContactInquiryDto>> GetInquiryByIdAsync(int id)
    {
        var c = await _context.ContactInquiries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
            return ApiResponse<ContactInquiryDto>.FailResult("Inquiry not found.", statusCode: 404);

        var dto = new ContactInquiryDto
        {
            Id = c.Id,
            CustomerName = c.CustomerName,
            Email = c.Email,
            Phone = c.Phone,
            Message = c.Message,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };

        return ApiResponse<ContactInquiryDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<ContactInquiryDto>> SaveInquiryAsync(SaveContactInquiryDto dto)
    {
        if (dto.Id <= 0)
        {
            var c = new ContactInquiry
            {
                CustomerName = dto.CustomerName.Trim(),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                Message = dto.Message,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ContactInquiries.Add(c);
            await _context.SaveChangesAsync();

            var result = new ContactInquiryDto
            {
                Id = c.Id,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Phone = c.Phone,
                Message = c.Message,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };

            return ApiResponse<ContactInquiryDto>.SuccessResult(result, "Inquiry submitted successfully.", statusCode: 201);
        }
        else
        {
            var c = await _context.ContactInquiries.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (c == null)
                return ApiResponse<ContactInquiryDto>.FailResult("Inquiry not found.", statusCode: 404);

            c.CustomerName = dto.CustomerName.Trim();
            c.Email = dto.Email?.Trim();
            c.Phone = dto.Phone?.Trim();
            c.Message = dto.Message;
            c.IsActive = dto.IsActive;
            c.UpdatedBy = dto.UpdatedBy;
            c.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new ContactInquiryDto
            {
                Id = c.Id,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Phone = c.Phone,
                Message = c.Message,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };

            return ApiResponse<ContactInquiryDto>.SuccessResult(result, "Inquiry updated successfully.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteInquiryAsync(int id, int? updatedBy = null)
    {
        var c = await _context.ContactInquiries.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
            return ApiResponse<bool>.FailResult("Inquiry not found.", statusCode: 404);

        c.IsDeleted = true;
        c.UpdatedBy = updatedBy;
        c.UpdatedAt = DateTime.UtcNow;

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

    public AuditLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<AuditLogDto>>> GetLogsAsync(int limit = 100)
    {
        var list = await _context.AuditLogs
            .Include(a => a.User)
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.FullName : null,
                ActionName = a.ActionName,
                TableName = a.TableName,
                RecordId = a.RecordId,
                LogDetails = a.LogDetails,
                IPAddress = a.IPAddress,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<AuditLogDto>>.SuccessResult(list, "Audit logs fetched successfully.");
    }

    public async Task<ApiResponse<bool>> CreateLogAsync(CreateAuditLogDto dto)
    {
        var log = new AuditLog
        {
            UserId = dto.UserId,
            ActionName = dto.ActionName,
            TableName = dto.TableName,
            RecordId = dto.RecordId,
            LogDetails = dto.LogDetails,
            IPAddress = dto.IPAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResult(true, "Audit log created successfully.", statusCode: 201);
    }
}
