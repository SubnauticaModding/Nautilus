namespace SMLHelper.Assets.Interfaces
{
    internal interface IScannable
    {
        /// <summary>
        /// Add a <see cref="PDAScanner.EntryData"/> to this Prefab to make it scannable.
        /// </summary>
        public PDAScanner.EntryData ScannerEntryData { get; }
    }
}
