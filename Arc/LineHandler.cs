using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atomic.Arc
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
        private readonly IAlarmLog alarmLog;

        public IPEndPoint ConnectedEndpoint { get; private set; }

        public LineHandler(IWebhookDispatch webhookDispatch, ArcConfig config, IArcLog arcLog, IAlarmLog alarmLog)
        {
            this.webhookDispatch = webhookDispatch;
            this.config = config;
            this.arcLog = arcLog;
            this.alarmLog = alarmLog;
        }

        public void StartLineHandler(Socket clientSocket, CancellationToken cToken)
        {
            lineHandlerTask = Task.Run(() => RunLineHandler(clientSocket, cToken), cToken);
        }

        public bool LineHandlerRunning { get; private set; } = true;

        private async Task RunLineHandler(Socket clientSocket, CancellationToken cToken)
        {
            clientSocket.ReceiveTimeout = (int)config.TimeoutSeconds * 1000;
            ConnectedEndpoint = (IPEndPoint)clientSocket.RemoteEndPoint;
            arcLog.Log($"Running line handler for {ConnectedEndpoint}");
            await Read(clientSocket, cToken);
            arcLog.Log($"Closing line handler for {ConnectedEndpoint}");
            clientSocket.Close();
            LineHandlerRunning = false;
        }

        private async Task<byte[]> ReadFromSocket(Socket clientSocket, CancellationToken cToken)
        {
            var alarmData = new List<byte>();
            while (!alarmData.Contains(config.Terminator))
            {
                var buffer = new byte[2048];
                var readBytes = await clientSocket.ReceiveAsync(buffer, SocketFlags.None, cToken);
                if (readBytes < 1) break;
                var newData = new byte[readBytes];
                Array.Copy(buffer, 0, newData, 0, readBytes);
                alarmData.AddRange(newData);
                Debug.WriteLine($"Read {readBytes} bytes from {ConnectedEndpoint}");
            }

            return alarmData.ToArray();
        }

        private async Task Read(Socket clientSocket, CancellationToken cToken)
        {
            var connected = false;
            while(!cToken.IsCancellationRequested)
            {
                var alarmData = await ReadFromSocket(clientSocket, cToken);
                if (alarmData.Length < 1) return;
                var data = Encoding.ASCII.GetString(alarmData.ToArray(), 0, alarmData.Length);
                if (!data.StartsWith(config.Heartbeat))
                {
                    arcLog.Log($"{ConnectedEndpoint} - Alarm received -> {data}");
                    var siaAlarm = AlarmDecoder.Process(data);
                    if (siaAlarm.Decoded)
                    {
                        alarmLog.LogAlarm(siaAlarm);
                        if (config.WebhookEnabled) await webhookDispatch.SendToWebhook(siaAlarm);
                    }
                }
                else
                {
                    arcLog.Log($"{ConnectedEndpoint} - Heartbeat");
                    if (!connected)
                    {
                        if (config.WebhookEnabled)
                            await webhookDispatch.SendToWebhook($"Endpoint connected {ConnectedEndpoint}");
                        connected = true;
                    }
                }

                await Ack(clientSocket, cToken);
            }

            if (connected)
                await webhookDispatch.SendToWebhook($"Endpoint disconnected {ConnectedEndpoint}");
        }

        private async Task Ack(Socket clientSocket, CancellationToken cToken)
            => await clientSocket.SendAsync(new[] { config.Ack }, SocketFlags.None, cToken);
    }
}
