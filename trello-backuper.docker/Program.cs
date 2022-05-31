using trello_backuper.cli;
using trello_backuper.docker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<BackupCli>();
    })
    .Build();

await host.RunAsync();
