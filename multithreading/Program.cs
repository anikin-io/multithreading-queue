using System;
using System.Collections.Concurrent;
using System.Threading;

namespace multithreading
{
    public class MyQueue
    {
        private BlockingCollection<int> queue;
        private int capacity;

        public MyQueue(int capacity)
        {
            this.capacity = capacity;
            queue = new BlockingCollection<int>(new ConcurrentQueue<int>(), capacity);
        }

        public void Produce(int item)
        {
            lock (queue)
            {
                while (queue.Count == capacity)
                {
                    Console.WriteLine("Буфер заполнен");
                    Monitor.Wait(queue); // ждем, пока буфер не освободится
                }
                queue.Add(item);
                Console.WriteLine("Произведено: " + item);
                Monitor.PulseAll(queue); // уведомляем все потоки, которые ждут на этом мониторе
            }
        }

        public int Consume()
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    Console.WriteLine("Буфер пуст");
                    Monitor.Wait(queue); // ждем, пока буфер не наполнится
                }
                int item = queue.Take();
                Console.WriteLine("Потреблено: " + item) ;
                Monitor.PulseAll(queue); // уведомляем все потоки, которые ждут на этом мониторе
                return item;
            }
        }
    }

    public class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Введите количество элементов: ");
            int count = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Введите размер очереди: ");
            int size = Convert.ToInt32(Console.ReadLine());

            MyQueue queue = new MyQueue(size); // общая очередь      
            
            Producer producer = new Producer(queue, count);
            Consumer consumer = new Consumer(queue, count);

            producer.Start();
            consumer.Start();
        }
    }

    public class Producer
    {
        private MyQueue queue;
        private int count;

        public Producer(MyQueue queue, int count)
        {
            this.queue = queue;
            this.count = count;
        }

        public void Start()
        {
            Thread producerThread = new Thread(() =>
            {
                try
                {
                    for (int i = 1; i < count+1; i++)
                    {
                        queue.Produce(i);
                        Thread.Sleep((int)(new Random().NextDouble() * 1000)); // имитация работы
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            producerThread.Start();
        }
    }

    public class Consumer
    {
        private MyQueue queue;
        private int count;

        public Consumer(MyQueue queue, int count)
        {
            this.queue = queue;
            this.count = count;
        }

        public void Start()
        {
            Thread consumerThread = new Thread(() =>
            {
                try
                {
                    for (int i = 1; i < count+1; i++)
                    {
                        queue.Consume();
                        Thread.Sleep((int)(new Random().NextDouble() * 1000)); // имитация работы
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            consumerThread.Start();
        }
    }
}
