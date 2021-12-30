namespace MexinamitWorkerBot.Api.Models.ApiLogin;

public class ApiLoginResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}

public class Data
{
    public string token { get; set; }
    public List<string> username { get; set; }
    public List<string> password { get; set; }
}

public static class LoginResponseHandler
{
    public static string HandleResponse(this ApiLoginResponse apiResponse, out string token)
    {
        var results = "";
        if (apiResponse.data.token is not null)
        {
            results = apiResponse.data.token;
            token = $"{apiResponse.data.token}";
            return results;
        }

        if (apiResponse.data.password is { Count: > 0 })
        {
            var passwordResponse = "";
            apiResponse.data.password.ForEach(p => passwordResponse += $"{p}\n");
            results += passwordResponse;
        }

        if (apiResponse.data.username is { Count: > 0 })
        {
            var usernameResponse = "";
            apiResponse.data.username.ForEach(p => usernameResponse += $"{p}\n");
            results += usernameResponse;
        }


        token = "";
        return results;
    }
}
