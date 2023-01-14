namespace SMLHelper.DependencyInjection;

using System.Collections.Generic;
using SMLHelper.Assets;

public class AssetCollection : IAssetCollection
{
    private readonly List<AssetDescriptor> _descriptors = new();
    private readonly List<ModPrefabRoot> _prefabs = new();

    public void AddDescriptor(AssetDescriptor item)
    {
        _descriptors.Add(item);
    }

    public List<AssetDescriptor> GetAssetDescriptors()
    {
        return _descriptors;
    }

    public void AddCustomPrefab(ModPrefabRoot customPrefab)
    {
        _prefabs.Add(customPrefab);
    }

    public List<ModPrefabRoot> GetCustomPrefabs()
    {
        return _prefabs;
    }
}

public interface IAssetCollection
{
    void AddDescriptor(AssetDescriptor descriptor);
    List<AssetDescriptor> GetAssetDescriptors();

    void AddCustomPrefab(ModPrefabRoot customPrefab);
    List<ModPrefabRoot> GetCustomPrefabs();
}