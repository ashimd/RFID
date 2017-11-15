using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using RFID.Common.Config;
using RFID.Config.DataContracts;
using System;

namespace RFID.Checkout
{
    class CheckoutConsole
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Checkout Console");

            // Create the namespace
            NamespaceManager manager =
                // new NamespaceManager(serviceBusUri, credentials);
                NamespaceManager.CreateFromConnectionString(AccountDetails.ConnectionString);

            // Create the MessagingFactory
            MessagingFactory factory =
                // MessagingFactory.Create(serviceBusUri, credentials);
                MessagingFactory.CreateFromConnectionString(AccountDetails.ConnectionString);

            // Delete the queue if it exists
            if (manager.QueueExists(AccountDetails.QueueName))
                manager.DeleteQueue(AccountDetails.QueueName);

            // Create a description for the queue
            QueueDescription rfidCheckoutQueueDescription =
                new QueueDescription(AccountDetails.QueueName)
                {
                    // Comment in to require duplicate detection
                    RequiresDuplicateDetection = true,
                    DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(10),

                    // Comment in to require sessions
                    RequiresSession = true
                };

            // Create a queue based on the queue description
            manager.CreateQueue(rfidCheckoutQueueDescription);

            // Use the MessagingFactory to create a queue client for the specified queue
            QueueClient queueClient = factory.CreateQueueClient(AccountDetails.QueueName);

            Console.WriteLine("Receiving Tag read messages...");
            while (true)
            {
                int receivedCount = 0;
                double billTotal = 0.0;

                // Comment in to use a session receiver
                Console.ForegroundColor = ConsoleColor.Cyan;
                var messageSession = queueClient.AcceptMessageSession();
                Console.WriteLine("Accepted session: " + messageSession.SessionId);

                Console.ForegroundColor = ConsoleColor.Yellow;

                while (true)
                {
                    // Receive a tag read message

                    // Swap comments to use a session receiver
                    //var receivedTagRead = queueClient.Receive(TimeSpan.FromSeconds(5));
                    var receivedTagRead = messageSession.Receive(TimeSpan.FromSeconds(5));

                    if (receivedTagRead != null)
                    {
                        // Process the message
                        RFIDTag tag = receivedTagRead.GetBody<RFIDTag>();
                        Console.WriteLine("Bill for {0}", tag.Product);
                        receivedCount++;
                        billTotal += tag.Price;

                        // Mark the message as complete
                        receivedTagRead.Complete();
                    }
                    else
                    {
                        break;
                    }
                }

                if (receivedCount > 0)
                {
                    // Bill the customer
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine
                        ("Bill customer ${0} for {1} items.", billTotal, receivedCount);
                    Console.WriteLine();
                    Console.ResetColor();
                }
            }
        }
    }
}
