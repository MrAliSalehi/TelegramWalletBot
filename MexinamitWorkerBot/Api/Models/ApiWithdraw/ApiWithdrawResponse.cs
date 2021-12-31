namespace MexinamitWorkerBot.Api.Models.ApiWithdraw;

public class ApiWithdrawResponse
{
    public Data data { get; set; }
    public string message { get; set; }
    public int status { get; set; }
}
public class Data
{
    public string result { get; set; }
    public List<string> amount { get; set; }
    public List<string> account { get; set; }
    public List<string> gateway { get; set; }
}
public static class WithdrawResponseHandler
{
    public static string HandleApiResponse(this ApiWithdrawResponse apiResponse)
    {
        if (!string.IsNullOrEmpty(apiResponse.data.result))
            return apiResponse.data.result;

        var results = "";
        if (apiResponse.data.account.Count > 0)
        {
            var accountResponse = "";
            apiResponse.data.account.ForEach(p => accountResponse += $"{p}\n");
            results = accountResponse;
        }
        if (apiResponse.data.amount.Count > 0)
        {
            var amountResponse = "";
            apiResponse.data.amount.ForEach(p => amountResponse += $"{p}\n");
            results = amountResponse;
        }
        if (apiResponse.data.gateway.Count > 0)
        {
            var gatewayResponse = "";
            apiResponse.data.gateway.ForEach(p => gatewayResponse += $"{p}\n");
            results = gatewayResponse;
        }
        return results;
    }
}