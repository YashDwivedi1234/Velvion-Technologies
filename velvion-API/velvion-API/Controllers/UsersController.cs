using Microsoft.AspNetCore.Mvc;
using velvion_API.DTOs.App;
using velvion_API.DTOs.Common;
using velvion_API.Services.Interfaces;

namespace velvion_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsers([FromQuery] bool activeOnly = false)
    {
        var result = await _userService.GetUsersAsync(activeOnly);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("save")]
    public async Task<ActionResult<ApiResponse<UserDto>>> SaveUser([FromBody] SaveUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<UserDto>.FailResult("Invalid model state."));

        var result = await _userService.SaveUserAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("delete/{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id, [FromQuery] int? updatedBy = null)
    {
        var result = await _userService.DeleteUserAsync(id, updatedBy);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<LoginResponseDto>.FailResult("Invalid login request."));

        var result = await _userService.LoginAsync(dto);
        return StatusCode(result.StatusCode, result);
    }
}
