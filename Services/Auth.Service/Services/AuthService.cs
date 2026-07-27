using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Auth.Service.DTOs;
using Auth.Service.Entities;
using Microsoft.EntityFrameworkCore;
using Auth.Service.Data;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Service.Services;

// ── TOKEN SERVICE ────────────────────────────────────────────────

public interface ITokenService
{
    string           GenerateAccessToken(User user);
    string           GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config) => _config = config;

    public string GenerateAccessToken(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User cannot be null");
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentNullException(nameof(user.Email), "User email cannot be null");
        if (string.IsNullOrWhiteSpace(user.FullName))
            throw new ArgumentNullException(nameof(user.FullName), "User fullname cannot be null");
        if (string.IsNullOrWhiteSpace(user.Role))
            throw new ArgumentNullException(nameof(user.Role), "User role cannot be null");

        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Name,           user.FullName),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim(ClaimTypes.MobilePhone,    user.Phone)
        };
        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = false, // Cho phép token hết hạn
            ValidateIssuerSigningKey = true,
            ValidIssuer              = _config["Jwt:Issuer"],
            ValidAudience            = _config["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!))
        };
        try { return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _); }
        catch { return null; }
    }
}

// ── AUTH SERVICE ─────────────────────────────────────────────────

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken);
    Task<ApiResponse<bool>>         ChangePasswordAsync(int userId, ChangePasswordRequest request);
}

public class AuthService : IAuthService
{
    private readonly AuthDbContext  _db;
    private readonly ITokenService  _tokenSvc;
    private readonly IConfiguration _config;

    public AuthService(AuthDbContext db, ITokenService tokenSvc, IConfiguration config)
    {
        _db       = db;
        _tokenSvc = tokenSvc;
        _config   = config;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
            return Fail<AuthResponse>("Email không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Password))
            return Fail<AuthResponse>("Mật khẩu không được để trống.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Fail<AuthResponse>("Email hoặc mật khẩu không đúng.");

        return await IssueTokens(user, "Đăng nhập thành công.");
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.FullName))
            return Fail<AuthResponse>("Tên đầy đủ không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Email))
            return Fail<AuthResponse>("Email không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Password))
            return Fail<AuthResponse>("Mật khẩu không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Phone))
            return Fail<AuthResponse>("Số điện thoại không được để trống.");

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Fail<AuthResponse>("Email đã được sử dụng.");

        try
        {
            var user = new User
            {
                FullName     = request.FullName.Trim(),
                Email        = request.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Phone        = request.Phone.Trim(),
                Address      = request.Address?.Trim(),
                Role         = "Customer"
            };
            
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Reload user from DB to ensure all properties are populated
            var savedUser = await _db.Users.FindAsync(user.Id);
            if (savedUser == null)
                return Fail<AuthResponse>("Lỗi khi tạo tài khoản. Vui lòng thử lại.");

            return await IssueTokens(savedUser, "Đăng ký thành công.");
        }
        catch (Exception ex)
        {
            return Fail<AuthResponse>($"Lỗi: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var principal = _tokenSvc.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null) return Fail<AuthResponse>("Access token không hợp lệ.");

        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var user  = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Fail<AuthResponse>("Refresh token không hợp lệ hoặc đã hết hạn.");

        return await IssueTokens(user, "Làm mới token thành công.");
    }

    // ── Helpers ──────────────────────────────────────────────────

    private async Task<ApiResponse<AuthResponse>> IssueTokens(User user, string message)
    {
        try
        {
            // Validate user object
            if (user == null)
                return Fail<AuthResponse>("User object is null");
            if (string.IsNullOrWhiteSpace(user.Email))
                return Fail<AuthResponse>("User email is null or empty");
            if (string.IsNullOrWhiteSpace(user.FullName))
                return Fail<AuthResponse>("User fullname is null or empty");
            if (string.IsNullOrWhiteSpace(user.Role))
                return Fail<AuthResponse>("User role is null or empty");
            if (string.IsNullOrWhiteSpace(user.Phone))
                return Fail<AuthResponse>("User phone is null or empty");

            var access  = _tokenSvc.GenerateAccessToken(user);
            var refresh = _tokenSvc.GenerateRefreshToken();
            var days    = double.Parse(_config["Jwt:RefreshExpiryDays"] ?? "7");

            user.RefreshToken       = refresh;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(days);
            await _db.SaveChangesAsync();

            var dto = new UserDto(
                user.Id, 
                user.FullName ?? string.Empty, 
                user.Email ?? string.Empty,
                user.Phone ?? string.Empty, 
                user.Address, 
                user.Role ?? "Customer", 
                user.CreatedAt);
                
            return new ApiResponse<AuthResponse>(true, message, new AuthResponse(access, refresh, dto));
        }
        catch (Exception ex)
        {
            return Fail<AuthResponse>($"Error in IssueTokens: {ex.Message} - {ex.StackTrace}");
        }
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return new ApiResponse<bool>(false, "Người dùng không tồn tại.", false);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return new ApiResponse<bool>(false, "Mật khẩu hiện tại không đúng.", false);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();
        return new ApiResponse<bool>(true, "Đổi mật khẩu thành công.", true);
    }

    private static ApiResponse<T> Fail<T>(string msg) => new(false, msg, default);
}
