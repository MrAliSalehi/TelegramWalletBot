namespace TelegramWallet.Api.Models.ApiSubscriptions;

public class ApiCheckSubscriptionsResponse
{
    public List<Datum> data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Datum
{
    public double price { get; set; }
    public int mlp { get; set; }
    public int month { get; set; }
    public int downloads { get; set; }
    public List<string> watch_on { get; set; }
    public int ads { get; set; }
    public int bonus { get; set; }
    public int multi_level_payment { get; set; }
    public int lotteries { get; set; }
    public int investment { get; set; }
    public string expiration { get; set; }
}
