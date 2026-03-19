using UnityEngine;
using TMPro;
using WorldConquest.Game;
using WorldConquest.Map;

namespace WorldConquest.UI
{
    /// <summary>
    /// Player HUD — top bar (country name + territory count) and bottom bar (military assets).
    /// Also displays notification popups in the top-right corner.
    ///
    /// Setup in scene:
    ///   - Attach to a Canvas GameObject
    ///   - Assign all [SerializeField] references in Inspector
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI countryNameText;
        [SerializeField] private TextMeshProUGUI territoryCountText;

        [Header("Bottom Bar — Military Assets")]
        [SerializeField] private TextMeshProUGUI troopsText;
        [SerializeField] private TextMeshProUGUI missilesText;
        [SerializeField] private TextMeshProUGUI airForceText;
        [SerializeField] private TextMeshProUGUI navyText;

        [Header("Notification (top-right)")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 4f;

        private float notificationTimer;

        void Awake()
        {
            MapEventBus.OnNotification += ShowNotification;
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }

        void OnDestroy()
        {
            MapEventBus.OnNotification -= ShowNotification;
        }

        void Start()
        {
            Refresh();
        }

        void Update()
        {
            if (notificationTimer > 0f)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 0f && notificationPanel != null)
                    notificationPanel.SetActive(false);
            }
        }

        public void Refresh()
        {
            if (GameManager.Instance == null || GameManager.Instance.PlayerCountry == null)
                return;

            var p = GameManager.Instance.PlayerCountry;

            if (countryNameText  != null) countryNameText.text    = p.Name;
            if (territoryCountText != null) territoryCountText.text = $"Territories: 1";
            if (troopsText    != null) troopsText.text    = $"Troops: {p.Troops:N0}";
            if (missilesText  != null) missilesText.text  = $"Missiles: {p.Missiles:N0}";
            if (airForceText  != null) airForceText.text  = $"Air Force: {p.AirForce:N0}";
            if (navyText      != null) navyText.text      = $"Navy: {p.Navy:N0}";
        }

        private void ShowNotification(string message)
        {
            if (notificationPanel == null || notificationText == null) return;
            notificationText.text = message;
            notificationPanel.SetActive(true);
            notificationTimer = notificationDuration;
        }
    }
}
