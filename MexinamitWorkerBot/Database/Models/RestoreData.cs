namespace MexinamitWorkerBot.Database.Models;

public partial class RestoreData
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}