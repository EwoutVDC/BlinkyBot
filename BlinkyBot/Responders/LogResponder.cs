
using MargieBot.Models;
using System.Diagnostics;
using System;
using System.IO;

namespace BlinkyBot.Responders
{
    class LogResponder : MargieBot.Responders.IResponder
    {
        //TODO: write lines to DB instead of debug/file
        //TODO: handle message editing, url's, etc...

        public bool CanRespond(ResponseContext context)
        {
            return context.Message.Text.Length > 0;
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            Debug.WriteLine("Raw json data: "+context.Message.RawData);

            //Timezone adjustments should be done by the frontend
            string logmsg = " [" + context.Message.TimeStamp.ToString("dd/MM/yyyy hh:mm:ss") + "] "
                + "<" + context.UserNameCache[context.Message.User.ID] + "> "
                + context.Message.Text;

            Debug.WriteLine(context.Message.ChatHub.Name //Name includes #
                + logmsg);

            using (StreamWriter outfile = File.AppendText(context.Message.ChatHub.Name + "_" + context.Message.TimeStamp.ToString("dd-MM-yyyy") + ".txt"))
            {
                outfile.WriteLine(logmsg);
            }

            return new BotMessage(); //empty response doesn't say anything
        }
    }
}
