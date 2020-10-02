using System;
using System.Collections.Concurrent;
using System.IO;

namespace Atomic.Arc
{
    public interface IArcLog
    {
        void Log(string log);
        string Next { get; }
        int Size { get; }
    }
    public class ArcLog : IArcLog
    {
        private readonly BlockingCollection<string> logList;
        public ArcLog()
        {
            logList = new BlockingCollection<string>();
        }

        public string Next => logList.Take();

        public int Size => logList.Count;

        public void Log(string log)
        {
            logList.Add(log);
            using StreamWriter w = File.AppendText("log.txt");
            w.Write($"\r\n{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()} - {log}" + log);
        }
    }
}
