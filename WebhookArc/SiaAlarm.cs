using System;
namespace WebhookArc
{
    public class SiaAlarm
    {
        public string Interface { get; set; }
        public string AccountNumber { get; set; }
        public string Code { get; set; }
        public string Zone { get; set; }
        public string Area { get; set; }
        public string User { get; set; }
        public string Timestamp { get; set; }
        public bool New { get; set; }
        public string Ascii { get; set; }
        public bool Decoded { get; set; } = false;
    }
}
