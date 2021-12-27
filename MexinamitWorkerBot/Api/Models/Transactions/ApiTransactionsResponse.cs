namespace TelegramWallet.Api.Models.Transactions;

public class ApiTransactionsResponse
{
    public List<Datum> data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Datum
{
    public int id { get; set; }
    public double amount { get; set; }
    public string type { get; set; }
    public int user { get; set; }
    public object? mlp_user { get; set; }
    public int level { get; set; }
    public bool bonus { get; set; }
    public bool referral { get; set; }
    public int referral_count { get; set; }
    public int remaining_referral_count { get; set; }
    public int referral_reward { get; set; }
    public string created_at { get; set; }
    public DateTime updated_at { get; set; }
}