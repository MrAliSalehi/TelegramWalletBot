namespace MexinamitWorkerBot.Api.Models.ApiLogin;

public class ApiLoginResponse
{
    public Data data { get; set; }
    public int status { get; set; }
}

public class Data
{
    public string token { get; set; }
}