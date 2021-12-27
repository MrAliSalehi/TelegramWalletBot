using MexinamitWorkerBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MexinamitWorkerBot.Classes.DataBase;

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

    public async Task<bool> AddChannelAsync(ForceJoinChannel channel)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var searchChannel = await db.ForceJoinChannels.AnyAsync(p => p.ChId == channel.ChId);
            if (searchChannel) return false;
            await db.ForceJoinChannels.AddAsync(channel);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> RemoveChannelAsync(ForceJoinChannel channel)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var findChannel = await db.ForceJoinChannels.SingleOrDefaultAsync(p => p.ChId == channel.ChId);
            if (findChannel is null)
                return false;

            db.ForceJoinChannels.Remove(findChannel);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}