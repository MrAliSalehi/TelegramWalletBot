using Microsoft.EntityFrameworkCore;
using TelegramWallet.Database.Models;

namespace TelegramWallet.Classes.DataBase;

public class ForceJoinController
{
    public async Task<List<ForceJoinChannel>> GetChannelsAsync()
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var getChannels = await db.ForceJoinChannels.ToListAsync();
            return getChannels;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<ForceJoinChannel>();
        }
    }
}