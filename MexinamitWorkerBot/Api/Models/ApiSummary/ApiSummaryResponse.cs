namespace TelegramWallet.Api.Models.ApiSummary;

public class ApiSummaryResponse
{
    public string message { get; set; }
    public string status { get; set; }
    public Data data { get; set; }
}

public class Data
{
    public string todayIncome { get; set; }
    public string weekIncome { get; set; }
    public string todayDeposits { get; set; }
    public string weekDeposits { get; set; }
    public string subsetsCount { get; set; }
    public string referralCount { get; set; }
}