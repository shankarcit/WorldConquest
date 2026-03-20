using System.Collections.Generic;
using UnityEngine;

namespace WorldConquest.Map
{
    public enum DiplomaticStatus { Neutral, Allied, AtWar, Broken }

    /// <summary>Terrain type per GDD §3.2 — affects conquest difficulty.</summary>
    public enum TerrainType { Plains, Desert, Forest, Mountains, Arctic, Island }

    public class CountryData
    {
        public string Name;
        public string ISO3;
        public int MilitaryRank;

        // Military assets
        public int Troops;
        public int Missiles;
        public int AirForce;
        public int Navy;

        // Diplomacy
        public DiplomaticStatus Status = DiplomaticStatus.Neutral;

        // Terrain (GDD §3.2)
        public TerrainType Terrain = TerrainType.Plains;

        // Territory (% of original land still held, 0 = conquered)
        public float TerritoryPercent = 1f;

        // Coastline flag (affects navy regeneration per GDD §8.4)
        public bool HasCoastline = true;

        // Map
        public List<Vector2[]> Polygons = new List<Vector2[]>(); // lon/lat pairs
    }
}
