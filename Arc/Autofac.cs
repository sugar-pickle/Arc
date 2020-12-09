using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Autofac;

namespace Atomic.Arc
{
    public static class Autofac
    {
        public static IContainer BuildContainer(ArcConfig config)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new IPEndPoint(IPAddress.Any, (int)config.ListenPort));
            builder.RegisterInstance(new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp));
            builder.RegisterInstance(config);

            builder.RegisterType<LineHandler>()
                .As<ILineHandler>()
                .InstancePerDependency();

            builder.RegisterType<LineHandlerFactory>()
                .As<ILineHandlerFactory>()
                .SingleInstance();

            builder.RegisterType<ArcLog>()
                .As<IArcLog>()
                .SingleInstance();

            builder.RegisterType<ArcServer>()
                .As<IArcServer>()
                .SingleInstance();

            builder.RegisterType<AccountsHandler>()
                .As<IAccountsHandler>()
                .SingleInstance();

            builder.RegisterType<GenericWebhookDispatch>()
                .As<IWebhookDispatch>()
                .SingleInstance();

            builder.RegisterType<ArcConsole>()
                .As<IArcConsole>()
                .SingleInstance();

            builder.RegisterType<AlarmLog>()
                .As<IAlarmLog>()
                .SingleInstance();

            return builder.Build();
        }
    }
}
