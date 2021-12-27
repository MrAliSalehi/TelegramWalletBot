using TelegramWallet.Classes;
using TelegramWallet.Classes.Extensions;

Console.WriteLine("Application Running");
//var builder = WebApplication.CreateBuilder(args);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();



Console.WriteLine("Executing Bot");
await new Bot().RunAsync();
Console.WriteLine("Bot Executed");
if (!File.Exists("Log.txt"))
{
    File.Create("Log.txt");
    Console.WriteLine("Log File Created");
}
await host.RunAsync();