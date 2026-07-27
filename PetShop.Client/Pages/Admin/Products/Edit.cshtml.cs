using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.Enums;

namespace PetShop.Client.Pages.Admin.Products;

public class ProductFormInput
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = "";
    public string  Description { get; set; } = "";
    public decimal Price       { get; set; }
    public int     Stock       { get; set; }
    public string  Category    { get; set; } = "";
    public string? ImageUrl    { get; set; }
}

[RequestSizeLimit(10 * 1024 * 1024)]
[RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
public class EditModel : PageModel
{
    private readonly IProductClientService _svc;
    private readonly IWebHostEnvironment   _env;
    private readonly ILogger<EditModel>    _logger;

    [BindProperty] public ProductFormInput Input         { get; set; } = new();
    [BindProperty] public IFormFile?       UploadedImage { get; set; }
    public bool    IsEdit       { get; set; }
    public string? ErrorMessage { get; set; }

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private const long MaxBytes = 5 * 1024 * 1024;

    public EditModel(IProductClientService svc, IWebHostEnvironment env, ILogger<EditModel> logger)
    {
        _svc    = svc;
        _env    = env;
        _logger = logger;
    }

    public IActionResult OnGet(int? id)
        => RedirectToPage("/Admin/Dashboard");

    public IActionResult OnPost()
        => RedirectToPage("/Admin/Dashboard");

    private async Task<string?> SaveUploadedImageAsync()
    {
        if (UploadedImage == null || UploadedImage.Length == 0)
            return null;

        if (UploadedImage.Length > MaxBytes)
        {
            ErrorMessage = "Ảnh không được vượt quá 5MB.";
            return null;
        }

        var extension = Path.GetExtension(UploadedImage.FileName ?? "");
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            ErrorMessage = "Chỉ hỗ trợ ảnh .jpg, .jpeg, .png, .webp hoặc .gif.";
            return null;
        }

        var webRoot = !string.IsNullOrWhiteSpace(_env.WebRootPath)
            ? _env.WebRootPath
            : Path.Combine(_env.ContentRootPath, "wwwroot");

        var uploadDir = Path.Combine(webRoot, "uploads", "products");

        try { Directory.CreateDirectory(uploadDir); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot create upload dir: {Dir}", uploadDir);
            ErrorMessage = "Không thể tạo thư mục lưu ảnh.";
            return null;
        }

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDir, fileName);

        try
        {
            await using var stream = System.IO.File.Create(filePath);
            await UploadedImage.CopyToAsync(stream);
            _logger.LogInformation("Saved product image: {Path}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {Path}", filePath);
            ErrorMessage = $"Không thể lưu file ảnh: {ex.Message}";
            return null;
        }

        return $"/uploads/products/{fileName}";
    }
}
