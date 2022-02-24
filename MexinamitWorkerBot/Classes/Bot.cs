using MexinamitWorkerBot.Api;
using MexinamitWorkerBot.Api.Models.ApiDonate;
using MexinamitWorkerBot.Api.Models.ApiLogin;
using MexinamitWorkerBot.Api.Models.ApiManualGateways;
using MexinamitWorkerBot.Api.Models.ApiReferral.ApiAds;
using MexinamitWorkerBot.Api.Models.ApiRegister;
using MexinamitWorkerBot.Api.Models.ApiRestoreData;
using MexinamitWorkerBot.Api.Models.ApiRestoreData.ApiForgetPassword;
using MexinamitWorkerBot.Api.Models.ApiRestoreData.ApiForgetUsername;
using MexinamitWorkerBot.Api.Models.ApiSecurity.ApiSecurityEncrypt;
using MexinamitWorkerBot.Api.Models.ApiVerifyUser;
using MexinamitWorkerBot.Api.Models.ApiWithdraw;
using MexinamitWorkerBot.Classes.DataBase;
using MexinamitWorkerBot.Database.Models;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static MexinamitWorkerBot.Classes.Dependencies;
using Message = Telegram.Bot.Types.Message;
using User = MexinamitWorkerBot.Database.Models.User;
namespace MexinamitWorkerBot.Classes;
//Todo : fix Language,usage of dic : Dependencies.LangDictionary[UserLang]["Register"]
//Todo : working on forget password
public class Bot : BackgroundService
{

    #region Injection
    private readonly DbController _dbController;
    private readonly ApiController _apiController;
    private readonly AdminController _adminController;
    private readonly ForceJoinController _forceJoinController;
    private readonly QuestionController _questionController;
    private readonly RestoreController _restoreController;
    public Bot()
    {
        _restoreController = new RestoreController();
        _questionController = new QuestionController();
        _forceJoinController = new ForceJoinController();
        _adminController = new AdminController();
        _dbController = new DbController();
        _apiController = new ApiController();
    }

    #endregion

    #region Buttons

    #region Settings

