using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldConquest.Game;

namespace WorldConquest.UI
{
    /// <summary>
    /// Save / Load UI buttons — GDD §13.
    ///
    /// Scene setup:
    ///   - Attach to any persistent UI GameObject (e.g. HUDPanel)
    ///   - Wire SaveButton, LoadButton in Inspector
    ///   - Optionally wire SavePathText to show the save file location
    ///
    /// Keyboard shortcuts are also handled in Update:
    ///   F5 = Quick Save,  F9 = Quick Load
    /// </summary>
    public class SaveLoadUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;

        [Header("Optional — display save path")]
        [SerializeField] private TextMeshProUGUI savePathText;

        void Awake()
        {
            if (saveButton != null) saveButton.onClick.AddListener(OnSave);
            if (loadButton != null) loadButton.onClick.AddListener(OnLoad);

            if (savePathText != null)
                savePathText.text = $"Save: {SaveSystem.GetSavePath()}";
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5)) OnSave();
            if (Input.GetKeyDown(KeyCode.F9)) OnLoad();
        }

        private void OnSave()
        {
            SaveSystem.Save();
            // Disable load button interactability hint update
            if (loadButton != null)
                loadButton.interactable = SaveSystem.SaveExists();
        }

        private void OnLoad()
        {
            if (!SaveSystem.SaveExists())
            {
                MapEventBus.OnNotification?.Invoke("No save file found.");
                return;
            }
            SaveSystem.Load();
        }
    }
}
