using System;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Zomo.Core.Common;
using Zomo.Core.Interfaces;

namespace Zomo.Core.Services
{
    [BotServiceInfo("Zomo", "This core bot service.", ServicePriority.BotOnly)]
    public class BotService : IBotService
    {
        private readonly IServiceProvider _services;
        private readonly Config _config;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;

        public BotService(IServiceProvider services)
        {
            _services = services;
            _config = _services.GetRequiredService<Config>();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _command = _services.GetRequiredService<CommandService>();
        }

        public void ServiceStart()
        {
            Logger.Write(ZomoApplication.Instance.AppName, "Bot service start.");
            if (!_config.HasVar("Token"))
            {
                Console.Write("Token: ");
                var token = Console.ReadLine();
                Console.WriteLine("Saving...");
                _config.Store("Token", token);
                _config.Save();
            }

            _command.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                _command.AddModulesAsync(assembly, _services);
            }

            _client.Log += Logger.Write;

            _client.LoginAsync(TokenType.Bot, _config.Get<string>("Token"));
            _client.StartAsync();
        }

        public void ServicePostStart()
        {
        }

        public void ServiceDispose()
        {
        }
    }
}