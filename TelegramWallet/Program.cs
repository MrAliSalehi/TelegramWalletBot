using TelegramWallet.Classes;
using TelegramWallet.Classes.Extensions;

Console.WriteLine("Application Running");
//var builder = WebApplication.CreateBuilder(args);

var host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();
if (!File.Exists("Log.txt"))
{
    File.Create("Log.txt");
    Console.WriteLine("Log File Created");
}
await host.RunAsync();