    private static readonly InlineKeyboardMarkup SettingsKeyboardMarkup = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("Change Language","Setting:ChangeLang"),
            InlineKeyboardButton.WithCallbackData("Logout","Setting:Logout"),
        }
    });

    #endregion

    #region Identity

    private static readonly KeyboardButton[][] ButtonsIdentity = new[]
    {
        new[] { new KeyboardButton("Login"), new KeyboardButton("Register")},
        new[] { new KeyboardButton("Forget User Name"),new KeyboardButton("Forget Password") }
    };

    private static readonly ReplyKeyboardMarkup IdentityKeyboardMarkup = new(ButtonsIdentity)
    {
        Keyboard = ButtonsIdentity,
        Selective = false,
        OneTimeKeyboard = false,
        ResizeKeyboard = true
    };
    private static readonly InlineKeyboardMarkup IdentifyKeyboard = new(new[] {
        InlineKeyboardButton.WithCallbackData("Register", "Identity:Register:BeginRegister"),
        InlineKeyboardButton.WithCallbackData("Cancel", "Identity:Register:CancelCleanStep"), });

    private static readonly InlineKeyboardMarkup RegisterKeyboard = new(new[]
    {
        InlineKeyboardButton.WithCallbackData("I Have An Account", "Identity:Register:BeginLogin"),
        InlineKeyboardButton.WithCallbackData("Cancel", "Identity:Register:Back:CancelCleanStep"),
    });
    private static readonly InlineKeyboardMarkup ResetPassKeyboard = new(new[] { InlineKeyboardButton.WithCallbackData("Cancel", "Identity:Register:CancelCleanStep"), });
    #endregion

    #region MainMenu
    private static readonly KeyboardButton[][] ButtonsMainMenu = new[]
    {
        new[] { new KeyboardButton("Statistics"), new KeyboardButton("Wallet"), },
        new[] { new KeyboardButton("Withdraw"),new KeyboardButton("Deposit") },
        new[] { new KeyboardButton("Referral Link"), new KeyboardButton("Referral Ads") },
        new[] {new KeyboardButton("Premium Account")},
        new[] { new KeyboardButton("Questions"), new KeyboardButton("Support"),new KeyboardButton("History") },
        new []{new KeyboardButton("Settings")}
    };

    private static readonly ReplyKeyboardMarkup MainMenuKeyboardMarkup = new(ButtonsMainMenu)
    {
        Keyboard = ButtonsMainMenu,
        Selective = false,
        OneTimeKeyboard = false,
        ResizeKeyboard = true
    };
    #endregion

    #region Admin

    #region Main Admin Keyboard
    private static readonly InlineKeyboardMarkup AdminKeyboardMarkup = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("Admins","Admin:AdminCommands"),
            InlineKeyboardButton.WithCallbackData("Channels","Admin:ChannelCommands"),
            InlineKeyboardButton.WithCallbackData("Questions","Admin:QuestionCommands"),
            InlineKeyboardButton.WithCallbackData("Exit","Admin:Exit"),
        }
    });
    #endregion

    #region Admin Sub Keyboards

    #region Admin
    private static readonly InlineKeyboardMarkup AdminCommandsMarkUp = new(new[] {
        new []
        { InlineKeyboardButton.WithCallbackData("Create Admin","Admin:AdminCommands:Create"),
            InlineKeyboardButton.WithCallbackData("Remove Admin","Admin:AdminCommands:Remove"), },
        new [] { InlineKeyboardButton.WithCallbackData("Admin List","Admin:AdminCommands:List"), },
        new [] { InlineKeyboardButton.WithCallbackData("Back","Admin:AdminCommands:Back:Main"), } });
    #endregion

    #region Channel
    private static readonly InlineKeyboardMarkup ChannelCommandsMarkUp = new(new[] {
        new []
        { InlineKeyboardButton.WithCallbackData("Add Channel","Admin:ChannelCommands:Add"),
            InlineKeyboardButton.WithCallbackData("Remove Channel","Admin:ChannelCommands:Remove"), },
        new [] { InlineKeyboardButton.WithCallbackData("Channel List", "Admin:ChannelCommands:List"), },
        new [] { InlineKeyboardButton.WithCallbackData("Back", "Admin:AdminCommands:Back:Main"), } });
    #endregion

    #region Question

    private static readonly InlineKeyboardMarkup QuestionCommandsMarkUp = new(new[] {
        new []
        { InlineKeyboardButton.WithCallbackData("Add Question","Admin:QuestionCommands:Add"),
            InlineKeyboardButton.WithCallbackData("Remove Question","Admin:QuestionCommands:Remove"), },
        new [] { InlineKeyboardButton.WithCallbackData("Question List", "Admin:QuestionCommands:List:CountrySelect"), },
        new [] { InlineKeyboardButton.WithCallbackData("Back", "Admin:QuestionCommands:Back:QuestionCommands"), } });
    #endregion

    #endregion

    #endregion

    #region Back To Main

    private static readonly KeyboardButton[][] BackToMainMenu = new[] { new[] { new KeyboardButton("Back To Main Menu") } };

    private static readonly ReplyKeyboardMarkup BackToMainMenuKeyboardMarkup = new(BackToMainMenu)
    {
        Keyboard = BackToMainMenu,
        Selective = false,
        OneTimeKeyboard = false,
        ResizeKeyboard = true
    };

    #endregion

    #endregion

    #region Handlers
    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update e, CancellationToken ct)
    {
        switch (e.Type)
        {
            case UpdateType.CallbackQuery when e.CallbackQuery != null:
                var checkJoinCallBackSide = await ForceJoinAsync(bot, e, ct);
                var callbackChatAction = await SendChatActionAsync(bot, e, ct, "callback");
                if (callbackChatAction && checkJoinCallBackSide.Count == 0)
                    await HandleCallBackQueryAsync(bot, e.CallbackQuery, ct);
                else
                {
                    var notJoinedChannels = "";
                    checkJoinCallBackSide.ForEach(p =>
                    {
                        notJoinedChannels += $"@{p.ChId}\n";
                    });
                    await bot.SendTextMessageAsync(e.CallbackQuery.From.Id,
                        $"Dear {e.CallbackQuery.From.Id}, You Did`nt Join In This Channel(s): \n {notJoinedChannels}\n To Use This Bot, Please First Join This Channel(s)",
                        cancellationToken: ct);
                }
                break;
            case UpdateType.Message when e.Message is { Text: { } }:
                var checkJoinMessageSide = await ForceJoinAsync(bot, e, ct);
                var messageChatAction = await SendChatActionAsync(bot, e, ct, "message");

                if (messageChatAction && checkJoinMessageSide.Count == 0)
                    await HandleMessageAsync(bot, e.Message, ct);
                else
                {
                    if (e.Message.From is null) return;
                    var notJoinedChannels = "";
                    checkJoinMessageSide.ForEach(p =>
                    {
                        notJoinedChannels += $"@{p.ChId}\n";
                    });
                    await bot.SendTextMessageAsync(e.Message.From.Id, $"Dear {e.Message.From.Id}, You Did`nt Join In This Channel(s): \n {notJoinedChannels}\n To Use This Bot, Please First Join This Channel(s)", cancellationToken: ct);
                }
                break;
        }
    }
    private async Task<List<ForceJoinChannel>> ForceJoinAsync(ITelegramBotClient bot, Update e, CancellationToken ct)
    {
        var userId = "";
        if (e.CallbackQuery is null)
        {
            if (e.Message?.From != null)
                userId = e.Message.From.Id.ToString();
        }
        else
        {
            userId = e.CallbackQuery.From.Id.ToString();

        }
        var results = new List<ForceJoinChannel>();
        var getChannels = await _forceJoinController.GetChannelsAsync();
        foreach (var channel in getChannels)
        {
            var joinStatus = await bot.GetChatMemberAsync($"@{channel.ChId}", Convert.ToInt64(userId), ct);
            if (!StatusList.Contains(joinStatus.Status.ToString()))
            {
                results.Add(channel);
            }
        }

        return results;
    }
    public async Task HandleCallBackQueryAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct)
    {
        if (e.Data is null) return;
        if (e.Message is null) return;

        var getUser = await _dbController.GetUserAsync(new User() { UserId = e.From.Id.ToString() });
        if (getUser is null)
        {
            await bot.SendTextMessageAsync(e.From.Id, "We Have Trouble With Finding Your Information In Service!\n Please Try Again Latter", cancellationToken: ct);
            return;
        }

        var canParse = Enum.TryParse(getUser.Language!, true, out Languages userLang);
        if (!canParse) userLang = Languages.English;

        try
        {

            #region Select Language
            if (LanguagesList.Contains($"{e.Data}"))
            {
                await bot.DeleteMessageAsync(e.From.Id, e.Message.MessageId, ct);
                Enum.TryParse(e.Data, out Languages lang);
                await _dbController.UpdateUserAsync(new User()
                {
                    UserId = e.From.Id.ToString(),
                    Language = $"{lang}"
                });
                await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[lang]["You Selected:"]} {e.Data}\n{LangDictionary[lang]["Choose an option."]}", replyMarkup: getUser.LoginStep == 3 ? MainMenuKeyboardMarkup : IdentityKeyboardMarkup, cancellationToken: ct);
            }
            #endregion

            #region Premium Account
            if (e.Data.StartsWith("Order"))
            {
                var splitData = e.Data.Split(":");
                var order = splitData[1];
                var chatId = Convert.ToInt32(splitData[2]);
                var buySub = await _apiController.BuyPremiumAccountAsync(getUser.Token ?? "");
                if (buySub is null)
                    await bot.EditMessageTextAsync(chatId, e.Message.MessageId, $"{LangDictionary[userLang]["This Service Is InActive.Please Try Again Latter!"]}", cancellationToken: ct);

                else
                {
                    switch (buySub.status)
                    {
                        case 201:
                        case 200:
                            await bot.EditMessageTextAsync(chatId, e.Message.MessageId, $"{LangDictionary[userLang]["Dear"]} {chatId},{LangDictionary[userLang]["you Ordered "]} {order}.\n {buySub.message}", cancellationToken: ct);
                            break;
                        case 401 or 405:
                            await bot.EditMessageTextAsync(chatId, e.Message.MessageId, $"{LangDictionary[userLang]["Authentication Failed.Please Try To Login Again"]}", cancellationToken: ct);
                            break;
                        default:
                            await bot.EditMessageTextAsync(chatId, e.Message.MessageId, $"{LangDictionary[userLang]["Operation Failed!"]}\n {buySub.message} ", cancellationToken: ct);
                            break;
                    }
                }

            }
            #endregion

            #region Back
            if (e.Data.StartsWith("Back"))
            {
                var splitData = e.Data.Split(":");
                var messageId = Convert.ToInt32(splitData[2]);
                var chatId = splitData[3];
                await bot.DeleteMessageAsync(chatId, messageId, ct);
            }
            #endregion

            #region Support

            if (e.Data.StartsWith("Support"))
            {
                await SupportAreaAsync(bot, e, ct, getUser);
            }

            #endregion

            #region Deposit

            if (e.Data.StartsWith("Deposit"))
            {
                var splitData = e.Data.Split(":");
                var value = splitData[1];
                var chatId = splitData[2];

                #region CustomValue
                if (value == "Custom")
                {
                    await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                    await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), DepositStep = 1 });
                    await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Enter the amount:"]}\n{LangDictionary[userLang]["Minimum amount to deposit 10$"]}", cancellationToken: ct);
                }
                #endregion

                #region Cancel
                else if (value == "Cancel")
                {
                    await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), DepositStep = 0 });
                    await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, $"<b>{LangDictionary[userLang]["Transaction Has Been Canceled"]}</b>", ParseMode.Html, cancellationToken: ct);
                }
                #endregion

                #region Payment Select-NonCustom Value
                else
                {

                    var continueKeyboardMarkup = new InlineKeyboardMarkup(new[] {
                    new [] {
                    InlineKeyboardButton.WithCallbackData("Web Money",$"FinishDeposit:{value}:{chatId}:Web Money"),
                    InlineKeyboardButton.WithCallbackData("Perfect Money",$"FinishDeposit:{value}:{chatId}:Perfect Money"),
                    },

                    new [] {
                        InlineKeyboardButton.WithCallbackData("SUSD Erc20",$"FinishDeposit:{value}:{chatId}:SUSD Erc20"),
                        InlineKeyboardButton.WithCallbackData("HUSD Erc20",$"FinishDeposit:{value}:{chatId}:HUSD Erc20"),
                        InlineKeyboardButton.WithCallbackData("True USD Erc20", $"FinishDeposit:{value}:{chatId}:True USD Erc20") },

                    new []
                    {
                    InlineKeyboardButton.WithCallbackData("Payeer",$"FinishDeposit:{value}:{chatId}:Payeer"),
                    InlineKeyboardButton.WithCallbackData("PUSD Erc20",$"FinishDeposit:{value}:{chatId}:PUSD Erc20"),
                        InlineKeyboardButton.WithCallbackData("MUSD Erc20",$"FinishDeposit:{value}:{chatId}:MUSD Erc20"),
                    },

                    new [] {
                    InlineKeyboardButton.WithCallbackData("USDC Erc20",$"FinishDeposit:{value}:{chatId}:USDC Erc20"),
                    InlineKeyboardButton.WithCallbackData("GUSD Erc20",$"FinishDeposit:{value}:{chatId}:GUSD Erc20"),
                    InlineKeyboardButton.WithCallbackData("BUSD Erc20",$"FinishDeposit:{value}:{chatId}:BUSD Erc20") },
                    new [] {
                    InlineKeyboardButton.WithCallbackData("Tether Trc20", $"FinishDeposit:{value}:{chatId}:Tether Trc20"),
                    InlineKeyboardButton.WithCallbackData("Tether Erc20",$"FinishDeposit:{value}:{chatId}:Tether Erc20"),
                    InlineKeyboardButton.WithCallbackData("DAI Erc20",$"FinishDeposit:{value}:{chatId}:DAI Erc20")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}","Deposit:Cancel:-"),
                    }

                    });
                    await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), DepositAmount = value });
                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"{LangDictionary[userLang]["Select Payment Method:"]}", replyMarkup: continueKeyboardMarkup, cancellationToken: ct);
                }
                #endregion

            }
            if (e.Data.StartsWith("FinishDeposit"))
            {
                await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), DepositStep = 0 });

                var splitData = e.Data.Split(":");
                var value = splitData[1];
                var paymentMethodNameReplace = splitData[3].ReplacePaymentName();
                switch (paymentMethodNameReplace)
                {

                    #region Perfect Money
                    case "PM":
                        var pendingMessagePm = await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Pending Request"]}", cancellationToken: ct);
                        var getAccounts = await _apiController.PerfectMoneyAccountDetailAsync(getUser.Token ?? "");
                        var selectedAccount = value == "USD" ? getAccounts?.data.usd_account ?? "-" : getAccounts?.data.eur_account ?? "-";
                        var cancelPmKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Continue"]}", $"ConfirmSiteAccDeposit:-"),
                            InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}", $"Deposit:Cancel:-"),
                        });
                        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), ManualAccount = selectedAccount });
                        await bot.EditMessageTextAsync(pendingMessagePm.Chat.Id, pendingMessagePm.MessageId, $"{LangDictionary[userLang]["Here Is Our Perfect Money Account Number:"]} \n ```{selectedAccount}```", ParseMode.MarkdownV2, replyMarkup: cancelPmKeyboard, cancellationToken: ct);


                        break;

                    case "Perfect Money":
                        var unitKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("USD",$"FinishDeposit:USD:{value}:PM"),
                            InlineKeyboardButton.WithCallbackData("EUR",$"FinishDeposit:EUR:{value}:PM"),
                        });
                        await bot.SendTextMessageAsync(e.Message.Chat.Id, $"{LangDictionary[userLang]["Please Select Your Payment-Unit:"]}", replyMarkup: unitKeyboard, cancellationToken: ct);
                        break;
                    #endregion

                    #region Manuals - ACTIVE
                    default:
                        var pendingMessage = await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Pending Request"]}", cancellationToken: ct);
                        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), DepositAmount = value });
                        var gateways = await _apiController.GateWaysListAsync(getUser.Token ?? "");
                        var selected = gateways.data.FirstOrDefault(p => p.manual_gateways.name == splitData[3]);
                        if (selected is null)
                            await bot.EditMessageTextAsync(pendingMessage.Chat.Id, pendingMessage.MessageId, $"{LangDictionary[userLang]["This Payment Method Is Not Available.Please Try Again Latter"]}", cancellationToken: ct);
                        else
                        {
                            var cancelKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Continue"]}", $"ConfirmSiteAccDeposit:-"),
                                InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}", $"Deposit:Cancel:-"),
                            });
                            var message =
                                splitData[3] == "Web Money" ?
                                $"{LangDictionary[userLang]["Transfer the amount to the WebMoney account"]} \"{selected.account}\" {LangDictionary[userLang]["And Press Continue"]}\n{LangDictionary[userLang]["Minimum deposit amount 10$"]}"
                                : $"{LangDictionary[userLang]["Enter The"]} {selected.manual_gateways.name} {LangDictionary[userLang]["Account Number:"]}\n{selected.account}";
                            await bot.EditMessageTextAsync(pendingMessage.Chat.Id, pendingMessage.MessageId, message, ParseMode.Html, replyMarkup: cancelKeyboard, cancellationToken: ct);
                            await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), ManualAccount = selected.account });
                        }
                        break;

                        #endregion
                }

            }

            if (e.Data.StartsWith("ConfirmSiteAccDeposit"))
            {
                //DepositConfirmSiteAcc:End:{e.Text}
                var splitData = e.Data.Split(':');
                if (splitData.Length > 2)
                {
                    switch (splitData[1])
                    {
                        case "End":
                            var msg = await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"{LangDictionary[userLang]["Pending Request"]}", cancellationToken: ct);

                            if (getUser.ManualAccount!.StartsWith("U") || getUser.ManualAccount.StartsWith("E"))
                            {
                                var createPayment = await _apiController.CreatePaymentAsync(getUser.Token ?? "");
                                var currency = getUser.ManualAccount.StartsWith("U") ? "USD" : "EUR";
                                var encrypt = await _apiController.EncryptionAsync(new ApiSecurityEncryptModel()
                                {
                                    amount = getUser.DepositAmount ?? "",
                                    currency = currency,
                                    payment = createPayment.data.payment_id
                                }, getUser.Token ?? "");
                                if (encrypt.status is 200 or 201)
                                {
                                    var urlKeyboard = new InlineKeyboardMarkup(new[]
                                    { InlineKeyboardButton.WithUrl($"{LangDictionary[userLang]["Process Payment"]}", $"{PerfectMoneyApiUrl}?key={createPayment.data.payment_id}"), });
                                    await bot.EditMessageTextAsync(msg.Chat.Id, msg.MessageId, $"{LangDictionary[userLang]["Your Payment Has Been Created.Continue With Link:"]}", replyMarkup: urlKeyboard, cancellationToken: ct);
                                }
                                else
                                    await bot.EditMessageTextAsync(msg.Chat.Id, msg.MessageId, $"{LangDictionary[userLang]["Sorry Right Now Our Service Is Not Available"]}", cancellationToken: ct);
                            }
                            else
                            {
                                var paymentStatus = await _apiController.ManualTransactionAsync(new ApiManualGatewaysModel()
                                { amount = getUser.DepositAmount ?? "", account = getUser.DepositAccount ?? "", transaction_id = splitData[2], manual_account = getUser.ManualAccount ?? "" }, getUser.Token ?? "");
                                if (paymentStatus?.status is 200 or 201)
                                {
                                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"{LangDictionary[userLang]["Your Request`s Result:"]}\n{paymentStatus.data.ExtractPaymentId() ?? "Nothing Found"}", cancellationToken: ct);
                                }

                                else
                                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"{LangDictionary[userLang]["We Got Some Problems:"]}\n{paymentStatus?.data}", cancellationToken: ct);

                            }


                            break;
                    }
                }
                else
                {
                    var cancelKeyboard = new InlineKeyboardMarkup(new[] { InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}", $"Deposit:Cancel:-"), });
                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"{LangDictionary[userLang]["Enter the Account Number:"]}\n{LangDictionary[userLang]["Example:"]} Z10101010101", replyMarkup: cancelKeyboard, cancellationToken: ct);
                    await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), DepositStep = 2 });
                }

            }
            #endregion

            #region Withdraw

            if (e.Data.StartsWith("WithDraw"))
            {
                await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                var splitData = e.Data.Split(":");
                var value = splitData[1];
                var chatId = splitData[2];


                switch (value)
                {
                    #region Previous
                    case "Previous":
                        {
                            var continueWithdraw = new InlineKeyboardMarkup(new[]
                            {
                                InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Continue"]}", $"ProcessWithdraw:{getUser.WithDrawAmount}:{getUser.WitchDrawPaymentMethod}"),
                                InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}","WithDraw:Cancel:-"),
                            });
                            await bot.SendTextMessageAsync(e.From.Id,
                                $"{LangDictionary[userLang]["You CheckList Is Ready:"]}\n <b>{LangDictionary[userLang]["Withdraw Amount:"]}</b> <i>{getUser.WithDrawAmount}</i>\n<b>{LangDictionary[userLang]["WithDraw Payment Method:"]}</b><i>{getUser.WitchDrawPaymentMethod}</i>\n<b>{LangDictionary[userLang]["Account Number:"]}</b> <i>{getUser.WithDrawAccount}</i>",
                                ParseMode.Html, cancellationToken: ct, replyMarkup: continueWithdraw);
                            break;
                        }
                    #endregion

                    #region New WithDraw
                    default:
                        {

                            break;
                        }
                    #endregion

                    #region CustomNumber

                    case "CustomNumber":
                        await _dbController.UpdateUserAsync(new User()
                        { UserId = e.From.Id.ToString(), WithDrawStep = 2 });
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Please Enter Your Custom Amount:"]}\n{LangDictionary[userLang]["Take Notice Your Amount Must Be More Than 10$,And Your Message Cannot Contains Any Other Thing"]}\n{LangDictionary[userLang]["Example: 12.43$ or 43.4"]}", cancellationToken: ct);
                        break;

                    #endregion

                    #region Cancel

                    case "Cancel":
                        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), WithDrawStep = 0 });
                        await bot.SendTextMessageAsync(e.From.Id, $"<b>{LangDictionary[userLang]["Transaction Has Been Canceled"]}</b>", ParseMode.Html, cancellationToken: ct);
                        break;
                        #endregion
                }

            }

            if (e.Data.StartsWith("FinishWithDraw"))
            {
                await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                var splitData = e.Data.Split(":");
                var value = splitData[1];
                var paymentMethod = splitData[3];


                switch (paymentMethod)
                {
                    #region PerfectMoney
                    case "Perfect Money":
                        var unitKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("USD",$"FinishWithDraw:{splitData[2]}:{value}:PerfectMoneyUSD"),
                            InlineKeyboardButton.WithCallbackData("EUR",$"FinishWithDraw:{splitData[2]}:{value}:PerfectMoneyEUR"),
                        });
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Please Select One Amount-Unit To Process Your Order:"]}", cancellationToken: ct, replyMarkup: unitKeyboard);
                        break;
                    #endregion

                    #region PerfectMoney
                    case "PerfectMoneyUSD" or "PerfectMoneyEUR":
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Now Please Send Your Account Number:"]}\n{LangDictionary[userLang]["Your Account Number Is Depend On Your Previous Selection"]}",
                            cancellationToken: ct);
                        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), WithDrawAmount = value, WithDrawStep = 4, WitchDrawPaymentMethod = paymentMethod });
                        break;
                    #endregion

                    #region Others:
                    default:
                        var accName = paymentMethod == "WebMoney" ? "WMZ" : "Account Number";
                        await bot.SendTextMessageAsync(e.From.Id, $"<b><i>Please Send Your {accName} </i></b>", ParseMode.Html, cancellationToken: ct);
                        await _dbController.UpdateUserAsync(new User()
                        {
                            UserId = e.From.Id.ToString(),
                            WithDrawStep = 1,
                            WitchDrawPaymentMethod = paymentMethod,
                            WithDrawAmount = value,
                        });
                        break;
                        #endregion

                }
            }

            if (e.Data.StartsWith("ProcessWithdraw"))
            {
                await _dbController.UpdateUserAsync(new User()
                {
                    UserId = e.From.Id.ToString(),
                    WithDrawStep = 0,

                });
                var results = await _apiController.WithdrawAsync(new ApiWithdrawModel() { account = getUser.WithDrawAccount ?? "", amount = getUser.WithDrawAmount ?? "", gateway = getUser.WitchDrawPaymentMethod ?? "", }, getUser.Token ?? "");


                await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                    $"{LangDictionary[userLang]["Your Request`s Result:"]} \n<b>{results.HandleApiResponse()}.</b>", ParseMode.Html, cancellationToken: ct);
            }
            #endregion

            #region Referral

            if (e.Data.StartsWith("Ref"))
                await ReferralAreaAsync(bot, e, ct, getUser);


            #endregion

            #region Admin

            if (e.Data.StartsWith("Admin"))
                await AdminAreaAsync(bot, e, ct);


            #endregion

            #region Setting

            if (e.Data.StartsWith("Setting"))
                await SettingAreaAsync(bot, e, ct, getUser);


            #endregion

            #region Register

            if (e.Data.StartsWith("Identity"))
                await IdentityAreaAsync(bot, e, ct, getUser);


            #endregion
        }
        catch (Exception exception)
        {
            await SendExToAdminAsync(exception, bot, ct);
            Log.Error(exception, "HandleCallBackQueryAsync");
            Console.WriteLine(exception);
            await bot.SendTextMessageAsync(e.From.Id, "We Got Some Problems \nPlease Wait Until We Fix It!\nIf Problem Still Resist Please Contact To Our Support Service!", cancellationToken: ct);
            await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), WithDrawStep = 0, DepositStep = 0 });
        }
    }
    public async Task HandleMessageAsync(ITelegramBotClient bot, Message e, CancellationToken ct)
    {

        try
        {
            if (e.Text is null or "" || e.From is null) return;

            #region Insert Db

            await _dbController.InsertNewUserAsync(new User()
            {
                UserId = e.Chat.Id.ToString(),
                Language = "English",
                LoginStep = 0,
                WithDrawAccount = "0",
                UserPass = "",
                WithDrawStep = 0
            });

            #endregion

            var getUser = await _dbController.GetUserAsync(new User() { UserId = e.Chat.Id.ToString() });
            if (getUser is null)
            {
                await bot.SendTextMessageAsync(e.From.Id, "Sorry We Have Some Issues With Identify You,Please Try Again Latter", cancellationToken: ct);
                return;
            }

            var canParse = Enum.TryParse(getUser.Language!, true, out Languages userLang);
            if (!canParse) userLang = Languages.English;

            #region Registeration Area

            switch (getUser.LoginStep)
            {
                #region Login

                case 1:
                    {
                        if (!e.Text.Contains(':'))
                        {
                            var getPassKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Forget Password"]}","Identity:Register:ForgetPass"),
                                InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Back"]}","Identity:Register:CancelCleanStep"),
                            });
                            await bot.SendTextMessageAsync(e.Chat.Id,
                                $"{LangDictionary[userLang]["Dear"]} {e.Text}\n{LangDictionary[userLang]["Please enter the Password:"]}", replyMarkup: getPassKeyboard, cancellationToken: ct);
                            await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 2, UserPass = $"{e.Text}" });
                        }
                        else
                            await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Your User Name Cannot Contains(:)!"]}", cancellationToken: ct);
                        break;
                    }

                case 2:
                    if (!e.Text.Contains(':'))
                    {
                        var checkTwoFactor = await _apiController.TwoStepVerifyAsync(new ApiVerifyModel() { username = getUser.UserPass, password = e.Text ?? "-" });
                        switch (checkTwoFactor?.status)
                        {
                            //201 : no factor , 404 : no need for factor , 403: user suspend

                            #region No-Need-For-TwoFactor
                            case 404:
                                {
                                    var loginResponse = await _apiController.LoginAsync(new ApiLoginModel() { username = getUser.UserPass, password = e.Text });
                                    if (loginResponse?.status is 200 or 201)
                                    {
                                        await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 3, Token = loginResponse.data.token });
                                        await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["You Are In Main Menu:"]}", replyMarkup: MainMenuKeyboardMarkup, cancellationToken: ct);
                                    }
                                    else
                                    {
                                        await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 0 });
                                        await bot.SendTextMessageAsync(e.Chat.Id,
                                            $"{LangDictionary[userLang]["Wrong User Or Password,Please Try Again:"]}",
                                            replyMarkup: IdentityKeyboardMarkup, cancellationToken: ct);
                                    }
                                    break;
                                }
                            #endregion

                            #region Need 2 Factor
                            case 201 or 200:
                                await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 7, UserPass = $"{getUser.UserPass}:{e.Text}" });
                                await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Your Account Has Two Step Verification. We Send A Code To Your Email.Please Enter It Here:"]}", cancellationToken: ct);
                                break;
                            #endregion

                            #region Suspend-405
                            case 405:
                                await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 0 });
                                await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Your Account Has Been Suspend!"]}", cancellationToken: ct);
                                break;
                            #endregion

                            #region Defualt

                            default:
                                await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 0 });
                                var textOutPut = checkTwoFactor?.status == 422 ? $"{LangDictionary[userLang]["Wrong User Or Password"]}" : $"{LangDictionary[userLang]["Sorry We Got Some Problems.PLease Come Back Latter!"]}";
                                await bot.SendTextMessageAsync(e.From.Id, $"<b>{textOutPut}</b>", ParseMode.Html, cancellationToken: ct);
                                break;

                                #endregion

                        }
                    }
                    else
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Your Password Cannot Contains (:)!"]}", cancellationToken: ct);
                    break;

                #region TwoStepVerify
                case 7:
                    var splitUserPass = getUser.UserPass?.Split(':');
                    var loginPendingMessage = await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["Pending Request"]}", cancellationToken: ct);
                    var loginWithTwoStep = await _apiController.LoginAsync(new ApiLoginModel()
                    {
                        username = splitUserPass?[0] ?? "",
                        password = splitUserPass?[1] ?? "",
                        code = e.Text ?? ""
                    });
                    var handleResponse = loginWithTwoStep.HandleResponse(out var token);
                    if (!string.IsNullOrEmpty(token))
                    {
                        await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 3 });
                        await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["You Are In Main Menu:"]}", replyMarkup: MainMenuKeyboardMarkup, cancellationToken: ct);
                    }
                    else
                    {
                        await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 0 });
                        await bot.EditMessageTextAsync(loginPendingMessage.Chat.Id, loginPendingMessage.MessageId, $"{LangDictionary[userLang]["Your Login Results:"]}\n{handleResponse}", cancellationToken: ct);
                    }
                    break;
                #endregion

                #endregion

                #region RegisteredUsers

                case 3:
                    await RegisteredAreaAsync(bot, e, ct, getUser);
                    break;

                #endregion

                #region Register

                #region Getting Password

                case 4:
                    if (!e.Text.Contains(':'))
                    {
                        var setPassKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Back"]}","Identity:Register:CancelCleanStep"),
                        });
                        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 5, UserPass = $"{e.Text}" });
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Please enter a strong Password:"]}", replyMarkup: setPassKeyboard, cancellationToken: ct);
                    }
                    else
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Your Email Cannot Contains(:)!"]}", cancellationToken: ct);

                    break;

                #endregion

                #region Get Link-optional

                case 5:
                    if (!e.Text.Contains(':'))
                    {
                        var continueRegister = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Continue"]}", "Identity:Register:ContinueWithOutLink"),
                        });
                        await _dbController.UpdateUserAsync(new User()
                        {
                            UserId = e.From.Id.ToString(),
                            LoginStep = 6,
                            UserPass = $"{getUser.UserPass}:{e.Text}"
                        });
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Please enter the referral code or press Continue"]}", replyMarkup: continueRegister, cancellationToken: ct);
                    }
                    else
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Your Password Cannot Contains (:)!"]}", cancellationToken: ct);

                    break;

                #endregion

                #region Finish Register With Link

                case 6:
                    if (!e.Text.Contains(':'))
                    {
                        await _dbController.UpdateUserAsync(new User()
                        { UserId = e.From.Id.ToString(), LoginStep = 0 });
                        var pendingMessage = await bot.SendTextMessageAsync(e.From.Id,
                            "<i>Please Wait A Second While We Processing Your Request...</i>", ParseMode.Html,
                            cancellationToken: ct);
                        if (getUser.UserPass is null or "")
                            await bot.SendTextMessageAsync(e.From.Id, "We Have problem With Storing Your Data \n Please Come Back Latter", cancellationToken: ct);
                        else
                        {
                            var emailPass = getUser.UserPass.Split(':');
                            var response = await _apiController.RegisterUserAsync(new ApiRegisterModel() { link = e.Text, has_invitation = "1", email = emailPass[0], password = emailPass[1] }, false);
                            await bot.EditMessageTextAsync(pendingMessage.Chat.Id, pendingMessage.MessageId, $"{LangDictionary[userLang]["Your Request`s Result:"]}\n{response.HandleResponse()}", cancellationToken: ct);
                        }
                    }
                    else
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Your Password Cannot Contains (:)!"]}", cancellationToken: ct);

                    break;

                #endregion

                #endregion

                #region ForgetPassword

                case 8:
                    //got email here - request for userName
                    if (e.Text.Length > 25)
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Sorry But Your Email Is Too Long"]}", cancellationToken: ct);
                    else
                    {
                        var pendingUpdate = await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Pending Request"]}", cancellationToken: ct);
                        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 9 });
                        await _restoreController.NewForgetPasswordAsync(new RestoreData() { UserId = e.From.Id.ToString(), Email = e.Text ?? "-" });
                        await bot.EditMessageTextAsync(pendingUpdate.Chat.Id, pendingUpdate.MessageId, $"{LangDictionary[userLang]["Please Enter Your UserName Now:"]}", cancellationToken: ct);
                    }
                    break;

                case 9:
                    //Got UserName- Send Request To Api
                    if (e.Text.Length > 25)
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Sorry But Your UserName Is Too Long"]}", cancellationToken: ct);
                    else
                    {
                        var requestInfo = await _restoreController.GetRequestAsync(e.From.Id);
                        var pendingUpdate = await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Pending Request"]}", cancellationToken: ct);
                        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 0 });
                        await _restoreController.UpdateForgetPassRequestAsync(new RestoreData() { UserId = e.From.Id.ToString(), UserName = e.Text ?? "-" });
                        var forgetPassRequest = await _apiController.ForgetPasswordAsync(new ApiForgetPasswordModel() { email = requestInfo.Email, username = e.Text });
                        await _restoreController.DeleteRequestAsync(new RestoreData() { UserId = e.From.Id.ToString() });
                        await bot.EditMessageTextAsync(pendingUpdate.Chat.Id, pendingUpdate.MessageId, $"{LangDictionary[userLang]["Your Request`s Result:"]} {forgetPassRequest?.data.result ?? "-"}", cancellationToken: ct);
                    }
                    break;
                #endregion

                #region ForgetUserName

                case 10:
                    //got email - send req to api 
                    var resetUsernameRequest = await _apiController.ForgetUsernameAsync(new ApiForgetUsernameModel() { email = e.Text ?? "-" });
                    await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Your Request`s Result:"]} \n{resetUsernameRequest.HandleApiResponse()}", cancellationToken: ct);
                    await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 0 });
                    break;

                    #endregion
            }

            #endregion

            #region Public Messages

            switch (e.Text)
            {
                #region Start

                case "/start":
                    var keyBoardMarkup = new InlineKeyboardMarkup(CreateInlineButton(LanguagesList));
                    await bot.SendTextMessageAsync(e.Chat.Id, "Please select a language:", replyMarkup: keyBoardMarkup, cancellationToken: ct);
                    break;

                #endregion

                #region Register

                case "Register":
                    await RegisterUserAsync(bot, e, ct);
                    break;

                #endregion

                #region ForgetPassword
                case "Forget Password":
                    await ForgetPasswordAsync(bot, e, ct);
                    break;
                #endregion

                #region Login

                case "Login":
                    await LoginUserAsync(bot, e, ct);
                    break;

                #endregion

                #region ReStoring Data-NextUpdate

                case "Forget User Name":
                    var resetUserNameKeyboard = new InlineKeyboardMarkup(new[] { InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}", "Identity:Register:CancelCleanStep"), });
                    await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 10 });
                    await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Enter Your Email Address:"]}", replyMarkup: resetUserNameKeyboard, cancellationToken: ct);
                    break;

                    #endregion

            }

            #endregion

            #region Main-Owner
            /*second one is project owner*/
            if (e is { From.Id: 1127927726 or 1222521875 })
            {
                if (e.Text.StartsWith("/admin"))
                {
                    var adminId = e.Text.Split(' ')[1];
                    var getAdminDetails = await _dbController.GetUserAsync(new User() { UserId = adminId });

                    if (getAdminDetails is null)
                        await bot.SendTextMessageAsync(e.From.Id, "User Not Found", cancellationToken: ct);
                    else
                        await _adminController.AddOwnerAsync(new Admin() { UserId = adminId, CurrentQuestionLanguage = getAdminDetails.Language });
                }
                else if (e.Text.StartsWith("/logOutAll"))
                {
                    var getIdList = await _dbController.GetAllUserIdAsync();
                    if (getIdList is not null)
                        foreach (var id in getIdList)
                        {
                            await _dbController.LogoutAsync(id);
                            await bot.SendTextMessageAsync(id, $"Dear {id}\nBot Has Been Updated!\nYou LoggedOut From System.\nPlease Press /start And Try To Login Again!", cancellationToken: ct);
                        }
                    await bot.SendTextMessageAsync(e.From.Id, "Done", cancellationToken: ct);

                }
                else if (e.Text.StartsWith("/notifUpdate"))
                {
                    var getMessage = e.Text.Split("/notifUpdate");
                    if (getMessage[1] is null or "")
                    {
                        await bot.SendTextMessageAsync(e.From.Id, "Please Enter Some Message !! \n/notifUpdate hello all users..", cancellationToken: ct);
                    }
                    else
                    {
                        var pending = await bot.SendTextMessageAsync(e.From.Id, "Sending...", cancellationToken: ct);
                        var getIdList = await _dbController.GetAllUserIdAsync();
                        if (getIdList is not null)
                            foreach (var id in getIdList)
                            {
                                await bot.SendTextMessageAsync(id, getMessage[1], cancellationToken: ct);
                            }

                        await bot.EditMessageTextAsync(pending.Chat.Id, pending.MessageId, "Message Sent To All Users", cancellationToken: ct);
                    }
                }
            }

            var getAllAdmins = await _adminController.GetAllAdminsAsync();
            if (getAllAdmins.Any(p => p.UserId == e.From.Id.ToString()))
            {
                await OwnerAsync(bot, e, ct);
                var getAdmin = await _adminController.GetAdminAsync(e.From.Id.ToString());

                #region CommandSteps

                if (getAdmin.CommandSteps is not 0)
                {
                    switch (getAdmin.CommandSteps)
                    {
                        #region CreateAdmin

                        case 1:
                            await _adminController.UpdateAdminAsync(new Admin()
                            { CommandSteps = 0, UserId = e.From.Id.ToString() });
                            var add = await _adminController.AddOwnerAsync(new Admin() { UserId = e.Text ?? "" });
                            if (add)
                                await bot.SendTextMessageAsync(e.From.Id, $"User:[{e.Text}] Is Now Admin!",
                                    replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
                            else
                                await bot.SendTextMessageAsync(e.From.Id,
                                    $"There Is A Problem During Adding This Admin!\n He Might Be Already Admin !",
                                    replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
                            break;

                        #endregion

                        #region Add Channel Id

                        case 2:
                            await _adminController.UpdateAdminAsync(new Admin()
                            { UserId = e.From.Id.ToString(), CommandSteps = 0 });
                            var addChannel = await _forceJoinController.AddChannelAsync(new ForceJoinChannel()
                            { ChId = e.Text ?? "" });
                            if (addChannel)
                                await bot.SendTextMessageAsync(e.From.Id, "Channel Has Been Added SuccessFully",
                                    replyMarkup: ChannelCommandsMarkUp, cancellationToken: ct);
                            else
                                await bot.SendTextMessageAsync(e.From.Id, "Channel Is Already In The List!",
                                    replyMarkup: ChannelCommandsMarkUp, cancellationToken: ct);

                            break;

                            #endregion
                    }
                }

                #endregion

                #region Question

                if (getAdmin.QuestionSteps is not 0)
                {
                    var cancelQuestioningMarkup = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Cancel",
                            "Admin:QuestionCommands:Back:QuestionCommandsCleanSteps"),
                    });
                    switch (getAdmin.QuestionSteps)
                    {
                        #region Add Question

                        case 1:
                            var canGetData = e.Text.TryGetQAndA(out var question, out var answer);
                            if (canGetData)
                            {

                                await bot.SendTextMessageAsync(e.From.Id,
                                    $"Your Question As : <i>{question}</i> \nAnd Your Answer As : <i>{answer}</i>\n Submitted In List!\n<b> Submit Another :\n Or Press Cancel</b>",
                                    ParseMode.Html, cancellationToken: ct, replyMarkup: cancelQuestioningMarkup);
                                await _questionController.CreateQuestionAsync(new Question()
                                {
                                    CreatorId = e.From.Id.ToString(),
                                    Answer = answer,
                                    Question1 = question,
                                    Language = getAdmin.CurrentQuestionLanguage
                                });
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(e.From.Id, $"Bad Syntax!\n Try Another! Or Press Cancel",
                                    cancellationToken: ct, replyMarkup: cancelQuestioningMarkup);
                            }

                            break;

                        #endregion

                        #region Remove Question

                        case 2:
                            var remove = await _questionController.RemoveQuestionAsync(e.Text ?? "");
                            if (remove)
                            {
                                await _adminController.UpdateAdminAsync(new Admin()
                                { UserId = e.From.Id.ToString(), QuestionSteps = 0 });
                                await bot.SendTextMessageAsync(e.From.Id, "Question Removed Successfully!",
                                    replyMarkup: QuestionCommandsMarkUp, cancellationToken: ct);
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(e.From.Id,
                                    "Question Not Found!\nIts Might Be Already Removed.",
                                    replyMarkup: cancelQuestioningMarkup, cancellationToken: ct);
                            }

                            break;

                            #endregion

                    }
                }

                #endregion
            }


            #endregion


        }
        catch (Exception exception)
        {
            Log.Error(exception, "HandleMessageAsync");
            await bot.SendTextMessageAsync(e.Chat.Id, "We Got Some Problems \nPlease Wait Until We Fix It!\nIf Problem Still Resist Please Contact To Our Support Service!", cancellationToken: ct);
            await SendExToAdminAsync(exception, bot, ct);
        }

    }
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        //var errorMessage = exception switch
        //{
        //    ApiRequestException apiRequestException
        //        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        //    _ => exception.ToString()
        //};
        await SendExToAdminAsync(exception, botClient, cancellationToken);
        Log.Error(exception, "HandleErrorAsync");
        Console.WriteLine(exception.Message);
    }
    #endregion

    #region Methods

    #region Login-Overlords
    private async Task LoginUserAsync(ITelegramBotClient bot, Message e, CancellationToken ct)
    {
        await bot.SendTextMessageAsync(e.Chat.Id, "Please Enter The Account Number:", replyMarkup: IdentifyKeyboard, cancellationToken: ct);
        await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 1 });
    }
    private async Task LoginUserAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct)
    {
        await bot.EditMessageTextAsync(e.Message!.Chat.Id, e.Message.MessageId, "Please Enter The Account Number:", replyMarkup: IdentifyKeyboard, cancellationToken: ct);
        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 1 });
    }
    #endregion

    #region Register-Overlords

    private async Task RegisterUserAsync(ITelegramBotClient bot, Message e, CancellationToken ct)
    {
        if (e.From is null) return;

        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 4 });
        await bot.SendTextMessageAsync(e.From.Id, "Please enter the ‌Email:", replyMarkup: RegisterKeyboard, cancellationToken: ct);
    }
    private async Task RegisterUserAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct)
    {
        if (e.Message is null) return;

        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 4 });
        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, "Please enter the ‌Email:", replyMarkup: RegisterKeyboard, cancellationToken: ct);
    }

    #endregion

    #region ForgetPass-Overlords

    private async Task ForgetPasswordAsync(ITelegramBotClient bot, Message e, CancellationToken ct)
    {
        if (e.From is null) return;
        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 8 });
        await bot.SendTextMessageAsync(e.From.Id, "Please Enter Your Email To Restore Your Account:", replyMarkup: ResetPassKeyboard, cancellationToken: ct);
    }
    private async Task ForgetPasswordAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct)
    {
        if (e.Message is null) return;
        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 8 });
        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, "Please Enter Your Email To Restore Your Account:", replyMarkup: ResetPassKeyboard
            , cancellationToken: ct);
    }
    #endregion

    private static async Task<bool> SendChatActionAsync(ITelegramBotClient bot, Update e, CancellationToken ct, string type)
    {
        try
        {
            switch (type)
            {
                case "callback":
                    if (e.CallbackQuery is null)
                        return false;
                    await bot.SendChatActionAsync(e.CallbackQuery.From.Id, ChatAction.Typing, ct);
                    return true;

                case "message":
                    if (e.Message?.From is null)
                        return false;
                    await bot.SendChatActionAsync(e.Message.From.Id, ChatAction.Typing, ct);
                    return true;
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"User Blocked Bot : {e.Message?.From?.Id}");
            Log.Error(ex, "SendChatActionAsync");
            return false;
        }

    }
    private async Task OwnerAsync(ITelegramBotClient bot, Message e, CancellationToken ct)
    {
        try
        {
            if (e.Text is null || e.From is null) return;

            if (e.Text is "!")
            {
                await bot.SendTextMessageAsync(e.From.Id, "Here Is Your Panel : ", replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            await SendExToAdminAsync(exception, bot, ct);
            Log.Error(exception, "OwnerAsync");
            await bot.SendTextMessageAsync(e.From.Id, "We Got Some Problems \nPlease Wait Until We Fix It!\nIf Problem Still Resist Please Contact To Our Support Service!", cancellationToken: ct);
        }


    }
    private List<List<InlineKeyboardButton>> CreateInlineButton(List<string> buttons)
    {
        var keyBoard = new List<List<InlineKeyboardButton>>();
        var add = 0;
        foreach (var button in buttons)
        {
            if (add % 3 == 0)
            {
                keyBoard.Add(new List<InlineKeyboardButton>());
            }
            keyBoard.Last().Add(button);

            add++;
        }

        return keyBoard;
    }
    private void CreateKeyboardButtonQuestions(List<string> buttons, out ReplyKeyboardMarkup questionKeyboardMarkup)
    {
        var keyBoard = new List<List<KeyboardButton>>();
        var add = 0;
        buttons.Insert(0, "Back To Main Menu");
        foreach (var button in buttons)
        {
            if (add % 1 == 0)
            {
                keyBoard.Add(new List<KeyboardButton>());
            }
            keyBoard.Last().Add(button);

            add++;
        }

        questionKeyboardMarkup = new ReplyKeyboardMarkup(keyBoard)
        { Keyboard = keyBoard, OneTimeKeyboard = false, Selective = false, ResizeKeyboard = true };
    }
    private List<List<InlineKeyboardButton>> CreateInlineButtonAdminList(List<string> buttons, bool needCallBack = true)
    {
        var keyBoard = new List<List<InlineKeyboardButton>>();
        var add = 0;
        foreach (var button in buttons)
        {

            if (add % 1 == 0)
            {

                keyBoard.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(button,
                        needCallBack ? $"Admin:AdminCommands:Remove:{button}" : "-")
                });

            }

            keyBoard.Last().Add(InlineKeyboardButton.WithCallbackData(button,
                needCallBack ? $"Admin:AdminCommands:Remove:{button}" : "-"));
            add++;

        }

        return keyBoard;
    }
    private List<List<InlineKeyboardButton>> CreateInlineButtonCountryList(List<string> buttons)
    {
        var keyBoard = new List<List<InlineKeyboardButton>>();
        var add = 0;
        foreach (var button in buttons)
        {
            if (add % 3 == 0)
            {
                keyBoard.Add(new List<InlineKeyboardButton>());
            }
            keyBoard.Last().Add(InlineKeyboardButton.WithCallbackData(button, $"Admin:QuestionCommands:Add:CountrySelect:{button}"));
            add++;
        }
        return keyBoard;
    }
    private List<List<InlineKeyboardButton>> CreateInlineButtonCountryListQuestionSelect(List<string> buttons)
    {
        var keyBoard = new List<List<InlineKeyboardButton>>();
        var add = 0;
        foreach (var button in buttons)
        {

            if (add % 3 == 0)
            {
                keyBoard.Add(new List<InlineKeyboardButton>());
            }

            keyBoard.Last().Add(InlineKeyboardButton.WithCallbackData(button, $"Admin:QuestionCommands:List:CountrySelect:{button}"));
            add++;

        }

        return keyBoard;
    }
    private static bool CheckAccountNumber(string paymentMethod, string accountNumber, out string units)
    {
        switch (paymentMethod)
        {
            case "PerfectMoneyUSD":
                units = "USD";
                return accountNumber.ToLower().StartsWith("u");
            case "PerfectMoneyEUR":
                units = "EUR";
                return accountNumber.ToLower().StartsWith("e");
            default:
                units = string.Empty;
                return false;
        }
    }
    private async Task AdminAreaAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct)
    {
        if (e.Data is null || e.Message is null) return;
        try
        {
            var splitData = e.Data.Split(":");
            var commandType = splitData[1];
            switch (commandType)
            {
                #region Admin Handlers

                case "AdminCommands":
                    if (splitData.Length > 2)
                    {
                        switch (splitData[2])
                        {
                            #region Create
                            case "Create":
                                await _adminController.UpdateAdminAsync(new Admin() { UserId = e.From.Id.ToString(), CommandSteps = 1 });
                                var cancelCreation = new InlineKeyboardMarkup(new[]
                                    { InlineKeyboardButton.WithCallbackData("Cancel", "Admin:AdminCommands:Back:AdminCommandsClearStep"), });
                                await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, "Send Your User-ID:", replyMarkup: cancelCreation, cancellationToken: ct);
                                break;
                            #endregion

                            #region Remove
                            case "Remove":
                                if (splitData.Length > 3)
                                {
                                    var idToRemove = splitData[3];
                                    var delete = await _adminController.DeleteOwnerAsync(new Admin() { UserId = idToRemove });
                                    if (delete)
                                        await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, $"User:[{idToRemove}] Has Been Deleted From Admin List!", replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
                                    else
                                        await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, $"There Is A Problem During Deleting This Admin!\n He Might Be Already Removed !", replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);

                                }
                                else
                                {
                                    var getAdmins = await _adminController.GetAllAdminsAsync();
                                    var key = CreateInlineButtonAdminList(getAdmins.AdminsNamesToList());
                                    key.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData("Back", "Admin:AdminCommands:Back:AdminCommands") });
                                    await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, "Here Is Your Admin List :\n Select One Of Them To Remove ",
                                        replyMarkup: new InlineKeyboardMarkup(key), cancellationToken: ct);
                                }
                                break;
                            #endregion

                            #region List
                            case "List":
                                var getAdminsList = await _adminController.GetAllAdminsAsync();
                                var keyboard = CreateInlineButtonAdminList(getAdminsList.AdminsNamesToList(), false);
                                keyboard.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData("Back", "Admin:AdminCommands:Back:AdminCommands") });
                                await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, "Here Is Your Admin List :\n Select One Of Them To Remove ",
                                    replyMarkup: new InlineKeyboardMarkup(keyboard), cancellationToken: ct);
                                break;
                            #endregion

                            #region Back

                            case "Back":
                                var backWhere = splitData[3];
                                switch (backWhere)
                                {
                                    #region Main
                                    case "Main":
                                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, "Here Is Your Panel :",
                                            replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
                                        break;
                                    #endregion

                                    #region AdminCommands
                                    case "AdminCommands":
                                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                            "Admin Commands:", replyMarkup: AdminCommandsMarkUp, cancellationToken: ct);
                                        break;
                                    #endregion

                                    #region AdminCommands + ClearStep
                                    case "AdminCommandsClearStep":
                                        await _adminController.UpdateAdminAsync(new Admin() { UserId = e.From.Id.ToString(), CommandSteps = 0 });
                                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                            "Admin Commands:", replyMarkup: AdminCommandsMarkUp, cancellationToken: ct);
                                        break;

                                        #endregion
                                }
                                break;

                                #endregion
                        }
                    }
                    else
                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, "Admin Commands:", replyMarkup: AdminCommandsMarkUp, cancellationToken: ct);

                    break;

                #endregion

                #region Channel Handlers
                case "ChannelCommands":
                    if (splitData.Length > 2)
                    {
                        switch (splitData[2])
                        {
                            #region Add
                            case "Add":
                                await _adminController.UpdateAdminAsync(new Admin() { UserId = e.From.Id.ToString(), CommandSteps = 2 });
                                var cancelCreation = new InlineKeyboardMarkup(new[]
                                    { InlineKeyboardButton.WithCallbackData("Cancel", "Admin:ChannelCommands:Back:ChannelCommandsClearStep"), });
                                await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, "Send Channel Id Without (@):\n (E.x : telegramBot)", replyMarkup: cancelCreation, cancellationToken: ct);
                                break;
                            #endregion

                            #region Remove
                            case "Remove":
                                if (splitData.Length > 3)
                                {
                                    var idToRemove = splitData[3];
                                    var deleteChannel = await _forceJoinController.RemoveChannelAsync(new ForceJoinChannel() { ChId = idToRemove });
                                    if (deleteChannel)
                                        await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, $"User:[{idToRemove}] Has Been Deleted From Admin List!", replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
                                    else
                                        await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, $"There Is A Problem During Deleting This Admin!\n He Might Be Already Removed !", replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);

                                }
                                else
                                {
                                    var getChannels = await _forceJoinController.GetChannelsAsync();
                                    var key = CreateInlineButtonAdminList(getChannels.ChannelNamesToList());
                                    key.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData("Back", "Admin:ChannelCommands:Back:ChannelCommands") });
                                    await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, "Here Is Your Channels List :\n Select One Of Them To Remove ",
                                        replyMarkup: new InlineKeyboardMarkup(key), cancellationToken: ct);
                                }
                                break;
                            #endregion

                            #region List
                            case "List":
                                var getChannelsList = await _forceJoinController.GetChannelsAsync();
                                var keyboard = CreateInlineButtonAdminList(getChannelsList.ChannelNamesToList(), false);
                                keyboard.Add(new List<InlineKeyboardButton>() { InlineKeyboardButton.WithCallbackData("Back", "Admin:ChannelCommands:Back:ChannelCommands") });
                                await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, "Here Is Your Admin List :\n Select One Of Them To Remove ",
                                    replyMarkup: new InlineKeyboardMarkup(keyboard), cancellationToken: ct);
                                break;
                            #endregion

                            #region Back
                            case "Back":
                                var backWhere = splitData[3];
                                switch (backWhere)
                                {
                                    case "ChannelCommands":
                                    case "ChannelCommandsClearStep":
                                        if (backWhere == "ChannelCommandsClearStep")
                                            await _adminController.UpdateAdminAsync(new Admin() { UserId = e.From.Id.ToString(), CommandSteps = 0 });

                                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                            "Admin Commands:", replyMarkup: ChannelCommandsMarkUp, cancellationToken: ct);
                                        break;
                                }
                                break;
                                #endregion
                        }
                    }
                    else
                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, "Channel Commands:", replyMarkup: ChannelCommandsMarkUp, cancellationToken: ct);

                    break;
                #endregion

                #region Questions

                case "QuestionCommands":
                    if (splitData.Length > 2)
                    {
                        switch (splitData[2])
                        {
                            #region Add
                            case "Add":
                                if (splitData.Length > 4)
                                {
                                    switch (splitData[3])
                                    {
                                        #region After Country Select
                                        case "CountrySelect":
                                            var country = splitData[4];
                                            var createdQuestion = await _questionController.CreateQuestionAsync(new Question() { CreatorId = e.From.Id.ToString(), Language = country });
                                            var cancelQuestionMarkup = new InlineKeyboardMarkup(new[]
                                            {
                                                InlineKeyboardButton.WithCallbackData("Cancel",
                                                    $"Admin:QuestionCommands:Back:QuestionCommandsCleanSteps:{createdQuestion?.Id}"),
                                            });
                                            await _adminController.UpdateAdminAsync(new Admin() { UserId = e.From.Id.ToString(), QuestionSteps = 1, CurrentQuestionLanguage = country });
                                            await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                                $"You Selected {country}\n Now Send Your Question And Answer:\n E.x :\n <i>?How Old Are You\n#Im 10yo</i>",
                                                ParseMode.Html, cancellationToken: ct, replyMarkup: cancelQuestionMarkup);
                                            break;
                                            #endregion
                                    }
                                }
                                else
                                {
                                    #region Already Have Country
                                    var getAdmin = await _adminController.GetAdminAsync(e.From.Id.ToString());
                                    if (getAdmin.CurrentQuestionLanguage is not null)
                                    {
                                        var cancelQuestionMarkup = new InlineKeyboardMarkup(new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Cancel",
                                                $"Admin:QuestionCommands:Back:QuestionCommandsCleanLang"),
                                        });
                                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                            $"You Currently Using [{getAdmin.CurrentQuestionLanguage} ] \nFor Creating Language With Different Country Language Click (New Language)\n Or You Can Your Question And Answer",
                                            cancellationToken: ct, replyMarkup: cancelQuestionMarkup);
                                    }
                                    #endregion

                                    #region NewCountry
                                    else
                                    {

                                        var countryListMarkup = new InlineKeyboardMarkup(CreateInlineButtonCountryList(LanguagesList));
                                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                            "Select A Country To Continue:", replyMarkup: countryListMarkup,
                                            cancellationToken: ct);
                                    }
                                    #endregion
                                }

                                break;

                            #endregion

                            #region Remove
                            case "Remove":
                                await _adminController.UpdateAdminAsync(new Admin()
                                { UserId = e.From.Id.ToString(), QuestionSteps = 2 });
                                await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, "Enter The Question ID To Remove:", cancellationToken: ct);
                                break;
                            #endregion

                            #region List
                            case "List":
                                if (splitData.Length > 4)
                                {
                                    switch (splitData[3])
                                    {
                                        case "CountrySelect":
                                            var cancelQuestionMarkup = new InlineKeyboardMarkup(new[]
                                            {
                                                InlineKeyboardButton.WithCallbackData("Cancel",
                                                    $"Admin:QuestionCommands:Back:QuestionCommands"),
                                            });
                                            var country = splitData[4];
                                            var questionList =
                                                await _questionController.QuestionListByCountryAsync(country);
                                            if (questionList.Count > 0)
                                            {
                                                var questions = questionList.QuestionsToString();
                                                await bot.SendTextMessageAsync(e.From.Id, questions, ParseMode.Html, cancellationToken: ct);
                                            }
                                            else
                                                await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                                    "No Question Found For This Language!", replyMarkup: cancelQuestionMarkup, cancellationToken: ct);
                                            break;
                                    }
                                }
                                else
                                {

                                    var countryListKeyboard = CreateInlineButtonCountryListQuestionSelect(LanguagesList);
                                    countryListKeyboard.Add(new List<InlineKeyboardButton>()
                                        {
                                            InlineKeyboardButton.WithCallbackData("Back","Admin:QuestionCommands:Back:QuestionCommands")
                                        });

                                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                        "PLease Select A Country To Display Its Own Questions:",
                                        replyMarkup: new InlineKeyboardMarkup(countryListKeyboard), cancellationToken: ct);
                                }

                                break;
                            #endregion

                            #region Back
                            case "Back":
                                if (splitData.Length > 3)
                                {
                                    switch (splitData[3])
                                    {
                                        #region QuestionCommands +QuestionCommandsCleanSteps
                                        case "QuestionCommands":
                                        case "QuestionCommandsCleanSteps":
                                            if (splitData[3] == "QuestionCommandsCleanSteps")
                                            {
                                                if (splitData.Length > 4)
                                                {
                                                    var idToRemove = splitData[4];
                                                    await _questionController.RemoveQuestionAsync(idToRemove);
                                                }
                                                await _adminController.UpdateAdminAsync(new Admin()
                                                {
                                                    UserId = e.From.Id.ToString(),
                                                    QuestionSteps = 0,
                                                    CurrentQuestionLanguage = null
                                                });
                                            }
                                            await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                                "Admin Commands:", replyMarkup: AdminCommandsMarkUp,
                                                cancellationToken: ct);
                                            break;
                                        #endregion

                                        #region QuestionCommandsCleanLang
                                        case "QuestionCommandsCleanLang":
                                            await _adminController.UpdateAdminAsync(new Admin()
                                            { UserId = e.From.Id.ToString(), CurrentQuestionLanguage = null });
                                            var countryListMarkup = new InlineKeyboardMarkup(CreateInlineButtonCountryList(LanguagesList));
                                            await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                                "Select A Country To Continue:", replyMarkup: countryListMarkup,
                                                cancellationToken: ct);
                                            break;
                                            #endregion

                                    }
                                }
                                else
                                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                        "Here Is Your Panel :", replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
                                break;
                                #endregion
                        }
                    }
                    else
                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, "Question Commands:", replyMarkup: QuestionCommandsMarkUp, cancellationToken: ct);

                    break;

                #endregion

                #region Exit
                case "Exit":
                    await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                    break;
                    #endregion
            }
        }
        catch (Exception exception)
        {
            await SendExToAdminAsync(exception, bot, ct);
            Log.Error(exception, "AdminAreaAsync");
            Console.WriteLine(exception);
            await bot.SendTextMessageAsync(e.From.Id,
                "We Got Some Problems \nPlease Wait Until We Fix It!\nIf Problem Still Resist Please Contact To Our Support Service!", cancellationToken: ct);
        }

    }
    private async Task RegisteredAreaAsync(ITelegramBotClient bot, Message e, CancellationToken ct, User user)
    {
        try
        {
            if (e.From is null || e.Text is null) return;
            var canParse = Enum.TryParse(user.Language!, true, out Languages userLang);
            if (!canParse) userLang = Languages.English;

            if (e.Text.StartsWith("/start") && e.Text.Split(' ').Length >= 2)
            {
                // /start payment_confirmPayment
                var splitData = e.Text.Split('_');
                var paymentId = splitData[1];
                var pendingMessage = await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Pending Request"]}", ParseMode.Html, cancellationToken: ct);
                var results = await _apiController.CheckPaymentAsync(new ApiManualCheckPaymentModel() { payment_id = paymentId }, user.Token ?? "");
                var verified = results.data.verified ? "Yes" : "No";
                await bot.EditMessageTextAsync(pendingMessage.Chat.Id, pendingMessage.MessageId, $"{LangDictionary[userLang]["Your Request`s Result:"]} \n<b>Verified:{verified}\nOrder Id: {results.data.order_id}\n Amount:{results.data.amount}\nPayment ID:{results.data.payment_id}</b>",
                    ParseMode.Html, cancellationToken: ct);
            }

            #region Withdraw 
            switch (user.WithDrawStep)
            {
                #region CheckList-GetAccountNumber
                case 1:
                    await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), WithDrawAccount = e.Text });
                    var continueWithdraw = new InlineKeyboardMarkup(new[] { InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Continue"]}", $"ProcessWithdraw:{user.WithDrawAmount}:{user.WitchDrawPaymentMethod}"), InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}", "WithDraw:Cancel:-"), });

                    await bot.SendTextMessageAsync(e.Chat.Id,
                        $"{LangDictionary[userLang]["You CheckList Is Ready:"]}\n <b>{LangDictionary[userLang]["Withdraw Amount:"]}</b> <i>{user.WithDrawAmount}</i>\n<b>{LangDictionary[userLang]["WithDraw Payment Method:"]}</b><i>{user.WitchDrawPaymentMethod}</i>\n<b>{LangDictionary[userLang]["Account Number:"]}</b> <i>{e.Text}</i>", ParseMode.Html, cancellationToken: ct, replyMarkup: continueWithdraw);
                    break;
                #endregion

                #region EnterCustom Amount
                case 2:
                    if (e.Text.Length > 20)
                    {
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["This Amount Is Too Long!Maximum Length Is 17"]}", cancellationToken: ct);
                        return;
                    }
                    var isValid = e.Text.TryParseAmount(out var parsedAmount);
                    if (isValid)
                    {
                        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), WithDrawAmount = parsedAmount.ToString() });
                        var chatId = e.From.Id;
                        var paymentKeyboardMarkup = new InlineKeyboardMarkup(new[] {
                                new [] {
                                InlineKeyboardButton.WithCallbackData("Payeer",$"FinishWithDraw:{parsedAmount}:{chatId}:Payeer"),
                                InlineKeyboardButton.WithCallbackData("WebMoney",$"FinishWithDraw:{parsedAmount}:{chatId}:WebMoney"),
                                InlineKeyboardButton.WithCallbackData("TUSD Erc20", $"FinishWithDraw:{parsedAmount}:{chatId}:TUSD Erc20") },

                                new [] {
                                InlineKeyboardButton.WithCallbackData("SUSD Erc20",$"FinishWithDraw:{parsedAmount}:{chatId}:SUSD Erc20"),
                                InlineKeyboardButton.WithCallbackData("HUSD Erc20",$"FinishWithDraw:{parsedAmount}:{chatId}:HUSD Erc20"),
                                InlineKeyboardButton.WithCallbackData("True USD Erc20", $"FinishWithDraw:{parsedAmount}:{chatId}:True USD Erc20") },
                                new []
                                {
                                InlineKeyboardButton.WithCallbackData("Perfect Money",$"FinishWithDraw:{parsedAmount}:{chatId}:Perfect Money"),
                                InlineKeyboardButton.WithCallbackData("PUSD Erc20",$"FinishWithDraw:{parsedAmount}:{chatId}:PUSD Erc20"),
                                InlineKeyboardButton.WithCallbackData("MUSD Erc20",$"FinishWithDraw:{parsedAmount}:{chatId}:MUSD Erc20"),
                                },
                                new [] {
                                InlineKeyboardButton.WithCallbackData("USDC Erc20",$"FinishWithDraw:{parsedAmount}:{chatId}:USDC Erc20"),
                                InlineKeyboardButton.WithCallbackData("GUSD Erc20",$"FinishWithDraw:{parsedAmount}:{chatId}:GUSD Erc20"),
                                InlineKeyboardButton.WithCallbackData("BUSD Erc20",$"FinishWithDraw:{parsedAmount}:{chatId}:BUSD Erc20") },
                                new [] {
                                InlineKeyboardButton.WithCallbackData("Tether Trc20", $"FinishWithDraw:{parsedAmount}:{chatId}:Tether Trc20"),
                                InlineKeyboardButton.WithCallbackData("Tether Erc20",$"FinishWithDraw:{parsedAmount}:{chatId}:Tether Erc20"),
                                InlineKeyboardButton.WithCallbackData("DAI Erc20",$"FinishWithDraw:{parsedAmount}:{chatId}:DAI Erc20")
                                }, new [] { InlineKeyboardButton.WithCallbackData("Cancel","WithDraw:Cancel:-"), } });

                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Select Payment Method:"]}", replyMarkup: paymentKeyboardMarkup, cancellationToken: ct);

                        //var continueCustomWithdrawKeyboard = new InlineKeyboardMarkup(new[]
                        //{
                        //InlineKeyboardButton.WithCallbackData("Continue",$"WithDraw:{parsedAmount}:{e.From.Id}"),
                        //InlineKeyboardButton.WithCallbackData("Cancel","WithDraw:Cancel:-"), });

                        // await bot.SendTextMessageAsync(e.From.Id, $"<b>Your Entered Amount Is [{parsedAmount}$]</b>\n If Its Correct Please Press Continue", ParseMode.Html, replyMarkup: //continueCustomWithdrawKeyboard, cancellationToken: ct);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Minimum amount to deposit 10$"]}", cancellationToken: ct);
                    }
                    break;

                #endregion

                #region AccountNumber Based On USD OR EUR

                case 4:
                    await bot.SendTextMessageAsync(e.From.Id, $"<b>{LangDictionary[userLang]["Please Wait Until We Check Your Account Number..."]}</b>",
                        ParseMode.Html, cancellationToken: ct);
                    if (CheckAccountNumber(user.WitchDrawPaymentMethod ?? "", e.Text ?? "", out var unit))
                    {
                        var msg = await bot.SendTextMessageAsync(e.From.Id, $"<b>{LangDictionary[userLang]["Your Account Number Has Been Verified"]}</b> ",
                            ParseMode.Html, cancellationToken: ct);
                        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), WithDrawAccount = e.Text ?? "", WithDrawStep = 0 });
                        await _apiController.WithdrawAsync(new ApiWithdrawModel()
                        {
                            account = e.Text ?? "",
                            amount = user.WithDrawAmount ?? "",
                            gateway = user.WitchDrawPaymentMethod ?? "",
                            units = unit

                        }, user.Token ?? "");
                        await bot.EditMessageTextAsync(msg.Chat.Id, msg.MessageId, $"<b>{LangDictionary[userLang]["Your Request Submitted SuccessFully!"]}</b>", ParseMode.Html, cancellationToken: ct);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(e.From.Id, $"<b{LangDictionary[userLang]["Your Account Number Is Wrong"]}></b> ", ParseMode.Html, cancellationToken: ct);
                    }
                    break;

                    #endregion
            }
            #endregion

            #region Deposit

            switch (user.DepositStep)
            {
                #region Custom Value
                case 1:
                    if (e.Text?.Length > 20)
                    {
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["This Amount Is Too Long!Maximum Length Is 17"]}", cancellationToken: ct);
                        return;
                    }
                    var isValid = e.Text.TryParseAmount(out var parsedAmount);
                    if (isValid)
                    {
                        await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), DepositAmount = parsedAmount.ToString() });

                        var continueCustomWithdrawKeyboard = new InlineKeyboardMarkup(new[]
                        {
                        InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Continue"]}",$"Deposit:{parsedAmount}:{e.From.Id}"),
                        InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}","Deposit:Cancel:-"),
                    });
                        await bot.SendTextMessageAsync(e.From.Id, $"<b>{LangDictionary[userLang]["Your Entered Amount Is"]} [{parsedAmount}$]</b>\n{LangDictionary[userLang]["If Its Correct Please Press Continue"]}", ParseMode.Html, replyMarkup: continueCustomWithdrawKeyboard, cancellationToken: ct);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(e.From.Id, $"<b>{LangDictionary[userLang]["Please Enter A Valid Number.Amount Must Be More Than 10.0$"]}</b> ", ParseMode.Html, cancellationToken: ct);
                    }
                    break;
                #endregion

                #region Manual Deposits

                #region GotAccount-Getting Transaction ID
                case 2:
                    var cancelKeyboard = new InlineKeyboardMarkup(new[] { InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}", $"Deposit:Cancel:-"), });
                    await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), DepositAccount = e.Text, DepositStep = 3 });
                    await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Please enter the Transaction ID:"]}", replyMarkup: cancelKeyboard, cancellationToken: ct);
                    break;
                #endregion

                #region GotTransactionID-Getting Final Confirm
                case 3:
                    if (e.Text?.Length > 70)
                    {
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["This Transaction Id Is Too Long! Maximum Length Is 70"]}", cancellationToken: ct);
                        return;
                    }
                    var cancelDepositKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Continue"]}", $"ConfirmSiteAccDeposit:End:{e.Text??"-"}"),
                        InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}", $"Deposit:Cancel:-"),
                    });
                    await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), DepositStep = 0 });
                    await bot.SendTextMessageAsync(e.From.Id,
                        $"Check the list:\nSource Account Number:{user.DepositAccount}\nTransaction ID:{e.Text}\nTransfer Amount:{user.DepositAmount}$\nDestination Account Number:{user.ManualAccount}\n\nPress \"OK\" if the information is correct.", replyMarkup: cancelDepositKeyboard, cancellationToken: ct);
                    break;
                    #endregion

                    #endregion
            }

            #endregion

            #region Questions

            if (e.Text?.StartsWith("Q-") ?? false)
            {
                //Q-1: how are you bro ?
                var splitQuestion = e.Text.Split('-'); // [0] q / [1] 1: how ...
                var question = splitQuestion[1].Split(':')[0]; // [0]

                var getQuestion = await _questionController.GetQuestionByQuestionAsync(new Question() { Id = Convert.ToInt32(question) });
                if (getQuestion is not null)
                {
                    await bot.SendTextMessageAsync(e.From.Id,
                        $"{LangDictionary[userLang]["Your Question:"]} {getQuestion.Question1}\n {LangDictionary[userLang]["Here Is Your Answer:"]}\n<i>{getQuestion.Answer}</i>", ParseMode.Html,
                        cancellationToken: ct);
                }
                else
                    await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["There Is No Answer Submitted For This Question!"]}",
                        cancellationToken: ct, replyMarkup: BackToMainMenuKeyboardMarkup);
            }

            #endregion

            #region PublicSteps

            switch (user.PublicSteps)
            {
                #region Tracking Payment
                case 1:
                    await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), PublicSteps = 0 });
                    var pendingMessage = await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Pending Request"]}", cancellationToken: ct);
                    var results = await _apiController.CheckPaymentAsync(new ApiManualCheckPaymentModel() { payment_id = e.Text ?? "" }, user.Token ?? "");
                    var verified = results.data.verified ? "Yes" : "No";
                    await bot.EditMessageTextAsync(pendingMessage.Chat.Id, pendingMessage.MessageId, $"Verified:{verified}\nOrder Id: {results.data.order_id}\n Amount:{results.data.amount}\nPayment ID:{results.data.payment_id}", cancellationToken: ct);
                    break;
                    #endregion
            }

            #endregion

            switch (e.Text)
            {
                #region Questions
                case "Questions":
                    var questions = await _questionController.QuestionListByCountryAsync(user.Language ?? "");
                    if (questions.Count > 0)
                    {
                        CreateKeyboardButtonQuestions(questions.QuestionNames(), out var replyKeyboardMarkup);
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Here Is Popular Questions:"]}", replyMarkup: replyKeyboardMarkup, cancellationToken: ct);
                    }
                    else
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["No Question Found For Your Language!"]}",
                            cancellationToken: ct);


                    break;
                #endregion

                #region Back To Main Menu

                case "Back To Main Menu":
                    await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["You Are In Main Menu:"]}", replyMarkup: user.LoginStep == 3 ? MainMenuKeyboardMarkup : IdentityKeyboardMarkup, cancellationToken: ct);
                    break;

                #endregion

                #region Settings

                case "Settings":
                    await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Your Settings:"]}", replyMarkup: SettingsKeyboardMarkup, cancellationToken: ct);
                    break;

                #endregion

                #region Premium Account
                case "Premium Account":
                    var getDetails = await _apiController.PremiumDetailsAsync();
                    if (getDetails is null)
                    {
                        await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["This Service Is Not Available Right Now!"]}", cancellationToken: ct);
                        return;
                    }
                    getDetails.ProcessSubscriptionDetails(out var downloadLimit,
                        out var resolutions,
                        out var subPrice,
                        out var watchOn,
                        out var canUseReferral, out var bonus, out var multiLevelPayment);

                    InlineKeyboardMarkup orderKeyboardMarkup = new(new[] { new[]
                {
                    InlineKeyboardButton.WithCallbackData("Order",$"Order:PremiumAccount:{e.Chat.Id}"),
                }});
                    var premiumText = $"<b>Price:</b><i>{subPrice}$</i>\n<b>Resolution:</b> <i>{resolutions}</i>\n<b>Download:</b> <i>{downloadLimit}</i>\n<b>Watch On:</b> <i>{watchOn}</i>\n<b>Referral Ads:</b> <i>{canUseReferral}</i>\n<b>Language Subtitle:</b> <i>All</i>\n<b>Earn Money:</b> <i>{multiLevelPayment},{bonus}</i> ";
                    await bot.SendTextMessageAsync(e.Chat.Id, premiumText, ParseMode.Html, replyMarkup: orderKeyboardMarkup, cancellationToken: ct);
                    break;
                #endregion

                #region Support
                case "Support":
                    var subMenuMessage = await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["Support Menu:"]}", cancellationToken: ct);
                    var supportKeyBoardMarkup = new InlineKeyboardMarkup(new[] { new[] {
                    InlineKeyboardButton.WithCallbackData("Ticket", "Support:Ticket"),
                    InlineKeyboardButton.WithCallbackData("Donate", "Support:Donate"),
                    InlineKeyboardButton.WithCallbackData("Tracking Payment", "Support:Track"),
                } });
                    await bot.EditMessageReplyMarkupAsync(subMenuMessage.Chat.Id, subMenuMessage.MessageId, supportKeyBoardMarkup, ct);
                    break;
                #endregion

                #region Deposit
                case "Deposit":
                    var valuesKeyboardMarkup = new InlineKeyboardMarkup(new[]
                    {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("15 USD",$"Deposit:15:{e.Chat.Id}"),
                        InlineKeyboardButton.WithCallbackData("25 USD",$"Deposit:25:{e.Chat.Id}"),
                        InlineKeyboardButton.WithCallbackData("50 USD",$"Deposit:50:{e.Chat.Id}"),
                        InlineKeyboardButton.WithCallbackData("100 USD",$"Deposit:100:{e.Chat.Id}"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Enter Custom Amount",$"Deposit:Custom:{e.Chat.Id}"),
                        InlineKeyboardButton.WithCallbackData("Cancel",$"Deposit:Cancel:-"), } });

                    await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["Select an option or press \"Enter Custom Amount\" to deposit another amount."]}", replyMarkup: valuesKeyboardMarkup, cancellationToken: ct);
                    break;
                #endregion

                #region WithDraw
                case "Withdraw":
                    if (!string.IsNullOrEmpty(user.WithDrawAmount) && !string.IsNullOrEmpty(user.WitchDrawPaymentMethod) && !string.IsNullOrEmpty(user.WithDrawAccount))
                    {
                        var amountKeyboardMarkup = new InlineKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("10 USD",$"WithDraw:10:{e.Chat.Id}"),
                            InlineKeyboardButton.WithCallbackData("25 USD",$"WithDraw:25:{e.Chat.Id}"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("50 USD",$"WithDraw:50:{e.Chat.Id}"),
                            InlineKeyboardButton.WithCallbackData("100 USD",$"WithDraw:100:{e.Chat.Id}"),
                            InlineKeyboardButton.WithCallbackData("All",$"WithDraw:All:{e.Chat.Id}"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Use Previous Withdraw Setups",$"WithDraw:Previous:{e.Chat.Id}"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Enter Custom Amount",$"WithDraw:CustomNumber:{e.Chat.Id}"),
                            InlineKeyboardButton.WithCallbackData("Cancel","WithDraw:Cancel:-"),
                        }

                    });
                        await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["Select Value Or Enter Your Own Custom Value By Selecting (Custom Amount) To Continue:"]}\n{LangDictionary[userLang]["(You Can Use The Last Option To Continue With Your Previous Method And Amount To Withdraw)"]}", replyMarkup: amountKeyboardMarkup, cancellationToken: ct);
                    }
                    else
                    {
                        var amountKeyboardMarkup = new InlineKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("10 USD",$"WithDraw:10:{e.Chat.Id}"),
                            InlineKeyboardButton.WithCallbackData("25 USD",$"WithDraw:25:{e.Chat.Id}"),
                            InlineKeyboardButton.WithCallbackData("50 USD",$"WithDraw:50:{e.Chat.Id}"),
                            InlineKeyboardButton.WithCallbackData("100 USD",$"WithDraw:100:{e.Chat.Id}"),
                            InlineKeyboardButton.WithCallbackData("All",$"WithDraw:All:{e.Chat.Id}"),
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Enter Custom Amount",$"WithDraw:CustomNumber:{e.Chat.Id}"),
                            InlineKeyboardButton.WithCallbackData("Cancel","WithDraw:Cancel:-"),
                        }
                     });
                        await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["Select Value Or Enter Your Own Custom Value By Selecting (Custom Amount) To Continue:"]}", replyMarkup: amountKeyboardMarkup, cancellationToken: ct);
                    }
                    break;
                #endregion

                #region Wallet

                case "Wallet":
                    var getInfo = await _apiController.InfoAsync(user.Token ?? "");
                    if (getInfo is null)
                        await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["Dear"]} {e.Chat.Id}\n{LangDictionary[userLang]["We Cant Get Your Info Right Now!"]}\n{LangDictionary[userLang]["Please Try Again Latter"]}", cancellationToken: ct);
                    else
                    {
                        var data = $"Balance: {getInfo.data.balance}$ \n{LangDictionary[userLang]["Account Number:"]}` {getInfo.data.wallet_number} `".EscapeUnSupportChars();
                        await bot.SendTextMessageAsync(e.Chat.Id, data, ParseMode.MarkdownV2, cancellationToken: ct);
                    }
                    break;

                #endregion

                #region Referral Ads
                case "Referral Ads":
                    try
                    {
                        var getReferralInfo = await _apiController.GetReferralAdsInfoAsync(user.Token ?? "");
                        var price = 6;
                        if (getReferralInfo is not null)
                        {
                            price = getReferralInfo.data.price;
                        }
                        var referralAdsKeyboard = new InlineKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData($"{price} USD",$"Ref:Ads:Check:{price}"),
                            InlineKeyboardButton.WithCallbackData($"{price * 2} USD",$"Ref:Ads:Check:{price * 2}"),
                            InlineKeyboardButton.WithCallbackData($"{price * 3} USD",$"Ref:Ads:Check:{price * 3}"),
                            InlineKeyboardButton.WithCallbackData($"{price * 4} USD",$"Ref:Ads:Check:{price * 4}"),
                            InlineKeyboardButton.WithCallbackData($"{price * 5} USD",$"Ref:Ads:Check:{price * 5}"),
                        } });
                        await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["Select Your Plan To Continue:"]}", replyMarkup: referralAdsKeyboard, cancellationToken: ct);
                    }
                    catch (Exception)
                    {
                        await bot.SendTextMessageAsync(e.Chat.Id, $"{LangDictionary[userLang]["This Service Is Not Available Right Now!"]}\n{LangDictionary[userLang]["Please Try Again Latter!"]}", cancellationToken: ct);

                    }

                    break;
                #endregion

                #region Referral Link

                case "Referral Link":
                    var checkSub = await _apiController.CheckSubscriptionsAsync(user.Token ?? "");
                    if (checkSub?.message == "nothing found")
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Please Buy A Subscription First!"]}", cancellationToken: ct);
                    else
                    {
                        var getLink = await _apiController.InfoAsync(user.Token ?? "");
                        if (getLink?.data is not null)
                            await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Here Is Your Referral Token:"]} `{getLink.data.link}`", ParseMode.MarkdownV2, cancellationToken: ct);
                        else
                            await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["This Service Is Not Available Right Now!"]}\n{LangDictionary[userLang]["Please Try Again Latter!"]}", cancellationToken: ct);

                    }
                    break;

                #endregion

                #region statistics
                case "Statistics":
                    var getSummary = await _apiController.SummaryAsync(user.Token ?? "");
                    if (getSummary is not null)
                    {
                        await bot.SendTextMessageAsync(e.From.Id,
                            $"<b>Your Conduct Summary : </b>\n <b>Today Income:</b> <i>{getSummary.data.todayIncome}</i>\n<b>Week Income:</b> <i>{getSummary.data.weekIncome}</i>\n<b>Today Deposits:</b> <i>{getSummary.data.todayDeposits}</i>\n<b>Week Deposits:</b> <i>{getSummary.data.weekDeposits}</i>\n<b>SubSets Count:</b> <i>{getSummary.data.subsetsCount}</i>\n<b>Referrals Count:</b> <i>{getSummary.data.referralCount}</i>",
                            ParseMode.Html, cancellationToken: ct);

                    }
                    else
                    {
                        await bot.SendTextMessageAsync(e.From.Id, "You Have No Activity In Record!", cancellationToken: ct);
                    }
                    break;
                #endregion

                #region History
                case "History":
                    var getTransactions = await _apiController.TransactionsAsync(user.Token ?? "");
                    if (getTransactions is null || getTransactions.data.Count < 1)
                    {
                        await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["No Transaction Found!"]}", cancellationToken: ct);
                    }
                    else
                    {
                        var sortedList = getTransactions.data.Take(10).OrderBy(p => p.type).ThenBy(p => p.id);
                        var results = "";
                        foreach (var trans in sortedList)
                        {
                            if (trans.mlp_user is not null)
                                results += $"ID:{trans.id}\nType:{trans.type} \nAmount: {trans.amount}\nLevel:{trans.level}\nDate:{trans.created_at}\n--------\n";
                            if (trans.bonus)
                                results += $"ID:{trans.id}\nType:{trans.type} \nAmount: {trans.amount}\nDate:{trans.created_at} \n--------\n";
                            if (trans.referral)
                                results += $"ID:{trans.id}\nType:{trans.type} \nAmount: {trans.amount}\nReferral Count: {trans.referral_count}\nRemaining Referral Count: {trans.remaining_referral_count}\nReferral Reward: {trans.referral_reward}\nDate:{trans.created_at} \n--------\n";
                            else
                                results += $"ID:{trans.id}\nType:{trans.type} \nAmount: {trans.amount}\nDate:{trans.created_at} \n--------\n";
                        }
                        await bot.SendTextMessageAsync(e.From.Id, results, cancellationToken: ct);

                    }
                    break;

                    #endregion
            }
        }
        catch (Exception exception)
        {
            await SendExToAdminAsync(exception, bot, ct);
            Log.Error(exception, "RegisteredAreaAsync");
            Console.WriteLine(exception);
            await bot.SendTextMessageAsync(e.From?.Id ?? e.Chat.Id, "We Got Some Problems \nPlease Wait Until We Fix It!\nIf Problem Still Resist Please Contact To Our Support Service!", cancellationToken: ct);

        }
    }
    private async Task SettingAreaAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct, User user)
    {
        try
        {
            if (e.Data is null || e.Message is null) return;
            var canParse = Enum.TryParse(user.Language!, true, out Languages userLang);
            if (!canParse) userLang = Languages.English;
            var splitData = e.Data.Split(":");
            var settingType = splitData[1];
            switch (settingType)
            {
                #region Change Language
                case "ChangeLang":
                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, "Select Your New Language Please: ", replyMarkup: new InlineKeyboardMarkup(CreateInlineButton(LanguagesList)), cancellationToken: ct);
                    break;
                #endregion

                #region Logout
                case "Logout":
                    if (splitData.Length > 2)
                    {
                        var status = splitData[2];
                        switch (status)
                        {
                            case "Yes":
                                var logOut = await _dbController.LogoutAsync(e.From.Id.ToString());
                                if (logOut)
                                {
                                    await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                                    await bot.SendTextMessageAsync(e.Message.Chat.Id, $"<b>{LangDictionary[userLang]["You LoggedOut From System!"]}</b>", ParseMode.Html, replyMarkup: IdentityKeyboardMarkup, cancellationToken: ct);
                                }
                                else
                                {
                                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"<b>{LangDictionary[userLang]["Something Wrong Happens.Please Try Again Latter!"]}</b>", cancellationToken: ct);
                                }
                                break;
                            case "No":
                                await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"<b>{LangDictionary[userLang]["You Canceled This Operation.Have A Great Day!"]}</b>", ParseMode.Html, cancellationToken: ct);
                                break;
                        }
                    }
                    else
                    {
                        var confirmLogoutKeyboard = new InlineKeyboardMarkup(new[] { InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Yes"]}", "Setting:Logout:Yes"), InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}", "Setting:Logout:No") });
                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"{LangDictionary[userLang]["Are You Sure You Wanna Logout?"]}", cancellationToken: ct, replyMarkup: confirmLogoutKeyboard);
                    }
                    break;
                    #endregion
            }
        }
        catch (Exception exception)
        {
            await SendExToAdminAsync(exception, bot, ct);
            Log.Error(exception, "SettingAreaAsync");
            Console.WriteLine(exception);
            await bot.SendTextMessageAsync(e.From.Id, "We Got Some Problems \nPlease Wait Until We Fix It!\nIf Problem Still Resist Please Contact To Our Support Service!", cancellationToken: ct);
        }
    }
    private async Task SupportAreaAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct, User user)
    {
        try
        {
            if (e.Data is null || e.Message is null) return;
            var canParse = Enum.TryParse(user.Language!, true, out Languages userLang);
            if (!canParse) userLang = Languages.English;
            var splitData = e.Data.Split(":");
            var areaType = splitData[1];
            switch (areaType)
            {
                #region Donate
                case "Donate":
                    await bot.DeleteMessageAsync(e.From.Id, e.Message.MessageId, ct);
                    var donateKeyboardMarkup = new InlineKeyboardMarkup(new[]
                    {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("5 USD","Support:ProcessDonate:5"),
                        InlineKeyboardButton.WithCallbackData("10 USD","Support:ProcessDonate:10"),
                        InlineKeyboardButton.WithCallbackData("25 USD","Support:ProcessDonate:25"),
                        InlineKeyboardButton.WithCallbackData("50 USD","Support:ProcessDonate:50"),
                        InlineKeyboardButton.WithCallbackData("100 USD","Support:ProcessDonate:100"),
                    }, new []
                    {
                        InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}","Support:Back"),
                    } });
                    var getInfo = await _apiController.InfoAsync(user.Token ?? "");
                    await bot.SendTextMessageAsync(e.From.Id,
                        $"{LangDictionary[userLang]["Select A Value To Donate:"]} \n{LangDictionary[userLang]["Your Current Balance Is:"]} {getInfo?.data.balance}", cancellationToken: ct, replyMarkup: donateKeyboardMarkup);
                    break;
                #endregion

                #region ProcessDonate
                case "ProcessDonate":
                    var donate = await _apiController.DonateAsync(new ApiDonateModel() { amount = splitData[2] }, user.Token ?? "");
                    if (donate is not null)
                    {
                        switch (donate.status)
                        {
                            case 200 or 201 or 403:
                                await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, $"<b>{donate.message}</b>", ParseMode.Html, cancellationToken: ct);
                                break;
                            default:
                                await bot.EditMessageTextAsync(e.From.Id, e.Message.MessageId, $"{LangDictionary[userLang]["This Service Is InActive.Please Try Again Latter!"]}", cancellationToken: ct);
                                break;
                        }
                    }
                    break;
                #endregion

                #region Ticket
                case "Ticket":
                    if (splitData.Length > 2)
                    {
                        switch (splitData[2])
                        {
                            case "Back":
                                await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                                break;
                        }
                    }
                    else
                    {
                        var contactKeyboard = new InlineKeyboardMarkup(new[]
                        { InlineKeyboardButton.WithUrl($"{LangDictionary[userLang]["Support"]}", "https://t.me/MEXINAMITen"),InlineKeyboardButton.WithCallbackData("Cancel","Support:Ticket:Back")});
                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"{LangDictionary[userLang]["You Can Contact To Our Support From This Link:"]}", replyMarkup: contactKeyboard, cancellationToken: ct);
                    }
                    break;
                #endregion

                #region Track
                case "Track":
                    var cancel = new InlineKeyboardMarkup(new[] { InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Cancel"]}", "Support:Back"), });
                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"<b>{LangDictionary[userLang]["Please Send Your Payment ID:"]}</b>", ParseMode.Html, replyMarkup: cancel, cancellationToken: ct);
                    await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), PublicSteps = 1 });
                    break;
                #endregion

                #region Cancel
                case "Back":
                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"<i>{LangDictionary[userLang]["Progress Has Been Canceled"]}</i>", ParseMode.Html, cancellationToken: ct);
                    break;
                    #endregion

            }
        }
        catch (Exception exception)
        {
            await SendExToAdminAsync(exception, bot, ct);
            Log.Error(exception, "SupportAreaAsync");
            await bot.SendTextMessageAsync(e.From.Id, "We Got Some Problems \nPlease Wait Until We Fix It!\nIf Problem Still Resist Please Contact To Our Support Service!", cancellationToken: ct);
        }

    }
    private async Task ReferralAreaAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct, User user)
    {
        try
        {
            if (e.Data is null || e.Message is null) return;

            var canParse = Enum.TryParse(user.Language!, true, out Languages userLang);
            if (!canParse) userLang = Languages.English;

            var splitData = e.Data.Split(":");
            var refType = splitData[1];
            switch (refType)
            {
                #region Ads
                case "Ads":
                    var adsType = splitData[2];
                    switch (adsType)
                    {
                        #region Check
                        case "Check":
                            await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct); var getValue = Convert.ToInt32(splitData[3]);
                            var continueAdsKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                InlineKeyboardButton.WithCallbackData($"{LangDictionary[userLang]["Continue"]}",$"Ref:Ads:Finish:{getValue / 6}"),
                            });
                            await bot.SendTextMessageAsync(e.From.Id, $"<b>{LangDictionary[userLang]["Here Is Your CheckList:"]}</b>\n<i>{getValue / 6} {LangDictionary[userLang]["Premium Account"]} {getValue} USD</i>", ParseMode.Html, cancellationToken: ct, replyMarkup: continueAdsKeyboard);
                            break;
                        #endregion

                        #region Finish
                        case "Finish":
                            try
                            {
                                await bot.DeleteMessageAsync(e.From.Id, e.Message.MessageId, ct);
                                var count = splitData[3];
                                var getResponse = await _apiController.BuyReferralAdsAsync(new ApiAdsModel() { persons = count }, user.Token ?? "");
                                if (getResponse is null)
                                {
                                    await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["This Service Is InActive.Please Try Again Latter!"]}", cancellationToken: ct);
                                }
                                else
                                    await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["Your Request`s Result:"]}\n{getResponse.data.result ?? getResponse.message}", cancellationToken: ct);
                            }
                            catch (Exception)
                            {
                                await bot.SendTextMessageAsync(e.From.Id, $"{LangDictionary[userLang]["We Got Some Problems Here!.Please Try Again Latter.If Problem Still Resist Please Contact Support!"]}", cancellationToken: ct);

                            }

                            break;
                            #endregion
                    }

                    break;
                #endregion

                #region Link
                case "Link":
                    break;
                    #endregion
            }
        }
        catch (Exception exception)
        {
            await SendExToAdminAsync(exception, bot, ct);
            Log.Error(exception, "ReferralAreaAsync");
            await bot.SendTextMessageAsync(e.From.Id, "We Got Some Problems \nPlease Wait Until We Fix It!\nIf Problem Still Resist Please Contact To Our Support Service!", cancellationToken: ct);

        }
    }
    private async Task IdentityAreaAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct, User user)
    {
        try
        {
            if (e.Data is null || e.Message is null) return;


            var canParse = Enum.TryParse(user.Language!, true, out Languages userLang);
            if (!canParse) userLang = Languages.English;

            var splitData = e.Data.Split(":");
            var identityTape = splitData[1];
            switch (identityTape)
            {
                #region Register
                case "Register":
                    switch (splitData[2])
                    {
                        #region startRegister

                        case "BeginRegister":
                            await RegisterUserAsync(bot, e, ct);
                            break;

                        #endregion

                        #region BeginLogin
                        case "BeginLogin":
                            await LoginUserAsync(bot, e, ct);
                            break;
                        #endregion

                        #region Forget Password

                        case "ForgetPass":
                            await ForgetPasswordAsync(bot, e, ct);
                            break;

                        #endregion

                        #region Finish Register
                        case "ContinueWithOutLink":
                            await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 0 });
                            await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, $"<i>{LangDictionary[userLang]["Please Wait A Second While We Processing Your Request..."]}</i>", ParseMode.Html,
                                cancellationToken: ct);
                            var emailPass = user.UserPass.Split(':');
                            var response = await _apiController.RegisterUserAsync(new ApiRegisterModel() { has_invitation = "0", email = emailPass[0], password = emailPass[1] }, true);
                            await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                                $"{LangDictionary[userLang]["Your Request`s Result:"]}\n<b>{response.HandleResponse()}</b>", ParseMode.Html, cancellationToken: ct);
                            break;
                        #endregion

                        #region Cancel
                        case "CancelCleanStep":
                            await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                            await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), LoginStep = 0 });
                            break;
                            #endregion
                    }
                    break;
                    #endregion
            }
        }
        catch (Exception exception)
        {
            await SendExToAdminAsync(exception, bot, ct);
            Log.Error(exception, "IdentityAreaAsync");
            // Console.WriteLine(exception);
            await bot.SendTextMessageAsync(e.From.Id, "We Got Some Problems \nPlease Wait Until We Fix It!\nIf Problem Still Resist Please Contact To Our Support Service!", cancellationToken: ct);
        }
    }
    private static async Task SendExToAdminAsync(Exception exception, ITelegramBotClient bot, CancellationToken ct)
    {
        await bot.SendTextMessageAsync(1127927726, $"{exception.Message}\n{exception.StackTrace}", cancellationToken: ct);
    }
    #endregion

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // cts.Cancel();
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var botClient = new TelegramBotClient(BotInformation.Token);
        // using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };
        await botClient.GetUpdatesAsync(offset: -1, cancellationToken: cancellationToken);
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
        var me = await botClient.GetMeAsync(cancellationToken);
        Console.WriteLine($"Start listening for @{me.Username}");

        await botClient.SendTextMessageAsync(1127927726, "<i>Application Is Up</i>", ParseMode.Html, cancellationToken: cancellationToken);

        await base.StartAsync(cancellationToken);
    }
}