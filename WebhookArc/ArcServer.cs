using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebhookArc
{
    public interface IArcServer
    {
        Task RunServer(CancellationToken cToken);
        IEnumerable<ILineHandler> LineHandlers { get; }
    }

    public class ArcServer : IArcServer
    {
        private readonly IPEndPoint endpoint;
        private readonly Socket socket;
        private readonly List<ILineHandler> lineHandlers;
        private readonly ILineHandlerFactory lineHandlerFactory;
        private readonly ArcConfig config;
        private readonly object listLock = new object();
        private readonly IArcLog arcLog;

        public ArcServer(ILineHandlerFactory lineHandlerFactory, IPEndPoint endpoint, Socket socket, ArcConfig config, IArcLog arcLog)
        {
            this.lineHandlerFactory = lineHandlerFactory;
            this.endpoint = endpoint;
            this.socket = socket;
            this.config = config;
            this.arcLog = arcLog;
            lineHandlers = new List<ILineHandler>();
        }

        public async Task RunServer(CancellationToken cToken)
        {
            socket.Bind(endpoint);
            socket.Listen(10);

            arcLog.Log($"ARC Server started - listening on port {config.ListenPort}");
            _ = Task.Run(() => Cleaner(cToken));

            while (!cToken.IsCancellationRequested)
            {
                var newSocket = await socket.AcceptAsync();
                var lineHandler = lineHandlerFactory.NewInstance();
                lineHandler.StartLineHandler(newSocket, cToken);
                lock (listLock)
                {
                    lineHandlers.Add(lineHandler);
                }
            }
        }

        public IEnumerable<ILineHandler> LineHandlers => lineHandlers;

        private async Task Cleaner(CancellationToken cToken)
        {
            arcLog.Log("ARC Server cleaner task started");
            while(!cToken.IsCancellationRequested)
            {
                await Task.Delay(30000);
                lock (listLock)
                {
                    foreach (var lh in lineHandlers)
                    {
                        if (!lh.LineHandlerRunning)
                        {
                            arcLog.Log($"Removing closed line handler for {lh.ConnectedEndpoint}");
                            lineHandlers.Remove(lh);
                        }
                    }
                }
            }
        }
    }
}
