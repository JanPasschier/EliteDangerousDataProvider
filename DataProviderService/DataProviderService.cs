﻿using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Utilities;

namespace EddiDataProviderService
{
    /// <summary>Access to EDDP data<summary>
    public class DataProviderService
    {
        private const string BASE = "http://api.eddp.co/";

        public static StarSystem GetSystemData(string system, decimal? x, decimal?y, decimal? z)
        {
            if (system == null) { return null; }

            // X Y and Z are provided to us with 3dp of accuracy even though they are x/32; fix that here
            if (x.HasValue)
            {
                x = (Math.Round(x.Value * 32))/32;
            }
            if (y.HasValue)
            {
                y = (Math.Round(y.Value * 32)) / 32;
            }
            if (z.HasValue)
            {
                z = (Math.Round(z.Value * 32)) / 32;
            }

            string response;
            try
            {
                response = Net.DownloadString(BASE + "systems/" + Uri.EscapeDataString(system));
            }
            catch (WebException)
            {
                response = null;
            }
            if (response == null || response == "")
            {
                // No information found on this system, or some other issue.  Create a very basic response
                response = @"{""name"":""" + system + @"""";
                if (x.HasValue)
                {
                    response = response + @", ""x"":" + ((decimal)x).ToString(CultureInfo.InvariantCulture);
                }
                if (y.HasValue)
                {
                    response = response + @", ""y"":" + ((decimal)y).ToString(CultureInfo.InvariantCulture);
                }
                if (z.HasValue)
                {
                    response = response + @", ""z"":" + ((decimal)z).ToString(CultureInfo.InvariantCulture);
                }
                response = response + @", ""stations"":[]";
                response = response + @", ""bodies"":[]}";
                Logging.Info("Generating dummy response " + response);
            }
            return StarSystemFromEDDP(response, x, y, z);
        }

        public static StarSystem StarSystemFromEDDP(string data, decimal? x, decimal? y, decimal? z)
        {
            return StarSystemFromEDDP(JObject.Parse(data), x, y, z);
        }

        public static StarSystem StarSystemFromEDDP(dynamic json, decimal? x, decimal? y, decimal? z)
        {
            StarSystem StarSystem = new StarSystem();
            StarSystem.name = (string)json["name"];
            StarSystem.x = json["x"] == null ? x : (decimal?)json["x"];
            StarSystem.y = json["y"] == null ? y : (decimal?)json["y"];
            StarSystem.z = json["z"] == null ? y : (decimal?)json["z"];

            if (json["is_populated"] != null)
            {
                // We have real data so populate the rest of the data
                StarSystem.EDDBID = (long)json["id"];
                StarSystem.population = (long?)json["population"] == null ? 0 : (long?)json["population"];
                StarSystem.allegiance = (string)json["allegiance"];
                StarSystem.government = (string)json["government"];
                StarSystem.faction = (string)json["faction"];
                StarSystem.primaryeconomy = (string)json["primary_economy"];
                StarSystem.state = (string)json["state"] == "None" ? null : (string)json["state"];
                StarSystem.security = (string)json["security"];
                StarSystem.power = (string)json["power"] == "None" ? null : (string)json["power"];
                StarSystem.powerstate = (string)json["power_state"];


                StarSystem.stations = StationsFromEDDP(StarSystem.name, json);
            }

            StarSystem.lastupdated = DateTime.Now;

            return StarSystem;
        }

        public static List<Station> StationsFromEDDP(string systemName, dynamic json)
        {
            List<Station> Stations = new List<Station>();

            if (json["stations"] != null)
            {
                foreach (dynamic station in json["stations"])
                {
                    Station Station = new Station();
                    Station.EDDBID = (long)station["id"];
                    Station.name = (string)station["name"];
                    Station.systemname = systemName;

                    Station.economies = new List<string>();
                    if (station["economies"] != null)
                    {
                        foreach (dynamic economy in station["economies"])
                        {
                            Station.economies.Add((string)economy);
                        }
                    }

                    Station.allegiance = (string)station["allegiance"];
                    Station.government = (string)station["government"];
                    Station.faction = (string)station["faction"];
                    Station.state = (string)station["state"] == "None" ? null : (string)station["state"];
                    Station.distancefromstar = (long?)station["distance_to_star"];
                    Station.hasrefuel = (bool?)station["has_refuel"];
                    Station.hasrearm = (bool?)station["has_rearm"];
                    Station.hasrepair = (bool?)station["has_repair"];
                    Station.hasoutfitting = (bool?)station["has_outfitting"];
                    Station.hasshipyard = (bool?)station["has_shipyard"];
                    Station.hasmarket = (bool?)station["has_market"];
                    Station.hasblackmarket = (bool?)station["has_blackmarket"];

                    if (((string)station["type"]) != null)
                    {
                        Station.model = ((string)station["type"]);
                        if (!stationModels.Contains((string)station["type"]))
                        {
                            Logging.Report("Unknown station model " + ((string)station["type"]));
                        }
                    }

                    string largestpad = (string)station["max_landing_pad_size"];
                    if (largestpad == "S") { largestpad = "Small"; }
                    if (largestpad == "M") { largestpad = "Medium"; }
                    if (largestpad == "L") { largestpad = "Large"; }
                    Station.largestpad = largestpad;

                    Stations.Add(Station);
                }
            }
            return Stations;
        }

        private static List<string> stationModels = new List<string>()
        {
            "Civilian Outpost",
            "Commercial Outpost",
            "Coriolis Starport",
            "Industrial Outpost",
            "Military Outpost",
            "Mining Outpost",
            "Ocellus Starport",
            "Orbis Starport",
            "Planetary Engineer Base",
            "Planetary Outpost",
            "Planetary Port",
            "Planetary Settlement",
            "Scientific Outpost",
            "Unknown Outpost",
            "Unknown Planetary",
            "Unknown Starport",
            "Unsanctioned Outpost"
        };

        static DataProviderService()
        {
            // We need to not use an expect header as it causes problems when sending data to a REST service
            var errorUri = new Uri(BASE + "error");
            var errorServicePoint = ServicePointManager.FindServicePoint(errorUri);
            errorServicePoint.Expect100Continue = false;
        }
    }
}
