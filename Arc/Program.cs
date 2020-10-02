using Autofac;
using Microsoft.Extensions.Configuration;

namespace Atomic.Arc
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var config = LoadConfig();
            var container = Autofac.BuildContainer(config);
            var arcConsole = container.Resolve<IArcConsole>();
            if (args.Length > 0 && args[0] == "-i")
                arcConsole.RunInteractiveConsole();
            else
                arcConsole.RunNonInteractive();
        }

        private static ArcConfig LoadConfig()
            => new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build().Get<ArcConfig>();
    }
}
