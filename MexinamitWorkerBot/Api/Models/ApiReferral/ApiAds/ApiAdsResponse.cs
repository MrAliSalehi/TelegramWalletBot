namespace MexinamitWorkerBot.Api.Models.ApiReferral.ApiAds;

public class ApiAdsResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public string status { get; set; }
}

public class Data
{
    public string result { get; set; }
}