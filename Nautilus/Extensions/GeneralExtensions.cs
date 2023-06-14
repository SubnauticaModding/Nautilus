using System;

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
}