using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Zomo.Core.Interfaces;

namespace Zomo.Core.Common
{
    /*
     * A static class to write logs
     */
    public static class Logger
    {
        public static EventHandler<string> OnWrite;

        public static void Write(string source, string message)
        {
            var now = DateTime.Now;
            string finalLog = $"[{now.Hour}:{now.Minute}:{now.Millisecond}] [{source}]: {message}";
            Console.WriteLine(finalLog);
            OnWrite?.Invoke(null, finalLog);
        }

        public static void Log(this IBotService obj, string message)
        {
            BotServiceInfo info = (BotServiceInfo) Attribute.GetCustomAttribute(obj.GetType(), typeof(BotServiceInfo));
            if (info != null)
            {
                Write(info.Name, message);
            }
        }

        public static void Log(this ModuleBase<SocketCommandContext> obj, string message)
        {
            Write("Module", message);
        }

        public static Task Write(LogMessage message)
        {
            Write(message.Source, message.Message);
            return Task.CompletedTask;
        }
    }
}