using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MargieBot.Models;
using System.Diagnostics;

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
            Debug.WriteLine("Raw json data: "+context.Message.RawData);

            Debug.WriteLine("[" + context.Message.TimeStamp.ToString("dd/MM/yyyy hh:mm:ss") + "] <" + context.Message.User.FormattedUserID + "> " + context.Message.Text);
            
            return new BotMessage();
        }
    }
}
