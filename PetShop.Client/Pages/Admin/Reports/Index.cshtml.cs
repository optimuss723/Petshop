using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Admin.Reports;

public class IndexModel : PageModel
{
    private readonly IOrderClientService _orderSvc;
    public OrderStatsResponse? Stats { get; set; }
    public string? ErrorMessage { get; set; }

    public IndexModel(IOrderClientService orderSvc) => _orderSvc = orderSvc;

    public async Task OnGetAsync()
    {
        var result = await _orderSvc.GetStatsAsync();
        if (result?.Success == true)
            Stats = result.Data;
        else
            ErrorMessage = result?.Message ?? "Không thể tải thống kê.";
    }

    public async Task<IActionResult> OnGetExportAsync()
    {
        var bytes = await _orderSvc.ExportExcelAsync();
        if (bytes == null || bytes.Length == 0)
            return RedirectToPage(new { error = "Không thể xuất file." });

        var fileName = $"orders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
