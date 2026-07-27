using PetShop.Client.Middleware;
using PetShop.Client.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var logDir = Path.Combine(builder.Environment.ContentRootPath, "logs");
Directory.CreateDirectory(logDir);
var errorLogPath = Path.Combine(logDir, "client-errors.log");

void WriteFatalLog(string title, Exception? exception)
{
    var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {title}{Environment.NewLine}{exception}{Environment.NewLine}";
    File.AppendAllText(errorLogPath, message);
}

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    WriteFatalLog("Unhandled exception", e.ExceptionObject as Exception);

TaskScheduler.UnobservedTaskException += (_, e) =>
{
    WriteFatalLog("Unobserved task exception", e.Exception);
    e.SetObserved();
};

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")))
    .SetApplicationName("PetShop.Client");
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
    options.ValueLengthLimit = 1024 * 1024;
    options.MultipartHeadersLengthLimit = 32 * 1024;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});

var apiBase = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

builder.Services.AddHttpClient("PetShopAPI", client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ── Core Services ──────────────────────────────────────────────
builder.Services.AddScoped<IApiClient,           ApiClient>();
builder.Services.AddScoped<IAuthClientService,   AuthClientService>();
builder.Services.AddScoped<IProductClientService,ProductClientService>();
builder.Services.AddScoped<IPetClientService,    PetClientService>();
builder.Services.AddScoped<IOrderClientService,  OrderClientService>();

// ── New Services (Features #5-#15) ─────────────────────────────
builder.Services.AddScoped<IUserClientService,    UserClientService>();
builder.Services.AddScoped<IReviewClientService,  ReviewClientService>();
builder.Services.AddScoped<ICategoryClientService,CategoryClientService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// ── Chờ ApiGateway sẵn sàng (giải quyết race condition khi chạy VS) ──
// Client tự động retry kết nối đến Gateway mỗi 1 giây, tối đa 60 giây.
// → Chạy F5 trong VS không cần đợi tay, browser vào là được luôn.
if (app.Environment.IsDevelopment())
{
    var gatewayUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    var logger     = app.Logger;

    using var probe = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
    var deadline    = DateTime.UtcNow.AddSeconds(60);
    var ready       = false;

    logger.LogInformation("⏳ Waiting for ApiGateway at {Url}...", gatewayUrl);

    while (DateTime.UtcNow < deadline && !ready)
    {
        try
        {
            var res = await probe.GetAsync(gatewayUrl);
            ready = true; // bất kỳ HTTP response nào (kể cả 404) = Gateway đang chạy
        }
        catch
        {
            await Task.Delay(1000);
        }
    }

    if (ready)
        logger.LogInformation("✅ ApiGateway is ready.");
    else
        logger.LogWarning("⚠️  ApiGateway did not respond within 60s — starting anyway.");
}

app.UseStaticFiles();
app.UseSession();
app.UseMiddleware<AuthMiddleware>();
app.UseRouting();
app.MapRazorPages();

app.Run();
