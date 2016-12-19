using ConnectedVehicleAndHomeServiceBus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopicSubsciber
{

    class Program
    {
        private static NamespaceManager nsManager;
        private static MessagingFactory factory;

        static void Main(string[] args)
        {
            string topicPath = "brokerhomevehicletopic";
            Initialize();
            Console.WriteLine(" Processing subscriptions for topic {0}", topicPath);
           // DeleteTopicSubscriptions(topicPath);
            CreateTopicSubscriptions(topicPath);
            while (true)
            {
                Console.WriteLine(" Recieving messages");
                //RecieveMessagesFromSubscriptions(topicPath, "AllVehicleAndHomeMessages");
                RecieveMessagesFromSubscriptions(topicPath, "DangeorusHomeTemperatureMessages");
                RecieveMessagesFromSubscriptions(topicPath, "DangeorusCarTemperatureMessages");
                RecieveMessagesFromSubscriptions(topicPath, "TheftMessages");
                Console.WriteLine("Recieved messages");
                Console.WriteLine("Press E to exit any other key continue");
                string input = Console.ReadLine();
                if(input.Equals("E", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }



            }
            Cleanup();
        }
        static void Initialize()
        {
            string ns = "ConnectedHomeAndVehicleManagement";
            Uri uri = ServiceBusEnvironment.CreateServiceUri("sb", ns, string.Empty);
            //Create manager level credentials using Shared Access Signature Token
            string keyName = "RootManageSharedAccessKey";
            string keyValue = "";
            TokenProvider credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, keyValue);
            //create namespace client - used to manage the namespace objects
            nsManager = new NamespaceManager(uri, credentials);


            //Create  subscriber only credentials using Shared Access Signature Token with LISTEN only
            keyName = "SubscriberAccessKey";
            keyValue = "";
            credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, keyValue);
            //create messaging factory
            factory = MessagingFactory.Create(uri, credentials);
        }
        /// <summary>
        /// Clean up any subscriptions that are not closed already
        /// </summary>
        static void Cleanup()
        {
            if (!(factory.IsClosed))
            {
                factory.Close();
            }
        }

        private static void DeleteTopicSubscriptions(string topic)
        {
            if (nsManager.SubscriptionExists(topic, "DangeorusHomeTemperatureMessages"))
            {
                nsManager.DeleteSubscription(topic, "DangeorusHomeTemperatureMessages");
            }
            if (nsManager.SubscriptionExists(topic, "DangeorusCarTemperatureMessages"))
            {
                nsManager.DeleteSubscription(topic, "DangeorusCarTemperatureMessages");
            }
            if (nsManager.SubscriptionExists(topic, "TheftMessages"))
            {
                nsManager.DeleteSubscription(topic, "TheftMessages");
            }

        }

        private static void CreateTopicSubscriptions(string topic)
        {
            //subcription to all messages for home and vehicles
            if (!nsManager.SubscriptionExists(topic, "AllVehicleAndHomeMessages"))
            {
                nsManager.CreateSubscription(topic, "AllVehicleAndHomeMessages");
            }
            Filter filter;
            //add temperature and type(car or home) based subscription
            if (!nsManager.SubscriptionExists(topic, "DangeorusHomeTemperatureMessages"))
            {
                filter = new SqlFilter("Temperature < 72.0");
                nsManager.CreateSubscription(topic, "DangeorusHomeTemperatureMessages", filter);
            }
            if (!nsManager.SubscriptionExists(topic, "DangeorusCarTemperatureMessages"))
            {
                filter = new CorrelationFilter("Car");
                nsManager.CreateSubscription(topic, "DangeorusCarTemperatureMessages", filter);
            }

            //add theft based subscription
            if (!nsManager.SubscriptionExists(topic, "TheftMessages"))
            {
                filter = new SqlFilter("Theft = 'Yes. Please call 911.'");
                nsManager.CreateSubscription(topic, "TheftMessages", filter);
            }
            

        }
        static void RecieveMessagesFromSubscriptions (string topic, string subscription)
        {
            List<HomeVehicle> homeAndVehiclemessages = new List<HomeVehicle>();
            Console.WriteLine("Recieving messages for {0}", subscription);

            //create subscription client in peeklock mode
            SubscriptionClient homeandVehicleSubscriptionClient = factory.CreateSubscriptionClient(topic, subscription, ReceiveMode.PeekLock);
            Console.WriteLine("Recieving Messages");
            BrokeredMessage message;
            while((message = homeandVehicleSubscriptionClient.Receive(new TimeSpan(0,0,1)))!= null)
            {
                var homeAndVehiclemessage = message.GetBody<HomeVehicle>();
                Console.WriteLine(" Recieved: {0},{1},{2}", message.SequenceNumber, message.MessageId, homeAndVehiclemessage.ToString());
                homeAndVehiclemessages.Add(homeAndVehiclemessage);
                message.Complete();
            }
            Console.WriteLine(" Recieved {0} Messages", homeAndVehiclemessages.Count);
            homeandVehicleSubscriptionClient.Close();

        }
    }
}
