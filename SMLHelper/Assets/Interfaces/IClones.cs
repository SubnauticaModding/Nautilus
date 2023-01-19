namespace SMLHelper.Assets.Interfaces
{
    /// <summary>
    /// A prefab that gets its prefab by copying one using the value set. 
    /// </summary>
    public interface ICloneTechType
    {
        /// <summary>
        /// The techtype to get the prefab for.
        /// </summary>
        public TechType TypeToClone { get; }
    }

    /// <summary>
    /// A prefab that gets its prefab by copying one using the value set. 
    /// </summary>
    public interface ICloneClassID
    {
        /// <summary>
        /// The ClassID to get the prefab for.
        /// </summary>
        public string ClassID { get; }
    }

    /// <summary>
    /// A prefab that gets its prefab by copying one using the value set.
    /// </summary>
    public interface ICloneFileName
    {
        /// <summary>
        /// The target prefabs full file name including path.
        /// </summary>
        public string FileName { get; }
    }
}