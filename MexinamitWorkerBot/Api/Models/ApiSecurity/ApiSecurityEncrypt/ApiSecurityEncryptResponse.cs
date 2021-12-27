namespace TelegramWallet.Api.Models.ApiSecurity.ApiSecurityEncrypt;

public class ApiSecurityEncryptResponse
{
    public string data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Data
{
    public List<string> payment { get; set; }
}