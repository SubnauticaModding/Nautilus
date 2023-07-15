using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;

namespace Nautilus.Utility;

internal class EnumTypeCache
{
    internal int Index;
    internal string Name;

    public EnumTypeCache()
    {
    }

    public EnumTypeCache(int index, string name)
    {
        Index = index;
        Name = name;
    }
}

internal static class EnumCacheProvider
{
    internal static Dictionary<Type, IEnumCache> CacheManagers { get; } = new();
    
    internal static void RegisterManager(Type enumType, IEnumCache manager)
    {
        if (!enumType.IsEnum)
            return;
        
        if (CacheManagers.ContainsKey(enumType))
            return;
        
        CacheManagers.Add(enumType, manager);
    }

    internal static bool TryGetManager(Type enumType, out IEnumCache manager)
    {
        manager = null;
        
        return enumType.IsEnum && CacheManagers.TryGetValue(enumType, out manager);
    }

    internal static IEnumCache EnsureManager<TEnum>() where TEnum : Enum
    {
        if (!TryGetManager(typeof(TEnum), out var manager))
        {
            manager = new EnumCacheManager<TEnum>();
            RegisterManager(typeof(TEnum), manager);
        }

        return manager;
    }
}

internal interface IEnumCache
{
    Dictionary<string, Assembly> TypesAddedBy { get; }
    IEnumerable<object> ModdedKeys { get; }
    int ModdedKeysCount { get; }
    bool TryGetValue(object key, out string name);
    bool ContainsKey(object key);
    bool TryParse(string value, out object type);
    EnumTypeCache RequestCacheForTypeName(string name, bool checkDeactivated = true, bool checkRequestedOnly = false, Assembly addedBy = null);
}

internal class EnumCacheManager<TEnum> : IEnumCache where TEnum : Enum
{
    private class DoubleKeyDictionary : IEnumerable<KeyValuePair<int, string>>
    {
        private readonly SortedDictionary<int, string> _mapIntString = new SortedDictionary<int, string>();
        private readonly SortedDictionary<TEnum, string> _mapEnumString = new SortedDictionary<TEnum, string>();

        private readonly SortedDictionary<string, TEnum> _mapStringEnum =
            new SortedDictionary<string, TEnum>(StringComparer.InvariantCultureIgnoreCase);

        private readonly SortedDictionary<string, int> _mapStringInt =
            new SortedDictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public bool TryGetValue(TEnum enumValue, out string name)
        {
            return _mapEnumString.TryGetValue(enumValue, out name);
        }

        public bool TryGetValue(string name, out TEnum enumValue)
        {
            return _mapStringEnum.TryGetValue(name, out enumValue);
        }

        public bool TryGetValue(string name, out int backingValue)
        {
            return _mapStringInt.TryGetValue(name, out backingValue);
        }

        public void Add(int backingValue, string name)
        {
            var enumValue = ConvertToObject(backingValue);
            Add(enumValue, backingValue, name);
        }

        public void Add(TEnum enumValue, int backingValue, string name)
        {
            _mapIntString.Add(backingValue, name);
            _mapEnumString.Add(enumValue, name);
            _mapStringEnum.Add(name, enumValue);
            _mapStringInt.Add(name, backingValue);

            if (backingValue > LargestIntValue)
                LargestIntValue = backingValue;
        }

        public void Remove(int backingValue, string name)
        {
            var enumValue = ConvertToObject(backingValue);
            Remove(enumValue, backingValue, name);
        }

        public void Remove(TEnum enumValue, int backingValue, string name)
        {
            _mapIntString.Remove(backingValue);
            _mapEnumString.Remove(enumValue);
            _mapStringEnum.Remove(name);
            _mapStringInt.Remove(name);
        }

        public int LargestIntValue { get; private set; }

        public IEnumerable<TEnum> KnownsEnumKeys => _mapEnumString.Keys;

        public int KnownsEnumCount => _mapEnumString.Count;

        public bool IsKnownKey(TEnum key)
        {
            return _mapEnumString.ContainsKey(key);
        }

        public bool IsKnownKey(string key)
        {
            return _mapStringEnum.ContainsKey(key);
        }

        public bool IsKnownKey(int key)
        {
            return _mapIntString.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
        {
            return _mapIntString.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _mapIntString.GetEnumerator();
        }

        public void Clear()
        {
            _mapIntString.Clear();
            _mapEnumString.Clear();
            _mapStringEnum.Clear();
            _mapStringInt.Clear();
        }
    }

