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
        //TODO: write lines to file/DB instead of debug
        //TODO: handle message editing, url's, etc...


        public bool CanRespond(ResponseContext context)
        {
            return context.Message.Text.Length > 0;
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            Debug.WriteLine("Raw json data: "+context.Message.RawData);

            Debug.WriteLine(context.Message.ChatHub.Name //Name includes #
                + " [" + context.Message.TimeStamp.ToString("dd/MM/yyyy hh:mm:ss") + "] "
                + "<" + context.UserNameCache[context.Message.User.ID] + "> "
                + context.Message.Text);
            
            return new BotMessage(); //empty response doesn't say anything
        }
    }
}
