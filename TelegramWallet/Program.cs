using System.Reflection;
using System.Text;
using TelegramWallet.Classes;


 var tokenSource = new CancellationTokenSource();
 var outFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "output");
 var outFilePath = Path.Combine(outFolderPath, $"{DateTime.Now:yyyy-MM-dd}.txt");
 AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
 {
     File.AppendAllLines(outFilePath, new List<String>() { "------------------- PROC EXIT SIGNAL ------------------- " }, Encoding.UTF8);
     tokenSource.Cancel();
 };
 if (!Directory.Exists(outFolderPath))
 {
     Directory.CreateDirectory(outFolderPath);
 }
 Console.WriteLine($"Output file: {outFilePath}");
 File.AppendAllLines(outFilePath, new List<String>() { "------------------- SERVICE START ------------------- " }, Encoding.UTF8);
 while (!tokenSource.Token.IsCancellationRequested)
 {
     File.AppendAllLines(outFilePath, new List<String>() { DateTime.Now.ToString() }, Encoding.UTF8);
     Console.WriteLine(DateTime.Now);
     Thread.Sleep(5000);
 }
 File.AppendAllLines(outFilePath, new List<String>() { "------------------- SERVICE STOP ------------------- " }, Encoding.UTF8);

 
 
 var builder = WebApplication.CreateBuilder(args);
var app =  builder.Build();
app.UseHttpsRedirection();


await new Bot().RunAsync();
Console.WriteLine("Bot Executed");
if (!File.Exists("Log.txt"))
{
    File.Create("Log.txt");
    Console.WriteLine("Log File Created");
}
app.Run();
