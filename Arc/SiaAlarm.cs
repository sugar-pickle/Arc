using System;
using System.Linq;

namespace Atomic.Arc
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

        public static string GetWebhookAlarmString(SiaAlarm alrm)
            => $"Account: {alrm.AccountNumber} ({alrm.Interface})\n{GetEventString(alrm)}\n";

        private static string GetEventString(SiaAlarm alrm)
        {
            var s = $"Event: {alrm.Code} - {alrm.Ascii}";
            if (!string.IsNullOrEmpty(alrm.Zone)) s += $"\tZone: {alrm.Zone}";
            if (!string.IsNullOrEmpty(alrm.Area)) s += $"\tArea: {alrm.Area}";
            if (!string.IsNullOrEmpty(alrm.User)) s += $"\tUser: {alrm.User}";

            return s;
        }
    }
}
