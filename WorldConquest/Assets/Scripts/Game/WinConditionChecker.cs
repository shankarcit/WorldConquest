using System.Linq;
using UnityEngine;
using WorldConquest.Map;

namespace WorldConquest.Game
{
    /// <summary>
    /// Checks win conditions after every battle — GDD §10.
    ///
    /// Win conditions:
    ///   World Domination  — control 100% of all countries
    ///   Superpower        — control 50%+ of countries AND have 5+ allies
    ///   Diplomatic Win    — form an alliance bloc of 10+ countries
    ///
    /// Attach to GameManager GameObject.
    /// </summary>
    public class WinConditionChecker : MonoBehaviour
    {
        [Header("Win Screen")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private TMPro.TextMeshProUGUI winTitleText;
        [SerializeField] private TMPro.TextMeshProUGUI winDetailText;
        [SerializeField] private UnityEngine.UI.Button restartButton;

        private bool gameWon = false;

        void Awake()
        {
            MapEventBus.OnCountryConquered += OnConquered;
            MapEventBus.OnNotification     += OnNotification;

            if (winPanel != null)
                winPanel.SetActive(false);

            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
        }

        void OnDestroy()
        {
            MapEventBus.OnCountryConquered -= OnConquered;
            MapEventBus.OnNotification     -= OnNotification;
        }

        private void OnConquered(CountryData loser, CountryData winner) => Check();
        private void OnNotification(string _) => Check(); // also check after alliance events

        public void Check()
        {
            if (gameWon) return;

            var gm = GameManager.Instance;
            if (gm == null || !gm.IsGameStarted) return;

            var all    = gm.AllCountries;
            var player = gm.PlayerCountry;
            if (all == null || player == null) return;

            int total     = all.Count;
            int conquered = all.Count(c => c != player && c.TerritoryPercent <= 0f);
            int controlled = conquered + 1; // +1 for player's own country
            int allies    = all.Count(c => c.Status == DiplomaticStatus.Allied);

            // GDD §10 — World Domination: control 100%
            if (controlled >= total)
            {
                TriggerWin("WORLD DOMINATION",
                    $"You have conquered every nation on Earth.\n" +
                    $"Countries controlled: {controlled}/{total}");
                return;
            }

            // GDD §10 — Superpower: 50%+ of world + 5 allies
            float controlPercent = (float)controlled / total;
            if (controlPercent >= 0.5f && allies >= 5)
            {
                TriggerWin("SUPERPOWER",
                    $"You control {controlPercent * 100f:F0}% of the world\n" +
                    $"with {allies} allied nations.");
                return;
            }

            // GDD §10 — Diplomatic Win: 10+ allies
            if (allies >= 10)
            {
                TriggerWin("DIPLOMATIC VICTORY",
                    $"You have united {allies} nations in an alliance bloc.\n" +
                    $"Peace through diplomacy.");
                return;
            }
        }

        private void TriggerWin(string title, string detail)
        {
            gameWon = true;
            Debug.Log($"WIN: {title}");

            if (winPanel == null) return;
            winTitleText.text  = title;
            winDetailText.text = detail;
            winPanel.SetActive(true);

            // Pause game
            Time.timeScale = 0f;
        }

        private void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
