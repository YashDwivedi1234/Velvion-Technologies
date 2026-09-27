using velvion_API.DTOs.App;
using velvion_API.DTOs.Common;

namespace velvion_API.Services.Interfaces;

public interface IUserService
{
    Task<ApiResponse<List<UserDto>>> GetUsersAsync(bool activeOnly = false);
    Task<ApiResponse<UserDto>> GetUserByIdAsync(int id);
    Task<ApiResponse<UserDto>> SaveUserAsync(SaveUserDto dto);
    Task<ApiResponse<bool>> DeleteUserAsync(int id, int? updatedBy = null);
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto);
}

public interface IPermissionService
{
    Task<ApiResponse<List<RoleMenuPermissionDto>>> GetPermissionsByRoleIdAsync(int roleId);
    Task<ApiResponse<bool>> SaveRolePermissionsAsync(BulkSaveRolePermissionDto dto);
    Task<ApiResponse<List<RoleMenuPermissionDto>>> GetAllPermissionsAsync();
}

public interface IBlogService
{
    Task<ApiResponse<List<BlogDto>>> GetBlogsAsync(bool activeOnly = false);
    Task<ApiResponse<BlogDto>> GetBlogByIdAsync(int id);
    Task<ApiResponse<BlogDto>> SaveBlogAsync(SaveBlogDto dto);
    Task<ApiResponse<bool>> DeleteBlogAsync(int id, int? updatedBy = null);
}

public interface IPortfolioService
{
    Task<ApiResponse<List<PortfolioDto>>> GetPortfoliosAsync(bool activeOnly = false);
    Task<ApiResponse<PortfolioDto>> GetPortfolioByIdAsync(int id);
    Task<ApiResponse<PortfolioDto>> SavePortfolioAsync(SavePortfolioDto dto);
    Task<ApiResponse<bool>> DeletePortfolioAsync(int id, int? updatedBy = null);
}

public interface ITeamMemberService
{
    Task<ApiResponse<List<TeamMemberDto>>> GetTeamMembersAsync(bool activeOnly = false);
    Task<ApiResponse<TeamMemberDto>> GetTeamMemberByIdAsync(int id);
    Task<ApiResponse<TeamMemberDto>> SaveTeamMemberAsync(SaveTeamMemberDto dto);
    Task<ApiResponse<bool>> DeleteTeamMemberAsync(int id, int? updatedBy = null);
}

public interface ITestimonialService
{
    Task<ApiResponse<List<TestimonialDto>>> GetTestimonialsAsync(bool activeOnly = false);
    Task<ApiResponse<TestimonialDto>> GetTestimonialByIdAsync(int id);
    Task<ApiResponse<TestimonialDto>> SaveTestimonialAsync(SaveTestimonialDto dto);
    Task<ApiResponse<bool>> DeleteTestimonialAsync(int id, int? updatedBy = null);
}

public interface IContactInquiryService
{
    Task<ApiResponse<List<ContactInquiryDto>>> GetInquiriesAsync(bool activeOnly = false);
    Task<ApiResponse<ContactInquiryDto>> GetInquiryByIdAsync(int id);
    Task<ApiResponse<ContactInquiryDto>> SaveInquiryAsync(SaveContactInquiryDto dto);
    Task<ApiResponse<bool>> DeleteInquiryAsync(int id, int? updatedBy = null);
}

public interface IAuditLogService
{
    Task<ApiResponse<List<AuditLogDto>>> GetLogsAsync(int limit = 100);
    Task<ApiResponse<bool>> CreateLogAsync(CreateAuditLogDto dto);
}
