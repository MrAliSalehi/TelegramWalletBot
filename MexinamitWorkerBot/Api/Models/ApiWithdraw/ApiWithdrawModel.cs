namespace MexinamitWorkerBot.Api.Models.ApiWithdraw;

public class ApiWithdrawModel
{

    public string amount { get; set; }
    public string account { get; set; }
    public string gateway { get; set; }
    public string units { get; set; } = "USD";
}