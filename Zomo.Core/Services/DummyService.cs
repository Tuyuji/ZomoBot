using System;
using Zomo.Core.Common;
using Zomo.Core.Interfaces;
using static Zomo.Core.Common.Logger;

namespace Zomo.Core.Services
{
    [BotServiceInfo("Dummy", "Test service", ServicePriority.Low)]
    public class DummyService : IBotService
    {
        public DummyService(IServiceProvider services)
        {
            this.Log("Service constructor.");
        }

        public void ServiceStart()
        {
            this.Log("Service start.");
        }

        public void ServicePostStart()
        {
            this.Log("Service post start.");
        }

        public void ServiceDispose()
        {
        }
    }
}