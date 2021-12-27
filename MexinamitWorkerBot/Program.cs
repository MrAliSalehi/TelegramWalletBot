using MexinamitWorkerBot;
using MexinamitWorkerBot.Classes;

if (!File.Exists("Log.txt"))
{
    File.Create("Log.txt");
    Console.WriteLine("Log File Created");
}

Console.WriteLine("Application Running");
var host = Host.CreateDefaultBuilder(args)
    .UseSystemd().ConfigureServices(services =>
    {
        services.AddHostedService<Bot>();
    })
    .Build();

await host.RunAsync();