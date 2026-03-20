using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldConquest.Map;
using WorldConquest.Game;

namespace WorldConquest.UI
{
    /// <summary>
    /// Military Deployment Screen — GDD §8.2.
    /// Opens when a war is declared. Player chooses how many of each asset to commit.
    /// Calls CombatSystem.ResolveBattle() on confirm and shows the result.
    ///
    /// Setup in scene:
    ///   - Attach to a UI Panel (centred, shown on war declaration)
    ///   - Assign all [SerializeField] references in Inspector
    ///   - Subscribe to MapEventBus.OnWarDeclared in Awake (done automatically)
    /// </summary>
    public class DeploymentScreen : MonoBehaviour
    {
        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Troop Sliders")]
        [SerializeField] private Slider  troopsSlider;
        [SerializeField] private Slider  missilesSlider;
        [SerializeField] private Slider  airForceSlider;
        [SerializeField] private Slider  navySlider;

        [Header("Value Labels")]
        [SerializeField] private TextMeshProUGUI troopsLabel;
        [SerializeField] private TextMeshProUGUI missilesLabel;
        [SerializeField] private TextMeshProUGUI airForceLabel;
        [SerializeField] private TextMeshProUGUI navyLabel;

        [Header("Terrain Info")]
        [SerializeField] private TextMeshProUGUI terrainLabel;
        [SerializeField] private TextMeshProUGUI difficultyLabel;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [Header("Result Panel")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button resultCloseButton;

        private CountryData targetCountry;

        void Awake()
        {
            MapEventBus.OnWarDeclared += OpenFor;

            troopsSlider.onValueChanged.AddListener(_   => RefreshLabels());
            missilesSlider.onValueChanged.AddListener(_ => RefreshLabels());
            airForceSlider.onValueChanged.AddListener(_ => RefreshLabels());
            navySlider.onValueChanged.AddListener(_     => RefreshLabels());

            confirmButton.onClick.AddListener(OnConfirm);
            cancelButton.onClick.AddListener(Close);
            resultCloseButton.onClick.AddListener(() => resultPanel.SetActive(false));

            gameObject.SetActive(false);
            resultPanel.SetActive(false);
        }

        void OnDestroy()
        {
            MapEventBus.OnWarDeclared -= OpenFor;
        }

        private void OpenFor(CountryData target)
        {
            targetCountry = target;
            var player = GameManager.Instance?.PlayerCountry;
            if (player == null) return;

            titleText.text = $"Deploy Forces Against {target.Name}";

            // Configure sliders to player's available assets
            SetupSlider(troopsSlider,   player.Troops);
            SetupSlider(missilesSlider, player.Missiles);
            SetupSlider(airForceSlider, player.AirForce);
            SetupSlider(navySlider,     player.Navy);

            float mod = CombatSystem.GetTerrainModifier(target.Terrain);
            terrainLabel.text    = $"Terrain: {target.Terrain}";
            difficultyLabel.text = $"Difficulty Modifier: ×{mod:F1}";

            RefreshLabels();
            gameObject.SetActive(true);
        }

        private void SetupSlider(Slider slider, int max)
        {
            slider.minValue = 0;
            slider.maxValue = max;
            slider.wholeNumbers = true;
            slider.value = Mathf.RoundToInt(max * 0.5f); // default 50%
        }

        private void RefreshLabels()
        {
            var p = GameManager.Instance?.PlayerCountry;
            if (p == null) return;

            troopsLabel.text   = $"{(int)troopsSlider.value:N0} / {p.Troops:N0}";
            missilesLabel.text = $"{(int)missilesSlider.value:N0} / {p.Missiles:N0}";
            airForceLabel.text = $"{(int)airForceSlider.value:N0} / {p.AirForce:N0}";
            navyLabel.text     = $"{(int)navySlider.value:N0} / {p.Navy:N0}";
        }

        private void OnConfirm()
        {
            var player = GameManager.Instance?.PlayerCountry;
            if (player == null || targetCountry == null) return;

            var order = new DeploymentOrder
            {
                Target   = targetCountry,
                Troops   = (int)troopsSlider.value,
                Missiles = (int)missilesSlider.value,
                AirForce = (int)airForceSlider.value,
                Navy     = (int)navySlider.value,
            };

            BattleResult result = CombatSystem.ResolveBattle(order, targetCountry);
            CombatSystem.ApplyResult(player, targetCountry, order, result);

            ShowResult(result);
            gameObject.SetActive(false);

            // Notify HUD to refresh asset counts
            FindObjectOfType<PlayerHUD>()?.Refresh();
        }

        private void ShowResult(BattleResult result)
        {
            string outcomeStr = result.Outcome switch
            {
                BattleOutcome.Victory   => "<color=#00ff88>VICTORY</color>",
                BattleOutcome.Stalemate => "<color=#ffcc00>STALEMATE</color>",
                BattleOutcome.Defeat    => "<color=#ff4444>DEFEAT</color>",
                _                       => "UNKNOWN"
            };

            resultText.text =
                $"{outcomeStr}\n\n" +
                $"Territory Captured: {result.CapturePercent:F1}%\n\n" +
                $"<b>Your Losses:</b>\n" +
                $"  Troops:    -{result.TroopsLost:N0}\n" +
                $"  Missiles:  -{result.MissilesLost:N0}\n" +
                $"  Air Force: -{result.AirForceLost:N0}\n" +
                $"  Navy:      -{result.NavyLost:N0}\n\n" +
                $"<b>Enemy Losses:</b>\n" +
                $"  Troops:    -{result.DefenderTroopsLost:N0}\n" +
                $"  Air Force: -{result.DefenderAirForceLost:N0}\n" +
                $"  Navy:      -{result.DefenderNavyLost:N0}";

            resultPanel.SetActive(true);

            string notif = result.Outcome == BattleOutcome.Victory
                ? $"Victory! Captured {result.CapturePercent:F0}% of {targetCountry.Name}."
                : result.Outcome == BattleOutcome.Stalemate
                    ? $"Stalemate with {targetCountry.Name}. Small gains."
                    : $"Defeated by {targetCountry.Name}. Heavy losses.";
            MapEventBus.OnNotification?.Invoke(notif);
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
