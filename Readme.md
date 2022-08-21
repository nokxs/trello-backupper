# Trello Backupper, the backup creator for Trello boards

This tool is designed to do a full backup of Trello boards, including attachments.

It is available as

- dotnet tool
- docker container

## First steps

1. Get the app key from https://trello.com/app-key
2. Use the app key to get the token: https://trello.com/1/authorize?scope=read&expiration=never&name=backup&key=REPLACE_WITH_YOUR_API_KEY&response_type=token <-- do not forget to insert your API Key

## Install

You can use the tool as dotnet tool or as docker container, which will periodically do backups.

### As dotnet tool

Run `dotnet tool install --global trello-backuper.cli` to install the tool globally. This requires installed [dotnet 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

After installation, run `trello-backuper --app-key [Your App key] --token [Your token] backup [Your target directory]`. This will do
a full backup of your trello account.

To see additional options run `trello-backupper backup --help`.

### As docker container

The docker image is available on [DockerHub](https://hub.docker.com/r/liofly/trello-backupper). To run it, three environment variables are necessary:

- `TRELLO_APP_KEY` (Required): The app key retrieved from trello. See first steps section in this readme.
- `TRELLO_TOKEN` (Required): The token retrieved from trelle. See first steps section in this readme.
- `CRON` (Required): A Crontab pattern to specify when the backup should run. Tip: Use https://crontab.guru/ to define the pattern.
- `WEB_HOOK_URL (Optional): Calls the url at specific events. See web hook section for more details.`
- `WEB_HOOK_HTTP_METHOD (Optional): Defines the http method used for calling the web hook. See web hook section for more details.`

Backups get stored in `/backup` in the container. Mount it to your local disk for easy access of the backup.

Save the folowwing as `docker-compose.yml` and start it with `docker-compose up -d`. It will start a first backup immediatly and then do a backup every night at 2 AM:
```
version: '3.4'

services:
  trello-backuper.docker:
    image: liofly/trello-backupper
    restart: always
    volumes:
    - ./localBackup:/backup
    environment:
        TRELLO_TOKEN: "[Your trello token]"
        TRELLO_APP_KEY: "[Your trello app key]"
        CRON: "0 2 * * *"
```

## Web Hooks

Both, the dotnet tool and the docker container, have the ability to call a web hook on specific events: 

- The backup is started
- The backup is finished
- An error occured during backup

The specified url will be called as is. To add message details, add `$message$` to your url. It will be subsitute with
the message text, which is URL encoded.

**Example:** 

The URL 

```
http://my-server/my/path?value=$message$
``` 

will be substituted to

```
http://my-server/my/path?value=Backup%20started
```

### Http Method

By default http GET is used. See the docs in the dotnet tool or docker section on how to specify the method.

Both allow specifying the following methods:

- GET
- POST
- PUT
- PATCH

The body is always empty.
