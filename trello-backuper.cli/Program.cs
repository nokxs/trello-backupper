﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using trello_backuper.cli;
using trello_backuper.cli.WebHook;
using trello_backuper.lib;

var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging(configure => configure.AddConsole());

serviceCollection.AddSingleton<BackupCli>();
serviceCollection.AddSingleton<BackupCreator>();
serviceCollection.AddSingleton<BackupCommand>();
serviceCollection.AddSingleton<WebHookCaller>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var backupCli = serviceProvider.GetRequiredService<BackupCli>();
return await backupCli.Run(args);