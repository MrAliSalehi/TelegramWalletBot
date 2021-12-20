using Microsoft.Extensions.FileProviders;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramWallet.Classes;

public class Bot
{
    //Todo : move in dependencies and get lang from db 
    public static Dependencies.Languages UserLang = Dependencies.Languages.English;
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

        // Send cancellation request to stop bot
        cts.Cancel();
    }

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

    #region Support
    private static readonly InlineKeyboardMarkup SupportKeyboardMarkup = new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithUrl("Ticket", "https://google.com"),
            InlineKeyboardButton.WithUrl("Donate", "https://buyMeACoffee.com"),
            InlineKeyboardButton.WithCallbackData("Back","Back:MainMenu"),
        }
    });

    #endregion

    #endregion

    #region Handlers
    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update e, CancellationToken ct)
    {

        if (e.Type is UpdateType.CallbackQuery && e.CallbackQuery != null)
            await HandleCallBackQueryAsync(bot, e.CallbackQuery, ct);

        if (e.Type is UpdateType.Message && e.Message != null)
            await HandleMessageAsync(bot, e.Message, ct);

    }
    private async Task HandleCallBackQueryAsync(ITelegramBotClient bot, CallbackQuery e, CancellationToken ct)
    {
        if (e.Data is null) return;

        if (Dependencies.LanguagesList.Contains($"{e.Data}"))
        {
            await bot.DeleteMessageAsync(e.From.Id, e.Message.MessageId, ct);
            Enum.TryParse(e.Data, out Dependencies.Languages lang);
            //Todo : save lang in db 
            await bot.SendTextMessageAsync(e.From.Id, $"<i>You Selected : {e.Data}</i>", ParseMode.Html, replyMarkup: IdentityKeyboardMarkup, cancellationToken: ct);
        }

        if (e.Data.StartsWith("Order"))
        {
            var splitData = e.Data.Split(":");
            var order = splitData[1];
            var chatId = Convert.ToInt32(splitData[2]);
            await bot.SendTextMessageAsync(chatId, $"User {chatId}, you Ordered {order}.\n Payment Was SuccessFull.", cancellationToken: ct);
        }
        if (e.Data.StartsWith("Back"))
        {
            var splitData = e.Data.Split(":");
            var messageId = Convert.ToInt32(splitData[2]);
            var chatId = splitData[3];
            await bot.DeleteMessageAsync(chatId, messageId, ct);
        }

        #region Deposit
        if (e.Data.StartsWith("Deposit"))
        {
            var splitData = e.Data.Split(":");
            var value = splitData[1];
            var chatId = splitData[2];
            var continueKeyboardMarkup = new InlineKeyboardMarkup(new[] {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Payeer",$"FinishDeposit:{value}:{chatId}:Payeer"),
                    InlineKeyboardButton.WithCallbackData("WebMoney",$"FinishDeposit:{value}:{chatId}:WebMoney"),
                    InlineKeyboardButton.WithCallbackData("TUSD Erc20", $"FinishDeposit:{value}:{chatId}:TUSD Erc20")
                },

                new [] { InlineKeyboardButton.WithCallbackData("Perfect Money",$"FinishDeposit:{value}:{chatId}:Perfect Money"), },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("USDC Erc20",$"FinishDeposit:{value}:{chatId}:USDC Erc20"),
                    InlineKeyboardButton.WithCallbackData("GUSD Erc20",$"FinishDeposit:{value}:{chatId}:GUSD Erc20"),
                    InlineKeyboardButton.WithCallbackData("BUSD Erc20",$"FinishDeposit:{value}:{chatId}:BUSD Erc20")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Tether Trc20", $"FinishDeposit:{value}:{chatId}:Tether Trc20"),
                    InlineKeyboardButton.WithCallbackData("Tether Erc20",$"FinishDeposit:{value}:{chatId}:Tether Erc20"),
                    InlineKeyboardButton.WithCallbackData("DAI Erc20",$"FinishDeposit:{value}:{chatId}:DAI Erc20")
                },
            });
            await bot.EditMessageTextAsync(e.Message.Chat.Id, e.Message.MessageId, " Select Payment Method :", replyMarkup: continueKeyboardMarkup, cancellationToken: ct);

        }
        if (e.Data.StartsWith("FinishDeposit"))
        {
            await bot.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, ct);
            var paymentKeyboard = new InlineKeyboardMarkup(new[] { InlineKeyboardButton.WithUrl("Pay", "https://google.com"), });
            var splitData = e.Data.Split(":");
            var value = splitData[1];
            var chatId = splitData[2];
            var paymentMethod = splitData[3];
            await bot.SendTextMessageAsync(e.Message.Chat.Id,
                $"<b>Account ID:</b><i>{e.From.Id}</i>\n <b>Payment Method:</b> <i>{paymentMethod}</i> \n <b>Amount Deposit:</b> <i>{value}</i>\n <b>Current Balance:</b> <i> 1.12$ </i>",
                replyMarkup: paymentKeyboard,
                parseMode: ParseMode.Html,
                cancellationToken: ct);
        }
        #endregion

        #region Withdrow

        if (e.Data.StartsWith("WithDraw"))
        {
            var splitData = e.Data.Split(":");
            var value = splitData[1];
            var chatId = splitData[2];
            var paymentKeyboardMarkup = new InlineKeyboardMarkup(new[] {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Payeer",$"FinishWithDraw:{value}:{chatId}:Payeer"),
                    InlineKeyboardButton.WithCallbackData("WebMoney",$"FinishWithDraw:{value}:{chatId}:WebMoney"),
                    InlineKeyboardButton.WithCallbackData("TUSD Erc20", $"FinishWithDraw:{value}:{chatId}:TUSD Erc20")
                },

                new [] { InlineKeyboardButton.WithCallbackData("Perfect Money",$"FinishWithDraw:{value}:{chatId}:Perfect Money"), },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("USDC Erc20",$"FinishWithDraw:{value}:{chatId}:USDC Erc20"),
                    InlineKeyboardButton.WithCallbackData("GUSD Erc20",$"FinishWithDraw:{value}:{chatId}:GUSD Erc20"),
                    InlineKeyboardButton.WithCallbackData("BUSD Erc20",$"FinishWithDraw:{value}:{chatId}:BUSD Erc20")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Tether Trc20", $"FinishWithDraw:{value}:{chatId}:Tether Trc20"),
                    InlineKeyboardButton.WithCallbackData("Tether Erc20",$"FinishWithDraw:{value}:{chatId}:Tether Erc20"),
                    InlineKeyboardButton.WithCallbackData("DAI Erc20",$"FinishWithDraw:{value}:{chatId}:DAI Erc20")
                },
            });
            await bot.SendTextMessageAsync(e.Message.Chat.Id," Select Payment Method :", replyMarkup: paymentKeyboardMarkup, cancellationToken: ct);

        }


        #endregion
    }

    private async Task HandleMessageAsync(ITelegramBotClient bot, Message e, CancellationToken ct)
    {
        Console.WriteLine($"Message: '{e.Chat.Id}' from :[{e.Text}].");
        if (e.Text == "/start")
        {
            var keyBoardMarkup = new InlineKeyboardMarkup(CreateInlineButton(Dependencies.LanguagesList));
            await bot.SendTextMessageAsync(e.Chat.Id, "Select Language Please : ", replyMarkup: keyBoardMarkup, cancellationToken: ct);
        }

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

            #region Login
            case "Login":
                var sendMainMenu = await bot.SendTextMessageAsync(e.Chat.Id, "<b>You Are In Main Menu :</b> ", ParseMode.Html,
                    replyMarkup: MainMenuKeyboardMarkup, cancellationToken: ct);
                await bot.DeleteMessageAsync(e.Chat.Id, e.MessageId, ct);
                break;
            #endregion

            #region Support
            case "Support":
                var subMenuMessage = await bot.SendTextMessageAsync(e.Chat.Id, "<b> Support Sub-Menu :</b> ", ParseMode.Html, cancellationToken: ct);
                var keyBoardMarkup = new InlineKeyboardMarkup(new[] { new[]
                {
                    InlineKeyboardButton.WithUrl("Ticket", "https://google.com"),
                    InlineKeyboardButton.WithUrl("Donate", "https://buyMeACoffee.com"),
                    InlineKeyboardButton.WithCallbackData("Back",$"Back:MainMenu:{subMenuMessage.MessageId}:{subMenuMessage.Chat.Id}"),
                } });
                await bot.EditMessageReplyMarkupAsync(subMenuMessage.Chat.Id, subMenuMessage.MessageId, keyBoardMarkup, ct);
                break;
            #endregion

            #region Deposit
            case "Deposit":
                var valuesKeyboardMarkup = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("15 USD",$"Deposit:15:{e.Chat.Id}"),
                    InlineKeyboardButton.WithCallbackData("25 USD",$"Deposit:25:{e.Chat.Id}"),
                    InlineKeyboardButton.WithCallbackData("50 USD",$"Deposit:50:{e.Chat.Id}"),
                    InlineKeyboardButton.WithCallbackData("100 USD",$"Deposit:100:{e.Chat.Id}"),
                });
                await bot.SendTextMessageAsync(e.Chat.Id, "<b>Select A Value To Continue :</b> ", ParseMode.Html, replyMarkup: valuesKeyboardMarkup, cancellationToken: ct);
                break;
            #endregion

            #region WithDraw
            case "Withdraw":
                var amountKeyboardMarkup = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("10 USD",$"WithDraw:10:{e.Chat.Id}"),
                    InlineKeyboardButton.WithCallbackData("25 USD",$"WithDraw:25:{e.Chat.Id}"),
                    InlineKeyboardButton.WithCallbackData("50 USD",$"WithDraw:50:{e.Chat.Id}"),
                    InlineKeyboardButton.WithCallbackData("100 USD",$"WithDraw:100:{e.Chat.Id}"),
                    InlineKeyboardButton.WithCallbackData("All",$"WithDraw:All:{e.Chat.Id}"),
                });
                await bot.SendTextMessageAsync(e.Chat.Id, "<b>Select A Value To Continue :</b> ", ParseMode.Html, replyMarkup: amountKeyboardMarkup, cancellationToken: ct);
                break;
                #endregion
        }
    }
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
    #endregion

    #region Methods
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
    #endregion
}