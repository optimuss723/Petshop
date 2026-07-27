namespace User.Service.DTOs;

// ═══════════════════════════════════════════════════════════════
//  USER REQUEST DTOs
// ═══════════════════════════════════════════════════════════════

/// <summary>Cập nhật thông tin hồ sơ người dùng</summary>
public record UserUpdateRequest(
    string  FullName,
    string  Phone,
    string? Address);

/// <summary>Admin đổi role của user</summary>
public record ChangeRoleRequest(string NewRole);

// ═══════════════════════════════════════════════════════════════
//  USER RESPONSE DTOs
// ═══════════════════════════════════════════════════════════════

/// <summary>Thông tin đầy đủ của người dùng</summary>
public record UserResponse(
    int      Id,
    string   FullName,
    string   Email,
    string   Phone,
    string?  Address,
    string   Role,
    bool     IsActive,
    DateTime CreatedAt);

/// <summary>Thông tin tóm tắt cho danh sách</summary>
public record UserSummaryResponse(
    int    Id,
    string FullName,
    string Email,
    string Role,
    bool   IsActive);

// ═══════════════════════════════════════════════════════════════
//  COMMON
// ═══════════════════════════════════════════════════════════════

/// <summary>Wrapper chuẩn cho API responses của User Service</summary>
public record ApiResponse<T>(bool Success, string? Message, T? Data);
