using System;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool
{
    public class SimpleLockThreadPool : IThreadPool
    {
        private readonly Thread[] threads;
        private readonly Queue<Action> actions = new();
        private readonly object actionsLocker = new();
        private bool isStopped;
        public SimpleLockThreadPool(int concurrency)
        {
            threads = new Thread[concurrency];
            
            for (var index = 0; index < concurrency; index++)
            {
                threads[index] = new Thread(() =>
                {
                    while (!isStopped)
                    {
                        Action lastAction = null;
                        lock (actionsLocker)
                        {
                            while (actions.Count == 0)
                            {
                                Monitor.Wait(actionsLocker);
                            }

                            lastAction = actions.Dequeue();
                        }

                        lastAction.Invoke();
                    }
                });
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        public void Dispose()
        {
            isStopped = true;
            lock (actionsLocker)
            {
                Monitor.PulseAll(actionsLocker);
            }
        }

        public void EnqueueAction(Action action)
        {
            lock (actionsLocker)
            {
                actions.Enqueue(action);
                Monitor.Pulse(actionsLocker);
            }
        }
    }
}