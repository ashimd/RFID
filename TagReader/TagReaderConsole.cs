using System;
using Microsoft.ServiceBus.Messaging;
using RFID.Common.Config;
using RFID.Config.DataContracts;
using System.Threading;

namespace RFID.TagReader
{
    class TagReaderConsole
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Tag Reader Console");

            // Create the message factory
            MessagingFactory factory = MessagingFactory.CreateFromConnectionString
                (AccountDetails.ConnectionString);     // MessagingFactory.Create(serviceBusUrl, credentials);

            // Use the MessagingFactory to create a queue client for the specified queue
            QueueClient queueClient = factory.CreateQueueClient(AccountDetails.QueueName);

            // Create a sample order
            RFIDTag[] orderItems = new RFIDTag[]{
                new RFIDTag() { Product="Ball", Price = 4.99},
                new RFIDTag() { Product = "Whistle", Price = 1.95 },
                new RFIDTag() { Product = "Bat", Price = 12.99 },
                new RFIDTag() { Product = "Bat", Price = 12.99 },
                new RFIDTag() { Product = "Gloves", Price = 7.99 },
                new RFIDTag() { Product = "Gloves", Price = 7.99 },
                new RFIDTag() { Product = "Cap", Price = 9.99 },
                new RFIDTag() { Product = "Cap", Price = 9.99},
                new RFIDTag() { Product = "Shirt", Price = 14.99 },
                new RFIDTag() { Product = "Shirt", Price = 14.99 }
            };

            // Display the order data
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Order contains {0} items", orderItems.Length);
            Console.ForegroundColor = ConsoleColor.Yellow;

            double orderTotal = 0.0;
            foreach (RFIDTag tag in orderItems)
            {
                Console.WriteLine("{0} - ${1}", tag.Product, tag.Price);
                orderTotal += tag.Price;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Order value = ${0}.", orderTotal);
            Console.WriteLine();
            Console.ResetColor();

            Console.WriteLine("Press enter to scan...");
            Console.ReadLine();

            // Send the order with random duplicate tag reads
            int sentCount = 0;
            int position = 0;
            Random random = new Random(DateTime.Now.Millisecond);

            Console.WriteLine("Reading tags...");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;

            var sessionId = Guid.NewGuid().ToString();

            while (position < 10)
            {
                RFIDTag rfidTag = orderItems[position];

                // Create a new brokered message from the order item RFID tag
                BrokeredMessage tagRead = new BrokeredMessage(rfidTag);

                // Comment in to set message id
                tagRead.MessageId = rfidTag.TagId;

                // Comment in to set session id
                tagRead.SessionId = sessionId;

                // Send the message
                queueClient.Send(tagRead);
                //Console.WriteLine("Sent: {0}", orderItems[position].Product);
                Console.WriteLine("Sent: {0} - MessageId: {1}", orderItems[position].Product, tagRead.MessageId);

                // Randomly cause a duplicate message to be sent
                if (random.NextDouble() > 0.4)
                    position++;

                sentCount++;

                Thread.Sleep(100);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{0} total tag reads.", sentCount);
            Console.WriteLine();
            Console.ResetColor();
            Console.ReadLine();
        }
    }
}
