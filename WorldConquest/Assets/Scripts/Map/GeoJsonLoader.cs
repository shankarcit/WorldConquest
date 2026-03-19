using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using WorldConquest.Game;

namespace WorldConquest.Map
{
    public class GeoJsonLoader : MonoBehaviour
    {
        [SerializeField] private Material countryMaterial;
        [SerializeField] private float mapScale = 1f; // world units per degree

        private static readonly string GeoJsonPath = "Data/GeoJSON/countries.geojson";

        private readonly List<CountryData> countries = new();

        void Start()
        {
            LoadCountries();
            GetComponent<MilitaryDataLoader>()?.Apply(countries);
            GameManager.Instance?.OnCountriesLoaded(countries);
        }

        public List<CountryData> GetCountries() => countries;

        private void LoadCountries()
        {
            string fullPath = Path.Combine(Application.dataPath, GeoJsonPath);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"GeoJsonLoader: File not found at {fullPath}");
                return;
            }
            string json = File.ReadAllText(fullPath);
            JObject root = JObject.Parse(json);
            JArray features = (JArray)root["features"];

            foreach (JToken feature in features)
            {
                CountryData country = ParseFeature(feature);
                if (country != null)
                {
                    countries.Add(country);
                    BuildCountryObject(country);
                }
            }

            Debug.Log($"GeoJsonLoader: Loaded {countries.Count} countries.");
        }

        private CountryData ParseFeature(JToken feature)
        {
            JToken props = feature["properties"];
            string name = props["name"]?.ToString();
            if (string.IsNullOrEmpty(name)) return null;

            CountryData country = new CountryData
            {
                Name = name,
                ISO3 = props["ISO3166-1-Alpha-3"]?.ToString() ?? ""
            };

            JToken geometry = feature["geometry"];
            string geomType = geometry["type"]?.ToString();
            JArray coordinates = (JArray)geometry["coordinates"];

            if (geomType == "Polygon")
            {
                country.Polygons.Add(ParseRing(coordinates[0]));
            }
            else if (geomType == "MultiPolygon")
            {
                foreach (JToken polygon in coordinates)
                    country.Polygons.Add(ParseRing(polygon[0]));
            }

            return country;
        }

        private Vector2[] ParseRing(JToken ring)
        {
            var points = new List<Vector2>();
            foreach (JToken coord in ring)
            {
                float lon = coord[0].Value<float>();
                float lat = coord[1].Value<float>();
                points.Add(new Vector2(lon, lat));
            }
            return points.ToArray();
        }

        private void BuildCountryObject(CountryData country)
        {
            GameObject countryObj = new GameObject(country.Name);
            countryObj.transform.SetParent(transform);

            CountryMesh cm = countryObj.AddComponent<CountryMesh>();
            cm.Initialize(country, countryMaterial, mapScale);
        }
    }
}
