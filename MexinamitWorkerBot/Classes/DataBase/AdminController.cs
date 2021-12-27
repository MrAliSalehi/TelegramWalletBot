using MexinamitWorkerBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MexinamitWorkerBot.Classes.DataBase;

public class AdminController
{
    public async Task<Admin> GetAdminAsync(string userId)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var findAdmin = await db.Admins.FirstOrDefaultAsync(p => p.UserId == userId);
            return findAdmin ?? new Admin();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new Admin();
        }
    }
    public async Task<bool> AddOwnerAsync(Admin admin)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var exists = await db.Admins.AnyAsync(p => p.UserId == admin.UserId);
            if (exists)
                return false;
            else
            {
                await db.Admins.AddAsync(admin);
                await db.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    public async Task<bool> DeleteOwnerAsync(Admin admin)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var exists = await db.Admins.SingleOrDefaultAsync(p => p.UserId == admin.UserId);
            if (exists is null)
                return false;

            db.Admins.Remove(exists);
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    public async Task<List<Admin>> GetAllAdminsAsync()
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var getAdmins = await db.Admins.ToListAsync();
            return getAdmins;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<Admin>();
        }
    }
    public async Task<bool> UpdateAdminAsync(Admin admin)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var findUser = await db.Admins.FirstOrDefaultAsync(p => p.UserId == admin.UserId);
            if (findUser is null) return false;

            if (admin.CommandSteps is not null)
            {
                findUser.CommandSteps = admin.CommandSteps;
            }

            if (admin.CurrentQuestionLanguage is not null)
            {
                findUser.CurrentQuestionLanguage = admin.CurrentQuestionLanguage;
            }
            if (admin.QuestionSteps is not null)
            {
                findUser.QuestionSteps = admin.QuestionSteps;
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