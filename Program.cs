using System;

using System.Collections.Generic;
using System.Threading;

namespace ConsoleApp1

{

    //delegate declaration for creating events

    public delegate void PriceCutEvent(double roomPrice, Thread agentThread);

    public delegate void OrderProcessEvent(Order order, double orderAmount);

    public delegate void OrderCreationEvent();

    public class MainClass

    {
        public static MultiCellBuffer buffer;

        public static Thread[] travelAgentThreads;

        public static bool hotelThreadRunning = true;

        public static void Main(string[] args)

        {

            Console.WriteLine("Inside Main");

            buffer = new MultiCellBuffer();

            Hotel hotel = new Hotel();

            TravelAgent travelAgent = new TravelAgent();

            Thread hotelThread = new Thread(new ThreadStart(hotel.hotelFun));

            hotelThread.Start();

            Hotel.PriceCut += new PriceCutEvent(travelAgent.agentOrder);

            //short-hand notation   
            //Hotel.PriceCut += travelAgent.agentOrder;

            Console.WriteLine("Price cut event has been subscribed");

            TravelAgent.orderCreation += new OrderCreationEvent(hotel.takeOrder);

            Console.WriteLine("Order creation event has been subscribed");

            OrderProcessing.OrderProcess += new OrderProcessEvent(travelAgent.orderProcessConfirm);

            Console.WriteLine("Order process event has been subscribed");

            travelAgentThreads = new Thread[5];

            for (int i = 0; i < 5; i++)

            {

                Console.WriteLine("Creating travel agent thread {0}", (i + 1));

                travelAgentThreads[i] = new Thread(travelAgent.agentFun);

                travelAgentThreads[i].Name = (i + 1).ToString();

                travelAgentThreads[i].Start();

            }

        }

    }

    public class MultiCellBuffer

    {

        // Each cell can contain an order object

        private const int bufferSize = 3; //buffer size

        int usedCells;

        private Order[] c;

        public static Semaphore getSemaph;

        public static Semaphore setSemaph;        
 
        public MultiCellBuffer() //constructor

        {
            lock (this)
            {
                // add your implementation here
                usedCells = 0;                                          //initialize # of used cells
                setSemaph = new Semaphore(bufferSize, bufferSize);      
                getSemaph = new Semaphore(bufferSize, bufferSize);
                c = new Order[bufferSize];
            }

        }

        public void SetOneCell(Order data)
        {
            // add your implementation here
            //if semaphore count == 0 --> no write, return;
            //else
            setSemaph.WaitOne();

            lock (this)
            {
                while (usedCells == bufferSize) //thread waits if all cells are filled
                {
                    Monitor.Wait(this);
                }

                for (int i = 0; i < bufferSize; i++)
                {
                    if (c[i] == null) //makes sure there is no data being over-written 
                    {
                        c[i] = data;
                        usedCells++;
                        i = bufferSize; //exits loop
                    }
                }
                setSemaph.Release();
                Monitor.Pulse(this);
            }
        }

        public Order GetOneCell()

        {
            // add your implementation here
            getSemaph.WaitOne();
            Order result = null;
            lock (this)
            {
                while (usedCells == 0) //thread waits if no cells are full
                {
                    Monitor.Wait(this);
                }

                for (int i = 0; i < bufferSize; i++)
                {
                    if (c[i] != null) //makes sure there is valid data
                    {
                        result = new Order(c[i].getSenderId(), c[i].getCardNo(), c[i].getUnitPrice(), c[i].getQuantity());
                        c[i] = null;
                        usedCells--;
                        i = bufferSize; //exits loop
                    }
                }
                getSemaph.Release();
                Monitor.Pulse(this);
            }
            return result;

        }

    }

    public class Order
    {

        //identity of sender of order

        private string senderId;

        //credit card number

        private long cardNo;

        //unit price of room from hotel

        private double unitPrice;

        //quantity of rooms to order

        private int quantity;

        //parametrized constructor

        public Order(string senderId, long cardNo, double unitPrice, int quantity)

        {
            // add your implementation here
            this.senderId = senderId;
            this.cardNo = cardNo;
            this.unitPrice = unitPrice;
            this.quantity = quantity;
        }

        //get methods

        public string getSenderId()

        {

            // add your implementation here
            return this.senderId;

        }

        public long getCardNo()

        {

            // add your implementation here
            return this.cardNo;
        }

        public double getUnitPrice()

        {

            // add your implementation here
            return this.unitPrice;
        }

        public int getQuantity()

