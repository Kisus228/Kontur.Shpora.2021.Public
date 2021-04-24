using System;
using System.Threading;

namespace ReaderWriterLock
{
    public class RwLock : IRwLock
    {
        private static int _readLock;
        private static int _writeLock;
        private static readonly object LockObject = new();
        private static readonly object WaitObject = new();

        public void ReadLocked(Action action)
        {
            WaitIfLocked(ref _writeLock);

            Interlocked.Increment(ref _readLock);
            action.Invoke();
            Interlocked.Decrement(ref _readLock);
            
            PulseAllIfUnlocked(ref _readLock);
        }

        public void WriteLocked(Action action)
        {
            Interlocked.Increment(ref _writeLock);
            WaitIfLocked(ref _readLock);

            lock (LockObject)
            {
                action.Invoke();
            }

            Interlocked.Decrement(ref _writeLock);

            PulseAllIfUnlocked(ref _writeLock);
        }

        private static void PulseAllIfUnlocked(ref int lockObject)
        {
            if (Interlocked.CompareExchange(ref lockObject, 0, 0) != 0)
            {
                return;
            }

            lock (WaitObject)
            {
                Monitor.PulseAll(WaitObject);
            }
        }
        
        private static void WaitIfLocked(ref int lockObject)
        {
            while (Interlocked.CompareExchange(ref lockObject, 0, 0) != 0)
            {
                lock (WaitObject)
                {
                    if (Interlocked.CompareExchange(ref lockObject, 0, 0) != 0)
                        Monitor.Wait(WaitObject);
                }
            }
        }
    }
}