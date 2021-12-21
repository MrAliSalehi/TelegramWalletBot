using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using TelegramWallet.Api.Models;
using TelegramWallet.Api.Models.ApiLogin;
using TelegramWallet.Api.Models.ApiReferral.ApiAds;
using TelegramWallet.Api.Models.ApiSummary;
using TelegramWallet.Api.Models.ApiWithdraw;
using TelegramWallet.Classes;
using TelegramWallet.Classes.Extensions;

namespace TelegramWallet.Api;

public class ApiController
{
    //95315888 : 123456
    public async Task<ApiLoginResponse?> LoginAsync(ApiLoginModel loginModel)
    {
        var client = new RestClient(Dependencies.ApiUrl);
        var request = new RestRequest("/login");
        var jsonBody = JsonConvert.SerializeObject(loginModel);
        request.AddHeader("Content-Length", $"{loginModel.ToString()?.Length}");
        request.LoadDefaultHeaders().AddJsonBody(jsonBody);
        var response = await client.PostAsync<JsonArray>(request);
        var serializeResponse = JsonConvert.SerializeObject(response.First());
        var token = JsonConvert.DeserializeObject<ApiLoginResponse>(serializeResponse);
        return token;
    }

    public async Task<ApiWithdrawResponse?> WithdrawAsync(ApiWithdrawModel withdrawModel,string token)
    {
        var client = new RestClient(Dependencies.ApiUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };
        var request = new RestRequest("/profile/wallet/withdraw");
        var jsonBody = JsonConvert.SerializeObject(withdrawModel);
        request.AddHeader("Content-Length", $"{withdrawModel.ToString()?.Length}");
        request.LoadDefaultHeaders().AddJsonBody(jsonBody);
        var response = await client.PostAsync<JsonArray>(request);
        var serializeResponse = JsonConvert.SerializeObject(response.First());
        var finalResponse = JsonConvert.DeserializeObject<ApiWithdrawResponse>(serializeResponse);
        return finalResponse;
    }

    public async Task<ApiInfoResponse?> InfoAsync(string token)
    {
        var client = new RestClient(Dependencies.ApiUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };
        var request = new RestRequest("/profile/info");
        request.LoadDefaultHeaders();
        var response = await client.GetAsync<JsonArray>(request);
        var serializeResponse = JsonConvert.SerializeObject(response.First());
        var finalResponse = JsonConvert.DeserializeObject<ApiInfoResponse>(serializeResponse);
        return finalResponse;
    }

    public async Task<ApiAdsResponse?> BuyReferralAdsAsync(ApiAdsModel adsModel,string token)
    {
        var client = new RestClient(Dependencies.ApiUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };
        var request = new RestRequest("/referral/buy");
        var jsonBody = JsonConvert.SerializeObject(adsModel);
        request.LoadDefaultHeaders().AddJsonBody(jsonBody);
        var response = await client.PostAsync<JsonArray>(request);
        var serializeResponse = JsonConvert.SerializeObject(response.First());
        var finalResponse = JsonConvert.DeserializeObject<ApiAdsResponse>(serializeResponse);
        return finalResponse;
    }

    public async Task<ApiSummaryResponse?> SummaryAsync(string token)
    {
        var client = new RestClient(Dependencies.ApiUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };
        var request = new RestRequest("/profile/summary");
        request.LoadDefaultHeaders();
        var response = await client.GetAsync<JsonArray>(request);
        var serializeResponse = JsonConvert.SerializeObject(response.First());
        var finalResponse = JsonConvert.DeserializeObject<ApiSummaryResponse>(serializeResponse);
        return finalResponse;
    }
}