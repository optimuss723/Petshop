using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Order.Service.Data;
using Order.Service.Middleware;
using Order.Service.Hubs;
using Order.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// ── DATABASE (Orders + OrderItems) ────────────────────────────
builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// ── SERVICES ──────────────────────────────────────────────────
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMockShippingService, MockShippingService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IPayOsWebhookVerifier, PayOsWebhookVerifier>();
builder.Services.AddHostedService<PendingPayOsOrderCleanupService>();
builder.Services.AddHttpClient<IPayOsPaymentService, PayOsPaymentService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PayOS:BaseUrl"] ?? "https://api-merchant.payos.vn/");
});
builder.Services.AddHttpClient<ICatalogLookupService, CatalogLookupService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CatalogBaseUrl"] ?? "http://localhost:5002");
});

// ── SIGNALR ───────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── JWT ───────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        // Cho phép JWT qua query string cho SignalR
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && ctx.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────
// SignalR yêu cầu AllowCredentials() → phải dùng WithOrigins cụ thể
builder.Services.AddCors(opt => opt.AddPolicy("OrderPolicy", p =>
    p.WithOrigins(
            "http://localhost:50140",
            "https://localhost:50139",
            "http://localhost:5000",
            "http://localhost:5001",
            "http://localhost:5002",
            "http://localhost:5003")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));

// ── CONTROLLERS + SWAGGER ─────────────────────────────────────
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opt =>
    {
        opt.SuppressMapClientErrors = false;
    })
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        opt.JsonSerializerOptions.AllowTrailingCommas = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "📦 Order Service API",
        Version     = "v1",
        Description = "Microservice quản lý đơn hàng + real-time notifications qua SignalR"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }, []
    }});
});

// ── BUILD ─────────────────────────────────────────────────────
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// Swagger UI tại root URL
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service v1");
    c.RoutePrefix = string.Empty; // http://localhost:5003
    c.DocumentTitle = "📦 Order Service API";
});

app.UseCors("OrderPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
db.Database.Migrate();

app.Run();
