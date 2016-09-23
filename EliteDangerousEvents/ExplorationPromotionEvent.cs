﻿using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class ExplorationPromotionEvent : Event
    {
        public const string NAME = "Exploration promotion";
        public const string DESCRIPTION = "Triggered when your exploration rank increases";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Promotion\",\"Explore\":5}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ExplorationPromotionEvent()
        {
            VARIABLES.Add("rating", "The commander's new exploration rating");
        }

        [JsonProperty("rating")]
        public ExplorationRating rating{ get; private set; }

        public ExplorationPromotionEvent(DateTime timestamp, ExplorationRating rating) : base(timestamp, NAME)
        {
            this.rating = rating;
        }
    }
}
