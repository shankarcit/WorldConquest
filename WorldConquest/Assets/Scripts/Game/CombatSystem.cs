using UnityEngine;
using WorldConquest.Map;

namespace WorldConquest.Game
{
    /// <summary>
    /// Implements GDD §8.3 Combat Resolution and §8.4 Unit Attrition.
    /// Stateless — call ResolveBattle() and apply the returned result.
    /// </summary>
    public static class CombatSystem
    {
        // GDD §3.2 terrain difficulty modifiers
        public static float GetTerrainModifier(TerrainType terrain) => terrain switch
        {
            TerrainType.Plains    => 1.0f,
            TerrainType.Desert    => 1.3f,
            TerrainType.Forest    => 1.4f,
            TerrainType.Mountains => 1.8f,
            TerrainType.Arctic    => 2.0f,
            TerrainType.Island    => 1.5f,
            _                     => 1.0f
        };

        // GDD §8.3: Attack Power formula
        public static float CalcAttackPower(int troops, int missiles, int airForce, int navy)
            => (troops * 1f) + (missiles * 50f) + (airForce * 30f) + (navy * 20f);

        /// <summary>
        /// Resolve a battle between attacker's deployed forces and defender's full strength.
        /// Returns a BattleResult describing outcome and losses.
        /// </summary>
        public static BattleResult ResolveBattle(
            DeploymentOrder deployment,
            CountryData defender)
        {
            float attackPower  = CalcAttackPower(
                deployment.Troops, deployment.Missiles,
                deployment.AirForce, deployment.Navy);

            float terrainMod   = GetTerrainModifier(defender.Terrain);
            float effectiveAtk = attackPower / terrainMod;

            float defensePower = CalcAttackPower(
                defender.Troops, defender.Missiles,
                defender.AirForce, defender.Navy);

            float total = effectiveAtk + defensePower;
            float captureRatio = total > 0f ? effectiveAtk / total : 0f; // 0–1

            BattleOutcome outcome;
            if      (captureRatio > 0.60f) outcome = BattleOutcome.Victory;
            else if (captureRatio >= 0.40f) outcome = BattleOutcome.Stalemate;
            else                            outcome = BattleOutcome.Defeat;

            // Attacker losses (GDD §8.4)
            float attackerLossFactor = outcome switch
            {
                BattleOutcome.Victory   => 0.10f,  // 10% of deployed lost
                BattleOutcome.Stalemate => 0.25f,  // 25% lost
                BattleOutcome.Defeat    => 0.50f,  // 50% lost
                _                       => 0.25f
            };

            // Defender losses scale inversely to capture ratio
            float defenderLossFactor = captureRatio * 0.5f;

            return new BattleResult
            {
                Outcome        = outcome,
                CapturePercent = captureRatio * 100f,

                // Attacker losses
                TroopsLost    = Mathf.RoundToInt(deployment.Troops    * attackerLossFactor),
                MissilesLost  = deployment.Missiles,   // all missiles consumed (§8.4)
                AirForceLost  = Mathf.RoundToInt(deployment.AirForce  * attackerLossFactor),
                NavyLost      = Mathf.RoundToInt(deployment.Navy      * attackerLossFactor),

                // Defender losses
                DefenderTroopsLost    = Mathf.RoundToInt(defender.Troops    * defenderLossFactor),
                DefenderMissilesLost  = Mathf.RoundToInt(defender.Missiles  * defenderLossFactor),
                DefenderAirForceLost  = Mathf.RoundToInt(defender.AirForce  * defenderLossFactor),
                DefenderNavyLost      = Mathf.RoundToInt(defender.Navy      * defenderLossFactor),
            };
        }

        /// <summary>
        /// Apply a BattleResult to both attacker (player country) and defender.
        /// Mutates CountryData directly.
        /// </summary>
        public static void ApplyResult(
            CountryData attacker,
            CountryData defender,
            DeploymentOrder deployment,
            BattleResult result)
        {
            // Deduct deployed forces from attacker pool first
            attacker.Troops    -= deployment.Troops;
            attacker.Missiles  -= deployment.Missiles;
            attacker.AirForce  -= deployment.AirForce;
            attacker.Navy      -= deployment.Navy;

            // Then apply battle losses to attacker's deployed remainder
            attacker.Troops    -= result.TroopsLost;
            attacker.Missiles  -= result.MissilesLost;   // already consumed above
            attacker.AirForce  -= result.AirForceLost;
            attacker.Navy      -= result.NavyLost;

            // Clamp to zero
            attacker.Troops    = Mathf.Max(0, attacker.Troops);
            attacker.Missiles  = Mathf.Max(0, attacker.Missiles);
            attacker.AirForce  = Mathf.Max(0, attacker.AirForce);
            attacker.Navy      = Mathf.Max(0, attacker.Navy);

            // Apply losses to defender
            defender.Troops    = Mathf.Max(0, defender.Troops    - result.DefenderTroopsLost);
            defender.Missiles  = Mathf.Max(0, defender.Missiles  - result.DefenderMissilesLost);
            defender.AirForce  = Mathf.Max(0, defender.AirForce  - result.DefenderAirForceLost);
            defender.Navy      = Mathf.Max(0, defender.Navy      - result.DefenderNavyLost);

            // Update defender territory
            if (result.Outcome == BattleOutcome.Victory)
            {
                defender.TerritoryPercent -= result.CapturePercent / 100f;
                defender.TerritoryPercent  = Mathf.Max(0f, defender.TerritoryPercent);
            }

            // Country conquered?
            if (defender.TerritoryPercent <= 0f)
            {
                defender.TerritoryPercent = 0f;
                defender.Status = DiplomaticStatus.AtWar; // remains for record
                // Absorb 30% of defender's remaining assets (GDD §8.5)
                attacker.Troops   += Mathf.RoundToInt(defender.Troops   * 0.3f);
                attacker.AirForce += Mathf.RoundToInt(defender.AirForce * 0.3f);
                attacker.Navy     += Mathf.RoundToInt(defender.Navy     * 0.3f);
                MapEventBus.OnCountryConquered?.Invoke(defender, attacker);
            }
        }

        /// <summary>
        /// Slow regeneration tick — call once per game turn (GDD §8.4).
        /// </summary>
        public static void RegenerateTick(CountryData country, int regenTroops = 5000, int regenAir = 50)
        {
            country.Troops    += regenTroops;
            country.AirForce  += regenAir;
            if (country.HasCoastline)
                country.Navy  += 5;
            // Missiles do NOT regenerate
        }
    }

    // ── Supporting types ────────────────────────────────────────────────────

    public enum BattleOutcome { Victory, Stalemate, Defeat }

    /// <summary>Forces the player commits to a battle.</summary>
    public class DeploymentOrder
    {
        public CountryData Target;
        public int Troops;
        public int Missiles;
        public int AirForce;
        public int Navy;
    }

    /// <summary>Returned by CombatSystem.ResolveBattle().</summary>
    public class BattleResult
    {
        public BattleOutcome Outcome;
        public float CapturePercent;

        public int TroopsLost;
        public int MissilesLost;
        public int AirForceLost;
        public int NavyLost;

        public int DefenderTroopsLost;
        public int DefenderMissilesLost;
        public int DefenderAirForceLost;
        public int DefenderNavyLost;
    }
}
