using MexinamitWorkerBot.Classes;
using Serilog;
using Serilog.Core;
using Serilog.Events;

try
{
    Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft", LogEventLevel.Information).Enrich.FromLogContext()
        .WriteTo.File("logs.txt", rollingInterval: RollingInterval.Day).WriteTo.Console()
        .CreateLogger();

    Log.Information("Api Started");

    Console.WriteLine($"Application Running");
    var host = Host.CreateDefaultBuilder(args)
        .UseSystemd().ConfigureServices(services =>
        {
            services.AddHostedService<Bot>();
        })
        .Build();
    await host.RunAsync();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


