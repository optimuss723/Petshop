using Microsoft.EntityFrameworkCore;
using User.Service.Data;
using User.Service.DTOs;

namespace User.Service.Services;

public interface IUserService
{
    Task<ApiResponse<List<UserResponse>>>    GetAllAsync();
    Task<ApiResponse<UserResponse>>          GetByIdAsync(int id);
    Task<ApiResponse<UserResponse>>          UpdateAsync(int id, UserUpdateRequest request);
    Task<ApiResponse<bool>>                  DeactivateAsync(int id);
    Task<ApiResponse<bool>>                  ActivateAsync(int id);
}

public class UserService : IUserService
{
    private readonly UserDbContext _db;
    public UserService(UserDbContext db) => _db = db;

    public async Task<ApiResponse<List<UserResponse>>> GetAllAsync()
    {
        var users = await _db.Users.ToListAsync();
        return Ok(users.Select(ToResponse).ToList());
    }

    public async Task<ApiResponse<UserResponse>> GetByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return Fail<UserResponse>("Người dùng không tồn tại.");
        return Ok(ToResponse(user));
    }

    public async Task<ApiResponse<UserResponse>> UpdateAsync(int id, UserUpdateRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return Fail<UserResponse>("Người dùng không tồn tại.");

        user.FullName = request.FullName;
        user.Phone    = request.Phone;
        user.Address  = request.Address;
        await _db.SaveChangesAsync();
        return new ApiResponse<UserResponse>(true, "Cập nhật thành công.", ToResponse(user));
    }

    public async Task<ApiResponse<bool>> DeactivateAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return Fail<bool>("Người dùng không tồn tại.");
        user.IsActive = false;
        await _db.SaveChangesAsync();
        return new ApiResponse<bool>(true, "Vô hiệu hoá tài khoản thành công.", true);
    }

    public async Task<ApiResponse<bool>> ActivateAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return Fail<bool>("Người dùng không tồn tại.");
        user.IsActive = true;
        await _db.SaveChangesAsync();
        return new ApiResponse<bool>(true, "Kích hoạt tài khoản thành công.", true);
    }

    private static UserResponse ToResponse(Entities.User u) =>
        new(u.Id, u.FullName, u.Email, u.Phone, u.Address, u.Role, u.IsActive, u.CreatedAt);

    private static ApiResponse<T> Ok<T>(T data)       => new(true, null, data);
    private static ApiResponse<T> Fail<T>(string msg) => new(false, msg, default);
}
