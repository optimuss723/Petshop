using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using PetShop.Repository.Entities;
using PetShop.Repository.UnitOfWork;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.DTOs.Response;

namespace PetShop.API.Services;

// ── TOKEN SERVICE (Singleton — stateless JWT factory) ───────────
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config) => _config = config;

    public string GenerateAccessToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Name,           user.FullName),
            new Claim(ClaimTypes.Role,           user.Role.ToString())
        };
        var token = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
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
            ValidateLifetime         = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = _config["Jwt:Issuer"],
            ValidAudience            = _config["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!))
        };
        try {
            return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
        } catch { return null; }
    }
}

// ── AUTH SERVICE ─────────────────────────────────────────────────
public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork uow, ITokenService tokenService, IMapper mapper, IConfiguration config)
    {
        _uow          = uow;
        _tokenService = tokenService;
        _mapper       = mapper;
        _config       = config;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new ApiResponse<AuthResponse>(false, "Email hoặc mật khẩu không đúng.", null);

        var accessToken  = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:RefreshExpiryDays"]!));
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return new ApiResponse<AuthResponse>(true, "Đăng nhập thành công.",
            new AuthResponse(accessToken, refreshToken, _mapper.Map<UserResponse>(user)));
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        if (await _uow.Users.AnyAsync(u => u.Email == request.Email))
            return new ApiResponse<AuthResponse>(false, "Email đã được sử dụng.", null);

        var user = new User
        {
            FullName     = request.FullName,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone        = request.Phone,
            Address      = request.Address
        };
        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        var accessToken  = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return new ApiResponse<AuthResponse>(true, "Đăng ký thành công.",
            new AuthResponse(accessToken, refreshToken, _mapper.Map<UserResponse>(user)));
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null) return new ApiResponse<AuthResponse>(false, "Token không hợp lệ.", null);

        var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var user  = await _uow.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
            return new ApiResponse<AuthResponse>(false, "Refresh token không hợp lệ hoặc đã hết hạn.", null);

        var newAccess  = _tokenService.GenerateAccessToken(user);
        var newRefresh = _tokenService.GenerateRefreshToken();
        user.RefreshToken       = newRefresh;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return new ApiResponse<AuthResponse>(true, "Làm mới token thành công.",
            new AuthResponse(newAccess, newRefresh, _mapper.Map<UserResponse>(user)));
    }
}
