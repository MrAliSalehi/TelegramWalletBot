namespace MexinamitWorkerBot.Api.Models.ApiCreatePayment;

public class ApiCreatePaymentResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }

}
public class Data
{
    public string payment_id { get; set; }
    public int user { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime created_at { get; set; }
    public int id { get; set; }
}