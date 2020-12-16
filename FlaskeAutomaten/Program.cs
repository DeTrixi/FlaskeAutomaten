using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace FlaskeAutomaten
{
    class Program
    {
        // this i a queue of products

        private static Queue<string> Products = new();
        public static Queue<string> Sodas = new();
        public static Queue<string> Beers = new();


        //public static bool
        public static int SerialNumber = 1;

        static void Main(string[] args)
        {
            Programmers programmer = new Programmers();
            new Thread((o => { programmer.ConsumeSoda(); })).Start();
            Alcoholics alcoholic = new Alcoholics();
            new Thread((o => { alcoholic.DrinkBeer(); })).Start();

            Thread produceProducts = new Thread(ProducerOfBeerOrSoda);
            produceProducts.Start();
            Thread consumerSplitter = new Thread(ConsumerSplitter);
            consumerSplitter.Start();
            Console.WriteLine("Hello Teacher!");
            Console.ReadLine();
        }

        /// <summary>
        /// Method produces to Product Queue
        /// </summary>
        public static void ProducerOfBeerOrSoda()
        {
            Random ran = new Random();
            do
            {
                // creates 10 products and passes the baton
                Monitor.Enter(Products);
                while (Products.Count > 0 && Products.Count < 100) Monitor.Wait(Products);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Producing products.........!");

                Console.ResetColor();
                for (int i = 0; i < 10; i++)
                {
                    string prod = ran.Next(2) == 0 ? $"Beer {SerialNumber}" : $"Soda {SerialNumber}";
                    SerialNumber++;
                    Products.Enqueue(prod);
                    Console.WriteLine(prod);
                    Thread.Sleep(ran.Next(1000));
                }

                Monitor.PulseAll(Products);
                Monitor.Exit(Products);
                //Console.WriteLine("Produ");


                //Thread.Sleep(10);
            } while (true);
        }

        public static void ConsumerSplitter()
        {
            // Temp place holder for shortening the lock time on the soa and beer consumer
            Queue<string> sodas = new();
            // Temp place holder for shortening the lock time on the soa and beer consumer
            Queue<string> beers = new();
            do
            {
                // Locks the Products Queue
                Monitor.Enter(Products);
                while (Products.Count <= 0) Monitor.Wait(Products);
                {
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Splitting Products...........!");
                    // Checking that my temp threads is killed by the garbage collector
                    Console.WriteLine($"Current Thread Count Running: {Process.GetCurrentProcess().Threads.Count}");
                    Console.ResetColor();
                    // Splits all products in the queue into t2 temp queues an uses the temp not to lock all queue in this lock
                    while (Products.Count > 0)
                    {
                        Console.WriteLine(Products.Peek());
                        if (Products.Peek().Contains("Beer"))
                        {
                            beers.Enqueue(Products.Dequeue());
                        }
                        else
                        {
                            sodas.Enqueue(Products.Dequeue());
                        }
                    }

                    Thread.Sleep(1000);
                }
                // Wakes all subscribers to Priducts Queue
                Monitor.PulseAll(Products);
                // Exiting Products Queue
                Monitor.Exit(Products);

                // TODO MOVE TO SEPARATE CLASS AND CREAT ONE METHOD INSTEAD OF TWO
                // Creates a new thread for each move items method should maybe be a generic method
                new Thread(() => MoveToSodas(sodas)).Start();
                new Thread(() => MoveToBeers(beers)).Start();
            } while (true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="soda"></param>
        public static void MoveToSodas(Queue<string> soda)
        {
            Monitor.Enter(Sodas);
            while (soda.Count > 0) //Monitor.Wait(Sodas);
            {
                Sodas.Enqueue(soda.Dequeue());
            }


            Monitor.PulseAll(Sodas);
            Monitor.Exit(Sodas);
        }

        public static void MoveToBeers(Queue<string> beers)
        {
            Monitor.Enter(Beers);
            while (beers.Count <= 10) Monitor.Wait(Beers);
            {
                while (beers.Count > 0)
                {
                    Beers.Enqueue(beers.Dequeue());
                }
            }

            Monitor.PulseAll(Beers);
            Monitor.Exit(Beers);
        }


        public class Programmers
        {
            public void ConsumeSoda()
            {
                do
                {
                    Monitor.Enter(Sodas);

                    while (Sodas.Count <= 10) Monitor.Wait(Sodas);
                    {
                        while (Sodas.Count > 0)
                        {
                           Console.WriteLine($"Programmer is drinking {Sodas.Dequeue()}");
                        }

                        //Thread.Sleep(100);
                    }
                    Monitor.PulseAll(Sodas);
                    Monitor.Exit(Sodas);
                } while (true);
            }
        }

        public class Alcoholics
        {
            public void DrinkBeer()
            {
                do
                {
                    Monitor.Enter(Beers);
                    while (Beers.Count > 1) //Monitor.Wait(Beers);
                    {
                        //Thread.Sleep(100);
                        Console.WriteLine($"Alcoholic is drinking {Beers.Dequeue()}");
                    }

                    Monitor.PulseAll(Beers);
                    Monitor.Exit(Beers);
                } while (true);
            }
        }
    }
}


