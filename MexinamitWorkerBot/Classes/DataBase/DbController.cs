using JetBrains.Annotations;
using MexinamitWorkerBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MexinamitWorkerBot.Classes.DataBase;

public class DbController
{
    public async Task<User?> GetUserAsync(User user)
    {
        try
        {
            await using var context = new TelegramWallet_DbContext();
            var userExists = await context.Users.FirstOrDefaultAsync(p => p.UserId == user.UserId);

            return userExists ?? null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    public async Task<bool> LogoutAsync(string userId)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var findUser = await db.Users.SingleOrDefaultAsync(p => p.UserId == userId);
            if (findUser is null) return false;
            findUser.Token = null;
            findUser.Language = null;
            findUser.UserPass = null;
            findUser.LoginStep = 0;
            findUser.WithDrawStep = 0;
            findUser.DepositAmount = null;
            findUser.DepositStep = 0;
            findUser.WitchDrawPaymentMethod = null;
            findUser.WithDrawAccount = null;
            findUser.WithDrawAmount = null;
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    public async Task<bool> InsertNewUserAsync(User user)
    {
        try
        {
            await using var context = new TelegramWallet_DbContext();
            var userExists = await context.Users.AnyAsync(p => p.UserId == user.UserId);
            if (!userExists)
            {
                await context.AddAsync(user);
                await context.SaveChangesAsync();
            }
            return true;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var findUser = await db.Users.FirstOrDefaultAsync(p => p.UserId == user.UserId);
            if (findUser is null) return false;

            if (user.Language is not null)
            {
                findUser.Language = user.Language;
            }

            if (user.LoginStep is not null)
            {
                findUser.LoginStep = user.LoginStep;
            }

            if (user.WithDrawAccount is not null)
            {
                findUser.WithDrawAccount = user.WithDrawAccount;
            }

            if (user.UserPass is not null)
            {
                findUser.UserPass = user.UserPass;
            }

            if (user.WithDrawStep is not null)
            {
                findUser.WithDrawStep = user.WithDrawStep;
            }

            if (user.WitchDrawPaymentMethod is not null)
            {
                findUser.WitchDrawPaymentMethod = user.WitchDrawPaymentMethod;
            }

            if (user.WithDrawAmount is not null)
            {
                findUser.WithDrawAmount = user.WithDrawAmount;
            }

            if (user.Token is not null)
            {
                findUser.Token = user.Token;
            }

            if (user.DepositStep is not null)
            {
                findUser.DepositStep = user.DepositStep;
            }

            if (user.DepositAmount is not null)
            {
                findUser.DepositAmount = user.DepositAmount;
            }

            if (user.DepositAccount is not null)
            {
                findUser.DepositAccount = user.DepositAccount;
            }

            if (user.ManualAccount is not null)
            {
                findUser.ManualAccount = user.ManualAccount;
            }

            if (user.PublicSteps is not null)
            {
                findUser.PublicSteps = user.PublicSteps;
            }
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    [ItemCanBeNull]
    public async Task<List<string>> GetAllUserIdAsync()
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var getAllUsers = await db.Users.Select(p => p.UserId).ToListAsync();
            return getAllUsers;
        }
        catch (Exception e)
        {
            Log.Error(e, "GetAllUserIdAsync");
            return null;
        }
    }

}