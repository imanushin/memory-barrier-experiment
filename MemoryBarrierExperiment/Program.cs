using System;
using System.Threading;

namespace MemoryBarrierExperiment
{
    internal static class Program
    {
        private const int ExperimentCount = 1;

        private static readonly object SyncRoot = new object();

        private static readonly Barrier Barrier = new Barrier(2);
        private static readonly SemaphoreSlim ValueSetLock = new SemaphoreSlim(1);

        private static int _countOfTrue;
        private static int _countOfFalse;

        private static bool _targetValue;


        private static void Main(string[] args)
        {
            var updateThread = new Thread(() =>
            {
                for (int i = 0; i < ExperimentCount; i++)
                {
                    UpdateValue();
                }
            });

            var readThread = new Thread(() =>
            {
                for (int i = 0; i < ExperimentCount; i++)
                {
                    ReadValue();
                }
            });

            updateThread.Start();
            readThread.Start();

            updateThread.Join();
            readThread.Join();

            Thread.MemoryBarrier();

            Console.Out.WriteLine("True count: {0}, False count: {1}", _countOfTrue, _countOfFalse);
        }

        private static void UpdateValue()
        {
            ValueSetLock.Wait();
            Barrier.SignalAndWait();

            _targetValue = false;

            Thread.MemoryBarrier();

            lock (SyncRoot)
            {
                ValueSetLock.Release();

                _targetValue = true;
            }
        }

        private static void ReadValue()
        {
            Thread.MemoryBarrier();

            Barrier.SignalAndWait();

            ValueSetLock.Wait();
            ValueSetLock.Release();

            lock (SyncRoot)
            {
                if (_targetValue)
                {
                    Interlocked.Increment(ref _countOfTrue);
                }
                else
                {
                    Interlocked.Increment(ref _countOfFalse);
                }
            }
        }
    }
}
