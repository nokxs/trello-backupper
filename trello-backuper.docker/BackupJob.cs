using Quartz;
using trello_backuper.cli;

namespace trello_backuper.docker;

public class BackupJob : IJob
{
    private readonly ILogger<Worker> _logger;
    private readonly BackupCli _backupCli;

    public BackupJob(ILogger<Worker> logger, BackupCli backupCli)
    {
        _logger = logger;
        _backupCli = backupCli;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var appKey = Environment.GetEnvironmentVariable("TRELLO_APP_KEY") ?? throw new ArgumentException("Please provide a valid trello app key. See readme for details.");
        var token = Environment.GetEnvironmentVariable("TRELLO_TOKEN") ?? throw new ArgumentException("Please provide a valid trello token. See readme for details.");

        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        await _backupCli.Run(new[] { "--app-key", appKey, "--token", token, "backup", "/backup" });
    }
}