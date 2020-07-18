using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Zomo.Core.Interfaces;

namespace Zomo.Core.Services
{
    [BotServiceInfo("Zomo", "This core bot service.", ServicePriority.BotOnly)]
    public class BotService : IBotService
    {
        private readonly IServiceProvider _services;
        private readonly Config _config;

        public BotService(IServiceProvider services)
        {
            Console.WriteLine("Bot service constructor.");
            _services = services;
            _config = _services.GetRequiredService<Config>();
        }
        
        public void ServiceStart()
        {
            Console.WriteLine("Bot service start.");
            if (!_config.HasVar("Token"))
            {
                Console.Write("Token: ");
                var token = Console.ReadLine();
                Console.WriteLine("Saving...");
                _config.Store("Token", token);
                _config.Save();
            }
            
            
        }

        public void ServicePostStart()
        {
            Console.WriteLine("Bot service post start.");
        }

        public void ServiceDispose()
        {
            
        }
    }
}