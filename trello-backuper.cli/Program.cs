using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using trello.backuper.lib;
using trello.backupper.cli;
using trello.backupper.cli.WebHook;

var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging(configure => configure.AddConsole());

serviceCollection.AddSingleton<BackupCli>();
serviceCollection.AddSingleton<BackupCreator>();
serviceCollection.AddSingleton<BackupCommand>();
serviceCollection.AddSingleton<WebHookCaller>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var backupCli = serviceProvider.GetRequiredService<BackupCli>();
return await backupCli.Run(args);