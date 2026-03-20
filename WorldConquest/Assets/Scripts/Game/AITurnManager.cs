using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldConquest.Map;

namespace WorldConquest.Game
{
    /// <summary>
    /// Processes AI turns for all non-player countries — GDD §2.
    ///
    /// Each turn every surviving AI country independently:
    ///   • 30% chance to attack a random neutral/enemy country
    ///   • 20% chance to form an alliance with a random neutral country
    ///
    /// Attack AI uses 50% of its current forces as the deployment order.
    /// Alliance AI sets both countries' Status = Allied.
    ///
    /// Attach to GameManager GameObject alongside TurnManager.
    /// TurnManager.EndTurn() calls ProcessAITurn() after regen.
    /// </summary>
    public class AITurnManager : MonoBehaviour
    {
        [Header("AI Behaviour Probabilities (0–1)")]
        [SerializeField, Range(0f, 1f)] private float attackChance   = 0.30f;
        [SerializeField, Range(0f, 1f)] private float allianceChance = 0.20f;

        // AI will not attack if its attack power is below this fraction of the target's
        [SerializeField, Range(0f, 2f)]  private float minimumPowerRatio = 0.5f;

        public void ProcessAITurn()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.IsGameStarted) return;

            var all    = gm.AllCountries;
            var player = gm.PlayerCountry;
            if (all == null || player == null) return;

            // Work on a snapshot so conquests mid-loop don't cause index issues
            var aiCountries = all
                .Where(c => c != player && c.TerritoryPercent > 0f)
                .ToList();

            int attacksDone   = 0;
            int alliancesDone = 0;

            foreach (var ai in aiCountries)
            {
                // Skip if already eliminated (might change during loop)
                if (ai.TerritoryPercent <= 0f) continue;

                // ── Alliance attempt ──────────────────────────────────────
                if (Random.value < allianceChance)
                {
                    var neutralTarget = PickNeutralPartner(ai, all, player);
                    if (neutralTarget != null)
                    {
                        ai.Status           = DiplomaticStatus.Allied;
                        neutralTarget.Status = DiplomaticStatus.Allied;
                        alliancesDone++;
                        MapEventBus.OnNotification?.Invoke(
                            $"{ai.Name} formed an alliance with {neutralTarget.Name}.");
                    }
                }

                // ── Attack attempt ────────────────────────────────────────
                if (Random.value < attackChance)
                {
                    var target = PickAttackTarget(ai, all, player);
                    if (target == null) continue;

                    // Build deployment at 50% of AI's current assets
                    var order = new DeploymentOrder
                    {
                        Target   = target,
                        Troops   = Mathf.RoundToInt(ai.Troops    * 0.5f),
                        Missiles = Mathf.RoundToInt(ai.Missiles  * 0.5f),
                        AirForce = Mathf.RoundToInt(ai.AirForce  * 0.5f),
                        Navy     = Mathf.RoundToInt(ai.Navy      * 0.5f),
                    };

                    // Ensure minimum power threshold
                    float aiPower  = CombatSystem.CalcAttackPower(
                        order.Troops, order.Missiles, order.AirForce, order.Navy);
                    float defPower = CombatSystem.CalcAttackPower(
                        target.Troops, target.Missiles, target.AirForce, target.Navy);

                    if (defPower > 0f && aiPower / defPower < minimumPowerRatio)
                        continue; // too weak — skip this attack

                    BattleResult result = CombatSystem.ResolveBattle(order, target);
                    CombatSystem.ApplyResult(ai, target, order, result);
                    attacksDone++;

                    string outcomeStr = result.Outcome switch
                    {
                        BattleOutcome.Victory   => "defeated",
                        BattleOutcome.Stalemate => "stalemated with",
                        BattleOutcome.Defeat    => "failed to conquer",
                        _                       => "attacked"
                    };
                    MapEventBus.OnNotification?.Invoke(
                        $"AI: {ai.Name} {outcomeStr} {target.Name} " +
                        $"({result.CapturePercent:F0}% captured).");
                }
            }

            if (attacksDone > 0 || alliancesDone > 0)
                Debug.Log($"AITurnManager: {attacksDone} attacks, {alliancesDone} alliances this turn.");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>Pick a surviving country that this AI could attack (neutral or enemy, not allied).</summary>
        private CountryData PickAttackTarget(
            CountryData ai, List<CountryData> all, CountryData player)
        {
            var candidates = all
                .Where(c =>
                    c != ai &&
                    c.TerritoryPercent > 0f &&
                    c.Status != DiplomaticStatus.Allied)
                .ToList();

            if (candidates.Count == 0) return null;
            return candidates[Random.Range(0, candidates.Count)];
        }

        /// <summary>Pick a neutral surviving country to ally with (not self, not player, not already allied).</summary>
        private CountryData PickNeutralPartner(
            CountryData ai, List<CountryData> all, CountryData player)
        {
            var candidates = all
                .Where(c =>
                    c != ai &&
                    c != player &&
                    c.TerritoryPercent > 0f &&
                    c.Status == DiplomaticStatus.Neutral)
                .ToList();

            if (candidates.Count == 0) return null;
            return candidates[Random.Range(0, candidates.Count)];
        }
    }
}
