using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextFileLogger
{
    public sealed class SimpleHybirdLock : IDisposable
    {
        private bool disposedValue = false;

        private int m_waiters = 0;

        private int m_lock_tick = 0;

        private DateTime m_enterlock_time = DateTime.Now;

        private readonly Lazy<AutoResetEvent> m_waiterLock = new Lazy<AutoResetEvent>(() => new AutoResetEvent(initialState: false));

        private static long simpleHybirdLockCount;

        private static long simpleHybirdLockWaitCount;

        public bool IsWaitting => m_waiters != 0;

        public int LockingTick => m_lock_tick;

        public static long SimpleHybirdLockCount => simpleHybirdLockCount;

        public static long SimpleHybirdLockWaitCount => simpleHybirdLockWaitCount;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                m_waiterLock.Value.Close();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        public bool Enter()
        {
            Interlocked.Increment(ref simpleHybirdLockCount);
            if (Interlocked.Increment(ref m_waiters) == 1)
            {
                m_enterlock_time = DateTime.Now;
                return true;
            }

            Interlocked.Increment(ref simpleHybirdLockWaitCount);
            Interlocked.Increment(ref m_lock_tick);
            bool flag = m_waiterLock.Value.WaitOne();
            if (!flag)
            {
                Interlocked.Decrement(ref simpleHybirdLockCount);
                Interlocked.Decrement(ref simpleHybirdLockWaitCount);
                Interlocked.Decrement(ref m_lock_tick);
            }
            else
            {
                m_enterlock_time = DateTime.Now;
            }

            return flag;
        }

        public bool Leave()
        {
            Interlocked.Decrement(ref simpleHybirdLockCount);
            if (Interlocked.Decrement(ref m_waiters) == 0)
            {
                return true;
            }

            bool flag = false;
            flag = m_waiterLock.Value.Set();
            Interlocked.Decrement(ref simpleHybirdLockWaitCount);
            Interlocked.Decrement(ref m_lock_tick);
            return flag;
        }

        public override string ToString()
        {
            if (m_lock_tick > 0)
            {
                return $"SimpleHybirdLock[WaitOne-{DateTime.Now - m_enterlock_time}]";
            }

            if (m_waiters != 0)
            {
                return $"SimpleHybirdLock[OneLock-{DateTime.Now - m_enterlock_time}]";
            }

            return "SimpleHybirdLock[NoneLock]";
        }
    }
}