    private static readonly Type _underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

    internal readonly string EnumTypeName = typeof(TEnum).DeclaringType is { } d
        ? $"{d.Name}{typeof(TEnum).Name}"
        : $"{typeof(TEnum).Name}";

    internal bool cacheLoaded = false;

    private readonly HashSet<int> BannedIDs;
    private readonly int LargestBannedID;

    private readonly DoubleKeyDictionary entriesFromFile = new DoubleKeyDictionary();
    private readonly DoubleKeyDictionary entriesFromDeactivatedFile = new DoubleKeyDictionary();
    private readonly DoubleKeyDictionary entriesFromRequests = new DoubleKeyDictionary();

    public IEnumerable<TEnum> ModdedKeys => entriesFromRequests.KnownsEnumKeys;
    IEnumerable<object> IEnumCache.ModdedKeys => entriesFromRequests.KnownsEnumKeys.Cast<object>();

    public int ModdedKeysCount => entriesFromRequests.KnownsEnumCount;

    private readonly Dictionary<string, Assembly> _typesAddedBy = new();
    public Dictionary<string, Assembly> TypesAddedBy => _typesAddedBy;

    bool IEnumCache.TryGetValue(object value, out string name)
    {
        return TryGetValue(ConvertToObject(Convert.ToInt32(value)), out name);
    }

    public bool TryGetValue(TEnum key, out string value)
    {
        return entriesFromRequests.TryGetValue(key, out value);
    }

    public bool TryParse(string value, out TEnum type)
    {
        return entriesFromRequests.TryGetValue(value, out type);
    }

    // This method is referenced by the NewtonsoftJsonPatcher.UpdateCachedEnumCacheManagers method through reflection - PLEASE UPDATE THAT METHOD IF RENAMING!
    public string ValueToName(TEnum value)
    {
        if (entriesFromRequests.TryGetValue(value, out var name))
            return name;
        return null;
    }

    bool IEnumCache.TryParse(string value, out object type)
    {
        if (entriesFromRequests.TryGetValue(value, out TEnum enumValue))
        {
            type = enumValue;
            return true;
        }

        type = null;
        return false;
    }

    public void Add(TEnum value, int backingValue, string name, Assembly addedBy)
    {
        if (!entriesFromRequests.IsKnownKey(backingValue))
        {
            entriesFromRequests.Add(value, backingValue, name);
            TypesAddedBy[name] = addedBy;   
        }
    }

    bool IEnumCache.ContainsKey(object key)
    {
        return entriesFromRequests.IsKnownKey(ConvertToObject(Convert.ToInt32(key)));
    }

    // This method is referenced by the NewtonsoftJsonPatcher.UpdateCachedEnumCacheManagers method through reflection - PLEASE UPDATE THAT METHOD IF RENAMING!
    public bool ContainsEnumKey(TEnum key)
    {
        return entriesFromRequests.IsKnownKey(key);
    }

    // This method is referenced by the NewtonsoftJsonPatcher.UpdateCachedEnumCacheManagers method through reflection - PLEASE UPDATE THAT METHOD IF RENAMING!
    public bool ContainsStringKey(string key)
    {
        return entriesFromRequests.IsKnownKey(key);
    }

    internal EnumCacheManager()
    {
        int largestID = 0;
        var bannedIDs = ExtBannedIdManager.GetBannedIdsFor(typeof(TEnum).Name, GetBannedIds());
        BannedIDs = new HashSet<int>();
        foreach (int id in bannedIDs)
        {
            BannedIDs.Add(id);
            largestID = Math.Max(largestID, id);
        }

        LargestBannedID = largestID;
        EnumCacheProvider.RegisterManager(typeof(TEnum), this);
    }

    private List<int> GetBannedIds()
    {
        var bannedIndices = new List<int>();

        Array enumValues = Enum.GetValues(typeof(TEnum));

        foreach (object enumValue in enumValues)
        {
            int realEnumValue = Convert.ToInt32(enumValue);

            if (bannedIndices.Contains(realEnumValue))
                continue; // Already exists in list

            bannedIndices.Add(realEnumValue);
        }

        return bannedIndices;
    }

    private static TEnum ConvertToObject(int backingValue)
    {
        return (TEnum)Convert.ChangeType(backingValue, _underlyingType);
    }

    #region Caching

