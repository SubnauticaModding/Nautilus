namespace SMLHelper.Assets.Interfaces
{
    public enum BatteryType
    {
        Battery,
        PowerCell
    }

    public enum BatteryModel
    {
        Battery,
        IonBattery,
        PowerCell,
        IonPowerCell,
        Custom,
        IonCustom
    }

#pragma warning disable 0618
    public interface ICustomBattery: IModPrefab
#pragma warning restore 0618
    {
        /// <summary>
        /// Sets the Game Prefab to use as this prefabs basic model.<br/>
        /// Use Custom for SMLHelper to run your <see cref="ICustomPrefabAsync.GetGameObjectAsync"/> to get your custom prefab.
        /// </summary>
        public BatteryModel BatteryModel { get; }

        /// <summary>
        /// Is it to be registered as a Battery, PowerCell or Both?
        /// </summary>
        public BatteryType BatteryType { get; }

        /// <summary>
        /// The Max Power that this battery can hold when fully charged.
        /// </summary>
        public float PowerCapacity { get; }
    }
}
