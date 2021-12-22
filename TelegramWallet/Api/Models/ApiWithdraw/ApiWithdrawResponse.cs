namespace TelegramWallet.Api.Models.ApiWithdraw;

public class ApiWithdrawResponse
{
    public string data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}

public class Data
{
    public Dictionary<string,string> data { get; set; }
}

public class Message
{
    public Dictionary<string, string> message { get; set; }
}
public class Status
{
    public Dictionary<string, string> status { get; set; }
}