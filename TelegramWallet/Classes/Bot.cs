using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramWallet.Classes;

public class Bot
{

    public async Task RunAsync()
    {
        var botClient = new TelegramBotClient(Dependencies.BotInformation.Token);

        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };
        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token);
        var me = await botClient.GetMeAsync(cts.Token);
        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();
    }
    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update e, CancellationToken ct)
    {
        
        // Only process Message updates: 
        if (e.Type != UpdateType.Message)
            return;
        // Only process text messages
        if (e.Message!.Type != MessageType.Text)
            return;

        await bot.SendTextMessageAsync(e.Message.Chat.Id,
            Dependencies.LangDictionary[Dependencies.Languages.Spanish]["Welcome"],cancellationToken:ct);

        Console.WriteLine($"Message: '{e.Message.Chat.Id}' from :[{e.Message.Text}].");

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
}