using System.Collections.Generic;
using WorldConquest.Map;

namespace WorldConquest.Game
{
    /// <summary>
    /// Plain serializable snapshot of all mutable game state — GDD §13.
    ///
    /// Polygons are intentionally excluded: they are large, static, and always
    /// re-loaded from the GeoJSON on startup. On Load, each CountrySaveData is
    /// matched to the live CountryData by Name and its mutable fields are patched.
    /// </summary>
    [System.Serializable]
    public class GameSaveData
    {
        public int    currentTurn;
        public string playerCountryName;
        public List<CountrySaveData> countries = new();
    }

    [System.Serializable]
    public class CountrySaveData
    {
        public string          name;
        public string          iso3;
        public int             militaryRank;
        public int             troops;
        public int             missiles;
        public int             airForce;
        public int             navy;
        public float           territoryPercent;
        public bool            hasCoastline;
        public DiplomaticStatus status;
        public TerrainType     terrain;
        public int             gdp;
        public int             resources;

        public static CountrySaveData From(CountryData c) => new CountrySaveData
        {
            name             = c.Name,
            iso3             = c.ISO3,
            militaryRank     = c.MilitaryRank,
            troops           = c.Troops,
            missiles         = c.Missiles,
            airForce         = c.AirForce,
            navy             = c.Navy,
            territoryPercent = c.TerritoryPercent,
            hasCoastline     = c.HasCoastline,
            status           = c.Status,
            terrain          = c.Terrain,
            gdp              = c.GDP,
            resources        = c.Resources,
        };

        public void ApplyTo(CountryData c)
        {
            c.MilitaryRank     = militaryRank;
            c.Troops           = troops;
            c.Missiles         = missiles;
            c.AirForce         = airForce;
            c.Navy             = navy;
            c.TerritoryPercent = territoryPercent;
            c.HasCoastline     = hasCoastline;
            c.Status           = status;
            c.Terrain          = terrain;
            c.GDP              = gdp;
            c.Resources        = resources;
        }
    }
}
