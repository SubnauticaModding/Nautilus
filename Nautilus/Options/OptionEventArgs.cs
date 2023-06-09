using System;

namespace Nautilus.Options;

/// <summary>
/// Interface for event arguments for a <see cref="OptionItem"/>.
/// </summary>
public abstract class OptionEventArgs : EventArgs
{
    /// <summary>
    /// The ID of the <see cref="OptionItem"/> this event corresponds to.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Instantiates a new <see cref="OptionEventArgs"/> for handling an event from a <see cref="OptionItem"/>.
    /// </summary>
    /// <param name="id">The internal ID of the item.</param>
    public OptionEventArgs(string id)
    {
        Id = id;
    }
}

/// <summary>
/// Interface for event arguments for a <see cref="OptionItem"/>.
/// </summary>
public abstract class ConfigOptionEventArgs<T> : OptionEventArgs
{
    /// <summary>
    /// The typed Value of the <see cref="OptionItem"/> this event corresponds to.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Instantiates a new <see cref="ConfigOptionEventArgs{T}"/> for handling an event from a <see cref="OptionItem"/>.
    /// </summary>
    /// <param name="id">The internal ID of the option.</param>
    /// <param name="value">The new value of the option.</param>
    public ConfigOptionEventArgs(string id, T value) : base(id)
    {
        Value = value;
    }
}