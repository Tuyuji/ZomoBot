using System;
using Zomo.Core.Common;
using Zomo.ShineBot.Modules;

namespace Zomo.ShineBot
{
    public class ShineBotPlugin : ZomoPlugin
    {
        public override string Name => "Shine";

        public override void Init()
        {
            Console.WriteLine("ShineBot loaded");
            RegisterModule(typeof(ShineHelpful));
            RegisterModule(typeof(Fun));
        }
    }
}