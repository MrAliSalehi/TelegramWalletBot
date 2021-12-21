using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.FileProviders;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramWallet.Api;
using TelegramWallet.Api.Models.ApiLogin;
using TelegramWallet.Api.Models.ApiReferral.ApiAds;
using TelegramWallet.Api.Models.ApiWithdraw;
using TelegramWallet.Classes.DataBase;
using TelegramWallet.Classes.Extensions;
using TelegramWallet.Database.Models;
using Message = Telegram.Bot.Types.Message;
using User = TelegramWallet.Database.Models.User;

namespace TelegramWallet.Classes;

public class Bot
{
    //Todo : move in dependencies and get lang from db 

    #region Injection
    public static Dependencies.Languages UserLang = Dependencies.Languages.English;
    private readonly DbController _dbController;
    private readonly ApiController _apiController;
    private readonly AdminController _adminController;
    private readonly ForceJoinController _forceJoinController;
    public Bot()
    {
        _forceJoinController = new ForceJoinController();
        _adminController = new AdminController();
        _dbController = new DbController();
        _apiController = new ApiController();
    }

    #endregion

    #region Init
    public async Task RunAsync()
    {
        var botClient = new TelegramBotClient(Dependencies.BotInformation.Token);
        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);
        var me = await botClient.GetMeAsync(cts.Token);
        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();
        cts.Cancel();
    }
    #endregion

    #region Buttons

    #region Identity

    private static readonly KeyboardButton[][] ButtonsIdentity = new[]
    {
        new[] { new KeyboardButton(Dependencies.LangDictionary[UserLang]["Login"]), new KeyboardButton(Dependencies.LangDictionary[UserLang]["Register"]), },
        new[] { new KeyboardButton("Forget Password") },
        new[] { new KeyboardButton("Forget UserName") }
    };

    private static readonly ReplyKeyboardMarkup IdentityKeyboardMarkup = new(ButtonsIdentity)
    {
        Keyboard = ButtonsIdentity,
        Selective = false,
        OneTimeKeyboard = false,
        ResizeKeyboard = true
    };

    #endregion

    #region MainMenu
    private static readonly KeyboardButton[][] ButtonsMainMenu = new[]
    {
        new[] { new KeyboardButton("Statistics"), new KeyboardButton("Wallet"), },
        new[] { new KeyboardButton("Withdraw"),new KeyboardButton("Deposit") },
        new[] { new KeyboardButton("Referral Link"), new KeyboardButton("Referral Ads") },
        new[] {new KeyboardButton("Premium Account")},
        new[] { new KeyboardButton("Questions"), new KeyboardButton("Support"),new KeyboardButton("History") }
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
    private static readonly InlineKeyboardMarkup AdminKeyboardMarkup = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("Admins","Admin:AdminCommands"),
            InlineKeyboardButton.WithCallbackData("Channels","Admin:ChannelCommands"),
            InlineKeyboardButton.WithCallbackData("Exit","Admin:Exit"),
        }
    });
    private static readonly InlineKeyboardMarkup AdminCommandsMarkUp = new(new[] { new [] { InlineKeyboardButton.WithCallbackData("Create Admin","Admin:AdminCommands:Create"), InlineKeyboardButton.WithCallbackData("Remove Admin","Admin:AdminCommands:Remove"), },
        new [] { InlineKeyboardButton.WithCallbackData("Admin List","Admin:AdminCommands:List"), },
        new [] { InlineKeyboardButton.WithCallbackData("Back","Admin:AdminCommands:Back:Main"), } });

    #endregion

    #endregion

    #region Handlers
    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update e, CancellationToken ct)
    {
        switch (e.Type)
        {
            case UpdateType.CallbackQuery when e.CallbackQuery != null:
                var checkJoinCallBackSide = await ForceJoinAsync(bot, e, ct);

                if (checkJoinCallBackSide.Count == 0)
                    await HandleCallBackQueryAsync(bot, e.CallbackQuery, ct);
                else
                {
                    var notJoinedChannels = "";
                    checkJoinCallBackSide.ForEach(p =>
                    {
                        notJoinedChannels += $"{p.ChName} : @{p.ChId}\n";
                    });
                    await bot.SendTextMessageAsync(e.CallbackQuery.From.Id, $"Dear {e.CallbackQuery.From.Id}, You Did`nt Join In This Channel(s): \n {notJoinedChannels}\n To Use This Bot, Please First Join This Channel(s)", cancellationToken: ct);
                }
                break;
            case UpdateType.Message when e.Message != null:
                var checkJoinMessageSide = await ForceJoinAsync(bot, e, ct);

                if (checkJoinMessageSide.Count == 0)
                    await HandleMessageAsync(bot, e.Message, ct);
                else
                {
                    if (e.Message.From is null) return;
                    var notJoinedChannels = "";
                    checkJoinMessageSide.ForEach(p =>
                    {
                        notJoinedChannels += $"{p.ChName} : @{p.ChId}\n";
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
            if (!Dependencies.StatusList.Contains(joinStatus.Status.ToString()))
            {
                results.Add(channel);
            }
        }

        return results;
    }

    private async Task HandleCallBackQueryAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct)
    {
        if (e.Data is null) return;
        if (e.Message is null) return;

        var getUser = await _dbController.GetUserAsync(new User() { UserId = e.From.Id.ToString() });
        try
        {

            #region Select Language
            if (Dependencies.LanguagesList.Contains($"{e.Data}"))
            {
                await bot.DeleteMessageAsync(e.From.Id, e.Message.MessageId, ct);
                Enum.TryParse(e.Data, out Dependencies.Languages lang);
                await _dbController.UpdateUserAsync(new User()
                {
                    UserId = e.From.Id.ToString(),
                    Language = $"{lang}"
                });
                await bot.SendTextMessageAsync(e.From.Id, $"<i>You Selected : {e.Data}</i>", ParseMode.Html, replyMarkup: getUser.LoginStep == 3 ? MainMenuKeyboardMarkup : IdentityKeyboardMarkup, cancellationToken: ct);
            }
            #endregion

            #region Order
            if (e.Data.StartsWith("Order"))
            {
                var splitData = e.Data.Split(":");
                var order = splitData[1];
                var chatId = Convert.ToInt32(splitData[2]);
                await bot.SendTextMessageAsync(chatId, $"User {chatId}, you Ordered {order}.\n Payment Was SuccessFull.", cancellationToken: ct);
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
                await SupportAreaAsync(bot, e, ct);
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
                    await _dbController.UpdateUserAsync(new User()
                    { UserId = e.From.Id.ToString(), DepositStep = 1 });
                    await bot.SendTextMessageAsync(e.From.Id, "<b>Please Enter Your Custom Amount:\n (Take Notice Your Amount Must Be More Than 10$,And Your Message Cannot Contains Any Other Thing\n <i> (E.x : 12.43$ or 43.4 )) </i></b>",
                        ParseMode.Html, cancellationToken: ct);
                }
                #endregion

                #region Cancel
                else if (value == "Cancel")
                {
                    await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                    await _dbController.UpdateUserAsync(new User()
                    { UserId = e.From.Id.ToString(), DepositStep = 0 });
                    await bot.SendTextMessageAsync(e.From.Id, "<b>Transaction Has Been Canceled</b>",
                        ParseMode.Html, cancellationToken: ct);
                }
                #endregion

                #region Payment Select-NonCustom Value
                else
                {

                    var continueKeyboardMarkup = new InlineKeyboardMarkup(new[] {
                    new [] {
                    InlineKeyboardButton.WithCallbackData("Payeer",$"FinishDeposit:{value}:{chatId}:Payeer"),
                    InlineKeyboardButton.WithCallbackData("WebMoney",$"FinishDeposit:{value}:{chatId}:WebMoney"),
                    InlineKeyboardButton.WithCallbackData("TUSD Erc20", $"FinishDeposit:{value}:{chatId}:TUSD Erc20") },

                    new [] {
                        InlineKeyboardButton.WithCallbackData("SUSD Erc20",$"FinishDeposit:{value}:{chatId}:SUSD Erc20"),
                        InlineKeyboardButton.WithCallbackData("HUSD Erc20",$"FinishDeposit:{value}:{chatId}:HUSD Erc20"),
                        InlineKeyboardButton.WithCallbackData("True USD Erc20", $"FinishDeposit:{value}:{chatId}:True USD Erc20") },

                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Perfect Money",$"FinishDeposit:{value}:{chatId}:Perfect Money"),
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
                        InlineKeyboardButton.WithCallbackData("Cancel","Deposit:Cancel:-"),
                    }

                    });

                    await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, " Select Payment Method :", replyMarkup: continueKeyboardMarkup, cancellationToken: ct);
                }
                #endregion

            }
            if (e.Data.StartsWith("FinishDeposit"))
            {
                await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), DepositStep = 0 });
                var paymentKeyboard = new InlineKeyboardMarkup(new[] { InlineKeyboardButton.WithUrl("Pay", "https://google.com"), });
                var splitData = e.Data.Split(":");
                var value = splitData[1];
                var paymentMethodNameReplace = splitData[3].ReplacePaymentName();
                await bot.SendTextMessageAsync(e.Message.Chat.Id,
                    $"<b>Account ID:</b><i>{e.From.Id}</i>\n <b>Payment Method:</b> <i>{paymentMethodNameReplace}</i> \n <b>Amount Deposit:</b> <i>{value}</i>\n <b>Current Balance:</b> <i> 1.12$ </i>",
                    replyMarkup: paymentKeyboard,
                    parseMode: ParseMode.Html,
                    cancellationToken: ct);
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
                                InlineKeyboardButton.WithCallbackData("Continue", $"ProcessWithdraw:{getUser.WithDrawAmount}:{getUser.WitchDrawPaymentMethod}"),
                                InlineKeyboardButton.WithCallbackData("Cancel","WithDraw:Cancel:-"),
                            });
                            await bot.SendTextMessageAsync(e.From.Id,
                                $"You CheckList Is Ready:\n <b>Withdraw Amount : </b> <i>{getUser.WithDrawAmount}</i>\n<b>WithDraw Payment Method:</b><i>{getUser.WitchDrawPaymentMethod}</i>\n<b>Account Number:</b> <i>{getUser.WithDrawAccount}</i>",
                                ParseMode.Html, cancellationToken: ct, replyMarkup: continueWithdraw);
                            break;
                        }
                    #endregion

                    #region New WithDraw
                    default:
                        {
                            var paymentKeyboardMarkup = new InlineKeyboardMarkup(new[] {
                            new [] {
                                InlineKeyboardButton.WithCallbackData("Payeer",$"FinishWithDraw:{value}:{chatId}:Payeer"),
                                InlineKeyboardButton.WithCallbackData("WebMoney",$"FinishWithDraw:{value}:{chatId}:WebMoney"),
                                InlineKeyboardButton.WithCallbackData("TUSD Erc20", $"FinishWithDraw:{value}:{chatId}:TUSD Erc20") },

                            new [] {
                                InlineKeyboardButton.WithCallbackData("SUSD Erc20",$"FinishWithDraw:{value}:{chatId}:SUSD Erc20"),
                                InlineKeyboardButton.WithCallbackData("HUSD Erc20",$"FinishWithDraw:{value}:{chatId}:HUSD Erc20"),
                                InlineKeyboardButton.WithCallbackData("True USD Erc20", $"FinishWithDraw:{value}:{chatId}:True USD Erc20") },

                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Perfect Money",$"FinishWithDraw:{value}:{chatId}:Perfect Money"),
                                InlineKeyboardButton.WithCallbackData("PUSD Erc20",$"FinishWithDraw:{value}:{chatId}:PUSD Erc20"),
                                InlineKeyboardButton.WithCallbackData("MUSD Erc20",$"FinishWithDraw:{value}:{chatId}:MUSD Erc20"),
                            },

                            new [] {
                                InlineKeyboardButton.WithCallbackData("USDC Erc20",$"FinishWithDraw:{value}:{chatId}:USDC Erc20"),
                                InlineKeyboardButton.WithCallbackData("GUSD Erc20",$"FinishWithDraw:{value}:{chatId}:GUSD Erc20"),
                                InlineKeyboardButton.WithCallbackData("BUSD Erc20",$"FinishWithDraw:{value}:{chatId}:BUSD Erc20") },
                            new [] {
                                InlineKeyboardButton.WithCallbackData("Tether Trc20", $"FinishWithDraw:{value}:{chatId}:Tether Trc20"),
                                InlineKeyboardButton.WithCallbackData("Tether Erc20",$"FinishWithDraw:{value}:{chatId}:Tether Erc20"),
                                InlineKeyboardButton.WithCallbackData("DAI Erc20",$"FinishWithDraw:{value}:{chatId}:DAI Erc20")
                            },
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Cancel","WithDraw:Cancel:-"),
                            }

                            });

                            await _dbController.UpdateUserAsync(new User()
                            { UserId = e.From.Id.ToString(), WithDrawAmount = value });
                            await bot.SendTextMessageAsync(e.Message.Chat.Id, " Select Payment Method :", replyMarkup: paymentKeyboardMarkup, cancellationToken: ct);
                            break;
                        }
                    #endregion

                    #region CustomNumber

                    case "CustomNumber":
                        await _dbController.UpdateUserAsync(new User()
                        { UserId = e.From.Id.ToString(), WithDrawStep = 2 });
                        await bot.SendTextMessageAsync(e.From.Id, "<b>Please Enter Your Custom Amount:\n (Take Notice Your Amount Must Be More Than 10$,And Your Message Cannot Contains Any Other Thing\n <i> (E.x : 12.43$ or 43.4 )) </i></b>",
                            ParseMode.Html, cancellationToken: ct);
                        break;

                    #endregion

                    #region Cancel

                    case "Cancel":
                        await _dbController.UpdateUserAsync(new User()
                        { UserId = e.From.Id.ToString(), WithDrawStep = 0 });
                        await bot.SendTextMessageAsync(e.From.Id, "<b>Transaction Has Been Canceled</b>",
                            ParseMode.Html, cancellationToken: ct);
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

                #region PerfectMoney
                if (paymentMethod == "Perfect Money")
                {
                    var unitKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData("USD",$"FinishWithDraw:{splitData[2]}:{value}:PerfectMoneyUSD"),
                        InlineKeyboardButton.WithCallbackData("EUR",$"FinishWithDraw:{splitData[2]}:{value}:PerfectMoneyEUR"),
                    });
                    await bot.SendTextMessageAsync(e.From.Id, "Please Select One Amount-Unit To Process Your Order :",
                        cancellationToken: ct, replyMarkup: unitKeyboard);
                }
                else if (paymentMethod is "PerfectMoneyUSD" or "PerfectMoneyEUR")
                {
                    await bot.SendTextMessageAsync(e.From.Id,
                        "Now Please Send Your Account Number:\n (Your Account Number Is Depend On Your Previous Selection)",
                        cancellationToken: ct);
                    await _dbController.UpdateUserAsync(new User() { UserId = e.From.Id.ToString(), WithDrawAmount = value, WithDrawStep = 4, WitchDrawPaymentMethod = paymentMethod });
                }
                #endregion

                else
                {
                    await bot.SendTextMessageAsync(e.From.Id, "<b><i>Please Send Your WMZ: </i></b>", ParseMode.Html, cancellationToken: ct);
                    await _dbController.UpdateUserAsync(new User()
                    {
                        UserId = e.From.Id.ToString(),
                        WithDrawStep = 1,
                        WitchDrawPaymentMethod = paymentMethod,
                        WithDrawAmount = value,
                    });
                }
            }

            if (e.Data.StartsWith("ProcessWithdraw"))
            {
                await _dbController.UpdateUserAsync(new User()
                {
                    UserId = e.From.Id.ToString(),
                    WithDrawStep = 0,

                });
                await _apiController.WithdrawAsync(new ApiWithdrawModel()
                {
                    account = getUser.WithDrawAccount ?? "",
                    amount = getUser.WithDrawAmount ?? "",
                    gateway = getUser.WitchDrawPaymentMethod ?? "",

                }, getUser.Token ?? "");
                await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId,
                    "Your Withdraw Request Will Be Process In Next 24/48 Hours. \n<b>Thank You For Your Patience</b>",
                    ParseMode.Html, cancellationToken: ct);
            }
            #endregion

            #region Referral

            if (e.Data.StartsWith("Ref"))
            {
                await ReferralAreaAsync(bot, e, ct, getUser);
            }

            #endregion

            #region Admin

            if (e.Data.StartsWith("Admin"))
            {
                await AdminAreaAsync(bot, e, ct);
            }

            #endregion
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
    private async Task HandleMessageAsync(ITelegramBotClient bot, Message e, CancellationToken ct)
    {
        Console.WriteLine($"Message: '{e.Chat.Id}' from :[{e.Text}].");

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

        #region Registeration Area
        switch (getUser.LoginStep)
        {
            #region Login
            case 1:
                await bot.SendTextMessageAsync(e.Chat.Id, $"<b>Dear {e.Text} </b>\n Please Enter Your Password:", ParseMode.Html, cancellationToken: ct);
                await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 2, UserPass = $"{e.Text}" });
                break;
            case 2:
                var loginResponse = await _apiController.LoginAsync(new ApiLoginModel() { username = getUser.UserPass, password = e.Text });
                if (loginResponse is not null)
                {
                    await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 3, Token = loginResponse.data.token });
                    await bot.SendTextMessageAsync(e.Chat.Id, "<b>You Are In Main Menu :</b> ", ParseMode.Html, replyMarkup: MainMenuKeyboardMarkup, cancellationToken: ct);
                }
                else
                {
                    await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 0 });
                    await bot.SendTextMessageAsync(e.Chat.Id, "<b>Wrong User Or Password,Please Try Again :</b> ", ParseMode.Html, replyMarkup: IdentityKeyboardMarkup, cancellationToken: ct);
                }
                break;
            #endregion

            #region RegisteredUsers
            case 3:
                await RegisteredAreaAsync(bot, e, ct, getUser);
                break;
                #endregion
        }
        #endregion

        #region Public Messages
        switch (e.Text)
        {
            #region Start
            case "/start":
                var keyBoardMarkup = new InlineKeyboardMarkup(CreateInlineButton(Dependencies.LanguagesList));
                await bot.SendTextMessageAsync(e.Chat.Id, "Select Language Please : ", replyMarkup: keyBoardMarkup, cancellationToken: ct);
                break;
            #endregion

            #region Login
            case "Login":
                await bot.SendTextMessageAsync(e.Chat.Id, "<b>Please Enter Your User Name :</b>", ParseMode.Html, cancellationToken: ct);
                await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), LoginStep = 1 });
                await bot.DeleteMessageAsync(e.Chat.Id, e.MessageId, ct);
                break;
                #endregion

        }
        #endregion

        #region Main-Owner

        if (e.Chat.Id is 1127927726 or 1222521875)
        {
            await OwnerAsync(bot, e, ct);
            var getAdmin = await _adminController.GetAdminAsync(e.From.Id.ToString());
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
                            await bot.SendTextMessageAsync(e.From.Id, $"User:[{e.Text}] Is Now Admin!", replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
                        else
                            await bot.SendTextMessageAsync(e.From.Id, $"There Is A Problem During Adding This Admin!\n He Might Be Already Admin !", replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
                        break;
                    #endregion

                }
            }
        }

        #endregion

    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        //var errorMessage = exception switch
        //{
        //    ApiRequestException apiRequestException
        //        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        //    _ => exception.ToString()
        //};

        //Console.WriteLine(errorMessage);
        Console.WriteLine(exception.Message);
        return Task.CompletedTask;
    }
    #endregion

    #region Methods
    public async Task OwnerAsync(ITelegramBotClient bot, Message e, CancellationToken ct)
    {
        if (e.Text is null) return;

        if (e.From is null) return;

        if (e.Text is "!")
        {
            await bot.SendTextMessageAsync(e.From.Id, "Here Is Your Panel : ", replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
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
                    InlineKeyboardButton.WithCallbackData(button,needCallBack?$"Admin:AdminCommands:Remove:{button}":"-")
                });
                
            }
            keyBoard.Last().Add(InlineKeyboardButton.WithCallbackData(button, needCallBack ? $"Admin:AdminCommands:Remove:{button}" : "-"));
            add++;

        }

        return keyBoard;
    }//"

    private bool CheckAccountNumber(string paymentMethod, string accountNumber) =>
        paymentMethod switch { "PerfectMoneyUSD" => accountNumber.ToLower().StartsWith("u"), "PerfectMoneyEUR" => accountNumber.ToLower().StartsWith("e"), _ => false };

    private async Task AdminAreaAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct)
    {
        if (e.Data is null) return;
        if (e.Message is null) return;
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
                                        await bot.EditMessageTextAsync(e.From.Id,e.Message.MessageId, $"User:[{idToRemove}] Has Been Deleted From Admin List!", replyMarkup: AdminKeyboardMarkup, cancellationToken: ct);
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
                    {

                        await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, "Admin Commands:", replyMarkup: AdminCommandsMarkUp, cancellationToken: ct);
                    }
                    break;

                #endregion

                #region Channel Handlers
                case "ChannelCommands":
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
            Console.WriteLine(exception);
            throw;
        }

    }
    private async Task RegisteredAreaAsync(ITelegramBotClient bot, Message e, CancellationToken ct, User user)
    {
        #region Withdraw 
        switch (user.WithDrawStep)
        {
            #region CheckList-GetAccountNumber
            case 1:
                await _dbController.UpdateUserAsync(new User() { UserId = e.Chat.Id.ToString(), WithDrawAccount = e.Text });
                var continueWithdraw = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Continue", $"ProcessWithdraw:{user.WithDrawAmount}:{user.WitchDrawPaymentMethod}"),
                    InlineKeyboardButton.WithCallbackData("Cancel","WithDraw:Cancel:-"),
                });
                await bot.SendTextMessageAsync(e.Chat.Id,
                    $"You CheckList Is Ready:\n <b>Withdraw Amount : </b> <i>{user.WithDrawAmount}</i>\n<b>WithDraw Payment Method:</b><i>{user.WitchDrawPaymentMethod}</i>\n<b>Account Number:</b> <i>{e.Text}</i>",
                    ParseMode.Html, cancellationToken: ct, replyMarkup: continueWithdraw);
                break;
            #endregion

            #region EnterCustom Amount
            case 2:
                var isValid = e.Text.TryParseAmount(out var parsedAmount);
                if (isValid)
                {
                    await _dbController.UpdateUserAsync(new User()
                    { UserId = e.From.Id.ToString(), WithDrawAmount = parsedAmount.ToString() });

                    var continueCustomWithdrawKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Continue",$"WithDraw:{parsedAmount}:{e.From.Id}"),
                        InlineKeyboardButton.WithCallbackData("Cancel","WithDraw:Cancel:-"),
                    });
                    await bot.SendTextMessageAsync(e.From.Id, $"<b>Your Entered Amount Is [{parsedAmount}$]</b>\n If Its Correct Please Press Continue",
                        ParseMode.Html, replyMarkup: continueCustomWithdrawKeyboard, cancellationToken: ct);
                }
                else
                {
                    await bot.SendTextMessageAsync(e.From.Id, "<b>Please Enter A Valid Number.\n Amount Must Be More Than 10.0 $</b> ",
                        ParseMode.Html, cancellationToken: ct);
                }
                break;

            #endregion

            #region AccountNumber Based On USD OR EUR

            case 4:
                await bot.SendTextMessageAsync(e.From.Id, "<b>Please Wait Until We Check Your Account Number...</b> ",
                    ParseMode.Html, cancellationToken: ct);
                if (CheckAccountNumber(user.WitchDrawPaymentMethod ?? "", e.Text ?? ""))
                {
                    await bot.SendTextMessageAsync(e.From.Id, "<b>Your Account Number Has Been Verified</b> ",
                        ParseMode.Html, cancellationToken: ct);
                    await _dbController.UpdateUserAsync(new User()
                    { UserId = e.From.Id.ToString(), WithDrawAccount = e.Text ?? "", WithDrawStep = 0 });
                }
                else
                {
                    await bot.SendTextMessageAsync(e.From.Id, "<b>Your Account Number Is Wrong</b> ",
                        ParseMode.Html, cancellationToken: ct);
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
                var isValid = e.Text.TryParseAmount(out var parsedAmount);
                if (isValid)
                {
                    await _dbController.UpdateUserAsync(new User()
                    { UserId = e.From.Id.ToString(), DepositAmount = parsedAmount.ToString() });

                    var continueCustomWithdrawKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Continue",$"Deposit:{parsedAmount}:{e.From.Id}"),
                        InlineKeyboardButton.WithCallbackData("Cancel","Deposit:Cancel:-"),
                    });
                    await bot.SendTextMessageAsync(e.From.Id, $"<b>Your Entered Amount Is [{parsedAmount}$]</b>\n If Its Correct Please Press Continue",
                        ParseMode.Html, replyMarkup: continueCustomWithdrawKeyboard, cancellationToken: ct);
                }
                else
                {
                    await bot.SendTextMessageAsync(e.From.Id, "<b>Please Enter A Valid Number.\n Amount Must Be More Than 10.0 $</b> ",
                        ParseMode.Html, cancellationToken: ct);
                }
                break;


                #endregion
        }

        #endregion

        switch (e.Text)
        {

            #region Premium Account
            case "Premium Account":
                InlineKeyboardMarkup orderKeyboardMarkup = new(new[] { new[]
                {
                    InlineKeyboardButton.WithCallbackData("Order",$"Order:PremiumAccount:{e.Chat.Id}"),
                }});
                var premiumText = "<b>Resolution:</b> <i>480p 720p 1080p 4K 3D</i>\n<b>Download:</b> <i>Unlimited</i>\n<b>Watch On:</b> <i>TV, Laptop, Phone, Tablet</i>\n<b>Advertise:</b> <i>No Ads</i>\n<b>Language Subtitle:</b> <i>All</i>\n<b>Earn Money</b> <i>Multi Level Payment,Bounce</i> ";
                await bot.SendTextMessageAsync(e.Chat.Id, premiumText, ParseMode.Html, replyMarkup: orderKeyboardMarkup, cancellationToken: ct);
                break;
            #endregion

            #region Support
            case "Support":
                var subMenuMessage = await bot.SendTextMessageAsync(e.Chat.Id, "<b> Support Sub-Menu :</b> ", ParseMode.Html, cancellationToken: ct);
                var supportKeyBoardMarkup = new InlineKeyboardMarkup(new[] { new[]
                {
                    InlineKeyboardButton.WithCallbackData("Ticket", "Support:Ticket"),
                    InlineKeyboardButton.WithCallbackData("Donate", "Support:Donate"),
                    InlineKeyboardButton.WithCallbackData("Back",$"Back:MainMenu:{subMenuMessage.MessageId}:{subMenuMessage.Chat.Id}"),
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
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Cancel",$"Deposit:Cancel:-"),
                    }
                });
                await bot.SendTextMessageAsync(e.Chat.Id, "<b>Select Value Or Enter Your Own Custom Value By Selecting (Custom Amount) To Continue :</b> ", ParseMode.Html, replyMarkup: valuesKeyboardMarkup, cancellationToken: ct);
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
                    await bot.SendTextMessageAsync(e.Chat.Id, "<b>Select Value Or Enter Your Own Custom Value By Selecting (Custom Amount) To Continue :</b> \n (You Can Use The Last Option To Continue With Your Previous Method And Amount To Withdraw) ", ParseMode.Html, replyMarkup: amountKeyboardMarkup, cancellationToken: ct);
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
                    await bot.SendTextMessageAsync(e.Chat.Id, "<b>Select Value Or Enter Your Own Custom Value By Selecting (Custom Amount) To Continue :</b> ", ParseMode.Html, replyMarkup: amountKeyboardMarkup, cancellationToken: ct);
                }
                break;
            #endregion

            #region Wallet

            case "Wallet":
                var getInfo = await _apiController.InfoAsync(user.Token ?? "");

                var data = $"Balance: {getInfo.data.balance}$ \nAccount Number:` {getInfo.data.wallet_number} `".EscapeUnSupportChars();
                await bot.SendTextMessageAsync(e.Chat.Id, data, ParseMode.MarkdownV2, cancellationToken: ct);
                break;

            #endregion

            #region Referral Ads
            case "Referral Ads":
                var referralAdsKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("6 USD","Ref:Ads:Check:6"),
                        InlineKeyboardButton.WithCallbackData("12 USD","Ref:Ads:Check:12"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("18 USD","Ref:Ads:Check:18"),
                        InlineKeyboardButton.WithCallbackData("24 USD","Ref:Ads:Check:24"),
                        InlineKeyboardButton.WithCallbackData("30 USD","Ref:Ads:Check:30"),
                    }
                });
                await bot.SendTextMessageAsync(e.Chat.Id, "Select Your Plan To Continue: ", replyMarkup: referralAdsKeyboard, cancellationToken: ct);
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
                    await bot.SendTextMessageAsync(e.From.Id, "<i>You Have No Activity In Record !</i>", ParseMode.Html,
                        cancellationToken: ct);
                }
                break;
                #endregion
        }
    }

    private async Task SupportAreaAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct)
    {
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
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("25 USD","Support:ProcessDonate:25"),
                        InlineKeyboardButton.WithCallbackData("50 USD","Support:ProcessDonate:50"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("100 USD","Support:ProcessDonate:100"),
                    }

                });
                await bot.SendTextMessageAsync(e.From.Id,
                    $"Select A Value To Donate : \n (Your Current Balance Is : - )", cancellationToken: ct, replyMarkup: donateKeyboardMarkup);
                break;
            #endregion

            #region ProcessDonate
            case "ProcessDonate":
                await bot.DeleteMessageAsync(e.From.Id, e.Message.MessageId, ct);
                await bot.SendTextMessageAsync(e.From.Id, $"Tnx For Your {splitData[2]}$ Donate,Have A Great Day!",
                    cancellationToken: ct);
                break;
            #endregion

            case "Ticket":
                break;
        }
    }

    private async Task ReferralAreaAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct, User user)
    {
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
                        await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
                        var getValue = Convert.ToInt32(splitData[3]);
                        var continueAdsKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Continue",$"Ref:Ads:Finish:{getValue / 6}"),
                        });
                        await bot.SendTextMessageAsync(e.From.Id,
                            $"<b> Here Is Your CheckList: </b> \n<i>{getValue / 6} Premium Account {getValue} USD</i>",
                            ParseMode.Html, cancellationToken: ct, replyMarkup: continueAdsKeyboard);
                        break;
                    #endregion

                    #region Finish
                    case "Finish":
                        await bot.DeleteMessageAsync(e.From.Id, e.Message.MessageId, ct);
                        var count = splitData[3];
                        var getResponse = await _apiController.BuyReferralAdsAsync(new ApiAdsModel() { persons = count }, user.Token ?? "");
                        await bot.SendTextMessageAsync(e.From.Id,
                            $"<b>Your Payment Result:</b>\n <i>{getResponse.data}</i>", ParseMode.Html,
                            cancellationToken: ct);
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
    #endregion
}