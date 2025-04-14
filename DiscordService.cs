using System;
using System.Collections.Concurrent;
using System.IO;

namespace ValheimRcon
{
    internal class DiscordService : IDisposable
    {
        private const string Name = "RCON";

        private readonly IDisposable _thread;
        private readonly string _webhook;
        private readonly ConcurrentQueue<Message> _queue = new ConcurrentQueue<Message>();

        public DiscordService(string webhook)
        {
            _webhook = webhook;
            _thread = ThreadingUtil.RunPeriodicalInSingleThread(SendQueuedMessage, 333);
        }

        public void SendResult(string text, string filePath)
        {
            _queue.Enqueue(new Message
            {
                filePath = filePath,
                text = text,
            });
        }

        private void SendQueuedMessage()
        {
            if (string.IsNullOrEmpty(_webhook))
                return;

            if (!_queue.TryDequeue(out Message message))
                return;

            try
            {
                var filePath = message.filePath;
                var result = string.IsNullOrEmpty(filePath)
                    ? Discord.Send(message.text, Name, _webhook)
                    : Discord.SendFile(message.text, Path.GetFileName(filePath), Path.GetExtension(filePath), filePath, Name, _webhook);

                Log.Debug($"Sent to discord {message.text}");
            }
            catch (Exception ex)
            {
                Log.Error($"Cannot send to discord {message.text}\n{ex}");
            }
        }

        public void Dispose()
        {
            _thread.Dispose();
        }

        private struct Message
        {
            public string text;
            public string filePath;
        }
    }
}
