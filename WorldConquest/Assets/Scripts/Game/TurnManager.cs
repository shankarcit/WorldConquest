using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldConquest.Map;

namespace WorldConquest.Game
{
    /// <summary>
    /// Manages the game turn loop — GDD §8.4 (regen per turn).
    /// Each "End Turn" advances the turn counter, calls CombatSystem.RegenerateTick
    /// on all surviving countries, and notifies the HUD.
    ///
    /// Attach to GameManager GameObject.
    /// Wire the End Turn button in Inspector.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private TextMeshProUGUI turnCounterText;

        public int CurrentTurn { get; private set; } = 1;

        void Awake()
        {
            if (endTurnButton != null)
                endTurnButton.onClick.AddListener(EndTurn);

            UpdateTurnLabel();
        }

        public void EndTurn()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.IsGameStarted) return;

            CurrentTurn++;

            // Regenerate all surviving countries (GDD §8.4)
            foreach (CountryData country in gm.AllCountries)
            {
                if (country.TerritoryPercent > 0f)
                    CombatSystem.RegenerateTick(country);
            }

            UpdateTurnLabel();

            // Refresh HUD asset display
            FindObjectOfType<UI.PlayerHUD>()?.Refresh();

            // Check win conditions at end of each turn
            GetComponent<WinConditionChecker>()?.Check();

            // Let AI countries act this turn (GDD §2)
            GetComponent<AITurnManager>()?.ProcessAITurn();

            MapEventBus.OnTurnEnded?.Invoke(CurrentTurn);
            MapEventBus.OnNotification?.Invoke($"Turn {CurrentTurn} — forces regenerated.");
            Debug.Log($"TurnManager: Advanced to turn {CurrentTurn}");
        }

        /// <summary>Called by SaveSystem.Load() to restore the saved turn number.</summary>
        public void SetTurn(int turn)
        {
            CurrentTurn = turn;
            UpdateTurnLabel();
        }

        private void UpdateTurnLabel()
        {
            if (turnCounterText != null)
                turnCounterText.text = $"Turn {CurrentTurn}";
        }
    }
}
