using System;
using WorldConquest.Map;

namespace WorldConquest.Map
{
    /// <summary>
    /// Static event bus for map and game-wide interactions.
    /// </summary>
    public static class MapEventBus
    {
        public static Action<CountryData>              OnCountryClicked;
        public static Action<CountryData>              OnWarDeclared;
        public static Action<string>                   OnNotification;
        public static Action<CountryData, CountryData> OnCountryConquered; // (loser, winner)
        public static Action<int>                      OnTurnEnded;         // (turnNumber)
    }
}
