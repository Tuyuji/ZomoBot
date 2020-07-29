using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Util;
using Microsoft.Extensions.DependencyInjection;
using Zomo.Core.Interfaces;

namespace Zomo.Core.Common
{
    public abstract class ZomoApplication
    {
        private static ZomoApplication _instance;

        private IServiceProvider _services;
        private string _appName;
        private Config _botConfig = new Config("bot.cfg");
        private DiscordSocketClient _client = new DiscordSocketClient();
        private CommandService _command = new CommandService();
        private Dictionary<Type, BotServiceInfo> _botServices = new Dictionary<Type, BotServiceInfo>();

        public static ZomoApplication Instance => _instance;
        public string AppName => _appName;

        public IServiceProvider Services => _services;
        public DiscordSocketClient Client => _client;
        public CommandService CommandService => _command;

        public ZomoApplication(string appName = "Zomo")
        {
            _instance = this;
            _appName = appName;
        }

        public async Task Start()
        {
            var services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_botConfig)
                .AddSingleton(_command);

            AddBotServices(ref services);

            _services = services.BuildServiceProvider();

            //Start all the services which the bot is in.
            foreach (var service in _botServices)
            {
                IBotService botService = (IBotService) _services.GetRequiredService(service.Key);
                botService.ServiceStart();
            }


            //Now that all the services with the bot started
            //we call post start, mostly a just in case function.
            foreach (var service in _botServices)
            {
                IBotService botService = (IBotService) _services.GetRequiredService(service.Key);
                botService.ServicePostStart();
            }

            await Task.Delay(-1);
        }

        private void AddBotServices(ref IServiceCollection services)
        {
            Type type = typeof(IBotService);
            var types = Assembly.GetExecutingAssembly().GetTypes()
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