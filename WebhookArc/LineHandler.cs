using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebhookArc
{
    public interface ILineHandler
    {
        void StartLineHandler(Socket clientSocket, CancellationToken cToken);
        bool LineHandlerRunning { get; }
        IPEndPoint ConnectedEndpoint { get; }
    }

    public class LineHandler : ILineHandler
    {
        private readonly ArcConfig config;
        private readonly IWebhookDispatch webhookDispatch;
        private Task lineHandlerTask;
        private readonly IArcLog arcLog;

        public IPEndPoint ConnectedEndpoint { get; private set; }

        public LineHandler(IWebhookDispatch webhookDispatch, ArcConfig config, IArcLog arcLog)
        {
            this.webhookDispatch = webhookDispatch;
            this.config = config;
            this.arcLog = arcLog;
        }

        public void StartLineHandler(Socket clientSocket, CancellationToken cToken)
        {
            lineHandlerTask = Task.Run(() => RunLineHandler(clientSocket, cToken));
        }

        public bool LineHandlerRunning { get; private set; } = true;

        private async Task RunLineHandler(Socket clientSocket, CancellationToken cToken)
        {
            clientSocket.ReceiveTimeout = (int)config.TimeoutSeconds * 1000;
            ConnectedEndpoint = (IPEndPoint)clientSocket.RemoteEndPoint;
            arcLog.Log($"Running line handler - Incoming connection from {ConnectedEndpoint}");

            await Read(clientSocket, cToken);

            arcLog.Log($"Closing line handler for {ConnectedEndpoint}");
            LineHandlerRunning = false;
        }

        private async Task Read(Socket clientSocket, CancellationToken cToken)
        {
            while(!cToken.IsCancellationRequested)
            {
                var buffer = new byte[2048];
                var recvdBytes = await clientSocket.ReceiveAsync(buffer, SocketFlags.None, cToken);

                if (recvdBytes > 0)
                {
                    var data = Encoding.ASCII.GetString(buffer, 0, recvdBytes);
                    if (!data.Contains(config.Heartbeat))
                    {
                        arcLog.Log($"{ConnectedEndpoint} - Alarm received -> {data}");
                        var siaAlarm = AlarmDecoder.Process(data);
                        if (siaAlarm.Decoded)
                            await webhookDispatch.SendToWebhook(siaAlarm);
                    }

                    await Ack(clientSocket, cToken);
                }
                else return;
            }
        }

        private async Task Ack(Socket clientSocket, CancellationToken cToken)
            => await clientSocket.SendAsync(new byte[] { config.Ack }, SocketFlags.None, cToken);
    }
}
