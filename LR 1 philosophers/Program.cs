using System;
using System.Threading;

namespace LR_1_philosophers
{
    internal class Program
    {
        private static int PHILOSOPHERS = 10;
        
        private static int THINKS = 5;

        private static int _coundDead = 0;
        
        private static bool[] _busyfork = new bool[PHILOSOPHERS];

        private static int _philosophersWaitingCount = 0;
        
        private static object _philosophersWaitingCountLockObject = new object();
        
        private static Semaphore _philosophersMustWait = new Semaphore(0, 20);

        public static void Main(string[] args)
        {
            Thread[] readers = new Thread[PHILOSOPHERS];
            for (int i = 0; i < PHILOSOPHERS; i++)
            {
                readers[i] = new Thread(Thinking);
                readers[i].Name = $"{i}";
            }

            foreach (var t in readers)
            {
                t.Start();
            }

            foreach (var t in readers)
            {
                t.Join();
            }
        }

        static void Thinking()
        {
            for (int i = 0; i < THINKS; i++)
            {
                Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId} думает");
                Thread.Sleep(400);
                Eating();
            }
            EndThread();
        }

        private static void EndThread()
        {
            lock (_philosophersWaitingCountLockObject)
            {
                _coundDead++;
                Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId} умер ({_coundDead} / {PHILOSOPHERS})");
                Thread.CurrentThread.Abort();
            }
        }
        
        static void Eating()
        {
            lock (_philosophersWaitingCountLockObject)
            {
                int[] forks = CheckNearlyForks();
                if (_busyfork[forks[0]] && _busyfork[forks[1]])
                {
                    Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId} пытался поесть но вилки заняты");
                    IncreaseReadersWaitingCount();
                    _philosophersMustWait.WaitOne();
                    DecreaseReadersWaitingCount();
                }

                _busyfork[forks[0]] = true;
                _busyfork[forks[1]] = true;
                Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId} ест");
                Thread.Sleep(200);
                _busyfork[forks[0]] = false;
                _busyfork[forks[1]] = false;
            }
        }

        static int[] CheckNearlyForks()
        {
            lock (_philosophersWaitingCountLockObject)
            {
                int id = Int32.Parse(Thread.CurrentThread.Name);
                int[] forks = new int[2];
                if (id == 0)
                {
                    forks[0] = PHILOSOPHERS - 1;
                    forks[1] = 0;
                }
                else
                {
                    forks[0] = id;
                    forks[1] = id - 1;
                }
                return forks;
            }
        }
        
        /// <summary>
        /// Метод увеличения значение очереди
        /// </summary>
        public static void IncreaseReadersWaitingCount()
        {
            lock(_philosophersWaitingCountLockObject)
            {
                _philosophersWaitingCount++;
                Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId} увеличил значение очереди до {_philosophersWaitingCount}");
            }
        }
        
        /// <summary>
        /// Метод уменьшения значение очереди
        /// </summary>
        public static void DecreaseReadersWaitingCount()
        {
            lock(_philosophersWaitingCountLockObject)
            {
                _philosophersWaitingCount--;
                Console.WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId} уменьшил значение очереди до {_philosophersWaitingCount}");
            }
        }
    }
}