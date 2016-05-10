using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Data.SQLite;
using System.Data;

using System.Text;
using System.Threading.Tasks;

using MargieBot;
using MargieBot.Models;

namespace BlinkyBot
{
    class BlinkyBot : Bot
    {
        public const string DB_FILE_NAME = "blinkyDatabase.sqlite";
        public SQLiteConnection dbConnection { get; } //No setter

        public BlinkyBot()
        {
            //Open database
            if (!File.Exists(DB_FILE_NAME))
            {
                createDatabase();
            }

            dbConnection = new SQLiteConnection("Data source =" + DB_FILE_NAME + ";Version=3;");
            dbConnection.Open();

            if (dbConnection.State != ConnectionState.Open)
            {
                Console.WriteLine("Could not open db connection to '" + DB_FILE_NAME + "'");
                Program.WaitAndExit(1);
            }

            //TODO Load data - perhaps done in responders?
        }

        ~BlinkyBot()
        {
            if (dbConnection != null)
                dbConnection.Close();
            //TODO: ObjectDisposed exception on shutdown
        }

        public void Say(string channel, string text)
        {
            BotMessage message = new BotMessage();
            message.Text = text;
            message.ChatHub = this.ConnectedHubs.Values.First(hub => hub.Name == channel);
            this.Say(message).Wait();
        }

        private void createDatabase()
        {
            SQLiteConnection.CreateFile(DB_FILE_NAME);
            //TODO: Perhaps create a User table? ID - Nickname. How to handle nickname changes?
            //TODO: Review: Tables are created in the responders that need them. This might create responder dependencies.
        }
    }
}
