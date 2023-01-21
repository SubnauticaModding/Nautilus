namespace SMLHelper.Assets.Interfaces
{
    /// <summary>
    /// A prefab that gets its prefab by copying one using the value set. 
    /// </summary>
#pragma warning disable 0618
    public interface IClonePrefabTechType: IModPrefab
#pragma warning restore 0618
    {
        /// <summary>
        /// The techtype to get the prefab for.
        /// </summary>
        public TechType TypeToClone { get; }
    }

    /// <summary>
    /// A prefab that gets its prefab by copying one using the value set. 
    /// </summary>
#pragma warning disable 0618
    public interface IClonePrefabClassID: IModPrefab
#pragma warning restore 0618
    {
        /// <summary>
        /// The ClassID to get the prefab for.
        /// </summary>
        public string ClassID { get; }
    }

    /// <summary>
    /// A prefab that gets its prefab by copying one using the value set.
    /// </summary>
#pragma warning disable 0618
    public interface IClonePrefabFileName: IModPrefab
#pragma warning restore 0618
    {
        /// <summary>
        /// The target prefabs full file name including path.
        /// </summary>
        public string FileName { get; }
    }
}