using ConnectedVehicleAndHomeServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.ServiceBus.Messaging;

namespace TopicPublisher
{
    class Program
    {
        static List<HomeVehicle> GenerateInfo(int infoSetsRequired, bool addduplicates=false)
        {
            Random random = new Random();
            List<HomeVehicle> infoSet = new List<HomeVehicle>();
            string[] names = new string[]
            {"MyHome", "ParentsHome", "SiblingsHome", "MyCar", "MyParentsCar", "MySiblingsCar"
            };
            string[] temperatures = new string[]
            {"72.5", "80.3", "32.0", "10.0", "23.0", "72.0"
            };

            for (int i=0; i< infoSetsRequired; i++)
            {
              
                string tempType = string.Empty;
                string tempTemperature = string.Empty;
                string temptheftDetected = string.Empty;

                string tempName = names[random.Next(names.Length)];

                if (tempName.Contains("Car"))
                    tempType = "Car";
                else tempType = "Home";

                tempTemperature = temperatures[random.Next(temperatures.Length)];

                if (Convert.ToDecimal(tempTemperature) < 40)
                    temptheftDetected = "Yes. Please call 911.";
                else temptheftDetected = "No, Everything is good.";

                infoSet.Add(new HomeVehicle()
                {
                    ID = (DateTime.Now.Ticks - DateTime.Parse("12/12/2015").Ticks + i).ToString(),
                    Name = tempName,
                    Type = tempType,
                    temperature = tempTemperature,
                    theftDetected = temptheftDetected
                });
            }
            return infoSet;
        }
        


        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Enter a number to mock amount of Home Vehicle Messages to be sent");
                int noOfMessages;
                if (!Int32.TryParse(Console.ReadLine(), out noOfMessages))
                {
                    break;
                }
                SendHomeVehicleStatsToTopic(noOfMessages,true);
                

            }
           
        }

        

        static void SendHomeVehicleStatsToTopic( int noOfMessages,bool addDuplicates=false)
        {
            Random random = new Random();
            List<HomeVehicle> infoSet = GenerateInfo(noOfMessages);
            //add duplicate messages 25% percent of the time
            for(int i=0; i< noOfMessages; i++)
            {
                if (random.NextDouble() < 0.25 && addDuplicates)
                {
                    infoSet.Add(infoSet[i]);
                }
            }
            Console.WriteLine("Generated {0} unqiue and {1} duplicate", noOfMessages, infoSet.Count - noOfMessages);
           
            string connectionString = ConfigurationManager.AppSettings["PublisherConnectionString"];
            
            string topicPath = "brokerhomevehicletopic";
            TopicClient vehicleHomeUpdateTopicClient = TopicClient.CreateFromConnectionString(connectionString, topicPath);
            Console.WriteLine("Sending messages to the Topic {0}", topicPath);
            foreach (HomeVehicle info in infoSet)
            {
                BrokeredMessage message = new BrokeredMessage(info);
                message.Label = info.ToString();
                message.MessageId = info.Tag;
                //for creating subscriptions with correlationfilters
                message.CorrelationId = info.Type;
                message.Properties.Add("Temperature", info.temperature);
                message.Properties.Add("Theft", info.theftDetected);
                vehicleHomeUpdateTopicClient.Send(message);
                Console.WriteLine("Sent{0},{1}", message.MessageId, message.Label);
            }
            vehicleHomeUpdateTopicClient.Close();
            Console.WriteLine("Sent {0}  messages", infoSet.Count);

        }
    }
}
