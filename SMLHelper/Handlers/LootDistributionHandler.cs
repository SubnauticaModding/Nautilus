namespace SMLHelper.V2.Handlers
{
    using Interfaces;
    using Patchers;

    /// <summary>
    /// A handler that manages Loot Distribution.
    /// </summary>
    public class LootDistributionHandler : ILootDistributionHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static ILootDistributionHandler Main { get; } = new LootDistributionHandler();

        private LootDistributionHandler() { } // Hides constructor

        /// <summary>
        /// Adds in a custom entry into the Loot Distribution of the game.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="classId"></param>
        public static void AddCustomLootDistData(LootDistributionData.SrcData data, string classId)
        {
            Main.AddCustomLootDistData(data, classId);
        }

        void ILootDistributionHandler.AddCustomLootDistData(LootDistributionData.SrcData data, string classId)
        {
            LootDistributionPatcher.CustomSrcData.Add(classId, data);
        }
    }
}
