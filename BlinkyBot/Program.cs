using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MargieBot.Models;
using BlinkyBot.Responders;

namespace BlinkyBot
{
    public static class Program
    {
        public static void WaitAndExit(int code)
        {
            Console.WriteLine("Press ESC to stop");
            do
            {
                System.Threading.Thread.Sleep(50);
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            Environment.Exit(code);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please pass the slack api key as argument");
                WaitAndExit(1);
            }
            string slackApiKey = args[0];

            BlinkyBot blinky = new BlinkyBot();
            blinky.Responders.Add(new LogResponder(blinky.dbConnection));
            blinky.Responders.Add(new QuoteResponder(blinky.dbConnection));

            try
            {
                blinky.Connect(slackApiKey).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to slack api key '" + slackApiKey + "'");
                Console.WriteLine(e.ToString());
                WaitAndExit(1);
            }

            if (!blinky.IsConnected)
            {
                Console.WriteLine("Could not connect to slack api key '" + slackApiKey + "'");
                WaitAndExit(1);
            }

            System.Diagnostics.Debug.WriteLine("Connected hubs:");
            foreach(KeyValuePair<string, SlackChatHub> kv in blinky.ConnectedHubs)
            {
                System.Diagnostics.Debug.WriteLine(kv.Key + " : " + kv.Value.Name);
            }

            //blinky.Say("#blinky-testing", "I'm alive!");

            WaitAndExit(0);
            
            //blinky.Say("#blinky-testing", "Shutting down");
        }
    }
}
