namespace MexinamitWorkerBot.Api.Models.ApiRegister;

public class ApiRegisterResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Data
{
    public List<string> email { get; set; }
    public List<string> password { get; set; }
    public List<string> has_invitation { get; set; }
    public List<string> link { get; set; }
}

public static class ApiResponseHandler
{
    public static string HandleResponse(this ApiRegisterResponse apiRegister)
    {
        var results = "";
        if (apiRegister.data.password is { Count: > 0 })
        {
            var passwordResponse = "";
           apiRegister.data.password.ForEach(p=>passwordResponse+=$"{p}\n");
           results += passwordResponse;
        }

        if (apiRegister.data.email is { Count: > 0 })
        {
            var emailResponse = "";
            apiRegister.data.email.ForEach(p => emailResponse += $"{p}\n");
            results += emailResponse;
        }

        if (apiRegister.data.has_invitation is { Count: > 0 })
        {
            var hasInvResponse = "";
            apiRegister.data.has_invitation.ForEach(p => hasInvResponse += $"{p}\n");
            results += hasInvResponse;
        }

        if (apiRegister.data.link is {Count : > 0})
        {
            var linkResponse = "";
            apiRegister.data.link.ForEach(p => linkResponse += $"{p}\n");
            results += linkResponse;
        }
        return results;
    }
}