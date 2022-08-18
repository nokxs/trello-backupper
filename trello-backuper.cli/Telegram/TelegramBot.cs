using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace trello_backuper.cli.Telegram
{
    public class TelegramCredentialManager
    {
        public bool IsPasswordCorrect(string password)
        {
            // TODO: Get password as option and save it only in memory
            throw new NotImplementedException();
        }

        public void TryAddChatId(long chatId)
        {
            // TODO: Remember all chat ids in local config file. Every message is send to all these chat ids. 
            throw new NotImplementedException();
        }
    }

    public class TelegramBot
    {
        private readonly ILogger<TelegramBot> _logger;
        private readonly TelegramCredentialManager _telegramCredentialManager;
        private TelegramBotClient _botClient;

        public TelegramBot(ILogger<TelegramBot> logger, TelegramCredentialManager telegramCredentialManager)
        {
            _logger = logger;
            _telegramCredentialManager = telegramCredentialManager;
        }

        public async Task EnsureOneUserIsRegisteredAsync()
        {
            // TODO: Wait till at least one user is correctly registered. Log message, that we wait here
            throw new NotImplementedException();
        }

        public void StartListening(string telegramToken)
        {
            _botClient = new TelegramBotClient(telegramToken);
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions
            );

            _logger.LogInformation("Telegram bot connected. Started listening for telegram messages.");
        }

        public async Task TrySendMessage(string message)
        {
            // TODO: Send a message to all registed chat ids. Do nothing, if telegram is not enabled or no chat id is known
            throw new NotImplementedException();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            if (!messageText.StartsWith("/password"))
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"I don't know what to do with '{messageText}'. Log in by sending '/password <your password>'",
                    cancellationToken: cancellationToken);
                return;
            }

            var password = messageText.Remove(0, "/password".Length).Trim();

            if (_telegramCredentialManager.IsPasswordCorrect(password))
            {
                _telegramCredentialManager.TryAddChatId(chatId);

                // Echo received message text
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Password ok!",
                    cancellationToken: cancellationToken);
                return;
            }

            // Echo received message text
             await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Password wrong!",
                cancellationToken: cancellationToken);
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // TODO: Log error message with logger
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
