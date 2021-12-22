using System.Runtime.InteropServices.ComTypes;
using TelegramWallet.Classes;

var builder = WebApplication.CreateBuilder(args);
var app =  builder.Build();
app.UseHttpsRedirection();
await new Bot().RunAsync();
Console.WriteLine("Bot Executed");
app.Run();
