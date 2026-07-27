using System.Security.Claims;
using Auth.Service.DTOs;
using Auth.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Service.Controllers;

/// <summary>
/// Controller xử lý đăng nhập, đăng ký và refresh token.
/// Đây là điểm duy nhất phát hành JWT cho toàn bộ hệ thống.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Đăng nhập và nhận JWT token</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        return Ok(await _auth.LoginAsync(request));
    }

    /// <summary>Đăng ký tài khoản mới (role Customer)</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Log để debug
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AuthController>>();
        logger.LogInformation("Register request received: FullName={FullName}, Email={Email}, Phone={Phone}, Password={HasPassword}",
            request?.FullName ?? "NULL",
            request?.Email ?? "NULL", 
            request?.Phone ?? "NULL",
            !string.IsNullOrEmpty(request?.Password));

        if (request == null)
        {
            logger.LogWarning("Register request is NULL");
            return BadRequest(new { success = false, message = "Request body is null" });
        }

        if (!ModelState.IsValid)
        {
            logger.LogWarning("ModelState is invalid: {Errors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }
            
        return Ok(await _auth.RegisterAsync(request));
    }

    /// <summary>Làm mới access token bằng refresh token</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        return Ok(await _auth.RefreshTokenAsync(req.AccessToken, req.RefreshToken));
    }

    /// <summary>Đổi mật khẩu — cần đăng nhập (#6)</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(await _auth.ChangePasswordAsync(userId, req));
    }
}
