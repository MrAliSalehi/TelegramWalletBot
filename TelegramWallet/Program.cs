using System.Runtime.InteropServices.ComTypes;
using TelegramWallet.Classes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

 await new Bot().RunAsync();
Console.WriteLine("Bot Executed");
app.Run();
