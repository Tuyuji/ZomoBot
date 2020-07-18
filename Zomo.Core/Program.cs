using System;

namespace Zomo.Core
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new ZomoBot().Start().GetAwaiter().GetResult();
        }
    }
}