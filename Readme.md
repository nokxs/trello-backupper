# Trello Backupper, the backup creator for Trello boards

This tool is designed to do a full backup of Trello boards, including attachments.

It is available as

- dotnet tool
- docker container

## First steps

1. Get the app key from https://trello.com/app-key
2. Use the app key to get the token: https://trello.com/1/authorize?scope=read&expiration=never&name=backup&key=REPLACE_WITH_YOUR_API_KEY&response_type=token

## Install

You can use the tool as .net tool or as docker-container, which will periodically do backups.

### As dotnet tool

Run `dotnet tool install -g TODO` to install the tool globally.

After installation, run `trello-backuper --app-key [Your App key] --token [Your token] backup [Your target directory]`. This will do
a full backup of your trello account.

To see additional options run `trello-backupper backup --help`.

### As docker container