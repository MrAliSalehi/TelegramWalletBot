namespace TelegramWallet.Api.Models.ApiRegister;

public class ApiRegisterResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Data
{
    public List<string> link { get; set; }
}