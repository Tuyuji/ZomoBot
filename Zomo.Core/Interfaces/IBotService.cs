using System;
using Microsoft.Extensions.DependencyInjection;

namespace Zomo.Core.Interfaces
{
    public enum ServicePriority : UInt32
    {
        BotOnly = 0, //No other service is allowed to use this!
        High = 1,
        Mid = 2,
        Low = 3
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BotServiceInfo : Attribute
    {
        public BotServiceInfo(string name, string about, ServicePriority priority)
        {
            this.Name = name;
            this.About = about;
            this.Priority = priority;
        }

        public string Name { get; }
        public string About { get; }
        public ServicePriority Priority { get; }
    }

    public interface IBotService
    {
        //Called before bot start
        void ServiceStart();

        //Called after bot start
        void ServicePostStart();

        //Called when shutting down
        void ServiceDispose();
    }
}