    private string GetCacheDirectoryPath()
    {
        string saveDir = Path.Combine(Paths.ConfigPath , Assembly.GetExecutingAssembly().GetName().Name, $"{EnumTypeName}Cache");

        if (!Directory.Exists(saveDir))
            Directory.CreateDirectory(saveDir);

        return saveDir;
    }

    private string GetCachePath()
    {
        return Path.Combine(GetCacheDirectoryPath(), $"{EnumTypeName}Cache.txt");
    }

    private string GetDeactivatedCachePath()
    {
        return Path.Combine(GetCacheDirectoryPath(), $"{EnumTypeName}DeactivatedCache.txt");
    }

    internal void LoadCache()
    {
        if (cacheLoaded)
            return;

        ReadCacheFile(GetCachePath(), (index, name) => { entriesFromFile.Add(index, name); });

        ReadCacheFile(GetDeactivatedCachePath(), (index, name) =>
        {
            entriesFromDeactivatedFile.Add(index, name);
            entriesFromFile.Add(index, name);
        });

        cacheLoaded = true;
    }

    private void ReadCacheFile(string savePathDir, Action<int, string> loadParsedEntry)
    {
        try
        {
            if (!File.Exists(savePathDir))
            {
                cacheLoaded = true;
                return;
            }

            string[] allText = File.ReadAllLines(savePathDir);
            foreach (string line in allText)
            {
                string[] split = line.Split(':');
                string name = split[0];
                string index = split[1];

                loadParsedEntry.Invoke(Convert.ToInt32(index), name);
            }
        }
        catch (Exception exception)
        {
            InternalLogger.Error($"Caught exception while reading {savePathDir}{Environment.NewLine}{exception}");
        }
    }

    internal void SaveCache()
    {
        LoadCache();
        try
        {
            string savePathDir = GetCachePath();
            StringBuilder stringBuilder = new StringBuilder();

            foreach (KeyValuePair<int, string> entry in entriesFromRequests)
            {
                stringBuilder.AppendLine($"{entry.Value}:{entry.Key}");
            }

            File.WriteAllText(savePathDir, stringBuilder.ToString());


            savePathDir = GetDeactivatedCachePath();
            stringBuilder = new StringBuilder();

            foreach (KeyValuePair<int, string> entry in entriesFromFile)
            {
                if (!entriesFromRequests.TryGetValue(entry.Value, out int v))
                {
                    stringBuilder.AppendLine($"{entry.Value}:{entry.Key}");
                }
            }

            File.WriteAllText(savePathDir, stringBuilder.ToString());
            entriesFromFile.Clear();
        }
        catch (Exception exception)
        {
            InternalLogger.Error($"Caught exception while saving cache!{Environment.NewLine}{exception}");
        }
    }

    EnumTypeCache IEnumCache.RequestCacheForTypeName(string name, bool checkDeactivated, bool checkRequestedOnly, Assembly addedBy)
    {
        return RequestCacheForTypeName(name, checkDeactivated, checkRequestedOnly, addedBy);
    }

    internal EnumTypeCache RequestCacheForTypeName(string name, bool checkDeactivated = true, bool checkRequestedOnly = false, Assembly addedBy = null)
    {
        LoadCache();

        if (entriesFromRequests.TryGetValue(name, out int value))
        {
            return new EnumTypeCache(value, name);
        }
        else if (!checkRequestedOnly && entriesFromFile.TryGetValue(name, out value))
        {
            entriesFromRequests.Add(value, name);
            if(addedBy != null)
                TypesAddedBy[name] = addedBy;
            return new EnumTypeCache(value, name);
        }
        else if (!checkRequestedOnly && checkDeactivated && entriesFromDeactivatedFile.TryGetValue(name, out value))
        {
            entriesFromRequests.Add(value, name);
            if(addedBy != null)
                TypesAddedBy[name] = addedBy;
            entriesFromDeactivatedFile.Remove(value, name);
            return new EnumTypeCache(value, name);
        }

        return null;
    }

    internal int GetNextAvailableIndex()
    {
        LoadCache();

        int index = LargestBannedID + 1;

        while (entriesFromFile.IsKnownKey(index) ||
               entriesFromRequests.IsKnownKey(index) ||
               entriesFromDeactivatedFile.IsKnownKey(index) ||
               BannedIDs.Contains(index))
        {
            index++;
        }

        return index;
    }


    #endregion
}