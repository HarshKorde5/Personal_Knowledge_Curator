using Microsoft.AspNetCore.Mvc;
using PKC.Application.DTOs;
using PKC.Infrastructure.Services;

namespace PKC.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            var token = await _authService.RegisterAsync(dto);
            return Ok(new { token });
        }
        catch (InvalidOperationException ex)
        {
            // 409 Conflict: The user already exists
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // 400 Bad Request: Missing email or password
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error: Something else went wrong
            return StatusCode(500, new { message = "An unexpected error occurred."+ex });
        }
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            var token = await _authService.LoginAsync(dto);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException ex)
        {
            // This returns a clean 401 Unauthorized with just the message
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // This catches other errors (like database issues) and returns a 400
            return BadRequest(new { message = ex.Message });
        }
    }
}