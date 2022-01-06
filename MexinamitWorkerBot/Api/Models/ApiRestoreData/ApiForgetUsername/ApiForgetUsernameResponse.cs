namespace MexinamitWorkerBot.Api.Models.ApiRestoreData.ApiForgetUsername;

public class ApiForgetUsernameResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Data
{
    public List<string> email { get; set; }
    public string result { get; set; }

}

public static class ApiForgetUsernameHandler
{
    public static string HandleApiResponse(this ApiForgetUsernameResponse response)
    {
        var results = "";
        if (!string.IsNullOrEmpty(response.data.result))
        {
            results = response.data.result;
            return results;
        }

        if (response.data.email.Count > 0)
        {
            var responseEmail = "";
            response.data.email.ForEach(p => responseEmail += $"{p}\n");
            results = responseEmail;
        }
        return results;
    }
}