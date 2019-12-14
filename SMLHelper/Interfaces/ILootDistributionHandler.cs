namespace SMLHelper.V2.Interfaces
{
    /// <summary>
    /// A handler that manages Loot Distribution (spawning of fragments, fish, etc).
    /// </summary>
    public interface ILootDistributionHandler
    {
        /// <summary>
        /// Adds in a custom entry into the Loot Distribution of the game.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="classId"></param>
        void AddLootDistributionData(LootDistributionData.SrcData data, string classId);

        /// <summary>
        /// Edits Loot Distribution Data for existing/original class IDs. 
        /// </summary>
        void EditLootDistributionData(string classid, BiomeType biome, float probability, int count);
    }
}
