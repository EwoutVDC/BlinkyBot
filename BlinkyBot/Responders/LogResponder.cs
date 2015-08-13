using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MargieBot.Models;

namespace BlinkyBot.Responders
{
    class LogResponder : MargieBot.Responders.IResponder
    {
        public bool CanRespond(ResponseContext context)
        {
            return context.Message.Text.Length > 0;
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            System.Diagnostics.Debug.WriteLine(context.Message.RawData);
            
            return new BotMessage();
        }
    }
}
