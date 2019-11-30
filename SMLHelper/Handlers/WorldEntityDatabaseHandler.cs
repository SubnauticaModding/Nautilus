namespace SMLHelper.V2.Handlers
{
    using Interfaces;
    using SMLHelper.V2.Patchers;
    using UWE;

    /// <summary>
    /// 
    /// </summary>
    public class WorldEntityDatabaseHandler : IWorldEntityDatabaseHandler
    {
        /// <summary>
        /// 
        /// </summary>
        public static IWorldEntityDatabaseHandler Main { get; } = new WorldEntityDatabaseHandler();

        private WorldEntityDatabaseHandler() { } // Hides constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="data"></param>
        public static void AddCustomInfo(string classId, WorldEntityInfo data)
        {
            Main.AddCustomInfo(classId, data);
        }

        void IWorldEntityDatabaseHandler.AddCustomInfo(string classId, WorldEntityInfo data)
        {
            WorldEntityDatabasePatcher.CustomWorldEntityInfos.Add(classId, data);
        }
    }
}
