namespace MexinamitWorkerBot.Api.Models.ApiGateways;

public class ApiGatewaysResponse
{
    public List<Datum> data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class ManualGateways
{
    public int id { get; set; }
    public string name { get; set; }
}

public class Datum
{
    public int manual_gateway { get; set; }
    public string account { get; set; }
    public ManualGateways manual_gateways { get; set; }
}