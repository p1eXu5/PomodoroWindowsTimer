using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

Console.Write("Telegram bot token:");
string? token = Console.ReadLine();

while (token is null || !Regex.IsMatch(token, "^\\d{10}\\:.+"))
{
    Console.WriteLine("Token is not correct!");
    Console.Write("Telegram bot token:");
    token = Console.ReadLine();
}

var botClient = new TelegramBotClient(token);

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

var semaphore = new SemaphoreSlim(0, 1);

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    errorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);


var me = await botClient.GetMe();

Console.WriteLine($"Start listening for @{me.Username}");
Console.WriteLine("Send something to bot to get Chat ID.");

semaphore.Wait();

// Send cancellation request to stop bot
cts.Cancel();
semaphore.Dispose();

Console.WriteLine("Press any key to exit...");
Console.ReadKey(true);

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    var text = $"Chat ID is {chatId}.";
    Console.WriteLine(text);

    // Echo received message text
    Message sentMessage = await botClient.SendMessage(
        chatId: chatId,
        text: text,
        cancellationToken: cancellationToken);

    semaphore.Release();
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}