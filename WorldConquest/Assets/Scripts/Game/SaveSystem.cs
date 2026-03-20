using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using WorldConquest.Map;

namespace WorldConquest.Game
{
    /// <summary>
    /// JSON save / load system — GDD §13.
    ///
    /// Save file location: Application.persistentDataPath/savegame.json
    ///   Windows:  %AppData%\..\LocalLow\[Company]\[Product]\savegame.json
    ///   macOS:    ~/Library/Application Support/[Company]/[Product]/savegame.json
    ///
    /// Usage (from UI buttons or keyboard):
    ///   SaveSystem.Save();
    ///   SaveSystem.Load();
    ///   bool exists = SaveSystem.SaveExists();
    /// </summary>
    public static class SaveSystem
    {
        private const string FileName = "savegame.json";

        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, FileName);

        // ── Save ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Serialise all mutable game state to JSON and write to disk.
        /// Returns true on success.
        /// </summary>
        public static bool Save()
        {
            var gm = GameManager.Instance;
            if (gm == null || !gm.IsGameStarted)
            {
                Debug.LogWarning("SaveSystem: Cannot save — game not started.");
                return false;
            }

            var data = new GameSaveData
            {
                currentTurn       = FindTurnManager()?.CurrentTurn ?? 1,
                playerCountryName = gm.PlayerCountry?.Name ?? string.Empty,
            };

            foreach (var c in gm.AllCountries)
                data.countries.Add(CountrySaveData.From(c));

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            try
            {
                File.WriteAllText(SavePath, json);
                Debug.Log($"SaveSystem: Game saved to {SavePath}");
                MapEventBus.OnNotification?.Invoke("Game saved.");
                return true;
            }
            catch (IOException ex)
            {
                Debug.LogError($"SaveSystem: Save failed — {ex.Message}");
                return false;
            }
        }

        // ── Load ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Read save file and patch all live CountryData objects in-place.
        /// The map meshes and polygons are not touched — only mutable state changes.
        /// Returns true on success.
        /// </summary>
        public static bool Load()
        {
            if (!SaveExists())
            {
                Debug.LogWarning("SaveSystem: No save file found.");
                MapEventBus.OnNotification?.Invoke("No save file found.");
                return false;
            }

            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("SaveSystem: GameManager not found.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                var data    = JsonConvert.DeserializeObject<GameSaveData>(json);
                if (data == null)
                {
                    Debug.LogError("SaveSystem: Deserialisation returned null.");
                    return false;
                }

                // Build name-lookup for live countries
                var lookup = new System.Collections.Generic.Dictionary<string, CountryData>();
                foreach (var c in gm.AllCountries)
                    lookup[c.Name] = c;

                // Patch each saved country onto its live counterpart
                foreach (var saved in data.countries)
                {
                    if (lookup.TryGetValue(saved.name, out CountryData live))
                        saved.ApplyTo(live);
                    else
                        Debug.LogWarning($"SaveSystem: Country '{saved.name}' not found in live data — skipped.");
                }

                // Restore player assignment
                if (!string.IsNullOrEmpty(data.playerCountryName) &&
                    lookup.TryGetValue(data.playerCountryName, out CountryData player))
                {
                    gm.SetPlayerCountry(player);
                }

                // Restore turn counter
                var tm = FindTurnManager();
                if (tm != null)
                    tm.SetTurn(data.currentTurn);

                // Refresh visuals
                RefreshAllMeshVisuals();
                FindObjectOfType<UI.PlayerHUD>()?.Refresh();

                Debug.Log($"SaveSystem: Game loaded from {SavePath} (turn {data.currentTurn})");
                MapEventBus.OnNotification?.Invoke($"Game loaded — Turn {data.currentTurn}.");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"SaveSystem: Load failed — {ex.Message}");
                return false;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        public static bool SaveExists() => File.Exists(SavePath);

        public static string GetSavePath() => SavePath;

        private static TurnManager FindTurnManager() =>
            Object.FindObjectOfType<TurnManager>();

        /// <summary>
        /// After loading, trigger conquest visuals for countries that were
        /// already conquered when the save was made (TerritoryPercent == 0).
        /// CountryMesh listens to OnCountryConquered, but on load we patch
        /// data directly, so we re-fire the event for each dead country.
        /// </summary>
        private static void RefreshAllMeshVisuals()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            foreach (var c in gm.AllCountries)
            {
                if (c.TerritoryPercent <= 0f)
                    MapEventBus.OnCountryConquered?.Invoke(c, gm.PlayerCountry);
            }
        }

        // FindObjectOfType is in UnityEngine — bridge for static context
        private static T FindObjectOfType<T>() where T : Object =>
            Object.FindObjectOfType<T>();

        private static Object FindObjectOfType(System.Type t) =>
            Object.FindObjectOfType(t);
    }
}
