using Autofac;
using Microsoft.Extensions.Configuration;

namespace WebhookArc
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var config = LoadConfig();
            var container = Autofac.BuildContainer(config);
            var arcConsole = container.Resolve<IArcConsole>();
            arcConsole.RunConsole();
        }

        private static ArcConfig LoadConfig()
            => new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build().Get<ArcConfig>();
    }
}
