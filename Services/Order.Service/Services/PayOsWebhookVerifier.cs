using System.Security.Cryptography;
using System.Text;
using Order.Service.DTOs;

namespace Order.Service.Services;

public interface IPayOsWebhookVerifier
{
    bool Verify(PayOsWebhookRequest request);
}

public class PayOsWebhookVerifier : IPayOsWebhookVerifier
{
    private readonly IConfiguration _config;

    public PayOsWebhookVerifier(IConfiguration config) => _config = config;

    public bool Verify(PayOsWebhookRequest request)
    {
        var checksumKey = _config["PayOS:ChecksumKey"];
        if (string.IsNullOrWhiteSpace(checksumKey)
            || checksumKey == "CHANGE_ME"
            || string.IsNullOrWhiteSpace(request.Signature)
            || request.Data == null)
        {
            return false;
        }

        var signedData = BuildSignedData(request.Data);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedData));
        var calculated = Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(calculated),
            Encoding.UTF8.GetBytes(request.Signature.ToLowerInvariant()));
    }

    private static string BuildSignedData(PayOsWebhookData data)
    {
        var values = new SortedDictionary<string, string?>
        {
            ["accountNumber"] = data.AccountNumber,
            ["amount"] = data.Amount.ToString(),
            ["code"] = data.Code,
            ["counterAccountBankId"] = data.CounterAccountBankId,
            ["counterAccountBankName"] = data.CounterAccountBankName,
            ["counterAccountName"] = data.CounterAccountName,
            ["counterAccountNumber"] = data.CounterAccountNumber,
            ["currency"] = data.Currency,
            ["desc"] = data.Desc,
            ["description"] = data.Description,
            ["orderCode"] = data.OrderCode.ToString(),
            ["paymentLinkId"] = data.PaymentLinkId,
            ["reference"] = data.Reference,
            ["transactionDateTime"] = data.TransactionDateTime,
            ["virtualAccountName"] = data.VirtualAccountName,
            ["virtualAccountNumber"] = data.VirtualAccountNumber
        };

        return string.Join("&", values.Select(kv => $"{kv.Key}={kv.Value ?? string.Empty}"));
    }
}
