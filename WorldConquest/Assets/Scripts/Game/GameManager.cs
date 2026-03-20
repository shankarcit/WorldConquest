using System.Collections.Generic;
using UnityEngine;
using WorldConquest.Map;

namespace WorldConquest.Game
{
    /// <summary>
    /// Central game state manager.
    /// Assigns the player a random country at game start and exposes global game state.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Set by GeoJsonLoader after countries are loaded
        public List<CountryData> AllCountries { get; private set; } = new();

        public CountryData PlayerCountry { get; private set; }

        [Header("Splash Screen")]
        [SerializeField] private GameObject splashScreenPanel;

        private bool gameStarted = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>Called by GeoJsonLoader once all countries are ready.</summary>
        public void OnCountriesLoaded(List<CountryData> countries)
        {
            AllCountries = countries;
            AssignPlayerCountry();
        }

        private void AssignPlayerCountry()
        {
            if (AllCountries == null || AllCountries.Count == 0)
            {
                Debug.LogError("GameManager: No countries available to assign.");
                return;
            }

            int index = Random.Range(0, AllCountries.Count);
            PlayerCountry = AllCountries[index];
            Debug.Log($"GameManager: Player assigned to {PlayerCountry.Name} (Rank #{PlayerCountry.MilitaryRank})");

            if (splashScreenPanel != null)
                splashScreenPanel.SetActive(true);
        }

        /// <summary>Called by the splash screen Reroll button.</summary>
        public void RerollCountry()
        {
            AssignPlayerCountry();
        }

        /// <summary>Called by the splash screen Start button.</summary>
        public void StartGame()
        {
            if (splashScreenPanel != null)
                splashScreenPanel.SetActive(false);
            gameStarted = true;
            Debug.Log($"GameManager: Game started as {PlayerCountry.Name}");
        }

        public bool IsGameStarted => gameStarted;

        /// <summary>Called by SaveSystem.Load() to restore the player's country.</summary>
        public void SetPlayerCountry(CountryData country)
        {
            PlayerCountry = country;
            gameStarted   = true; // treat a loaded game as already started
            Debug.Log($"GameManager: Player country restored to {country.Name}");
        }
    }
}
