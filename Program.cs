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

        private Order[] multiCells;

        public static Semaphore getSemaph;

        public static Semaphore setSemaph;

        public MultiCellBuffer() //constructor

        {

            // add your implementation here

        }

        public void SetOneCell(Order data)

        {

            // add your implementation here

        }

        public Order GetOneCell()

        {
            // add your implementation here

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

        }

        //get methods

        public string getSenderId()

        {

            // add your implementation here

        }

        public long getCardNo()

        {

            // add your implementation here

        }

        public double getUnitPrice()

        {

            // add your implementation here

        }

        public int getQuantity()

        {

            // add your implementation here

        }

    }

    public class OrderProcessing

    {

        public static event OrderProcessEvent OrderProcess;

        //method to check for valid credit card number input

        public static bool creditCardCheck(long creditCardNumber)

        {

            // add your implementation here

        }

        //method to calculate the final charge after adding taxes, location charges, etc

        public static double calculateCharge(double unitPrice, int quantity)

        {

            // add your implementation here

        }

        //method to process the order

        public static void ProcessOrder(Order order)

        {

            // add your implementation here
        }

    }

    public class TravelAgent

    {

        public static event OrderCreationEvent orderCreation;

        public void agentFun()

        {

            // add your implementation here

        }

        public void orderProcessConfirm(Order order, double orderAmount)

        {

            // add your implementation here

        }

        private void createOrder(string senderId)

        {

            // add your implementation here

        }

        public void agentOrder(double roomPrice, Thread travelAgent) // Callback from hotel thread

        {

            // add your implementation here

        }

    }

    public class Hotel

    {

        static double currentRoomPrice = 100; //random current agent price

        static int threadNo = 0;

        static int eventCount = 0;

        public static event PriceCutEvent PriceCut;

        public void hotelFun()

        {

            // add your implementation here

        }

        //using random method to generate random room prices

        public double pricingModel()

        {

            // add your implementation here

        }

        public void updatePrice(double newRoomPrice)

        {

            // add your implementation here

        }

        public void takeOrder() // callback from travel agent

        {

            // add your implementation here

        }

    }

}

