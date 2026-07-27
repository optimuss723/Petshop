using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages;

public class IndexModel : PageModel
{
    private readonly IProductClientService _productSvc;
    private readonly IPetClientService     _petSvc;

    // Search/Filter params
    public string?  Search      { get; set; }
    public string?  Category    { get; set; }
    public decimal? MinPrice    { get; set; }
    public decimal? MaxPrice    { get; set; }
    public int      CurrentPage { get; set; } = 1;
    public int      PetPage     { get; set; } = 1;

    // Paginated results (khi đang lọc)
    public List<ProductResponse> Products     { get; set; } = [];
    public List<PetResponse>     Pets         { get; set; } = [];
    public int                   ProductTotal { get; set; }
    public int                   PetTotal     { get; set; }
    public int                   ProductPages { get; set; }
    public int                   PetPages     { get; set; }
    public const int             PageSize     = 9;

    // Sections riêng theo category (khi không lọc)
    public List<ProductResponse> FoodProducts      { get; set; } = [];
    public List<ProductResponse> ToyProducts       { get; set; } = [];
    public List<ProductResponse> AccessoryProducts { get; set; } = [];

    // Đang ở chế độ lọc hay hiển thị section?
    public bool IsFiltering => !string.IsNullOrEmpty(Search)
                            || !string.IsNullOrEmpty(Category)
                            || MinPrice.HasValue
                            || MaxPrice.HasValue;

    public IndexModel(IProductClientService productSvc, IPetClientService petSvc)
    {
        _productSvc = productSvc;
        _petSvc     = petSvc;
    }

    public async Task OnGetAsync(
        string?  search   = null,
        string?  category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int      page     = 1,
        int      petPage  = 1)
    {
        Search      = search;
        Category    = category;
        MinPrice    = minPrice;
        MaxPrice    = maxPrice;
        CurrentPage = page;
        PetPage     = petPage;

        if (IsFiltering)
        {
            // Chế độ tìm kiếm/lọc — hiển thị tất cả gộp chung
            var prodResult = await _productSvc.SearchAsync(search, category, minPrice, maxPrice, page, PageSize);
            if (prodResult?.Data != null)
            {
                Products     = prodResult.Data.Items;
                ProductTotal = prodResult.Data.TotalCount;
                ProductPages = prodResult.Data.TotalPages;
            }
        }
        else
        {
            // Trang chủ mặc định — load 3 section riêng (tối đa 4 mỗi nhóm)
            var allTask = _productSvc.GetAllAsync();
            var petTask = _petSvc.SearchAsync(null, null, null, null, "Available", petPage, PageSize);
            await Task.WhenAll(allTask, petTask);

            var all = allTask.Result?.Data ?? [];
            FoodProducts      = all.Where(p => p.Category == "Food")      .Take(4).ToList();
            ToyProducts       = all.Where(p => p.Category == "Toy")       .Take(4).ToList();
            AccessoryProducts = all.Where(p => p.Category == "Accessory") .Take(4).ToList();

            var petResult = petTask.Result;
            if (petResult?.Data != null)
            {
                Pets     = petResult.Data.Items;
                PetTotal = petResult.Data.TotalCount;
                PetPages = petResult.Data.TotalPages;
            }
        }
    }
}
