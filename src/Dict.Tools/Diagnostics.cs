using System;
using System.Diagnostics;

namespace Dict.Tools
{
    public class DebugTimer
    {
        public DebugTimer()
        {
            _sw.Start();
        }

        public void ReStart()
        {
            _sw.Restart();
        }

        public void Stop(string src = "")
        {
            _sw.Stop();
            var time = _sw.ElapsedMilliseconds;
            string msg;
            if (time < 2000)
                msg = $"Elapsed time {time} ms";
            else
                msg = $"Elapsed time {(time/1000):F2} s";

            if (src != "")
                msg = $"{src}. {msg}";

            Console.WriteLine(msg);
        }

        private Stopwatch _sw = new Stopwatch();
    }

    public class MemUsage
    {
        private long _total;

        public MemUsage()
        {
            _total = GC.GetTotalMemory(true);
        }

        public void MemChange()
        {
            var delta = GC.GetTotalMemory(true) - _total;
            string deltaStr;
            if (delta > 2048 * 1024)
            {
                deltaStr = $"{(delta/1024/1024):F1}Mb";
            }
            else if (delta > 2048)
            {
                deltaStr = $"{(delta/1024):F1}Kb";
            }
            else
            {
                deltaStr = $"{delta}b";
            }
                        
            Console.WriteLine($"Mem change: {deltaStr}");
        }
    }
}