namespace SMLHelper.Options
{
    using System;

    /// <summary>
    /// Interface for event arguments for a <see cref="Options.ModOption"/>.
    /// </summary>
    public abstract class ModEventArgs<T>: EventArgs
    {
        /// <summary>
        /// The ID of the <see cref="Options.ModOption"/> this event corresponds to.
        /// </summary>
        public abstract string Id { get; }
        #nullable enable
        public abstract T? Value { get; }
        #nullable disable

    }
}