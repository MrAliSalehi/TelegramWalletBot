using TelegramWallet.Classes;
Console.WriteLine("Application Running");
var builder = WebApplication.CreateBuilder(args);
var app =  builder.Build();
app.UseHttpsRedirection();

Console.WriteLine("Executing Bot");
await new Bot().RunAsync();
Console.WriteLine("Bot Executed");
if (!File.Exists("Log.txt"))
{
    File.Create("Log.txt");
    Console.WriteLine("Log File Created");
}
app.Run();
