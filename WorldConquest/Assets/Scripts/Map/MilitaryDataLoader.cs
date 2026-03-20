using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using WorldConquest.Game;

namespace WorldConquest.Map
{
    /// <summary>
    /// Loads military_rankings.json and applies stats to matching CountryData by ISO3 code.
    /// Attach this to the same GameObject as GeoJsonLoader and call Apply() after countries are loaded.
    /// </summary>
    public class MilitaryDataLoader : MonoBehaviour
    {
        private static readonly string MilitaryDataPath = "Data/Military/military_rankings.json";

        private Dictionary<string, JToken> rankingsByIso = new();

        void Awake()
        {
            string fullPath = Path.Combine(Application.dataPath, MilitaryDataPath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"MilitaryDataLoader: File not found at {fullPath}");
                return;
            }

            JObject root = JObject.Parse(File.ReadAllText(fullPath));
            foreach (JToken entry in root["rankings"])
            {
                string iso3 = entry["iso3"]?.ToString();
                if (!string.IsNullOrEmpty(iso3))
                    rankingsByIso[iso3] = entry;
            }
        }

        public void Apply(List<CountryData> countries)
        {
            int matched = 0;
            foreach (CountryData country in countries)
            {
                if (rankingsByIso.TryGetValue(country.ISO3, out JToken data))
                {
                    country.MilitaryRank = data["rank"].Value<int>();
                    country.Troops       = data["troops"].Value<int>();
                    country.Missiles     = data["missiles"].Value<int>();
                    country.AirForce     = data["airForce"].Value<int>();
                    country.Navy         = data["navy"].Value<int>();
                    matched++;
                }
                else
                {
                    // Unranked countries get minimal default stats
                    country.MilitaryRank = 999;
                    country.Troops       = 10000;
                    country.Missiles     = 0;
                    country.AirForce     = 20;
                    country.Navy         = 0;
                }
            }
            // Initialise GDP for all countries after military stats are set (GDD §12)
            foreach (CountryData country in countries)
                EconomySystem.InitializeGDP(country, totalRanked: 50);

            Debug.Log($"MilitaryDataLoader: Applied stats to {matched}/{countries.Count} countries.");
        }
    }
}
