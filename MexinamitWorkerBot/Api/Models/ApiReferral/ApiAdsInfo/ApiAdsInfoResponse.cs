namespace MexinamitWorkerBot.Api.Models.ApiReferral.ApiAdsInfo;

public class ApiAdsInfoResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }

}

public class Data
{
    public int price { get; set; }
    public int reward { get; set; }
}