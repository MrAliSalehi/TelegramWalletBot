using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using TelegramWallet.Api.Models;
using TelegramWallet.Api.Models.ApiLogin;
using TelegramWallet.Api.Models.ApiReferral.ApiAds;
using TelegramWallet.Api.Models.ApiSubscriptions;
using TelegramWallet.Api.Models.ApiSummary;
using TelegramWallet.Api.Models.ApiWithdraw;
using TelegramWallet.Api.Models.Donate;
using TelegramWallet.Api.Models.Transactions;
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
    public async Task<List<ApiWithdrawResponse>?> WithdrawAsync(ApiWithdrawModel withdrawModel,string token)
    {
        var client = new RestClient(Dependencies.ApiUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };
        var request = new RestRequest("/profile/wallet/withdraw");
        var jsonBody = JsonConvert.SerializeObject(withdrawModel);
        request.AddHeader("Content-Length", $"{withdrawModel.ToString()?.Length}");
        request.LoadDefaultHeaders().AddJsonBody(jsonBody);
        var response = await client.PostAsync<List<Dictionary<string, string>>>(request);
        var serializeResponse = JsonConvert.SerializeObject(response);
        var finalResponse = JsonConvert.DeserializeObject<List<ApiWithdrawResponse>>(serializeResponse);
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
    public async Task<ApiAdsInfoResponse?> GetReferralAdsInfoAsync( string token)
    {
        var client = new RestClient(Dependencies.ApiUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };
        var request = new RestRequest("/referral");
        request.LoadDefaultHeaders();
        var response = await client.GetAsync<JsonArray>(request);
        var serializeResponse = JsonConvert.SerializeObject(response.First());
        var finalResponse = JsonConvert.DeserializeObject<ApiAdsInfoResponse>(serializeResponse);
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
    public async Task<ApiBuySubscriptionsResponse?> BuyPremiumAccountAsync(string token)
    {
        var client = new RestClient(Dependencies.ApiUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };
        var request = new RestRequest("/subscriptions/1/buy");
        request.LoadDefaultHeaders();
        var response = await client.PostAsync<JsonArray>(request);
        var serializeResponse = JsonConvert.SerializeObject(response.First());
        var finalResponse = JsonConvert.DeserializeObject<ApiBuySubscriptionsResponse>(serializeResponse);
        return finalResponse;
    }
    public async Task<ApiPremiumDetailsResponse> PremiumDetailsAsync()
    {
        var client = new RestClient(Dependencies.ApiUrl);
        var request = new RestRequest("/subscriptions");
        request.LoadDefaultHeaders();
        var response = await client.GetAsync<ApiPremiumDetailsResponse>(request);
        return response;
    }
    public async Task<ApiCheckSubscriptionsResponse> CheckSubscriptionsAsync(string token)
    {
        var client = new RestClient(Dependencies.ApiUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };
        var request = new RestRequest("/subscriptions/purchased");
        request.LoadDefaultHeaders();
        var response = await client.GetAsync<ApiCheckSubscriptionsResponse>(request);
        return response;
    }
    public async Task<ApiDonateResponse?> DonateAsync(ApiDonateModel donateModel,string token)
    {
        var client = new RestClient(Dependencies.ApiUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };
        var request = new RestRequest("/profile/donate");
        request.LoadDefaultHeaders().AddJsonBody(JsonConvert.SerializeObject(donateModel));
        var response = await client.PostAsync<ApiDonateResponse>(request);
        return response;
    }

    public async Task<ApiTransactionsResponse> TransactionsAsync(string token)
    {
        var client = new RestClient(Dependencies.ApiUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };
        var request = new RestRequest("/profile/wallet/transactions");
        request.LoadDefaultHeaders();
        var response = await client.GetAsync<ApiTransactionsResponse>(request);
        return response;
    }
}