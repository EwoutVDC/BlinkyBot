using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MargieBot;
using MargieBot.Models;

namespace BlinkyBot
{
    class BlinkyBot : Bot
    {
        public void Say(string channel, string text)
        {
            BotMessage message = new BotMessage();
            message.Text = text;
            message.ChatHub = this.ConnectedHubs.Values.First(hub => hub.Name == channel);
            this.Say(message).Wait();
        }
    }
}
