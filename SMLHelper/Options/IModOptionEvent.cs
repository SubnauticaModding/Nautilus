namespace SMLHelper.V2.Options
{
    /// <summary>
    /// Interface for event arguments for a <see cref="ModOption"/>.
    /// </summary>
    public interface IModOptionEventArgs
    {
        /// <summary>
        /// The ID of the <see cref="ModOption"/> this event corresponds to.
        /// </summary>
        string Id { get; }
    }
}
