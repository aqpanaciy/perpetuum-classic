using Perpetuum.Accounting.Characters;
using Perpetuum.Log;

namespace Perpetuum.Common.Loggers
{
    public class ChatLogEvent : LogEventBase
    {
        public string Message { get; private set; }

        public ChatLogEvent(string message)
        {
            Message = message;
        }
    }

    public class ChatLogFormatter : ILogEventFormatter<ChatLogEvent,string>
    {
        public string Format(ChatLogEvent logEvent)
        {
            return $"[{logEvent.Timestamp:HH:mm:ss}] {logEvent.Message}";
        }
    }

    public static class ChatLoggerExtensions
    {
        public static void LogMessage(this IPLogger<ChatLogEvent> logger,Character sender,string message)
        {
            logger.Log(new ChatLogEvent($"<{sender.Nick}> {message}"));
        }
    }

    public delegate IPLogger<ChatLogEvent> ChatLoggerFactory(string directory, string filename);
}