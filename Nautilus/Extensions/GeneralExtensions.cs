using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Nautilus.Extensions;

/// <summary>
/// Contains extensions that are not exclusively specific to one type.
/// </summary>
public static class GeneralExtensions
{
    /// <summary>
    /// Adds an object to the end of the <see cref="Array"/>.
    /// </summary>
    /// <param name="array">The array to perform this action on.</param>
    /// <param name="item">The object to be added to the end of the <see cref="Array"/>. The value can be null for reference types.</param>
    public static void Add<T>(this T[] array, T item)
    {
        Array.Resize(ref array, array.Length + 1);
        array[^1] = item;
    }
    
    /// <summary>
    /// Removes the "(Clone)" part from names.
    /// </summary>
    /// <param name="this">The string to perform this action on.</param>
    /// <returns>The new string without "(Clone)". If the specified string does not contain "(Clone)", it simply returns the string as-is.</returns>
    public static string TrimClone(this string @this)
    {
        var pos = @this.IndexOf("(Clone)", StringComparison.Ordinal);
        return pos >= 0 ? @this.Remove(pos) : @this;
    }
    
    /// <summary>
    /// Adds a message and increases the life of it, instead of spamming it.
    /// </summary>
    /// <param name="this"></param>
    /// <param name="message">the message text</param>
    public static void AddHint(this ErrorMessage @this, string message)
    {
        var msg = @this.GetExistingMessage(message);
        if (msg is null)
            ErrorMessage.AddMessage(message);
        else if (msg.timeEnd <= Time.time + @this.timeFadeOut)
            msg.timeEnd += @this.timeFadeOut + @this.timeInvisible;
    }

    /// <summary>
    /// Concatenates string representations of the provided <paramref name="values"/>,
    /// using the specified <paramref name="separator"/> between them.
    /// </summary>
    /// <typeparam name="T">Type of value that will be converted to <see cref="string"/>.</typeparam>
    /// <param name="builder">The <see cref="StringBuilder"/>.</param>
    /// <param name="separator">The <see cref="string"/> to insert between each pair of values.</param>
    /// <param name="values">Values to concatenate into the <paramref name="builder"/>.</param>
    /// <returns>The provided <see cref="StringBuilder"/>.</returns>
    public static StringBuilder AppendJoin<T>(this StringBuilder builder, string separator, IEnumerable<T> values)
    {
        return builder.Append(string.Join(separator, values));
    }
}