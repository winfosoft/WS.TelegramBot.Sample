using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

internal class Program
{
    private static ITelegramBotClient botClient = null!;

    private static async Task Main(string[] args)
    {
        // Initialize the bot client with your token
        botClient = new TelegramBotClient("Your_Bot_Token");

        // Verify the bot is running
        var me = await botClient.GetMe();
        Console.WriteLine($"Bot is running: @{me.Username}");

        // Set up cancellation token to stop the bot
        using var cts = new CancellationTokenSource();

        // Receiver options for updates
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // Receive all update types
        };

        // Start receiving updates
        botClient.StartReceiving(
            HandleUpdateAsync,  // Update handler
            HandleErrorAsync,   // Error handler
            receiverOptions,
            cts.Token
        );

        Console.WriteLine("Press Escape to stop the bot...");
        while (Console.ReadKey(true).Key != ConsoleKey.Escape) { }

        // Stop the bot
        cts.Cancel();
    }

    // Handle incoming updates
    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Text: { } messageText, Chat: { Id: var chatId, Type: var chatType }, From: { Id: var userId } })
            return;

        Console.WriteLine($"Received message in {chatType} chat {chatId} from user {userId}: {messageText}");

        var response = messageText switch
        {
            string s when s.Contains("السلام عليكم", StringComparison.OrdinalIgnoreCase) => "وعليكم السلام ورحمة الله وبركاته",
            string s when s.Contains("السلام عليك", StringComparison.OrdinalIgnoreCase) => "وعليكم السلام ورحمة الله وبركاته",
            string s when s.Contains("كيف الحال", StringComparison.OrdinalIgnoreCase) => "الحمد الله",
            string s when s.Contains("مرحبا", StringComparison.OrdinalIgnoreCase) => "مرحبا بك",
            string s when s.Contains("وشراك", StringComparison.OrdinalIgnoreCase) => "لباس الحمد الله",
            _ => null
        };

        if (response != null)
        {
            // Send the response with reply functionality 
            await botClient.SendMessage(
                chatId: chatId,
                text: response,
                replyParameters: new ReplyParameters
                {
                    MessageId = update.Message.MessageId
                },
                cancellationToken: cancellationToken
            );
        }
    }

    // Handle polling errors
    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
