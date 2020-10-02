namespace Atomic.Arc
{
    public class ArcConfig
    {
        public uint ListenPort { get; set; } = 50001;
        public uint TimeoutSeconds { get; set; } = 30;
        public string Heartbeat { get; set; } = "1011           @    ";
        public byte Ack { get; set; } = 0x06;
        public string WebhookUrl { get; set; }
        public string AccountsFilename { get; set; } = "accounts.json";
    }
}
