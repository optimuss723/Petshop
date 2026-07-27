using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Staff;

public class AuditLogModel : PageModel
{
    private readonly IOrderClientService _orders;

    public List<AuditLogResponse> Logs { get; private set; } = [];
    public int PageNumber { get; private set; } = 1;
    public string? ErrorMessage { get; private set; }

    public AuditLogModel(IOrderClientService orders) => _orders = orders;

    public async Task OnGetAsync(int page = 1)
    {
        PageNumber = Math.Max(1, page);
        var result = await _orders.GetAuditLogsAsync(PageNumber);

        if (result?.Success == true)
        {
            Logs = result.Data ?? [];
            return;
        }

        ErrorMessage = result?.Message ?? "Khong tai duoc lich su thao tac.";
    }
}
