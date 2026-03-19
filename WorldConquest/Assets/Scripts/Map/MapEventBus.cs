using System;

namespace WorldConquest.Map
{
    /// <summary>
    /// Simple static event bus for map interactions.
    /// </summary>
    public static class MapEventBus
    {
        public static Action<CountryData> OnCountryClicked;
    }
}
