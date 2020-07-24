using System;

namespace Zomo.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            new ServerApplication().Start().GetAwaiter().GetResult();
        }
    }
}