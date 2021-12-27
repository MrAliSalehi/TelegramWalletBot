namespace MexinamitWorkerBot.Api.Models.ApiPmAccountData;

public class ApiPmAccountDataResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Data
{
    public string usd_account { get; set; }
    public string eur_account { get; set; }
}