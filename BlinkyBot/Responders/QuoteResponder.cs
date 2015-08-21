using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.SQLite;
using MargieBot.Models;


namespace BlinkyBot.Responders
{
    class QuoteResponder : MargieBot.Responders.IResponder
    {
        public const string quoteStart = "!quote";
        public const string quoteAdd = "add";
        public const string quoteDel = "del";
        public const string quoteGet = "get"; //get by id
        public const string quoteSearch = "search"; //TODO
        public const string quoteRandom = "";
        public const string quoteHelp = "help"; //TODO

        private List<string> quotes = new List<string>();
        private SQLiteConnection dbConn;

        public QuoteResponder(SQLiteConnection dbConn)
        {
            this.dbConn = dbConn;
        }


        private int GetQuoteIndex(string request, BotMessage reply)
        {
            string idStr = request.Substring(request.IndexOf(" ") + 1);
            int id = -1;

            try
            {
                id = Convert.ToInt32(idStr);
            }
            catch (FormatException)
            {
                reply.Text = "Badly formatted id: '" + idStr + "'";
                return -1;
            }

            if (id < 1 || id >= quotes.Count)
            {
                reply.Text = "I don't have a quote #" + id;
                return -1;
            }

            return id-1; //quote id = index + 1
        }

        public bool CanRespond(ResponseContext context)
        {
            //TODO: don't respond to bots - needs context.Message.User.isBot property

            Debug.WriteLine("Message '"+ context.Message.Text + "' respondable: " + context.Message.Text.StartsWith("!quote"));
            return context.Message.Text.StartsWith("!quote");
        }

        public BotMessage GetResponse(ResponseContext context)
        {

            string request = context.Message.Text.Length > quoteStart.Length ? context.Message.Text.Substring(quoteStart.Length+1) : "";
            Debug.WriteLine("request: '" + request + "'");
            BotMessage reply = new BotMessage() { ChatHub = context.Message.ChatHub };

            string command = request.Contains(" ") ? request.Substring(0, request.IndexOf(" ")) : request;
            Debug.WriteLine("command: '" + command + "'");

            switch (command)
            {
                case quoteAdd:
                    string quote = request.Substring(request.IndexOf(" ") + 1)
                        + " (Added by " + context.UserNameCache[context.Message.User.ID]
                        + " at " + context.Message.TimeStamp.ToString("dd/MM/yyyy hh:mm:ss") + ")";
                    if (quote.Length > 0)
                    {
                        quotes.Add(quote);
                        reply.Text = "Success! Added quote #" + quotes.Count + ": " + quote;
                    }
                    else
                    {
                        reply.Text = "Nope! Not adding empty quote.";
                    }
                            
                    break;

                case quoteDel:
                    {
                        int id = GetQuoteIndex(request, reply);

                        if (id != -1)
                        {
                            reply.Text = "Succesfully deleted quote #" + id + " '" + quotes[id] + "'";
                            quotes.RemoveAt(id);
                        }

                        break;
                    }

                case quoteGet:
                    {
                        int id = GetQuoteIndex(request, reply);
                        if (id != -1)
                        {
                            reply.Text = "Quote #" + id + " " + quotes[id];
                        }
                        break;
                    }

                case quoteRandom:
                    {
                        Random rnd = new Random();
                        int id = rnd.Next(0, quotes.Count);
                        reply.Text = "Quote #" + id + " " + quotes[id];
                        break;
                    }

                default:
                    Debug.WriteLine("Unsupported quote command '" + command + "' (request '" + request + "')");
                    break;
            }

            return reply;
        }
    }
}
