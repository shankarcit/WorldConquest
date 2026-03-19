using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldConquest.Map;
using WorldConquest.Game;

namespace WorldConquest.UI
{
    /// <summary>
    /// Country Justification Menu — slides in from the left when a country is clicked.
    /// Shows country name, military rank, diplomatic status, and action buttons.
    ///
    /// Setup in scene:
    ///   - Attach to a UI Panel (left side, anchored left)
    ///   - Assign all [SerializeField] references in Inspector
    ///   - Subscribe to MapEventBus.OnCountryClicked automatically via Awake
    /// </summary>
    public class CountryJustificationMenu : MonoBehaviour
    {
        [Header("Info Labels")]
        [SerializeField] private TextMeshProUGUI countryNameText;
        [SerializeField] private TextMeshProUGUI militaryRankText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI troopsText;
        [SerializeField] private TextMeshProUGUI missilesText;
        [SerializeField] private TextMeshProUGUI airForceText;
        [SerializeField] private TextMeshProUGUI navyText;

        [Header("Action Buttons")]
        [SerializeField] private Button allianceButton;
        [SerializeField] private Button warButton;
        [SerializeField] private Button closeButton;

        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmPanel;
        [SerializeField] private TextMeshProUGUI confirmPromptText;
        [SerializeField] private TMP_InputField confirmInputField;
        [SerializeField] private Button confirmSubmitButton;
        [SerializeField] private Button confirmCancelButton;

        private CountryData selectedCountry;
        private bool pendingAlliance;

        void Awake()
        {
            MapEventBus.OnCountryClicked += OnCountryClicked;

            allianceButton.onClick.AddListener(OnAllianceClicked);
            warButton.onClick.AddListener(OnWarClicked);
            closeButton.onClick.AddListener(Close);
            confirmSubmitButton.onClick.AddListener(OnConfirmSubmit);
            confirmCancelButton.onClick.AddListener(OnConfirmCancel);

            gameObject.SetActive(false);
            confirmPanel.SetActive(false);
        }

        void OnDestroy()
        {
            MapEventBus.OnCountryClicked -= OnCountryClicked;
        }

        private void OnCountryClicked(CountryData country)
        {
            selectedCountry = country;
            Populate(country);
            gameObject.SetActive(true);
        }

        private void Populate(CountryData country)
        {
            countryNameText.text = country.Name;
            militaryRankText.text = country.MilitaryRank < 999
                ? $"Military Rank: #{country.MilitaryRank}"
                : "Military Rank: Unranked";
            statusText.text = $"Status: {country.Status}";
            troopsText.text    = $"Troops:     {country.Troops:N0}";
            missilesText.text  = $"Missiles:   {country.Missiles:N0}";
            airForceText.text  = $"Air Force:  {country.AirForce:N0}";
            navyText.text      = $"Navy:       {country.Navy:N0}";

            // Can't alliance/war with yourself
            bool isSelf = GameManager.Instance != null &&
                          GameManager.Instance.PlayerCountry == country;
            allianceButton.interactable = !isSelf && country.Status != DiplomaticStatus.Allied;
            warButton.interactable      = !isSelf && country.Status != DiplomaticStatus.AtWar;
        }

        private void OnAllianceClicked()
        {
            pendingAlliance = true;
            ShowConfirm($"Type \"{selectedCountry.Name}\" to request an alliance:");
        }

        private void OnWarClicked()
        {
            pendingAlliance = false;
            ShowConfirm($"Type \"{selectedCountry.Name}\" to declare war:");
        }

        private void ShowConfirm(string prompt)
        {
            confirmPromptText.text = prompt;
            confirmInputField.text = "";
            confirmPanel.SetActive(true);
        }

        private void OnConfirmSubmit()
        {
            if (confirmInputField.text.Trim() != selectedCountry.Name)
            {
                confirmPromptText.text = "Name does not match. Try again:";
                return;
            }

            confirmPanel.SetActive(false);

            if (pendingAlliance)
                ProcessAllianceRequest();
            else
                ProcessWarDeclaration();
        }

        private void OnConfirmCancel()
        {
            confirmPanel.SetActive(false);
        }

        private void ProcessAllianceRequest()
        {
            // 50/50 AI acceptance per GDD section 7.1
            bool accepted = Random.value >= 0.5f;
            selectedCountry.Status = accepted ? DiplomaticStatus.Allied : selectedCountry.Status;

            string result = accepted
                ? $"{selectedCountry.Name} accepted your alliance request!"
                : $"{selectedCountry.Name} declined your alliance request.";

            Debug.Log(result);
            MapEventBus.OnNotification?.Invoke(result);
            Populate(selectedCountry);
        }

        private void ProcessWarDeclaration()
        {
            selectedCountry.Status = DiplomaticStatus.AtWar;
            string msg = $"War declared on {selectedCountry.Name}!";
            Debug.Log(msg);
            MapEventBus.OnNotification?.Invoke(msg);
            MapEventBus.OnWarDeclared?.Invoke(selectedCountry);
            Populate(selectedCountry);
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
