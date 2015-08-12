using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MargieBot;
using MargieBot.Models;

namespace SlackLogQuote
{
    class Program
    {
        private static void say(Bot bot, string text)
        {
            BotMessage message = new BotMessage();
            message.ChatHub = bot.ConnectedHubs.Values.First(hub => hub.Name == "#blinky-testing");
            message.Text = text;
            bot.Say(message).Wait();
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please pass the slack api key as argument");
                Environment.Exit(1);
            }
            string slackApiKey = args[0];

            Bot bot = new Bot();
            bot.Responders.Add(new Responders.LogResponder());

            try
            {
                bot.Connect(slackApiKey).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to slack api key '" + slackApiKey + "'");
                Console.WriteLine(e.ToString());
                Environment.Exit(1);
            }

            if (!bot.IsConnected)
            {
                Console.WriteLine("Could not connect to slack api key '" + slackApiKey + "'");
                Environment.Exit(1);
            }

            System.Diagnostics.Debug.WriteLine("Connected hubs:");
            foreach(KeyValuePair<string, SlackChatHub> kv in bot.ConnectedHubs)
            {
                System.Diagnostics.Debug.WriteLine(kv.Key + " : " + kv.Value.Name);
            }

            //say(bot, "I'm alive!");

            Console.WriteLine("Press ESC to stop");
            do
            {
                System.Threading.Thread.Sleep(50);
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            //say(bot, "Shutting down");
        }
    }
}
