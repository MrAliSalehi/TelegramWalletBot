namespace MexinamitWorkerBot.Api.Models.ApiRestoreData.ApiForgetPassword;

public class ApiForgetPasswordResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Data
{
    public string result { get; set; }
}