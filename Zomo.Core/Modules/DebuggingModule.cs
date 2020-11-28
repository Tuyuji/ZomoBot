using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Zomo.Core.Interfaces;

namespace Zomo.Core.Modules
{
#if DEBUG
    public class DebuggingModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider _services;

        public DebuggingModule(IServiceProvider services)
        {
            this._services = services;
        }

        [Command("Services")]
        public async Task ListServices()
        {
            EmbedBuilder builder = new EmbedBuilder();

            Type type = typeof(IBotService);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .Where(p => p.IsClass);


            foreach (var service in types)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(service);
                BotServiceInfo info = null;
                foreach (var attr in attributes)
                {
                    if (attr is BotServiceInfo serviceInfo)
                    {
                        builder.AddField(serviceInfo.Name,
                            $"Desc: \"{serviceInfo.About}\"\n" +
                            $"Priority: {serviceInfo.Priority.ToString()}");
                    }
                }
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
#endif 
}