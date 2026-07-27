using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Pets;

public class DetailModel : PageModel
{
    private readonly IPetClientService    _petSvc;
    private readonly IReviewClientService _reviewSvc;
    private readonly IConfiguration       _config;

    public PetResponse?         Pet           { get; set; }
    public List<ReviewResponse> Reviews       { get; set; } = [];
    public double               AverageRating => Reviews.Count > 0 ? Reviews.Average(r => r.Rating) : 0;
    public string?              ErrorMessage  { get; set; }
    public string               ApiBaseUrl    => _config["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

    public DetailModel(IPetClientService petSvc, IReviewClientService reviewSvc, IConfiguration config)
    {
        _petSvc    = petSvc;
        _reviewSvc = reviewSvc;
        _config    = config;
    }

    public async Task OnGetAsync(int id)
    {
        var pr = await _petSvc.GetByIdAsync(id);
        if (pr?.Success == true) Pet = pr.Data;
        else ErrorMessage = "Thú cưng không tồn tại.";

        var rr = await _reviewSvc.GetByPetAsync(id);
        Reviews = rr?.Data ?? [];
    }
}

