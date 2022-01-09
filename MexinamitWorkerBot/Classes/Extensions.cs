using System.Diagnostics;
using MexinamitWorkerBot.Database.Models;
using RestSharp;

namespace MexinamitWorkerBot.Classes;

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

        var canBeInt = double.TryParse(userInput, out var parsedInt);
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
    public static bool TryGetQAndA(this string? userInput, out string question, out string answer)
    {
        var isValidSyntax = true;
        if (string.IsNullOrEmpty(userInput))
        {
            question = "";
            answer = "";
            return false;
        }
        if (!userInput.Contains('?') || !userInput.Contains('#'))
        {
            isValidSyntax = false;
            question = "";
            answer = "";
            return isValidSyntax;
        }
        else
        {

            // ? how.. .?
            // # idk ....
            var getQuestionMarkIndex = userInput.IndexOf('?') + 1;
            var getExclamationMarkIndex = userInput.IndexOf('#') + 1;
            question = userInput.Substring(getQuestionMarkIndex, getExclamationMarkIndex - 2);
            answer = userInput.Substring(getExclamationMarkIndex);
            return isValidSyntax;

        }
    }
    public static string QuestionsToString(this List<Question> questions)
    {
        var questionString = "Your Questions : \n";
        questions.ForEach(p =>
        {
            questionString += $"<b>[ID:{p.Id}]</b>\n-{p.Question1}\n-{p.Answer}\n--------------\n";
        });
        return questionString;
    }
    public static List<string> QuestionNames(this List<Question> questions)
    {
        var results = new List<string>();
        questions.ForEach(p => results.Add($"Q-{p.Id}:{p.Question1}"));
        return results;
    }
    public static void ProcessSubscriptionDetails(this ApiPremiumDetailsResponse apiResponse, out string downloadLimit,
        out string resolutions, out string price, out string watchOn, out string canUseReferral, out string bonus, out string multiLevelPayment)
    {
        downloadLimit = apiResponse.Data.First().Downloads == 0 ? "Unlimited" : $"{apiResponse.Data.First().Downloads} Times";
        var finalRes = "";
        apiResponse.Data.First().MovieResolutions.ForEach(p => finalRes += $"{p.Name},");
        resolutions = finalRes;
        price = apiResponse.Data.First().Price.ToString();
        var finalWatchOn = "";
        apiResponse.Data.First().WatchOn.ForEach(p => finalWatchOn += $"{p},");
        watchOn = finalWatchOn;
        canUseReferral = apiResponse.Data.First().Ads == 1 ? "Unlimitted" : "Limited Referral Service";
        bonus = apiResponse.Data.First().Bonus == 1 ? "With Bonus" : "Without Bonus";
        multiLevelPayment = apiResponse.Data.First().MultiLevelPayment == 1 ? "Multi Level Bonus" : "Bonus Limited";
    }

    public static string ExtractPaymentId(this string mainResponse)
    {
        var firstSplit = mainResponse.Split(' ')[1];
        return firstSplit.Split(' ')[0];

    }
}