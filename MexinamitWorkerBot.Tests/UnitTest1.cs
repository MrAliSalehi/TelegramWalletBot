using System;
using System.Threading.Tasks;
using MexinamitWorkerBot.Api;
using MexinamitWorkerBot.Api.Models.ApiSecurity.ApiSecurityEncrypt;
using MexinamitWorkerBot.Classes;
using NUnit.Framework;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MexinamitWorkerBot.Tests
{
    public class Tests
    {
        private ApiController _api;
        private ITelegramBotClient bot;
        [SetUp]
        public void Setup()
        {
            _api = new ApiController();
            bot = new TelegramBotClient("5016105194:AAHzTZx51UwTilPSXmFD5YChY-J_Wxhr04c");
        }

        [Test]
        public async Task method1()
        {
            //todo: create payment
            //todo : get amount-currency-transID-
            var msg = await bot.SendTextMessageAsync(1127927726, "<b>Pending Request ...</b>", ParseMode.Html);
            await Task.Delay(5000);
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiIxIiwianRpIjoiNWUyY2U3NGI5NzcwYzc3ZjA5MzMxOTc3MzYyMDExNWRiNGM2ODhiZDFlNDRjMTc2ODI0MWViYTQzYTllMjZlYTAxODAyNmNkZjA4Yjc2YWQiLCJpYXQiOjE2NDA2MTY3MzEuMDgyOTA2MDA3NzY2NzIzNjMyODEyNSwibmJmIjoxNjQwNjE2NzMxLjA4MjkxMTAxNDU1Njg4NDc2NTYyNSwiZXhwIjoxNjcyMTUyNzMxLjA4MDcxMzAzMzY3NjE0NzQ2MDkzNzUsInN1YiI6IjUiLCJzY29wZXMiOltdfQ.EHZjdDkHto4o0x20j8RPG83zuORpE3BdbEJ1lXqoxPK0keR3nTRNQXxHtVPumohgCc8Jz48OfMGGNOZ3FyvhcnxEx2-YBBOiXVniF7K5Bm61CPJbiLv7CZz823uDZx5TfsxP5ZMsHOAlr6F9tZCJ8mJYxU7TcKyTYDLsoJNeLbgct31AEHMhyCV6txVUbwtDRlBhZ9ou4W7zMLhmmOJGBGBhSivoFNhkKm38uSin4gO8wwDsF8GC9uaDoA5dbsM1ARBqeHTwFMm0WFid2uGRMIMEycEJmU0m8lqLIzo4h3wA0G8NU8dDFZiH0orbJUWAgwryKWoI1mDK_xF1iPACEPJK6_ss3qC2LqfqKVl1Wv5uvV4yEPwXYLKkHlrxOzoM_e1VBhukmsYWTl67TgWwbuhTrjGCQh_iWppxay85nv1pevt3amDuV8OZ4yILh7mSBp3MlQ1tralmDsP3sSI49BQ_HHuGpg-I0kOc_nLkSKv8r5AP6YNq_ZlKTeLKmfFqqR9oKx_dMpz0aWgEKBrfW6VZt3sMgqcMoTvAzhE7lolZfx_spruqQQrWpZd_VN2leUu_P4UGPWnoO-20OJg2e30NQYdbpIl-VBFOEohDRDxCHAXVUgOaIXHoNMHFiTWy95X1ISl4cW1JvS57JsoWV7emUUaFzpspM1J6QLDXkjI";
            
            var payId = await _api.CreatePaymentAsync(token);
            Console.WriteLine($"{Dependencies.PerfectMoneyApiUrl}?key={payId.data.payment_id}");
            var crypt = await _api.EncryptionAsync(new ApiSecurityEncryptModel() {amount ="22.1",currency = "USD",payment = payId.data.payment_id}, token);
            var urlKeyboard = new InlineKeyboardMarkup(new[]
                { InlineKeyboardButton.WithUrl("Process Payment", $"{Dependencies.PerfectMoneyApiUrl}?key={payId.data.payment_id}"), });
            
            await bot.EditMessageTextAsync(msg.Chat.Id, msg.MessageId, "Your Payment Has Been Created\n Continue With Link:", replyMarkup: urlKeyboard);
            Console.WriteLine($"crypt:{crypt.data}");
            Assert.AreNotEqual(null,payId.data.payment_id);
        }
    }
}