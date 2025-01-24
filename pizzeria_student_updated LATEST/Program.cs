using System;
using System.Collections.Generic;
using System.Threading;

namespace pizzeria
{
    internal class Program
    {
        public static int n_slices = 4;
        public static int n_customers = 200;
        public static int n_pizzaioli = n_customers;

        public static LinkedList<PizzaOrder> order = new();
        public static LinkedList<PizzaDish> pickUp = new();
        public static LinkedList<PizzaSlice> workingsurface = new();

        public static Pizzaiolo[] pizzaioli = new Pizzaiolo[n_pizzaioli];
        public static Customer[] customers = new Customer[n_customers];

        public static Semaphore orderSemaphore = new Semaphore(0, int.MaxValue);
        //public static Semaphore readyOrdersSemaphore = new Semaphore(0, int.MaxValue);

        public static List<Thread> pizzaioloThreads = new List<Thread>();
        public static List<Thread> customerThreads = new List<Thread>();

        static void Main(string[] args)
        {
            if (n_customers % n_slices != 0)
            {
                throw new Exception("n_customers must be a multiple of n_slices");
            }

            InitPeople();
            ActivatePizzaioli();
            ActivateCustomers();

            foreach (Thread t in pizzaioloThreads)
            {
                t.Join();
                
            }

            foreach (Thread t in customerThreads)
            {
                t.Join();
                
            }

            /*// Ensure all customers have completed their eating process
            lock (Customer.GetLockObject())
            {
                while (Customer.GetCompletedCustomers() < n_customers)
                {
                    Monitor.Wait(Customer.GetLockObject());
                }
            }*/

            Console.WriteLine("All should customers have eaten a pizza slice.");
            Console.WriteLine($"Pickup location: There are {pickUp.Count} pizzas left.");
            Console.WriteLine($"Working location: There are {workingsurface.Count} slices left.");
            Console.WriteLine($"Order location: There are {order.Count} orders left.");
            Console.ReadLine();
        }

        private static void ActivateCustomers()
        {
            for (int i = 0; i < n_customers; i++)
            {
                customers[i] = new Customer(i);
                Thread customerThread = new Thread(customers[i].Start);
                customerThreads.Add(customerThread);
                customerThread.Start();
            }
        }

        private static void ActivatePizzaioli()
        {
            for (int i = 0; i < n_pizzaioli; i++)
            {
                pizzaioli[i] = new Pizzaiolo(i);
                Thread pizzaioloThread = new Thread(pizzaioli[i].Start);
                pizzaioloThreads.Add(pizzaioloThread);
                pizzaioloThread.Start();
            }
        }

        private static void InitPeople()
        {
            for (int i = 0; i < n_pizzaioli; i++)
            {
                pizzaioli[i] = new Pizzaiolo(i);
            }

            for (int i = 0; i < n_customers; i++)
            {
                customers[i] = new Customer(i);
            }
        }
    }

    // Order states enumeration
    public enum OrderState
    {
        Ordered,
        Working,
        Ready
    }

    // PizzaDish class representing a pizza with slices
    public class PizzaDish
    {
        public int Slices { get; private set; }
        private string _id;

        public PizzaDish(int slices, string id)
        {
            Slices = slices;
            _id = id;
        }

        public int RemoveSlice()
        {
            if (Slices > 0)
            {
                Slices--;
            }
            else
            {
                throw new Exception($"{_id} No more slices");
            }
            return Slices;
        }
    }

    // Represents a pizza order state
    public class PizzaOrder
    {
        private int sliceprepared = 0;
        public OrderState State { get; private set; }

        public PizzaOrder()
        {
            State = OrderState.Ordered;
        }

        public void StartWorking()
        {
            State = OrderState.Working;
        }

        public PizzaSlice FinishWorking(string sliceId)
        {
            State = OrderState.Ready;
            return new PizzaSlice(sliceId);
        }
    }

    // Represents a single pizza slice
    public class PizzaSlice
    {
        private string _id;

        public PizzaSlice(string id)
        {
            _id = id;
        }
    }
}
