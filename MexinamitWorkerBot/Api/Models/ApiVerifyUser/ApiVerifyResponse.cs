namespace MexinamitWorkerBot.Api.Models.ApiVerifyUser;

public class ApiVerifyResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Data
{
    public List<string> username { get; set; }
}