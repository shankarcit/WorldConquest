using UnityEngine;
using WorldConquest.Map;

namespace WorldConquest.Game
{
    /// <summary>
    /// Economy system — GDD §12 (GDP / resource income per turn).
    ///
    /// Design:
    ///   GDP is derived from military rank at game start (higher rank = larger economy).
    ///   Each turn a country earns:
    ///       Income = GDP × TerritoryPercent
    ///   (losing territory reduces income — occupied countries earn nothing.)
    ///
    ///   Accumulated Resources auto-convert to military assets:
    ///       Troops    — 5 resources per troop
    ///       Air Force — 200 resources per aircraft
    ///   Resources are capped at GDP × 3 (3-turn bank) to prevent runaway stockpiles.
    ///
    /// GDP scale (totalRanked = 50):
    ///   Rank  1 → GDP 10,000  (≈2,000 bonus troops/turn at full territory)
    ///   Rank 25 → GDP  5,200  (≈1,040 bonus troops/turn)
    ///   Rank 50 → GDP    200  (≈40 bonus troops/turn)
    ///   Unranked → GDP   50
    ///
    /// TurnManager calls EconomyTick() for every surviving country after RegenerateTick.
    /// MilitaryDataLoader calls InitializeGDP() once per country after setting military stats.
    /// </summary>
    public static class EconomySystem
    {
        // Conversion rates
        public const int TroopCost    = 5;    // resources per 1 troop
        public const int AircraftCost = 200;  // resources per 1 aircraft

        // Cap multiplier: Resources stored ≤ GDP × this value
        private const int ResourceCap = 3;

        // ── Initialisation ───────────────────────────────────────────────────

        /// <summary>
        /// Set a country's GDP based on its military rank.
        /// Call once after MilitaryDataLoader.Apply().
        /// </summary>
        public static void InitializeGDP(CountryData country, int totalRanked = 50)
        {
            if (country.MilitaryRank >= 999)
            {
                country.GDP = 50; // unranked micro-state
            }
            else
            {
                // Linear scale: rank 1 → totalRanked * 200, rank N → (totalRanked - N + 1) * 200
                country.GDP = (totalRanked - country.MilitaryRank + 1) * 200;
                country.GDP = Mathf.Max(country.GDP, 50);
            }
            country.Resources = 0; // start with empty treasury
        }

        // ── Per-turn tick ────────────────────────────────────────────────────

        /// <summary>
        /// Called every End Turn for each surviving country.
        /// Adds income, then converts resources to military assets.
        /// </summary>
        public static void EconomyTick(CountryData country)
        {
            if (country.TerritoryPercent <= 0f) return; // conquered — no income

            // Earn income proportional to remaining territory
            int income = Mathf.RoundToInt(country.GDP * country.TerritoryPercent);
            country.Resources += income;

            // Cap the treasury
            int cap = country.GDP * ResourceCap;
            if (country.Resources > cap)
                country.Resources = cap;

            // Convert resources → troops
            int newTroops = country.Resources / TroopCost;
            if (newTroops > 0)
            {
                country.Troops    += newTroops;
                country.Resources -= newTroops * TroopCost;
            }

            // Convert resources → aircraft (every AircraftCost)
            int newAircraft = country.Resources / AircraftCost;
            if (newAircraft > 0)
            {
                country.AirForce  += newAircraft;
                country.Resources -= newAircraft * AircraftCost;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>Projected income this turn (before applying territory losses).</summary>
        public static int ProjectedIncome(CountryData country) =>
            Mathf.RoundToInt(country.GDP * country.TerritoryPercent);
    }
}
