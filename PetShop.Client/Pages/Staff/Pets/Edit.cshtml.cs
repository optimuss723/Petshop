using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http.Features;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.Enums;

namespace PetShop.Client.Pages.Staff.Pets;

public class PetFormInput
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = "";
    public string  Breed       { get; set; } = "";
    public string  Category    { get; set; } = "";
    public int     AgeMonths   { get; set; }
    public decimal Price       { get; set; }
    public string  Description { get; set; } = "";
    public string? ImageUrl    { get; set; }
    public string  Status      { get; set; } = "Available";
}

[RequestSizeLimit(10 * 1024 * 1024)]
[RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
public class EditModel : PageModel
{
    private readonly IPetClientService _svc;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EditModel> _logger;

    [BindProperty] public PetFormInput Input { get; set; } = new();
    [BindProperty] public IFormFile? UploadedImage { get; set; }
    public bool    IsEdit       { get; set; }
    public string? ErrorMessage { get; set; }

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    public EditModel(IPetClientService svc, IWebHostEnvironment env, ILogger<EditModel> logger)
    {
        _svc = svc;
        _env = env;
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
            Input = new PetFormInput
            {
                Id          = p.Id,
                Name        = p.Name,
                Breed       = p.Breed,
                Category    = p.Category,
                AgeMonths   = p.AgeMonths,
                Price       = p.Price,
                Description = p.Description,
                ImageUrl    = p.ImageUrl,
                Status      = p.Status
            };
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            IsEdit = Input.Id > 0;
            if (!ModelState.IsValid) return Page();

            var imageUrl = await SaveUploadedImageAsync();
            if (imageUrl == null && !string.IsNullOrEmpty(ErrorMessage))
                return Page();

            if (!string.IsNullOrEmpty(imageUrl))
                Input.ImageUrl = imageUrl;

            if (!Enum.TryParse<PetCategory>(Input.Category, out var category))
            {
                ErrorMessage = "Vui lòng chọn loài thú cưng hợp lệ.";
                return Page();
            }

            if (!Enum.TryParse<PetStatus>(Input.Status, out var status))
            {
                ErrorMessage = "Vui lòng chọn trạng thái thú cưng hợp lệ.";
                return Page();
            }

            var request = new PetRequest(
                Input.Name,
                Input.Breed,
                category,
                Input.AgeMonths,
                Input.Price,
                Input.Description,
                Input.ImageUrl,
                status);

            if (Input.Id > 0)
            {
                var result = await _svc.UpdateAsync(Input.Id, request);
                if (result?.Success != true) { ErrorMessage = result?.Message; return Page(); }
                TempData["Message"] = "Cập nhật thú cưng thành công.";
            }
            else
            {
                var result = await _svc.CreateAsync(request);
                if (result?.Success != true) { ErrorMessage = result?.Message; return Page(); }
                TempData["Message"] = "Thêm thú cưng thành công.";
            }

            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save pet from admin form.");
            ErrorMessage = $"Không thể lưu thú cưng: {ex.Message}";
            return Page();
        }
    }

    private async Task<string?> SaveUploadedImageAsync()
    {
        if (UploadedImage == null || UploadedImage.Length == 0)
            return null;

        if (UploadedImage.Length > MaxImageSizeBytes)
        {
            ErrorMessage = "Ảnh thú cưng không được vượt quá 5MB.";
            return null;
        }

        var extension = Path.GetExtension(UploadedImage.FileName ?? "");
        if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
        {
            ErrorMessage = "Chỉ hỗ trợ ảnh .jpg, .jpeg, .png, .webp hoặc .gif.";
            return null;
        }

        // Ưu tiên WebRootPath (ASP.NET cung cấp chính xác),
        // fallback sang ContentRootPath/wwwroot khi chạy từ IDE
        var webRoot = !string.IsNullOrWhiteSpace(_env.WebRootPath)
            ? _env.WebRootPath
            : Path.Combine(_env.ContentRootPath, "wwwroot");

        var uploadDir = Path.Combine(webRoot, "uploads", "pets");

        try
        {
            Directory.CreateDirectory(uploadDir);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tạo thư mục upload: {Dir}", uploadDir);
            ErrorMessage = "Không thể tạo thư mục lưu ảnh. Kiểm tra quyền ghi thư mục wwwroot.";
            return null;
        }

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDir, fileName);

        try
        {
            await using var stream = System.IO.File.Create(filePath);
            await UploadedImage.CopyToAsync(stream);
            _logger.LogInformation("Lưu ảnh Pet thành công: {Path}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu file ảnh: {Path}", filePath);
            ErrorMessage = $"Không thể lưu file ảnh: {ex.Message}";
            return null;
        }

        return $"/uploads/pets/{fileName}";
    }
}
