namespace SMLHelper.Options
{
    using System;

    public abstract class OptionEventArgs : EventArgs
    {
        /// <summary>
        /// The ID of the <see cref="Options.ModOption"/> this event corresponds to.
        /// </summary>
        public string Id { get; }

        public OptionEventArgs(string id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// Interface for event arguments for a <see cref="Options.ModOption"/>.
    /// </summary>
    public abstract class ConfigOptionEventArgs<T>: OptionEventArgs
    {
        public T Value { get; }

        public ConfigOptionEventArgs(string id, T value) : base(id)
        {
            Value = value;
        }
    }
}