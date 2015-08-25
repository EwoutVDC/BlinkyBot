
using MargieBot.Models;
using System.Diagnostics;
using System;
using System.IO;
using System.Data.SQLite;

using Epoch.Extensions;

namespace BlinkyBot.Responders
{
    class LogResponder : MargieBot.Responders.IResponder
    {
        //TODO: handle message editing, url's, etc...

        public static TimeZoneInfo fileLogTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        
        private SQLiteConnection dbConn;

        public LogResponder(SQLiteConnection dbConn)
        {
            this.dbConn = dbConn;

            try
            {
                SQLiteCommand cmd = new SQLiteCommand(
                    "CREATE TABLE IF NOT EXISTS logs (channel TEXT, timestamp INTEGER, user TEXT, message TEXT)",
                    dbConn);
                cmd.ExecuteNonQuery();

                //TODO: Add indexing to logs table
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("InvalidOperationException caught: " + e.ToString());
                Console.WriteLine("LogResponder could not create log table");
                Program.WaitAndExit(1);
            }
        }

        public bool CanRespond(ResponseContext context)
        {
            return context.Message.Text.Length > 0;
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            Debug.WriteLine("Raw json data: "+context.Message.RawData);

            //Timezone adjustments for file logging is done here since files = frontend
            string logmsg = " [" + TimeZoneInfo.ConvertTimeFromUtc(context.Message.TimeStamp, fileLogTimeZone).ToString("dd/MM/yyyy HH:mm:ss") + "] "
                + "<" + context.UserNameCache[context.Message.User.ID] + "> "
                + context.Message.Text;

            Debug.WriteLine(context.Message.ChatHub.Name //Name includes #
                + logmsg);

            using (StreamWriter outfile = File.AppendText(context.Message.TimeStamp.ToString("yyyy-MM-dd") + "_" + context.Message.ChatHub.Name + ".txt"))
            {
                outfile.WriteLine(logmsg);
            }

            addMessageToDb(context);

            return new BotMessage(); //empty response doesn't say anything
        }

        private void addMessageToDb(ResponseContext context)
        {
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(
                      "INSERT INTO logs (channel, timestamp, user, message) values ("
                      + "@channelname, @timestamp, @username, @message)",
                      dbConn);
                cmd.Parameters.AddWithValue("@channelname", context.Message.ChatHub.Name);
                //No timezone adjustment for the database, this should be done in the frontend (for the local timezone since some weirdos don't live in CET)
                cmd.Parameters.AddWithValue("@timestamp", context.Message.TimeStamp.ToUnix());
                cmd.Parameters.AddWithValue("@username", context.UserNameCache[context.Message.User.ID]);
                cmd.Parameters.AddWithValue("@message", context.Message.Text);

                Debug.WriteLine("Executing sql:");
                Debug.WriteLine(cmd.CommandText);
                cmd.ExecuteNonQuery();
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("InvalidOperationException caught: " + e.ToString());
            }
        }
    }
}
