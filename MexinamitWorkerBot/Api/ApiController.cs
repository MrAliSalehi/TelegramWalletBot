using JetBrains.Annotations;
using MexinamitWorkerBot.Api.Models;
using MexinamitWorkerBot.Api.Models.ApiCreatePayment;
using MexinamitWorkerBot.Api.Models.ApiDonate;
using MexinamitWorkerBot.Api.Models.ApiGateways;
using MexinamitWorkerBot.Api.Models.ApiLogin;
using MexinamitWorkerBot.Api.Models.ApiManualGateways;
using MexinamitWorkerBot.Api.Models.ApiPmAccountData;
using MexinamitWorkerBot.Api.Models.ApiReferral.ApiAds;
using MexinamitWorkerBot.Api.Models.ApiReferral.ApiAdsInfo;
using MexinamitWorkerBot.Api.Models.ApiRegister;
using MexinamitWorkerBot.Api.Models.ApiRestoreData.ApiForgetPassword;
using MexinamitWorkerBot.Api.Models.ApiRestoreData.ApiForgetUsername;
using MexinamitWorkerBot.Api.Models.ApiSecurity.ApiSecurityEncrypt;
using MexinamitWorkerBot.Api.Models.ApiSubscriptions;
using MexinamitWorkerBot.Api.Models.ApiSummary;
using MexinamitWorkerBot.Api.Models.ApiVerifyUser;
using MexinamitWorkerBot.Api.Models.ApiWithdraw;
using MexinamitWorkerBot.Api.Models.Transactions;
using MexinamitWorkerBot.Classes;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using Serilog;

namespace MexinamitWorkerBot.Api;

public class ApiController
{

