using System;
using System.Collections;
using System.Collections.Generic;

namespace Nautilus.Utility;

/// <summary>
/// This dictionary structure automatically checks for duplicate keys as they are being added to the collection.
/// Duplicate entries are logged and removed from the final collection.
/// </summary>
/// <typeparam name="K">The Key Type</typeparam>
/// <typeparam name="V">The Value Type</typeparam>
public class SelfCheckingDictionary<K, V> : IDictionary<K, V>
{
    /// <summary>
    /// Maintains a collection of the keys that have encountered duplicates and how many of them were discarded.
    /// </summary>
    private readonly Dictionary<K, int> DuplicatesDiscarded;

    /// <summary>
    /// Maintains the final collection of only unique keys.
    /// </summary>
    private readonly Dictionary<K, V> UniqueEntries;
    private readonly string CollectionName;

    private readonly Func<K, string> ToLogString;

    private SelfCheckingDictionary(Func<K, string> toLog)
    {
        ToLogString = toLog ?? ((k) => k.ToString());
    }

    /// <summary>
    /// Creates a <see cref="SelfCheckingDictionary{K, V}"/> with an optional ToString function.
    /// </summary>
    /// <param name="collectionName"></param>
    /// <param name="toLog"></param>
    public SelfCheckingDictionary(string collectionName, Func<K, string> toLog = null)
        : this(toLog)
    {
        CollectionName = collectionName;
        UniqueEntries = new Dictionary<K, V>();
        DuplicatesDiscarded = new Dictionary<K, int>();
    }

    /// <summary>
    /// Creates a <see cref="SelfCheckingDictionary{K, V}"/> with an EqualityComparer and an optional ToString function.
    /// </summary>
    /// <param name="collectionName"></param>
    /// <param name="equalityComparer"></param>
    /// <param name="toLog"></param>
    public SelfCheckingDictionary(string collectionName, IEqualityComparer<K> equalityComparer, Func<K, string> toLog = null)
        : this(toLog)
    {
        CollectionName = collectionName;
        UniqueEntries = new Dictionary<K, V>(equalityComparer);
        DuplicatesDiscarded = new Dictionary<K, int>(equalityComparer);
    }

    /// <summary>
    /// Gets a key value pair from the collection or sets a key value pair into the collection.
    /// When setting, if a key already exists, the previous entry will be discarded.
    /// </summary>
    /// <param name="key">The unique key.</param>
    /// <returns>The value corresponding to the key.</returns>
    public V this[K key]
    {
        get
        {
            if(key == null)
            {
                LogNullKeyError();
                return default;
            }
            return UniqueEntries[key];
        }

        set
        {
            if(key == null)
            {
                LogNullKeyError();
                return;
            }

            if(UniqueEntries.ContainsKey(key))
            {
                if(DuplicatesDiscarded.ContainsKey(key))
                {
                    DuplicatesDiscarded[key]++;
                }
                else
                {
                    DuplicatesDiscarded.Add(key, 1); // Original is overwritten.
                }

                DupFoundLastDiscardedLog(key);
            }

            UniqueEntries[key] = value;
        }
    }

    /// <summary>
    /// Gets a collection containing the keys in the <see cref="SelfCheckingDictionary{K, V}"/>
    /// </summary>
    public ICollection<K> Keys => UniqueEntries.Keys;

    /// <summary>
    /// Gets a collection containing the values in the <see cref="SelfCheckingDictionary{K, V}"/>
    /// </summary>
    public ICollection<V> Values => UniqueEntries.Values;

    /// <summary>
    /// Gets the number of unique entries in the <see cref="SelfCheckingDictionary{K, V}"/>
    /// </summary>
    public int Count => UniqueEntries.Count;

    /// <summary>
    /// Defaults to false.
    /// </summary>
    public bool IsReadOnly { get; } = false;

    /// <summary>
    /// Add a new entry the collection.
    /// If a duplicate key is found, the new value will be discarded.
    /// </summary>
    /// <param name="key">The unique key.</param>
    /// <param name="value">The value.</param>
    public void Add(K key, V value)
    {
        if(key == null)
        {
            LogNullKeyError();
            return;
        }

        if (DuplicatesDiscarded.ContainsKey(key))
        {
            DuplicatesDiscarded[key]++;
            DupFoundNewDiscardedLog(key);
            return;
        }

        if (UniqueEntries.ContainsKey(key))
        {
            DuplicatesDiscarded.Add(key, 1);
            DupFoundNewDiscardedLog(key);
            return;
        }

        UniqueEntries.Add(key, value);
    }

    /// <summary>
    /// Add a new entry the collection.
    /// If a duplicate key is found, the new value will be discarded.
    /// </summary>
    /// <param name="item">The key value pair.</param>
    public void Add(KeyValuePair<K, V> item)
    {
        Add(item.Key, item.Value);
    }


#pragma warning disable 1591
    public void Clear()
    {
        UniqueEntries.Clear();
        DuplicatesDiscarded.Clear();
    }

    public bool Contains(KeyValuePair<K, V> item)
    {

        if(item.Key == null)
        {
            LogNullKeyError();
            return false;
        }

        return UniqueEntries.TryGetValue(item.Key, out V value) && value.Equals(item.Value);
    }

    public bool ContainsKey(K key)
    {
        if(key == null)
        {
            LogNullKeyError();
            return false;
        }

        return UniqueEntries.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
    {
        foreach (KeyValuePair<K, V> pair in UniqueEntries)
        {
            array[arrayIndex++] = pair;
        }
    }

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
        return UniqueEntries.GetEnumerator();
    }

    public bool Remove(K key)
    {
        if(key == null)
        {
            LogNullKeyError();
            return false;
        }

        return UniqueEntries.Remove(key) | DuplicatesDiscarded.Remove(key);
    }

    public bool Remove(KeyValuePair<K, V> item)
    {
        if(item.Key == null)
        {
            LogNullKeyError();
            return false;
        }
        return UniqueEntries.Remove(item.Key) | DuplicatesDiscarded.Remove(item.Key);
    }

    public bool TryGetValue(K key, out V value)
    {
        if(key == null)
        {
            LogNullKeyError();
            value= default(V);
            return false;
        }
        return UniqueEntries.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return UniqueEntries.GetEnumerator();
    }

#pragma warning restore 1591

    private void LogNullKeyError()
    {
        InternalLogger.Error($"Tried to Add to or Access {CollectionName} Dictionary with a Null Key!");
    }

    /// <summary>
    /// Informs the user that the new entry for the specified key has been discarded.
    /// </summary>
    /// <param name="key">The no longer unique key.</param>
    private void DupFoundNewDiscardedLog(K key)
    {
        string keyLogString = ToLogString(key);
        InternalLogger.Warn($"{CollectionName} already exists for '{keyLogString}'.{Environment.NewLine}" +
                            $"New value has been rejected. {Environment.NewLine}" +
                            $"So far we have discarded or overwritten {DuplicatesDiscarded[key]} entries for '{keyLogString}'.");
    }

    /// <summary>
    /// Informs the user that the previous entry for the specified key has been discarded.
    /// </summary>
    /// <param name="key">The no longer unique key.</param>
    private void DupFoundLastDiscardedLog(K key)
    {
        string keyLogString = ToLogString(key);
        InternalLogger.Warn($"{CollectionName} already exists for '{keyLogString}'.{Environment.NewLine}" +
                            $"Original value has been overwritten by later entry.{Environment.NewLine}" +
                            $"So far we have discarded or overwritten {DuplicatesDiscarded[key]} entries for '{keyLogString}'.");
    }
}