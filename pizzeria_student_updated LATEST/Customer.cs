using System;
using System.Threading;
using System.Collections.Concurrent;

namespace pizzeria
{
    public class Customer
    {
        public int _id { get; private set; }
        private static int completedCustomers = 0;
        private static object lockObj = new object();

        public Customer(int id)
        {
            this._id = id;
        }

        public void Start()
        {
            Life();
        }

        public void Life()
        {
            Thread.Sleep(new Random().Next(50, 200));
            Console.WriteLine($"Customer {_id} about to order a pizza slice");

            PizzaOrder p = new PizzaOrder();

            lock (Program.order)
            {
                Program.order.AddFirst(p);
            }

            Program.orderSemaphore.Release();
            Console.WriteLine($"Customer {_id} waits for a pizza slice");

            TryToPickUpPizza();
        }

        private void TryToPickUpPizza()
        {
            PizzaDish pizza;
            bool temp = false;

            lock (Program.pickUp)
            {
                if (Program.pickUp.Count > 0)
                {
                    pizza = Program.pickUp.First.Value;
                    pizza.RemoveSlice();

                    if (pizza.Slices == 0)
                    {
                        Program.pickUp.RemoveFirst();
                        temp = true;
                    }

                    if (temp)
                    {
                        Console.WriteLine($"Customer {_id} has eaten the final slice. Remaining pizzas: {Program.pickUp.Count}");
                    }
                    else
                    {
                        Console.WriteLine($"Customer {_id} has eaten a slice. Remaining slices: {pizza.Slices}, Total pizzas: {Program.pickUp.Count}");
                    }

                    //OnPizzaEaten();
                    return;
                }
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(50);
                TryToPickUpPizza();
            });
        }

        //private void OnPizzaEaten()
        //{
        //    lock (lockObj)
        //    {
        //        completedCustomers++;
        //        if (completedCustomers == Program.n_customers)
        //        {
                   
        //            Monitor.PulseAll(lockObj);
        //        }
        //    }
        //}

        //public static int GetCompletedCustomers()
        //{
        //    lock (lockObj)
        //    {
        //        return completedCustomers;
        //    }
        //}

        //public static object GetLockObject()
        //{
        //    return lockObj;
        //}
    }
}
