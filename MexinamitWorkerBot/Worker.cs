using TelegramWallet.Classes;

namespace MexinamitWorkerBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private Bot botClient;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            botClient = new Bot();
            await botClient.RunAsync();
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.Log(LogLevel.Information,"Bot Is Running");
                await Task.Delay(60 * 1000, stoppingToken);
            }
        }
    }
}