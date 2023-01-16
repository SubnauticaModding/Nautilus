namespace SMLHelper.DependencyInjection;

using System.Collections.Generic;
using SMLHelper.Assets;
using SMLHelper.Assets.Interfaces;

public class AssetCollection : IAssetCollection
{
    private readonly List<AssetDescriptor> _descriptors = new();
    private readonly List<IModPrefab> _prefabs = new();

    public void AddDescriptor(AssetDescriptor item)
    {
        _descriptors.Add(item);
    }

    public List<AssetDescriptor> GetAssetDescriptors()
    {
        return _descriptors;
    }

    public void AddCustomPrefab(IModPrefab customPrefab)
    {
        _prefabs.Add(customPrefab);
    }

    public List<IModPrefab> GetCustomPrefabs()
    {
        return _prefabs;
    }
}

public interface IAssetCollection
{
    void AddDescriptor(AssetDescriptor descriptor);
    List<AssetDescriptor> GetAssetDescriptors();

    void AddCustomPrefab(IModPrefab customPrefab);
    List<IModPrefab> GetCustomPrefabs();
}