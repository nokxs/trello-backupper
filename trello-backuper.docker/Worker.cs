using CronSTD;
using trello_backuper.cli;

namespace trello_backuper.docker
{
    public class Worker : BackgroundService
    {
        private readonly CronDaemon _cronDaemon = new();
        private readonly ILogger<Worker> _logger;
        private readonly BackupCli _backupCli;

        public Worker(ILogger<Worker> logger, BackupCli backupCli)
        {
            _logger = logger;
            _backupCli = backupCli;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cron = Environment.GetEnvironmentVariable("CRON") ?? throw new ArgumentException("Please provide a valid cron expression. See readme for details.");
            _logger.LogInformation($"Running with cron pattern {cron}");

            _cronDaemon.AddJob(cron, CronAction);
            _cronDaemon.Start();
            await Task.Delay(-1, stoppingToken);
        }

        private async void CronAction()
        {
            var appKey = Environment.GetEnvironmentVariable("TRELLO_APP_KEY") ?? throw new ArgumentException("Please provide a valid trello app key. See readme for details.");
            var token = Environment.GetEnvironmentVariable("TRELLO_TOKEN") ?? throw new ArgumentException("Please provide a valid trello token. See readme for details.");

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await _backupCli.Run(new[] { "--app-key", appKey, "--token", token, "backup", "/backup" });
        }
    }
}