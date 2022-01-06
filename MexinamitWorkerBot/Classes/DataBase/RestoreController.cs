using MexinamitWorkerBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MexinamitWorkerBot.Classes.DataBase;

public class RestoreController
{
    public async Task<RestoreData> GetRequestAsync(long userId)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var findRequest = await db.RestoreData.FirstOrDefaultAsync(p => p.UserId == userId.ToString());
            return findRequest;
        }
        catch (Exception e)
        {
            Log.Error(e, "GetRequestAsync");
            return null;
        }
    }
    public async Task NewForgetPasswordAsync(RestoreData restore)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            await db.RestoreData.AddAsync(restore);
            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "NewForgetPasswordAsync");
        }
    }
    public async Task UpdateForgetPassRequestAsync(RestoreData restore)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var findRequest = await db.RestoreData.FirstOrDefaultAsync(p => p.UserId == restore.UserId);
            if (findRequest is null) return;

            if (restore.Email is not null)
            {
                findRequest.Email = restore.Email;
            }

            if (restore.Password is not null)
            {
                findRequest.Password = restore.Password;
            }

            if (restore.UserName is not null)
            {
                findRequest.UserName = restore.UserName;
            }

            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "UpdateForgetPassRequestAsync");
        }
    }
    public async Task DeleteRequestAsync(RestoreData restore)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var find = await db.RestoreData.FirstOrDefaultAsync(p => p.UserId == restore.UserId);
            if (find is null) return;
            db.RestoreData.Remove(find);
            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "NewForgetPasswordAsync");
        }
    }

}