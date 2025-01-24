using System;
using System.Threading;

namespace pizzeria
{
    public class Pizzaiolo
    {
        public int _id { get; private set; }
        private static int completedOrders = 0;
        private static readonly object lockObj = new object();
        private static volatile bool stopRequested = false;

        public Pizzaiolo(int id)
        {
            this._id = id;
        }

        public void Start()
        {
            Life();
        }

        public void Life()
        {
            
            Program.orderSemaphore.WaitOne();
            PizzaOrder p;

            lock (Program.order)
            {
                p = Program.order.First.Value;
                Program.order.RemoveFirst();
            }

            Console.WriteLine($"Pizzaiolo {_id} is about to take the pizza order");
            Thread.Sleep(new Random().Next(50, 200));

            p.StartWorking();
            Thread.Sleep(new Random().Next(50, 200));

            Console.WriteLine($"Pizzaiolo {_id} about to finish the pizza slice");
            PizzaSlice s = p.FinishWorking(_id.ToString());

            Console.WriteLine($"Pizzaiolo {_id} about to deposit the pizza slice");
            lock (Program.workingsurface)
            {
                
                Program.workingsurface.AddFirst(s);

                if (Program.workingsurface.Count == Program.n_slices)
                {
                    lock (Program.pickUp)
                    {
                        Program.pickUp.AddFirst(new PizzaDish(Program.n_slices, s.ToString()));

                    }

                    Program.workingsurface.Clear();
                }
            }

            Console.WriteLine($"Pizzaiolo {_id} finished and goes to sleep.");
        }
             
    }
}
