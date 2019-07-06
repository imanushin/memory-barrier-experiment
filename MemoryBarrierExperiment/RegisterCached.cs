using System;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryBarrierExperiment
{
    internal static class RegisterCached
    {
        private static int _a;

        public static void Main()
        {
            Console.Out.WriteLine("Start {0}", typeof(RegisterCached));

            var task = new Task(Bar);
            task.Start();
            Thread.Sleep(1000);
            _a = 0;
            task.Wait();
        }

        private static void Bar()
        {
            _a = 1;
            while (_a == 1)
            {
                //Thread.MemoryBarrier();
            }
        }
    }
}
