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

        public static Semaphore getSemaph;      //semaphore for reading

        public static Semaphore setSemaph;      //semaphore for writing

        public MultiCellBuffer() //constructor
        {
            lock (this)
            {
                
                usedCells = 0;                                          //initialize # of used cells
                c = new Order[bufferSize];                              //initialize the buffer
                setSemaph = new Semaphore(bufferSize, bufferSize);      //# empty cells = 3
                getSemaph = new Semaphore(0, bufferSize);               //# filled cells = 0
            }
        }

        public void SetOneCell(Order data)
        {
            
            Console.WriteLine("Setting in buffer cell");
            setSemaph.WaitOne();                //# empty cells -= 1
            
            lock (this)     // lock the buffer while writing to it
            {
                while (usedCells == bufferSize) //thread waits if all cells are filled
                {
                    Monitor.Wait(this);
                }

                for (int i = 0; i < bufferSize; i++)    //find the available cell to write
                {
                    if (c[i] == null)   //check if this cell is available 
                    {
                        c[i] = data;
                        usedCells++;
                        i = bufferSize; //exits
                    }
                }
                getSemaph.Release();            //# filled cells += 1
                Monitor.Pulse(this);            //notify thread that was waiting
            }
            Console.WriteLine("Exit setting in buffer");
        }

        public Order GetOneCell()

        {
            
            getSemaph.WaitOne();                //#filled cells -= 1
            Order result = null;
            Monitor.Enter(this);                //lock the buffer by using Monitor.Enter
            try {
                while (usedCells == 0) //wait if there is nothing to read
                {
                    Monitor.Wait(this);
                }

                for (int i = 0; i < bufferSize; i++)
                {
                    if (c[i] != null) //makes sure there is valid data
                    {
                        result = new Order(c[i].getSenderId(), c[i].getCardNo(), c[i].getUnitPrice(), c[i].getQuantity());
                        c[i] = null;    //set the cell to null
                        usedCells--;
                        i = bufferSize; //exits
                    }
                }
                setSemaph.Release();            //#empty cells += 1
                Monitor.Pulse(this);            //notify thread that was waiting
            } finally {  Monitor.Exit(this); }
            Console.WriteLine("Exit reading buffer");
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
            
            this.senderId = senderId;
            this.cardNo = cardNo;
            this.unitPrice = unitPrice;
            this.quantity = quantity;
        }

        //get methods

        public string getSenderId()

        {

            
            return this.senderId;

        }

        public long getCardNo()

        {

            
            return this.cardNo;
        }

        public double getUnitPrice()

        {

            
            return this.unitPrice;
        }

        public int getQuantity()

        {

            
            return this.quantity;
        }

    }

    public class OrderProcessing

    {

        public static event OrderProcessEvent OrderProcess;

        //method to check for valid credit card number input

        public static bool creditCardCheck(long creditCardNumber)

        {
            
            //valid cardNo is in range [10000000,99999999]
            return (creditCardNumber >= 10000000) && (creditCardNumber <= 99999999);

        }

        //method to calculate the final charge after adding taxes, location charges, etc

        public static double calculateCharge(double unitPrice, int quantity)

        {
            
            Random random = new Random();   
            double tax = (unitPrice * quantity) * 0.1;                      //tax is fixed at 10%
            double locationCharge = 20.0 + (random.NextDouble() * 61.0);    //locationCharge is in range [20,80]
            return (unitPrice * quantity) + tax + locationCharge;

        }

        //method to process the order

        public static void ProcessOrder(Order order)

        {
            
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
            Console.WriteLine("Starting travel agent now");
            
            while (MainClass.hotelThreadRunning) {
                Thread.Sleep(random.Next(1000, 4000));       //An agent creates an order every 1-4 seconds
                createOrder(Thread.CurrentThread.Name);
            }
        }

        public void orderProcessConfirm(Order order, double orderAmount)

        {
            
            Console.WriteLine("Travel Agent {0}'s order is confirmed. The amount to be charged is ${1}", order.getSenderId(), orderAmount.ToString("0.00"));
        }

        private void createOrder(string senderId)

        {
            Console.WriteLine("Inside create order");
            
            Int32 cardNo = random.Next(10000000, 100000000); //generates a random valid credit card number
            Int32 quantity = random.Next(5, 51); //generates a random number of units needed [5,50]

            Order order = new Order(senderId, cardNo, reducedPrice, quantity); //creates orderObject with generated data            
            
            MainClass.buffer.SetOneCell(order); //inserts order into the MultiCellBuffer
            orderCreation(); //emits event to subscribers

        }

        public void agentOrder(double roomPrice, Thread travelAgent) // Callback from hotel thread

        {
            Console.WriteLine("Incoming order for room with price ${0}", roomPrice.ToString("0.00"));
            // write the new reduced price to reducedPrice (a class variable) 
            reducedPrice = roomPrice;            
                        
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
            double newPrice = 80.0 + (random.NextDouble() * 81.0);
            Console.WriteLine("New price is ${0}", newPrice.ToString("0.00"));
            return newPrice;
        }

        public void updatePrice(double newRoomPrice)

        {        
            //Compare the new and current price
            if (PriceCut != null && newRoomPrice < currentRoomPrice) {
                Console.WriteLine("Updating the price and calling price cut event");
                PriceCut(newRoomPrice, MainClass.travelAgentThreads[threadNo]);     //emit priceCutEvent
                if (threadNo == 4)                      // take turn to emit price cut event to each agents
                    threadNo = 0;
                else
                    threadNo++;
                eventCount++;
            }
            currentRoomPrice = newRoomPrice;        //update the price
        }

        public void takeOrder() // callback from travel agent

        {
            
            Order order = MainClass.buffer.GetOneCell();                            //retrieves the order from the MultiCellBuffer
            Thread thread = new Thread(() => OrderProcessing.ProcessOrder(order));  //declares a new thread to process the order
            thread.Start();                                                         //starts the orderProcessing thread
        }

    }

}

