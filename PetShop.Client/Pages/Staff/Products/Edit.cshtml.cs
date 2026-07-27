using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.Enums;

namespace PetShop.Client.Pages.Staff.Products;

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

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id.HasValue)
        {
            IsEdit = true;
            var result = await _svc.GetByIdAsync(id.Value);
            if (result?.Data == null) return RedirectToPage("Index");

            var p = result.Data;
            Input = new ProductFormInput
            {
                Id          = p.Id,
                Name        = p.Name,
                Description = p.Description,
                Price       = p.Price,
                Stock       = p.Stock,
                Category    = p.Category,
                ImageUrl    = p.ImageUrl
            };
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        IsEdit = Input.Id > 0;
        if (!ModelState.IsValid) return Page();

        try
        {
            // Xử lý upload ảnh (ưu tiên file upload hơn URL)
            var uploadedUrl = await SaveUploadedImageAsync();
            if (uploadedUrl == null && !string.IsNullOrEmpty(ErrorMessage))
                return Page();

            if (!string.IsNullOrEmpty(uploadedUrl))
                Input.ImageUrl = uploadedUrl;

            if (!Enum.TryParse<ProductCategory>(Input.Category, out var category))
            {
                ErrorMessage = "Vui lòng chọn danh mục hợp lệ.";
                return Page();
            }

            var request = new ProductRequest(
                Input.Name, Input.Description,
                Input.Price, Input.Stock,
                category, Input.ImageUrl);

            if (Input.Id > 0)
            {
                var result = await _svc.UpdateAsync(Input.Id, request);
                if (result?.Success != true) { ErrorMessage = result?.Message; return Page(); }
                TempData["Message"] = "Cập nhật sản phẩm thành công.";
            }
            else
            {
                var result = await _svc.CreateAsync(request);
                if (result?.Success != true) { ErrorMessage = result?.Message; return Page(); }
                TempData["Message"] = "Thêm sản phẩm thành công.";
            }

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save product.");
            ErrorMessage = $"Không thể lưu sản phẩm: {ex.Message}";
            return Page();
        }
    }

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
