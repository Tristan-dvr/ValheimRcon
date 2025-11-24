using System;
using System.Collections.Concurrent;
using System.IO;

namespace ValheimRcon
{
    internal class DiscordService : IDisposable
    {
        private const string Name = "RCON";

        private readonly IDisposable _thread;
        private readonly ConcurrentQueue<Message> _queue = new ConcurrentQueue<Message>();

        public DiscordService()
        {
            _thread = ThreadingUtil.RunPeriodicalInSingleThread(SendQueuedMessage, 333);
        }

        public void SendResult(string url, string text, string filePath)
        {
            if (string.IsNullOrEmpty(url))
                return;

            _queue.Enqueue(new Message
            {
                url = url,
                filePath = filePath,
                text = text,
            });
        }

        private void SendQueuedMessage()
        {
            if (!_queue.TryDequeue(out Message message) || string.IsNullOrEmpty(message.url))
                return;

            try
            {
                var filePath = message.filePath;
                if (string.IsNullOrEmpty(filePath))
                {
                    Discord.Send(message.text, Name, message.url);
                }
                else
                {
                    var fileName = Path.GetFileName(filePath);
                    var fileExtension = Path.GetExtension(filePath);

                    Discord.SendFile(message.text, fileName, fileExtension, filePath, Name, message.url);
                }

                Log.Debug($"Sent to discord (symbols:{message.text.Length})");
            }
            catch (Exception ex)
            {
                Log.Error($"Cannot send to discord (symbols:{message.text.Length})\n{ex}");
            }
        }

        public void Dispose()
        {
            _thread.Dispose();
        }

        private struct Message
        {
            public string url;
            public string text;
            public string filePath;
        }
    }
}
