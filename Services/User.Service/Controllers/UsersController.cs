using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.Service.DTOs;
using User.Service.Services;

namespace User.Service.Controllers;

/// <summary>
/// Quản lý hồ sơ người dùng.
/// - Admin: xem tất cả, vô hiệu hoá tài khoản
/// - User: xem và cập nhật hồ sơ của bản thân
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _svc;
    public UsersController(IUserService svc) => _svc = svc;

    private int    UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string Role   => User.FindFirst(ClaimTypes.Role)!.Value;

    /// <summary>Lấy tất cả user — chỉ Admin</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    /// <summary>Lấy user theo ID — chỉ Admin</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    /// <summary>Lấy hồ sơ bản thân</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe() => Ok(await _svc.GetByIdAsync(UserId));

    /// <summary>Cập nhật hồ sơ — Admin có thể cập nhật bất kỳ, user chỉ cập nhật của mình</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateRequest request)
    {
        if (Role != "Admin" && UserId != id)
            return Forbid();
        return Ok(await _svc.UpdateAsync(id, request));
    }

    /// <summary>Vô hiệu hoá tài khoản — chỉ Admin</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
        => Ok(await _svc.DeactivateAsync(id));

    /// <summary>Kích hoạt lại tài khoản — chỉ Admin (#7)</summary>
    [HttpPatch("{id:int}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(int id)
        => Ok(await _svc.ActivateAsync(id));
}