        {

            // add your implementation here
            return this.quantity;
        }

    }

    public class OrderProcessing

    {

        public static event OrderProcessEvent OrderProcess;

        //method to check for valid credit card number input

        public static bool creditCardCheck(long creditCardNumber)

        {
            // add your implementation here
            return (creditCardNumber >= 10000000) && (creditCardNumber <= 99999999);

        }

        //method to calculate the final charge after adding taxes, location charges, etc

        public static double calculateCharge(double unitPrice, int quantity)

        {
            // add your implementation here
            Random random = new Random();   
            double tax = (unitPrice * quantity) * 0.1;                      //tax is fixed at 10%
            double locationCharge = 20.0 + (random.NextDouble() * 61.0);    //locationCharge is in range [20,80]
            return (unitPrice * quantity) + tax + locationCharge;

        }

        //method to process the order

        public static void ProcessOrder(Order order)

        {
            // add your implementation here
            //calculate the orderAmount;
            double orderAmount = calculateCharge(order.getUnitPrice(),order.getQuantity());
            
            //if the card is valid, emit an event on order processed successfully
            if (creditCardCheck(order.getCardNo())) {
                OrderProcess(order, orderAmount);
            }
        }

    }

    public class TravelAgent

    {

        public static event OrderCreationEvent orderCreation;

        public static double reducedPrice;

        private static Random random = new Random();

        public void agentFun()

        {
            // add your implementation here
            while (MainClass.hotelThreadRunning) {
                Thread.Sleep(random.Next(1000, 4000));
                createOrder(Thread.CurrentThread.Name);
            }
        }

        public void orderProcessConfirm(Order order, double orderAmount)

        {
            // add your implementation here
            Console.WriteLine("Agent {0}'s order has been processed. The amount to be charged is $" + orderAmount + " ($" + order.getUnitPrice() + " per room for " + order.getQuantity() + " rooms).", order.getSenderId(), Thread.CurrentThread.Name);

        }

        private void createOrder(string senderId)

        {

            // add your implementation here
            Int32 cardNo = random.Next(10000000, 100000000); //generates a random valid credit card number
            Int32 quantity = random.Next(5, 50); //generates a random number of units needed (between 5 and 50)

            Order order = new Order(senderId, cardNo, reducedPrice, quantity); //creates orderObject with generated data

            Console.WriteLine("Agent {0}'s order has been created at {1}.", senderId, DateTime.Now.ToString("hh:mm:ss"));

            MainClass.buffer.SetOneCell(order); //inserts order into the MultiCellBuffer
            orderCreation(); //emits event to subscribers

        }

        public void agentOrder(double roomPrice, Thread travelAgent) // Callback from hotel thread

        {

            // write the new reduced price to reducedPrice (a class variable) 
            reducedPrice = roomPrice;

            //read the new price 

            //calculate the # of rooms to order
            Console.WriteLine("Rooms are on sale. Agent {0} is about to place an order.", travelAgent.Name);            
            //send the order to the MultiCellBuffer
            if (travelAgent!=null)
                createOrder(travelAgent.Name);

        }

    }

    public class Hotel

    {
        private static Random random = new Random();

        static double currentRoomPrice = 100; //random current agent price

        static int threadNo = 0;

        static int eventCount = 0;

        public static event PriceCutEvent PriceCut;

        public void hotelFun()

        {

            // add your implementation here
            while (eventCount < 10)
            {
                Thread.Sleep(random.Next(1000, 2000)); //generates a new price every 1 - 2 seconds
                updatePrice(pricingModel());                
            }
            MainClass.hotelThreadRunning = false; // lets retailer threads know that the chicken thread has ended


        }

        //using random method to generate random room prices

        public double pricingModel()
        {
            // generate random double in range [80,160]
            return 80.0 + (random.NextDouble() * 81.0);
        }

        public void updatePrice(double newRoomPrice)

        {

            // add your implementation here
            currentRoomPrice = newRoomPrice;

            //Compare the new and current price
            if (PriceCut != null && newRoomPrice < currentRoomPrice) {
                PriceCut(newRoomPrice, MainClass.travelAgentThreads[threadNo]);
                threadNo++;
                eventCount++;
            }
        }

        public void takeOrder() // callback from travel agent

        {

            // add your implementation here
            Order order = MainClass.buffer.GetOneCell(); //retrieves the order from the MultiCellBuffer
            Thread thread = new Thread(() => OrderProcessing.ProcessOrder(order));
            thread.Start(); //starts the order processing thread
        }

    }

}

