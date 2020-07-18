using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Zomo.Core.Interfaces;
using Zomo.Core.Services;

namespace Zomo.Core
{
    public class ZomoBot
    {
        private Config _botConfig = new Config("bot.cfg");
        private Dictionary<Type, BotServiceInfo> _botServices = new Dictionary<Type, BotServiceInfo>();
        
        public async Task Start()
        {
            var services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient())
                .AddSingleton(_botConfig)
                .AddSingleton(new CommandService());

            AddBotServices(ref services);
            
            var serviceProvider = services.BuildServiceProvider();
            
            //Start all the services which the bot is in.
            foreach (var service in _botServices)
            {
                IBotService botService = (IBotService)serviceProvider.GetRequiredService(service.Key);
                botService.ServiceStart();
            }
                

            //Now that all the services with the bot started
            //we call post start, mostly a just in case function.
            foreach (var service in _botServices)
            {
                IBotService botService = (IBotService)serviceProvider.GetRequiredService(service.Key);
                botService.ServicePostStart();
            }

            await Task.Delay(-1);
        }

        private void AddBotServices(ref IServiceCollection services)
        {
            Type type = typeof(IBotService);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .Where(p => p.IsClass);

            var unsortedInfos = new Dictionary<Type, BotServiceInfo>();
            
            foreach (var service in types)
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(service);
                BotServiceInfo info = null;
                foreach (var attr in attributes)
                    if (attr is BotServiceInfo serviceInfo)
                        unsortedInfos[service] = serviceInfo;
            }
            
            var sortedInfos = from pair in unsortedInfos orderby (int) pair.Value.Priority select pair;
            
            foreach (var service in sortedInfos)
            {
                _botServices.Add(service.Key, service.Value);
                services.AddSingleton(service.Key);
            }
        }
    }
}