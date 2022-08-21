using CronSTD;
using trello.backupper.cli;

namespace trello.backupper.docker
{
    public class Worker : BackgroundService
    {
        private readonly CronDaemon _cronDaemon = new();
        private readonly ILogger<Worker> _logger;
        private readonly BackupCli _backupCli;

        private bool _isBackupRunning;

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

            // Start first backup right away
            CronAction();
            
            await Task.Delay(-1, stoppingToken);
        }

        private async void CronAction()
        {
            if (_isBackupRunning)
            {
                _logger.LogWarning("{time}: Backup already running. Skipping this run...", DateTimeOffset.Now);
                return;
            }

            try
            {
                _isBackupRunning = true;
                
                var appKey = Environment.GetEnvironmentVariable("TRELLO_APP_KEY") ?? throw new ArgumentException("Please provide a valid trello app key. See readme for details.");
                var token = Environment.GetEnvironmentVariable("TRELLO_TOKEN") ?? throw new ArgumentException("Please provide a valid trello token. See readme for details.");
                var webHookUrl = Environment.GetEnvironmentVariable("WEB_HOOK_URL");
                var webHookHttpMethod = Environment.GetEnvironmentVariable("WEB_HOOK_HTTP_METHOD");

                _logger.LogInformation("--> Backup started at {time}", DateTimeOffset.Now);
                var cliArguments = CreateCliArguments(appKey, token, webHookUrl, webHookHttpMethod).ToArray();
                await _backupCli.Run(cliArguments);
                _logger.LogInformation("--> Backup done at {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during backup. This will not prevent the next run.");
            }
            finally
            {
                _isBackupRunning = false;
            }
        }
        
        private static IEnumerable<string> CreateCliArguments(string appKey, string token, string? webHookUrl, string? webHookHttpMethod)
        {
            yield return "--app-key";
            yield return appKey;
            yield return "--token";
            yield return token;
            yield return "backup";
            yield return "/backup";

            if (!string.IsNullOrWhiteSpace(webHookUrl))
            {
                yield return "--web-hook-url";
                yield return webHookUrl;

                if (!string.IsNullOrWhiteSpace(webHookHttpMethod))
                {
                    yield return "--web-hook-http-method";
                    yield return webHookHttpMethod;
                }
            }
        }
    }
}