using trello_backuper.cli;
using trello_backuper.docker;
using trello_backuper.lib;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<BackupCreator>();
        services.AddSingleton<BackupCommand>();
        services.AddSingleton<BackupCli>();
    })
    .Build();

await host.RunAsync();
