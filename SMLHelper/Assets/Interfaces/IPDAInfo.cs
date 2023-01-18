namespace SMLHelper.Assets.Interfaces
{
    public interface IPDAInfo
    {
        /// <summary>
        /// Override with the main group in the PDA blueprints where this item appears.
        /// </summary>
        public TechGroup GroupForPDA { get; }

        /// <summary>
        /// Override with the category within the group in the PDA blueprints where this item appears.
        /// </summary>
        public TechCategory CategoryForPDA { get; }
    }
}
