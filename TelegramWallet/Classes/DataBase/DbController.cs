using Microsoft.EntityFrameworkCore;
using TelegramWallet.Database.Models;

namespace TelegramWallet.Classes.DataBase;

public class DbController
{
    public async Task<User> GetUserAsync(User user)
    {
        try
        {
            await using var context = new TelegramWallet_DbContext();
            var userExists = await context.Users.FirstOrDefaultAsync(p => p.UserId == user.UserId);

            return userExists ?? new User();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new User();
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