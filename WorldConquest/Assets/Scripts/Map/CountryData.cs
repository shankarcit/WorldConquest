using System.Collections.Generic;
using UnityEngine;

namespace WorldConquest.Map
{
    public enum DiplomaticStatus { Neutral, Allied, AtWar, Broken }

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

        // Map
        public List<Vector2[]> Polygons = new List<Vector2[]>(); // lon/lat pairs
    }
}
