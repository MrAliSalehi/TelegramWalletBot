using System.Runtime.InteropServices.ComTypes;
using Newtonsoft.Json;
using RestSharp;
using TelegramWallet.Api.Models;
using TelegramWallet.Database.Models;

namespace TelegramWallet.Classes.Extensions;

public static class Extensions
{
    public static RestRequest LoadDefaultHeaders(this RestRequest request)
    {
        request.AddHeader("Content-Type", $"application/json");
        request.AddHeader("Accept", "application/json");
        return request;
    }

    public static List<string> AdminsNamesToList(this List<Admin> admins) => admins.Select(admin => admin.UserId).ToList(); 
    public static List<string> ChannelNamesToList(this List<ForceJoinChannel> channels) => channels.Select(channel => channel.ChId).ToList(); 
    public static string EscapeUnSupportChars(this string mainString) => mainString.Replace(".", @"\.");
    public static string ReplacePaymentName(this string paymentName) => paymentName switch
    {
        "TUSD Erc20" => "Terra USD Erc20",
        "BUSD Erc20" => "Binance USD Erc20",
        "USDC Erc20" => "USD Coin Erc20",
        "GUSD Erc20" => "Gemini USD Erc20",
        "PUSD Erc20" => "PAX USD Erc20",
        "SUSD Erc20" => "Stable USD Erc20",
        "MUSD Erc20" => "mStable USD Erc20",
        _ => paymentName
    };
    public static bool TryParseAmount(this string userInput, out double parsedAmount)
    {
        if (userInput.Contains("$"))
        {
            userInput = userInput.Replace("$", "");
        }

        var canBeInt =double.TryParse(userInput, out var parsedInt);
        if (canBeInt)
        {
            if (parsedInt >= 10.0)
            {
                parsedAmount = parsedInt;
                return true;
            }
            parsedAmount = 0;
            return false;
        }
        parsedAmount = 0;
        return false;
    }
}