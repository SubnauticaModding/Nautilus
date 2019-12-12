namespace SMLHelper.V2.Utility
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;

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

    internal class EnumCacheManager<T> where T : Enum
    {
        private class DoubleKeyDictionary : IEnumerable<KeyValuePair<int, string>>
        {
            private readonly Dictionary<int, string> MapIntString = new Dictionary<int, string>();
            private readonly Dictionary<T, string> MapEnumString = new Dictionary<T, string>();
            private readonly Dictionary<string, T> MapStringEnum = new Dictionary<string, T>();
            private readonly Dictionary<string, int> MapStringInt = new Dictionary<string, int>();

            public bool TryGetValue(T enumValue, out string name)
            {
                return MapEnumString.TryGetValue(enumValue, out name);
            }

            public bool TryGetValue(string name, out T enumValue)
            {
                return MapStringEnum.TryGetValue(name, out enumValue);
            }

            public bool TryGetValue(string name, out int backingValue)
            {
                return MapStringInt.TryGetValue(name, out backingValue);
            }

            public void Add(int backingValue, string name)
            {
                Add((T)(object)backingValue, backingValue, name);
            }

            public void Add(T enumValue, int backingValue, string name)
            {
                MapIntString.Add(backingValue, name);
                MapEnumString.Add(enumValue, name);
                MapStringEnum.Add(name, enumValue);
                MapStringInt.Add(name, backingValue);

                if (backingValue > this.LargestIntValue)
                    this.LargestIntValue = backingValue;
            }

            public int LargestIntValue { get; private set; }

            public IEnumerable<T> KnownsEnumKeys => MapEnumString.Keys;

            public bool IsKnownKey(T key)
            {
                return MapEnumString.ContainsKey(key);
            }

            public bool IsKnownKey(int key)
            {
                return MapIntString.ContainsKey(key);
            }

            public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
            {
                return MapIntString.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return MapIntString.GetEnumerator();
            }
        }

        internal readonly string EnumTypeName;
        internal readonly int StartingIndex;
        internal bool cacheLoaded = false;

        private readonly HashSet<int> BannedIDs;
        private readonly int LargestBannedID;

        private readonly DoubleKeyDictionary entriesFromFile = new DoubleKeyDictionary();
        private readonly DoubleKeyDictionary entriesFromDeactivatedFile = new DoubleKeyDictionary();
        private readonly DoubleKeyDictionary entriesFromRequests = new DoubleKeyDictionary();

        public IEnumerable<T> ModdedKeys => entriesFromRequests.KnownsEnumKeys;

        public bool TryGetValue(T key, out string value)
        {
            return entriesFromRequests.TryGetValue(key, out value);
        }

        public bool TryParse(string value, out T type)
        {
            return entriesFromRequests.TryGetValue(value, out type);
        }

        public void Add(T value, int backingValue, string name)
        {
            if (!entriesFromRequests.IsKnownKey(backingValue))
                entriesFromRequests.Add(value, backingValue, name);
        }

        public bool ContainsKey(T key)
        {
            return entriesFromRequests.IsKnownKey(key);
        }

        internal EnumCacheManager(string enumTypeName, int startingIndex, IEnumerable<int> bannedIDs)
        {
            EnumTypeName = enumTypeName;
            StartingIndex = startingIndex;

            int largestID = 0;
            BannedIDs = new HashSet<int>();
            foreach (int id in bannedIDs)
            {
                BannedIDs.Add(id);
                largestID = Math.Max(largestID, id);
            }

            LargestBannedID = largestID;
        }

        #region Caching

        private string GetCacheDirectoryPath()
        {
            string saveDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{EnumTypeName}Cache");

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

            try
            {
                string savePathDir = GetCachePath();

                if (!File.Exists(savePathDir))
                {
                    Logger.Info($"No {EnumTypeName} cache file was found. One will be created when the game is saved.");
                    cacheLoaded = true; // Just so it wont keep calling this over and over again.
                    return;
                }

                string[] allText = File.ReadAllLines(savePathDir);

                foreach (string line in allText)
                {
                    string[] split = line.Split(':');
                    string name = split[0];
                    string index = split[1];

                    entriesFromFile.Add(Convert.ToInt32(index), name);
                }
            }
            catch (Exception exception)
            {
                Logger.Error($"Caught exception while reading cache!{Environment.NewLine}{exception}");
            }

            try
            {
                string savePathDir = GetDeactivatedCachePath();

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

                    entriesFromDeactivatedFile.Add(Convert.ToInt32(index), name);
                    entriesFromFile.Add(Convert.ToInt32(index), name);
                }
            }
            catch (Exception exception)
            {
                Logger.Error($"Caught exception while reading Deactivated cache!{Environment.NewLine}{exception}");
            }

            cacheLoaded = true;
        }

        internal void SaveCache()
        {
            LoadCache();
            try
            {
                string savePathDir = GetCachePath();
                var stringBuilder = new StringBuilder();

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

            }
            catch (Exception exception)
            {
                Logger.Error($"Caught exception while saving cache!{Environment.NewLine}{exception}");
            }
        }

        internal EnumTypeCache GetCacheForTypeName(string name)
        {
            LoadCache();

            if (entriesFromRequests.TryGetValue(name, out int value) || entriesFromFile.TryGetValue(name, out value))
            {
                return new EnumTypeCache(value, name);
            }

            return null;
        }

        internal int GetNextAvailableIndex()
        {
            LoadCache();

            int index = StartingIndex + 1;

            while (entriesFromFile.IsKnownKey(index) ||
                   entriesFromRequests.IsKnownKey(index) ||
                   BannedIDs.Contains(index))
            {
                index++;
            }

            return index;
        }

        internal bool IsIndexAvailable(int index)
        {
            LoadCache();

            if (BannedIDs.Contains(index))
                return false;

            if (entriesFromFile.IsKnownKey(index))
                return false;

            if (entriesFromRequests.IsKnownKey(index))
                return false;

            return true;
        }

        #endregion
    }
}
