using TelegramWallet.Classes;
using TelegramWallet.Database.Models;

var builder = WebApplication.CreateBuilder(args);
var app =  builder.Build();
app.UseHttpsRedirection();
var canConnect = await new TelegramWallet_DbContext().Database.CanConnectAsync();
while (!canConnect)
{
    Console.WriteLine("Cant Connect To Db\n Connection Problem...");
    Console.ReadLine();
    Console.Clear();
    Console.WriteLine("Creating Manual ConnectionString\nEnter Server Ip:");
    var serverIp = Console.ReadLine();
    Console.WriteLine("DataBase Name:");
    var dbName = Console.ReadLine();
    Console.WriteLine("Instance Name:");
    var instanceName = Console.ReadLine();
    Console.WriteLine("userName: (need Db_Writer & Db_Reader Permissions)");
    var userName = Console.ReadLine();
    Console.WriteLine("Password:");
    var password = Console.ReadLine();
    var checkValue = new List<string>() { serverIp, dbName, instanceName, userName, password }.Any(string.IsNullOrEmpty);
    if (checkValue)
    {
        Console.WriteLine("All Of The Parameters Are Required\n Please Fill Them Again!");
        Console.ReadKey();
        break;
    }
    Console.WriteLine("ReTrying To Connect ...");
    Dependencies.ConnectionString = Dependencies.NewConnectionString(userName, password, serverIp, dbName, instanceName);
    canConnect = await new TelegramWallet_DbContext().Database.CanConnectAsync();
}

await new Bot().RunAsync();
Console.WriteLine("Bot Executed");
if (!File.Exists("\\Log.txt"))
{
    File.Create("\\Log.txt");
    Console.WriteLine("Log File Created");
}
app.Run();
