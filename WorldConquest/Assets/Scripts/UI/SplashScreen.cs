using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldConquest.Game;
using WorldConquest.Map;

namespace WorldConquest.UI
{
    /// <summary>
    /// Splash screen shown at game start — GDD §5.1.
    /// Displays the player's randomly assigned country with stats.
    /// Provides Reroll (once) and Start buttons.
    ///
    /// Setup in scene:
    ///   - Attach to a full-screen UI Panel
    ///   - Assign [SerializeField] references in Inspector
    ///   - GameManager.splashScreenPanel should reference this GameObject
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        [Header("Country Info")]
        [SerializeField] private TextMeshProUGUI countryNameText;
        [SerializeField] private TextMeshProUGUI militaryRankText;
        [SerializeField] private TextMeshProUGUI troopsText;
        [SerializeField] private TextMeshProUGUI missilesText;
        [SerializeField] private TextMeshProUGUI airForceText;
        [SerializeField] private TextMeshProUGUI navyText;

        [Header("Buttons")]
        [SerializeField] private Button rerollButton;
        [SerializeField] private Button startButton;

        [Header("Reroll")]
        [SerializeField] private TextMeshProUGUI rerollRemainingText;

        private int rerollsUsed = 0;
        private const int MaxRerolls = 1; // GDD §5.1: one reroll per new game

        void Awake()
        {
            rerollButton.onClick.AddListener(OnReroll);
            startButton.onClick.AddListener(OnStart);
        }

        void OnEnable()
        {
            // Refresh display whenever panel becomes visible
            Refresh();
        }

        private void Refresh()
        {
            CountryData p = GameManager.Instance?.PlayerCountry;
            if (p == null) return;

            countryNameText.text  = p.Name;
            militaryRankText.text = p.MilitaryRank < 999
                ? $"Global Military Rank: #{p.MilitaryRank}"
                : "Global Military Rank: Unranked";
            troopsText.text    = $"Troops       {p.Troops:N0}";
            missilesText.text  = $"Missiles     {p.Missiles:N0}";
            airForceText.text  = $"Air Force    {p.AirForce:N0}";
            navyText.text      = $"Navy         {p.Navy:N0}";

            int remaining = MaxRerolls - rerollsUsed;
            rerollRemainingText.text  = $"Rerolls remaining: {remaining}";
            rerollButton.interactable = remaining > 0;
        }

        private void OnReroll()
        {
            if (rerollsUsed >= MaxRerolls) return;
            rerollsUsed++;
            GameManager.Instance?.RerollCountry();
            Refresh();
        }

        private void OnStart()
        {
            GameManager.Instance?.StartGame();
            // Notify HUD to populate with player country
            FindObjectOfType<PlayerHUD>()?.Refresh();
        }
    }
}
