using Microsoft.AspNetCore.Mvc;
using velvion_API.DTOs.App;
using velvion_API.DTOs.Common;
using velvion_API.Services.Interfaces;

namespace velvion_API.Controllers;

// ==========================================
// 1. PERMISSIONS CONTROLLER
// ==========================================
[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    public PermissionsController(IPermissionService permissionService) => _permissionService = permissionService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleMenuPermissionDto>>>> GetAllPermissions()
    {
        var result = await _permissionService.GetAllPermissionsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("role/{roleId:int}")]
    public async Task<ActionResult<ApiResponse<List<RoleMenuPermissionDto>>>> GetPermissionsByRoleId(int roleId)
    {
        var result = await _permissionService.GetPermissionsByRoleIdAsync(roleId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("bulk-save")]
    public async Task<ActionResult<ApiResponse<bool>>> SaveRolePermissions([FromBody] BulkSaveRolePermissionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<bool>.FailResult("Invalid model state."));
        var result = await _permissionService.SaveRolePermissionsAsync(dto);
        return StatusCode(result.StatusCode, result);
    }
}

// ==========================================
// 2. BLOGS CONTROLLER
// ==========================================
[ApiController]
[Route("api/[controller]")]
public class BlogsController : ControllerBase
{
    private readonly IBlogService _blogService;
    public BlogsController(IBlogService blogService) => _blogService = blogService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BlogDto>>>> GetBlogs([FromQuery] bool activeOnly = false)
    {
        var result = await _blogService.GetBlogsAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BlogDto>>> GetBlogById(int id)
    {
        var result = await _blogService.GetBlogByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("save")]
    public async Task<ActionResult<ApiResponse<BlogDto>>> SaveBlog([FromBody] SaveBlogDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<BlogDto>.FailResult("Invalid model state."));
        var result = await _blogService.SaveBlogAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteBlog(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _blogService.DeleteBlogAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }
}

// ==========================================
// 3. PORTFOLIO CONTROLLER
// ==========================================
[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    public PortfolioController(IPortfolioService portfolioService) => _portfolioService = portfolioService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PortfolioDto>>>> GetPortfolios([FromQuery] bool activeOnly = false)
    {
        var result = await _portfolioService.GetPortfoliosAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PortfolioDto>>> GetPortfolioById(int id)
    {
        var result = await _portfolioService.GetPortfolioByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("save")]
    public async Task<ActionResult<ApiResponse<PortfolioDto>>> SavePortfolio([FromBody] SavePortfolioDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<PortfolioDto>.FailResult("Invalid model state."));
        var result = await _portfolioService.SavePortfolioAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePortfolio(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _portfolioService.DeletePortfolioAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }
}

// ==========================================
// 4. TEAM MEMBERS CONTROLLER
// ==========================================
[ApiController]
[Route("api/[controller]")]
public class TeamMembersController : ControllerBase
{
    private readonly ITeamMemberService _teamMemberService;
    public TeamMembersController(ITeamMemberService teamMemberService) => _teamMemberService = teamMemberService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TeamMemberDto>>>> GetTeamMembers([FromQuery] bool activeOnly = false)
    {
        var result = await _teamMemberService.GetTeamMembersAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TeamMemberDto>>> GetTeamMemberById(int id)
    {
        var result = await _teamMemberService.GetTeamMemberByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("save")]
    public async Task<ActionResult<ApiResponse<TeamMemberDto>>> SaveTeamMember([FromBody] SaveTeamMemberDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<TeamMemberDto>.FailResult("Invalid model state."));
        var result = await _teamMemberService.SaveTeamMemberAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTeamMember(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _teamMemberService.DeleteTeamMemberAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }
}

// ==========================================
// 5. TESTIMONIALS CONTROLLER
// ==========================================
[ApiController]
[Route("api/[controller]")]
public class TestimonialsController : ControllerBase
{
    private readonly ITestimonialService _testimonialService;
    public TestimonialsController(ITestimonialService testimonialService) => _testimonialService = testimonialService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TestimonialDto>>>> GetTestimonials([FromQuery] bool activeOnly = false)
    {
        var result = await _testimonialService.GetTestimonialsAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TestimonialDto>>> GetTestimonialById(int id)
    {
        var result = await _testimonialService.GetTestimonialByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("save")]
    public async Task<ActionResult<ApiResponse<TestimonialDto>>> SaveTestimonial([FromBody] SaveTestimonialDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<TestimonialDto>.FailResult("Invalid model state."));
        var result = await _testimonialService.SaveTestimonialAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTestimonial(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _testimonialService.DeleteTestimonialAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }
}

// ==========================================
// 6. CONTACT INQUIRIES CONTROLLER
// ==========================================
[ApiController]
[Route("api/[controller]")]
public class ContactInquiriesController : ControllerBase
{
    private readonly IContactInquiryService _inquiryService;
    public ContactInquiriesController(IContactInquiryService inquiryService) => _inquiryService = inquiryService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ContactInquiryDto>>>> GetInquiries([FromQuery] bool activeOnly = false)
    {
        var result = await _inquiryService.GetInquiriesAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ContactInquiryDto>>> GetInquiryById(int id)
    {
        var result = await _inquiryService.GetInquiryByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("save")]
    public async Task<ActionResult<ApiResponse<ContactInquiryDto>>> SaveInquiry([FromBody] SaveContactInquiryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<ContactInquiryDto>.FailResult("Invalid model state."));
        var result = await _inquiryService.SaveInquiryAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("resolve/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkResolved(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _inquiryService.MarkResolvedAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteInquiry(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _inquiryService.DeleteInquiryAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }
}

// ==========================================
// 7. AUDIT LOGS CONTROLLER
// ==========================================
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    public AuditLogsController(IAuditLogService auditLogService) => _auditLogService = auditLogService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetLogs([FromQuery] int limit = 100)
    {
        var result = await _auditLogService.GetLogsAsync(limit);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<bool>>> CreateLog([FromBody] CreateAuditLogDto dto)
    {
        var result = await _auditLogService.CreateLogAsync(dto);
        return StatusCode(result.StatusCode, result);
    }
}

// ==========================================
// 8. JOB POSTINGS CONTROLLER — New
// ==========================================
[ApiController]
[Route("api/[controller]")]
public class JobPostingsController : ControllerBase
{
    private readonly IJobPostingService _jobPostingService;
    public JobPostingsController(IJobPostingService jobPostingService) => _jobPostingService = jobPostingService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<JobPostingDto>>>> GetJobPostings([FromQuery] bool activeOnly = false)
    {
        var result = await _jobPostingService.GetJobPostingsAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<JobPostingDto>>> GetJobPostingById(int id)
    {
        var result = await _jobPostingService.GetJobPostingByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("save")]
    public async Task<ActionResult<ApiResponse<JobPostingDto>>> SaveJobPosting([FromBody] SaveJobPostingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<JobPostingDto>.FailResult("Invalid model state."));
        var result = await _jobPostingService.SaveJobPostingAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteJobPosting(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _jobPostingService.DeleteJobPostingAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }
}

// ==========================================
// 9. JOB APPLICATIONS CONTROLLER — New
// ==========================================
[ApiController]
[Route("api/[controller]")]
public class JobApplicationsController : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService;
    public JobApplicationsController(IJobApplicationService jobApplicationService) => _jobApplicationService = jobApplicationService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<JobApplicationDto>>>> GetApplications([FromQuery] int? jobPostingId = null)
    {
        var result = await _jobApplicationService.GetApplicationsAsync(jobPostingId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<JobApplicationDto>>> GetApplicationById(int id)
    {
        var result = await _jobApplicationService.GetApplicationByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("save")]
    public async Task<ActionResult<ApiResponse<JobApplicationDto>>> SaveApplication([FromBody] SaveJobApplicationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<JobApplicationDto>.FailResult("Invalid model state."));
        var result = await _jobApplicationService.SaveApplicationAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("status/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, [FromQuery] string status, [FromQuery] string? notes = null, [FromQuery] int? updatedBy = null)
    {
        var result = await _jobApplicationService.UpdateApplicationStatusAsync(id, status, notes, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteApplication(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _jobApplicationService.DeleteApplicationAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }
}

// ==========================================
// 10. DASHBOARD CONTROLLER — New (Sprint 5)
// ==========================================
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats()
    {
        var result = await _dashboardService.GetDashboardStatsAsync();
        return StatusCode(result.StatusCode, result);
    }
}
