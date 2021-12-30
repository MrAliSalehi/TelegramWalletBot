using System;
using System.Threading.Tasks;
using MexinamitWorkerBot.Api;
using MexinamitWorkerBot.Api.Models.ApiVerifyUser;
using NUnit.Framework;

namespace TestProject1
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            var x = new ApiController();
            var f = await x.TwoStepVerifyAsync(new ApiVerifyModel() { password ="123456" ,username = "49195512" });
            Console.WriteLine($"{f.status}");
        }
    }
}