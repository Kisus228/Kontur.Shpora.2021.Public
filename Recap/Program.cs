using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Recap
{
    class Program
    {
        private static readonly object Locker = new();

        static void Main(string[] args)
        {
            var processorNum = 1;
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)(1 << processorNum);

            var dict = new Dictionary<string, string> {["test"] = "test"};


            var list = new List<long>();
            var n = 100;
            var ticks = 0L;
            var cacheTicks = ticks;
            long failedAfter = 0;
            var t = new Thread(() =>
            {
                var sw = new Stopwatch();
                sw.Start();
                while (true)
                {
                    ticks = sw.ElapsedTicks;
                }
            });
            t.Start();
            while (list.Count != n)
            {
                if (ticks != cacheTicks)
                {
                    list.Add(ticks - cacheTicks);
                    cacheTicks = ticks;
                }
            }
            Console.WriteLine(string.Join("\n", list.Select(x => (double)x / 2 / Stopwatch.Frequency * 1000)));
            Console.ReadLine();
        }
    }
}
