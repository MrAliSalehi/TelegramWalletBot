namespace TelegramWallet.Api.Models.ApiManualGateways;

public class ApiManualGatewaysModel
{
    public string manual_account { get; set; }
    public string account { get; set; }
    public string amount { get; set; }
    public string transaction_id { get; set; }
}