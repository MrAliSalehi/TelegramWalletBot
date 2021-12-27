namespace MexinamitWorkerBot.Api.Models;

public class ApiInfoResponse
{
    public Data data { get; set; }
}

public class Data
{
    public string id { get; set; }
    public string email { get; set; }
    public string username { get; set; }
    public string link { get; set; }
    public string balance { get; set; }
    public string wallet_number { get; set; }
}