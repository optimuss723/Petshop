using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── LOAD OCELOT CONFIG ────────────────────────────────────────
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ── JWT (Gateway xác thực token trước khi forward request) ───
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", opt => opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    });

// ── OCELOT ────────────────────────────────────────────────────
builder.Services.AddOcelot();

// ── CORS (Client gọi vào Gateway) ────────────────────────────
builder.Services.AddCors(opt => opt.AddPolicy("GatewayPolicy", p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ── BUILD ─────────────────────────────────────────────────────
var app = builder.Build();

app.UseCors("GatewayPolicy");
app.UseAuthentication();
app.UseAuthorization();

// Ocelot middleware — phải là cuối cùng
await app.UseOcelot();

app.Run();
