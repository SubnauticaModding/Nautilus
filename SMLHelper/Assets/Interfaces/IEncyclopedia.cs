namespace SMLHelper.Assets.Interfaces
{
    internal interface IEncyclopedia
    {
        /// <summary>
        /// Add a <see cref="PDAEncyclopedia.EntryData"/> into the PDA's Encyclopedia for this object.
        /// </summary>
        public PDAEncyclopedia.EntryData EncyclopediaEntryData { get; }
    }
}
