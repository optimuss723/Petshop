using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Products;

public class DetailModel : PageModel
{
    private readonly IProductClientService _productSvc;
    private readonly IReviewClientService  _reviewSvc;
    private readonly IConfiguration        _config;

    public ProductResponse?      Product       { get; set; }
    public List<ReviewResponse>  Reviews       { get; set; } = [];
    public double                AverageRating => Reviews.Count > 0 ? Reviews.Average(r => r.Rating) : 0;
    public string?               ErrorMessage  { get; set; }
    public string                ApiBaseUrl    => _config["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

    public DetailModel(IProductClientService productSvc, IReviewClientService reviewSvc, IConfiguration config)
    {
        _productSvc = productSvc;
        _reviewSvc  = reviewSvc;
        _config     = config;
    }

    public async Task OnGetAsync(int id)
    {
        var pr = await _productSvc.GetByIdAsync(id);
        if (pr?.Success == true) Product = pr.Data;
        else ErrorMessage = "Sản phẩm không tồn tại.";

        var rr = await _reviewSvc.GetByProductAsync(id);
        Reviews = rr?.Data ?? [];
    }
}

