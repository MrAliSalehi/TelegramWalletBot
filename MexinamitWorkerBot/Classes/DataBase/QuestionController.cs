using MexinamitWorkerBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MexinamitWorkerBot.Classes.DataBase;

public class QuestionController
{
    public async Task<Question?> CreateQuestionAsync(Question question)
    {
        try
        {
            await using var context = new TelegramWallet_DbContext();
            await context.Questions.AddAsync(question);
            await context.SaveChangesAsync();
            var getQuestion = await context.Questions.SingleOrDefaultAsync(p =>
                p.CreatorId == question.CreatorId && p.Language == question.Language && p.Question1 == null &&
                p.Answer == null);

            return getQuestion;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<Question?> GetQuestionByQuestionAsync(Question question)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var find = await db.Questions.FirstOrDefaultAsync(p => p.Id == question.Id);
            return find;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new Question();
        }
    }
    public async Task<List<Question>> QuestionListByCountryAsync(string country)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var getList = await db.Questions.Where(p => p.Language == country).ToListAsync();
            return getList;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<Question>();
        }
    }
    public async Task<bool> RemoveQuestionAsync(string id)
    {
        try
        {
            await using var context = new TelegramWallet_DbContext();
            var findQuestion = await context.Questions.SingleOrDefaultAsync(p => p.Id == Convert.ToInt32(id));
            context.Questions.Remove(findQuestion);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    public async Task<bool> UpdateQuestionAsync(Question question)
    {
        try
        {
            await using var db = new TelegramWallet_DbContext();
            var findQuestion = await db.Questions.FirstOrDefaultAsync(p =>
                p.CreatorId == question.CreatorId && p.Answer == null && p.Question1 == null);
            if (findQuestion is null) return false;
            findQuestion.Answer = question.Answer;
            findQuestion.Language = question.Language;
            findQuestion.Question1 = question.Question1;
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