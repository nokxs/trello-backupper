using trello.backuper.lib;
using trello.backupper.cli;
using trello.backupper.cli.WebHook;
using trello.backupper.docker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<BackupCreator>();
        services.AddSingleton<BackupCommand>();
        services.AddSingleton<BackupCli>();
        services.AddSingleton<WebHookCaller>();
    })
    .Build();

await host.RunAsync();