    public async Task<ApiLoginResponse?> LoginAsync(ApiLoginModel loginModel)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl);
            var request = new RestRequest("/login");
            var jsonBody = JsonConvert.SerializeObject(loginModel);

            request.LoadDefaultHeaders().AddJsonBody(jsonBody);
            var response = await client.PostAsync<ApiLoginResponse>(request);
            return response;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "LoginAsync");

            return null;
        }

    }
    public async Task<ApiWithdrawResponse?> WithdrawAsync(ApiWithdrawModel withdrawModel, string token)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            var request = new RestRequest("/profile/wallet/withdraw");
            var jsonBody = JsonConvert.SerializeObject(withdrawModel);
            request.AddHeader("Content-Length", $"{withdrawModel.ToString()?.Length}");
            request.LoadDefaultHeaders().AddJsonBody(jsonBody);
            var response = await client.PostAsync<ApiWithdrawResponse>(request);
            var ser = JsonConvert.SerializeObject(response);
            return response;

        }
        catch (Exception exception)
        {

            Log.Error(exception, "WithdrawAsync");
            return null;
        }

    }
    public async Task<ApiInfoResponse?> InfoAsync(string token)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            var request = new RestRequest("/profile/info");
            request.LoadDefaultHeaders();
            var response = await client.GetAsync<ApiInfoResponse>(request);
            return response;
        }
        catch (Exception exception)
        {

            Log.Error(exception, "InfoAsync");
            return null;
        }

    }
    public async Task<ApiAdsResponse?> BuyReferralAdsAsync(ApiAdsModel adsModel, string token)
    {
        try
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
        catch (Exception exception)
        {

            Log.Error(exception, "BuyReferralAdsAsync");
            return null;
        }

    }
    public async Task<ApiAdsInfoResponse?> GetReferralAdsInfoAsync(string token)
    {
        try
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
        catch (Exception exception)
        {

            Log.Error(exception, "GetReferralAdsInfoAsync");
            return null;
        }

    }
    public async Task<ApiSummaryResponse?> SummaryAsync(string token)
    {
        try
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
        catch (Exception exception)
        {

            Log.Error(exception, "SummaryAsync");
            return null;
        }

    }
    public async Task<ApiBuySubscriptionsResponse?> BuyPremiumAccountAsync(string token)
    {
        try
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
        catch (Exception exception)
        {

            Log.Error(exception, "BuyPremiumAccountAsync");
            return null;
        }

    }
    public async Task<ApiPremiumDetailsResponse?> PremiumDetailsAsync()
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl);
            var request = new RestRequest("/subscriptions");
            request.LoadDefaultHeaders();
            var response = await client.GetAsync<ApiPremiumDetailsResponse>(request);
            return response;
        }
        catch (Exception exception)
        {

            Log.Error(exception, "PremiumDetailsAsync");
            return null;
        }

    }
    public async Task<ApiCheckSubscriptionsResponse?> CheckSubscriptionsAsync(string token)
    {
        try
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
        catch (Exception exception)
        {

            Log.Error(exception, "CheckSubscriptionsAsync");
            return null;
        }

    }
    public async Task<ApiDonateResponse?> DonateAsync(ApiDonateModel donateModel, string token)
    {
        try
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
        catch (Exception exception)
        {

            Log.Error(exception, "DonateAsync");
            return null;
        }

    }
    public async Task<ApiTransactionsResponse?> TransactionsAsync(string token)
    {
        try
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
        catch (Exception exception)
        {

            Log.Error(exception, "TransactionsAsync");
            return null;
        }

    }
    public async Task<ApiRegisterResponse?> RegisterUserAsync(ApiRegisterModel registerModel, bool ignoreLink)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl);
            var request = new RestRequest("/register");
            request.LoadDefaultHeaders().AddJsonBody(JsonConvert.SerializeObject(registerModel, new JsonSerializerSettings() { NullValueHandling = ignoreLink ? NullValueHandling.Ignore : NullValueHandling.Include }));
            var response = await client.PostAsync<ApiRegisterResponse>(request);
            return response;
        }
        catch (Exception exception)
        {

            Log.Error(exception, "RegisterUserAsync");
            return null;
        }

    }
    public async Task<ApiGatewaysResponse?> GateWaysListAsync(string token)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            var request = new RestRequest("/gateways/manual-gateways");
            request.LoadDefaultHeaders();
            var response = await client.GetAsync<ApiGatewaysResponse>(request);
            return response;
        }
        catch (Exception exception)
        {

            Log.Error(exception, "GateWaysListAsync");
            return null;
        }
    }
    public async Task<ApiManualGatewaysResponse?> ManualTransactionAsync(ApiManualGatewaysModel model, string token)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            var request = new RestRequest("/payments/manuals/transaction/submit");
            request.LoadDefaultHeaders().AddJsonBody(JsonConvert.SerializeObject(model));
            var response = await client.PostAsync<ApiManualGatewaysResponse>(request);
            return response;
        }
        catch (Exception exception)
        {

            Log.Error(exception, "ManualTransactionAsync ");
            return null;
        }

    }
    public async Task<ApiManualCheckPaymentResponse?> CheckPaymentAsync(ApiManualCheckPaymentModel model, string token)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            var request = new RestRequest("/payments/check");
            request.LoadDefaultHeaders().AddJsonBody(JsonConvert.SerializeObject(model));
            var response = await client.PostAsync<ApiManualCheckPaymentResponse>(request);
            return response;
        }
        catch (Exception exception)
        {

            Log.Error(exception, "CheckPaymentAsync ");
            return null;
        }

    }
    public async Task<ApiPmAccountDataResponse?> PerfectMoneyAccountDetailAsync(string token)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            var request = new RestRequest("/gateways/perfect-money/account");
            request.LoadDefaultHeaders();
            var response = await client.GetAsync<ApiPmAccountDataResponse>(request);
            return response;
        }
        catch (Exception exception)
        {

            Log.Error(exception, "PerfectMoneyAccountDetailAsync ");
            return null;
        }

    }
    public async Task<ApiCreatePaymentResponse?> CreatePaymentAsync(string token)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            var request = new RestRequest("/payments/create");
            request.LoadDefaultHeaders();
            var response = await client.GetAsync<ApiCreatePaymentResponse>(request);
            return response;
        }
        catch (Exception exception)
        {

            Log.Error(exception, "CreatePaymentAsync ");
            return null;
        }

    }
    public async Task<ApiSecurityEncryptResponse?> EncryptionAsync(ApiSecurityEncryptModel model, string token)
    {

        try
        {
            var client = new RestClient(Dependencies.ApiUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };
            var request = new RestRequest("/encrypt");
            request.LoadDefaultHeaders().AddJsonBody(JsonConvert.SerializeObject(model));
            var response = await client.PostAsync<ApiSecurityEncryptResponse>(request);
            return response;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "EncryptionAsync ");
            return null;
        }
    }
    public async Task<ApiVerifyResponse?> TwoStepVerifyAsync(ApiVerifyModel model)
    {

        try
        {
            var client = new RestClient(Dependencies.ApiUrl);
            var request = new RestRequest("/auth/2fa");
            request.LoadDefaultHeaders().AddJsonBody(JsonConvert.SerializeObject(model));
            var response = await client.PostAsync<ApiVerifyResponse>(request);
            return response;
        }
        catch (Exception exception)
        {
            Log.Error(exception, " TwoStepVerifyAsync");
            return null;
        }
    }

    [ItemCanBeNull]
    public async Task<ApiForgetPasswordResponse> ForgetPasswordAsync(ApiForgetPasswordModel model)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl);
            var request = new RestRequest("/auth/password/reset");
            request.LoadDefaultHeaders().AddJsonBody(JsonConvert.SerializeObject(model));
            var response = await client.PostAsync<ApiForgetPasswordResponse>(request);
            return response;
        }
        catch (Exception e)
        {
            Log.Error(e, "ForgetPasswordAsync");
            return null;
        }
    }

    [ItemCanBeNull]
    public async Task<ApiForgetUsernameResponse> ForgetUsernameAsync(ApiForgetUsernameModel model)
    {
        try
        {
            var client = new RestClient(Dependencies.ApiUrl);
            var request = new RestRequest("/auth/username/recover");
            request.LoadDefaultHeaders().AddJsonBody(JsonConvert.SerializeObject(model));
            var response = await client.PostAsync<ApiForgetUsernameResponse>(request);
            return response;
        }
        catch (Exception e)
        {
            Log.Error(e, "ForgetPasswordAsync");
            return null;
        }
    }
}