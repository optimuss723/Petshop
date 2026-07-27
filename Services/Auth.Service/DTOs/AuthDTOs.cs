using System.ComponentModel.DataAnnotations;

namespace Auth.Service.DTOs;

// ── REQUEST DTOs ─────────────────────────────────────────────────

/// <summary>Yêu cầu đăng nhập</summary>
public record LoginRequest(
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email,
    
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    string Password);

/// <summary>Yêu cầu đăng ký tài khoản mới</summary>
public record RegisterRequest(
    [Required(ErrorMessage = "Tên đầy đủ không được để trống")]
    [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
    string FullName,
    
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email,
    
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    string Password,
    
    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    string Phone,
    
    string? Address);

/// <summary>Yêu cầu đổi mật khẩu — người dùng đã đăng nhập</summary>
public record ChangePasswordRequest(
    [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
    string CurrentPassword,
    
    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
    string NewPassword);

/// <summary>Yêu cầu làm mới access token</summary>
public record RefreshTokenRequest(
    [Required(ErrorMessage = "Access token không được để trống")]
    string AccessToken,
    
    [Required(ErrorMessage = "Refresh token không được để trống")]
    string RefreshToken);

// ── RESPONSE DTOs ────────────────────────────────────────────────

/// <summary>Phản hồi xác thực, bao gồm tokens và thông tin user</summary>
public record AuthResponse(
    string Token,
    string RefreshToken,
    UserDto User);

/// <summary>Thông tin cơ bản của người dùng trả về sau xác thực</summary>
public record UserDto(
    int Id,
    string FullName,
    string Email,
    string Phone,
    string? Address,
    string Role,
    DateTime CreatedAt);

// ── COMMON WRAPPER ───────────────────────────────────────────────

/// <summary>Wrapper chuẩn cho tất cả API responses</summary>
public record ApiResponse<T>(bool Success, string? Message, T? Data);


