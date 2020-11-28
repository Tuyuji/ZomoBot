using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        private readonly List<ZomoPlugin> _plugins;

        public BotService(IServiceProvider services)
        {
            _services = services;
            _config = _services.GetRequiredService<Config>();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _command = _services.GetRequiredService<CommandService>();
            _plugins = new List<ZomoPlugin>();
        }

        public void ServiceStart()
        {
            Logger.Write(ZomoApplication.Instance.AppName, "Bot service start.");

            LoadPlugins();

            _command.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                _command.AddModulesAsync(assembly, _services);
            }

            _client.Log += Logger.Write;
            _client.Connected += OnConnected;

            string token = Environment.GetEnvironmentVariable("ZOMO_BOT_TOKEN");
            if(string.IsNullOrEmpty(token))
                token = (string) _config.Get<string>("Token");

            if (token == "")
            {
                this.Log("Failed to get token via config or env var.");
                Environment.Exit(-1);
            }
            
            _client.LoginAsync(TokenType.Bot, token);
            _client.StartAsync();
        }

        private Task OnConnected()
        {
            ZomoApplication.Instance.Status = BotStatus.Online;
            return Task.CompletedTask;
        }

        private void LoadPlugins()
        {
            const string pluginsFolder = "Plugins";
            try
            {
                if (!Directory.Exists(pluginsFolder))
                    Directory.CreateDirectory(pluginsFolder);

                foreach (var file in Directory.GetFiles(pluginsFolder + '/'))
                {
                    if (file.ToLower().EndsWith(".dll"))
                    {
                        var fullFilePath = $"{Directory.GetCurrentDirectory()}/{file}";
                        var assembly = Assembly.LoadFile(fullFilePath);

                        var pluginType = assembly.GetTypes()
                            .Where(t => t.IsClass)
                            .First(t => t.IsSubclassOf(typeof(ZomoPlugin)));

                        if (pluginType == null) return;

                        var plugin = Activator.CreateInstance(pluginType) as ZomoPlugin;
                        if (plugin == null) return;

                        plugin.Init();
                        _plugins.Add(plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log(ex.ToString());
                throw ex;
            }
        }

        public void ServicePostStart()
        {
        }

        public void ServiceDispose()
        {
            ZomoApplication.Instance.Status = BotStatus.ShuttingDown;
            
            foreach (var plugin in _plugins)
                plugin?.Destroy();

            ZomoApplication.Instance.Status = BotStatus.Offline;
        }
    }
}