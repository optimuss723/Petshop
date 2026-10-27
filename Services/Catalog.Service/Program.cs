using System.Text;
using Catalog.Service.Data;
using Catalog.Service.Entities;
using Catalog.Service.Hubs;
using Catalog.Service.Middleware;
using Catalog.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── DATABASE (Products + Pets) ────────────────────────────────
builder.Services.AddDbContext<CatalogDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// ── AUTOMAPPER ────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// ── SERVICES ──────────────────────────────────────────────────
builder.Services.AddScoped<IProductService,  ProductService>();
builder.Services.AddScoped<IPetService,      PetService>();
builder.Services.AddScoped<IReviewService,   ReviewService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddHttpClient<IOrderPurchaseValidator, OrderPurchaseValidator>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderBaseUrl"] ?? "http://localhost:5003");
});

// ── SIGNALR ───────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── JWT (verify only — không phát hành token ở đây) ──────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    });
builder.Services.AddAuthorization();

// ── CORS (SignalR yêu cầu AllowCredentials → WithOrigins cụ thể) ──
builder.Services.AddCors(opt => opt.AddPolicy("CatalogPolicy", p =>
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

// ── CONTROLLERS + ODATA + SWAGGER ─────────────────────────────
var odataBuilder = new ODataConventionModelBuilder();
odataBuilder.EntitySet<Product>("Products");
odataBuilder.EntitySet<Pet>("Pets");

builder.Services.AddControllers(options =>
    {
        // Media Formatter & Content Negotiation — đáp ứng yêu cầu Accept header
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;
    })
    .AddXmlSerializerFormatters()          // Thêm XML input/output formatters
    .ConfigureApiBehaviorOptions(opt =>
    {
        opt.SuppressMapClientErrors = false;
    })
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        opt.JsonSerializerOptions.AllowTrailingCommas = true;
    })
    .AddOData(opt => opt
        .Select()
        .Filter()
        .OrderBy()
        .Count()
        .SetMaxTop(100)
        .AddRouteComponents("odata", odataBuilder.GetEdmModel()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "🐾 Catalog Service API",
        Version     = "v1",
        Description = "Microservice quản lý sản phẩm và thú cưng"
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog Service v1");
    c.RoutePrefix = string.Empty; // http://localhost:5002
    c.DocumentTitle = "🐾 Catalog Service API";
});

app.UseCors("CatalogPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<CatalogHub>("/hubs/catalog");   // ws://localhost:5002/hubs/catalog

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
db.Database.Migrate();

app.Run();
