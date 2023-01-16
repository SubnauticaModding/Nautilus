namespace SMLHelper.Assets.Interfaces
{
    using SMLHelper.API;

    public interface ICustomBattery
    {
        /// <summary>
        /// Is it to be registered as a Battery, PowerCell or Both?
        /// </summary>
        public BatteryType BatteryType { get; }

        /// <summary>
        /// The PrefabInfo associated with this ModPrefab.
        /// </summary>
        public PrefabInfo PrefabInfo { get; }
    }
}
