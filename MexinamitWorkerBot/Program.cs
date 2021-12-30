using MexinamitWorkerBot.Classes;
using Version = MexinamitWorkerBot.Version.Version;

if (!File.Exists("Log.txt"))
{
    File.Create("Log.txt");
    Console.WriteLine("Log File Created");
}

//if (!File.Exists("var.doNotTouchMe"))
//{
//    File.Create("var.doNotTouchMe");
//    Console.WriteLine("Ver. Created");
//}
//var getVersion = await Version.HandelVersionAsync(new CancellationToken());

Console.WriteLine($"Application Running");
var host = Host.CreateDefaultBuilder(args)
    .UseSystemd().ConfigureServices(services =>
    {
        services.AddHostedService<Bot>();
    })
    .Build();
await host.RunAsync();
