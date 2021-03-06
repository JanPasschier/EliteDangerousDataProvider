﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class LiftoffEvent : Event
    {
        public const string NAME = "Liftoff";
        public const string DESCRIPTION = "Triggered when your ship lifts off from a planet's surface";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"Liftoff\",\"Latitude\":63.468872,\"Longitude\":157.599380}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static LiftoffEvent()
        {
            VARIABLES.Add("longitude", "The longitude from where the commander has lifted off");
            VARIABLES.Add("latitude", "The latitude from where the commander has lifted off");
        }

        [JsonProperty("longitude")]
        public decimal longitude { get; private set; }

        [JsonProperty("latitude")]
        public decimal latitude { get; private set; }

        public LiftoffEvent(DateTime timestamp, decimal longitude, decimal latitude) : base(timestamp, NAME)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }
}
