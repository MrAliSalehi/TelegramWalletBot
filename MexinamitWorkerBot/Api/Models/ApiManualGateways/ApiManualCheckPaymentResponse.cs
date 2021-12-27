namespace MexinamitWorkerBot.Api.Models.ApiManualGateways;

public class ApiManualCheckPaymentResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Data
{
    public bool verified { get; set; }
    public string order_id { get; set; }
    public string payment_id { get; set; }
    public string amount { get; set; }
}