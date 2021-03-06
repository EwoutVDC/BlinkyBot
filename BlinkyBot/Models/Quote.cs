﻿using System;

using MargieBot.Models;
using Epoch.Extensions;

namespace BlinkyBot.Models
{
    class Quote
    {
        public long id { get; set; }
        public string channel { get; set; }
        public DateTime timestamp { get; set; }
        public string user { get; set; }
        public string text { get; set; }

        public Quote() { id = -1; }

        public void setTimestamp(int unixTimeStamp)
        {
            timestamp = unixTimeStamp.FromUnix();
        }

        public string ToString(TimeZoneInfo timezone)
        {
            return "Quote #" + id + ": '" + text + "' (Added by " + user
                    + " at " + TimeZoneInfo.ConvertTimeFromUtc(timestamp, timezone).ToString("dd/MM/yyyy HH:mm:ss")
                    + "in channel '" + channel + "')";
        }
    }
}
