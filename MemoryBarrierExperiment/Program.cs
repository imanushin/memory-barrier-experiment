using System;
using System.Threading;

namespace MemoryBarrierExperiment
{
    internal static class Program
    {
        private const int ExperimentCount = 100;

        private static readonly object SyncRoot = new object();

        private static readonly Barrier
            TaskStartBarrier = new Barrier(2, _ =>
            {
                _targetValue = false;

                Thread.MemoryBarrier();
            });

        private static readonly Barrier TaskFinishBarrier = new Barrier(2);

        private static readonly SemaphoreSlim ValueSetLock = new SemaphoreSlim(1, 1);

        private static int _countOfTrue;
        private static int _countOfFalse;

        private static bool _targetValue;

        private static void Main()
        {
            var updateThread = new Thread(() =>
            {
                for (var i = 0; i < ExperimentCount; i++)
                {
                    UpdateValue();
                }
            });

             var readThread = new Thread(() =>
            {
                for (var i = 0; i < ExperimentCount; i++)
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
            TaskStartBarrier.SignalAndWait();

            lock (SyncRoot)
            {
                ValueSetLock.Release();
                
                _targetValue = true;

                Monitor.Wait(SyncRoot);

                if (_targetValue)
                {
                    Interlocked.Increment(ref _countOfTrue);
                }
                else
                {
                    Interlocked.Increment(ref _countOfFalse);
                }
            }

            TaskFinishBarrier.SignalAndWait();
        }

        private static void ReadValue()
        {
            TaskStartBarrier.SignalAndWait();

            ValueSetLock.Wait();
            ValueSetLock.Release();

            lock (SyncRoot)
            {
                _targetValue = false;

                Monitor.PulseAll(SyncRoot);

                Thread.Sleep(10);
            }

            TaskFinishBarrier.SignalAndWait();
        }
    }
}
