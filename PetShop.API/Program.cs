using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using PetShop.API.Hubs;
using PetShop.API.Middleware;
using PetShop.API.Profiles;
using PetShop.API.Services;
using PetShop.Repository.Data;
using PetShop.Repository.Entities;
using PetShop.Repository.Repositories;
using PetShop.Repository.UnitOfWork;
using PetShop.Shared.Validators;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────────
// DATABASE
// ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<PetShopDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// ──────────────────────────────────────────────────────────────
// REPOSITORY + UNIT OF WORK  (Singleton-safe via scoped DbContext)
// ──────────────────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ──────────────────────────────────────────────────────────────
// BUSINESS SERVICES  (Singleton pattern cho stateless services)
// ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,    AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPetService,     PetService>();
builder.Services.AddScoped<IOrderService,   OrderService>();
builder.Services.AddScoped<IUserService,    UserService>();
builder.Services.AddSingleton<ITokenService, TokenService>(); // Singleton vì stateless

// ──────────────────────────────────────────────────────────────
// AUTOMAPPER
// ──────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ──────────────────────────────────────────────────────────────
// FLUENT VALIDATION
// ──────────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();

// ──────────────────────────────────────────────────────────────
// JWT AUTHENTICATION
// ──────────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ──────────────────────────────────────────────────────────────
// ODATA
// ──────────────────────────────────────────────────────────────
var odataBuilder = new ODataConventionModelBuilder();
odataBuilder.EntitySet<Product>("Products");
odataBuilder.EntitySet<Pet>("Pets");
odataBuilder.EntitySet<Order>("Orders");

builder.Services.AddControllers()
    .AddOData(options => options
        .Select().Filter().OrderBy().Expand().Count().SetMaxTop(100)
        .AddRouteComponents("odata", odataBuilder.GetEdmModel()));

// ──────────────────────────────────────────────────────────────
// SIGNALR
// ──────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ──────────────────────────────────────────────────────────────
// CORS
// ──────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("PetShopPolicy", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:5001"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ──────────────────────────────────────────────────────────────
// SWAGGER
// ──────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PetShop API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Nhập: Bearer {token}",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }, []
    }});
});

// ──────────────────────────────────────────────────────────────
// BUILD
// ──────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PetShop API v1"));
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("PetShopPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");

// Auto migrate on startup
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<PetShopDbContext>();
db.Database.Migrate();

app.Run();
