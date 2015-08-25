using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.SQLite;
using MargieBot.Models;
using BlinkyBot.Models;

namespace BlinkyBot.Responders
{
    class QuoteResponder : MargieBot.Responders.IResponder
    {
        public static TimeZoneInfo quoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        public const string quoteStart = "!quote";
        public const string quoteAdd = "add";
        public const string quoteDel = "del";
        public const string quoteGet = "get"; //get by id
        public const string quoteSearch = "search"; //TODO
        public const string quoteRandom = "";
        public const string quoteHelp = "help"; //TODO
        
        private SQLiteConnection dbConn;

        public QuoteResponder(SQLiteConnection dbConn)
        {
            this.dbConn = dbConn;

            try
            {
                SQLiteCommand cmd = new SQLiteCommand(
                    "CREATE TABLE IF NOT EXISTS quotes (id INTEGER PRIMARY KEY, channel TEXT, timestamp INTEGER, user TEXT, text TEXT)",
                    dbConn);
                cmd.ExecuteNonQuery();

                //TODO: Add indexing to quotes table
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("InvalidOperationException caught: " + e.ToString());
                Console.WriteLine("QuoteResponder could not create quotes table");
                Program.WaitAndExit(1);
            }
            
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
                    {
                        string quoteText = request.Substring(request.IndexOf(" ") + 1);

                        if (quoteText.Length > 0)
                        {
                            Quote quote = new Quote
                            {
                                channel = context.Message.ChatHub.Name,
                                user = context.UserNameCache[context.Message.User.ID],
                                timestamp = context.Message.TimeStamp,
                                text = quoteText
                            };

                            if (addQuoteToDb(ref quote))
                            {
                                reply.Text = "Success! Added " + quote.ToString(quoteTimeZone);
                            }
                            else
                            {
                                reply.Text = "Failed to add " + quote.ToString(quoteTimeZone);
                            }
                        }
                        else
                        {
                            reply.Text = "Nope! Not adding empty quote";
                        }

                        break;
                    }

                case quoteDel:
                    {
                        long id = GetRequestQuoteId(request, reply);

                        if (id != -1)
                        {
                            Quote deletedQuote = delQuoteFromDb(id);
                            if (deletedQuote != null)
                            {
                                reply.Text = "Succesfully deleted " + deletedQuote.ToString(quoteTimeZone);
                            }
                            else
                            {
                                reply.Text = "Could not delete quote #" + id;
                            }
                        }

                        break;
                    }

                case quoteGet:
                    {
                        long id = GetRequestQuoteId(request, reply);
                        Quote quote = getQuoteById(id);
                        if (quote != null)
                        {
                            reply.Text = quote.ToString(quoteTimeZone);
                        }
                        else
                        {
                            reply.Text = "Could not find quote #" + id;
                        }
                        break;
                    }

                case quoteRandom:
                    {
                        Quote quote = getRandomQuote();
                        if (quote != null)
                        {
                            reply.Text = quote.ToString(quoteTimeZone);
                        }
                        else
                        {
                            reply.Text = "Could not get random quote";
                        }
                        break;
                    }

                default:
                    Debug.WriteLine("Unsupported quote command '" + command + "' (request '" + request + "')");
                    reply.Text = "Unsupported quote command '" + command + "' (request '" + request + "')";
                    break;
            }

            return reply;
        }

        private long GetRequestQuoteId(string request, BotMessage reply)
        {
            string idStr = request.Substring(request.IndexOf(" ") + 1);
            long id = -1;

            try
            {
                id = Convert.ToInt64(idStr);
            }
            catch (FormatException)
            {
                reply.Text = "Badly formatted id: '" + idStr + "'";
                return -1;
            }

            return id;
        }

        private bool addQuoteToDb(ref Quote quote)
        {
            quote.id = -1;
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(
                      "INSERT INTO quotes (channel, timestamp, user, text) values ("
                      + "@channelname, @timestamp, @username, @text);"
                      + " SELECT last_insert_rowid()",
                      dbConn);
                cmd.Parameters.AddWithValue("@channelname", quote.channel);
                //No timezone adjustment for the database, this should be done in the frontend (for the local timezone since some weirdos don't live in CET)
                cmd.Parameters.AddWithValue("@timestamp", (new DateTimeOffset(quote.timestamp)).ToUnixTimeSeconds());
                cmd.Parameters.AddWithValue("@username", quote.user);
                cmd.Parameters.AddWithValue("@text", quote.text);

                Debug.WriteLine("Executing sql:");
                Debug.WriteLine(cmd.CommandText);
                quote.id = (long)cmd.ExecuteScalar();
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("InvalidOperationException caught: " + e.ToString());
            }
            return quote.id != -1;
        }

        private Quote delQuoteFromDb(long quoteId)
        {
            try
            {
                Quote delQuote = getQuoteById(quoteId);
                if (delQuote != null)
                {
                    SQLiteCommand cmd = new SQLiteCommand(
                          "DELETE FROM quotes WHERE id = @id",
                          dbConn);
                    cmd.Parameters.AddWithValue("@id", quoteId);

                    Debug.WriteLine("Executing sql:");
                    Debug.WriteLine(cmd.CommandText);
                    int affectedRows = cmd.ExecuteNonQuery();
                    if (affectedRows != 0)
                        return delQuote;
                }
                Debug.WriteLine("Could not find quote #" + quoteId);
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("InvalidOperationException caught: " + e.ToString());
            }
            return default(Quote);
        }

        private Quote getQuoteById(long quoteId)
        {
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(
                        "SELECT * FROM quotes WHERE id = @id",
                        dbConn);
                cmd.Parameters.AddWithValue("@id", quoteId);

                Debug.WriteLine("Executing sql:");
                Debug.WriteLine(cmd.CommandText);
                SQLiteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Quote quote = new Quote();
                    quote.id = (long)reader["id"];
                    quote.channel = (string)reader["channel"];
                    quote.setTimestamp((long)reader["timestamp"]);
                    quote.user = (string)reader["user"];
                    quote.text = (string)reader["text"];
                    return quote;
                }
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("InvalidOperationException caught: " + e.ToString());
            }
            return default(Quote);
        }

        private Quote getRandomQuote()
        {
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(
                        "SELECT * FROM quotes ORDER BY RANDOM() LIMIT 1",
                        dbConn);

                Debug.WriteLine("Executing sql:");
                Debug.WriteLine(cmd.CommandText);
                SQLiteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Quote quote = new Quote();
                    quote.id = (long)reader["id"];
                    quote.channel = (string)reader["channel"];
                    quote.setTimestamp((long)reader["timestamp"]);
                    quote.user = (string)reader["user"];
                    quote.text = (string)reader["text"];
                    return quote;
                }
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("InvalidOperationException caught: " + e.ToString());
            }
            return default(Quote);
        }


    }
}
