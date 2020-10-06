using System.Collections.Generic;

namespace Atomic.Arc
{
    public interface IAlarmLog
    {
        void LogAlarm(SiaAlarm alarm);
        IEnumerable<SiaAlarm> GetAllAlarms { get; }
    }
    public class AlarmLog : IAlarmLog
    {
        private readonly object logLock = new object();
        private readonly List<SiaAlarm> alarms = new List<SiaAlarm>();

        public void LogAlarm(SiaAlarm alarm)
        {
            lock (logLock) alarms.Add(alarm);
        }
        
        public IEnumerable<SiaAlarm> GetAllAlarms {
            get { lock (logLock) return alarms; }
        }
    }
}