namespace SMLHelper.DependencyInjection;

using System.Collections.Generic;
using SMLHelper.Assets;

public class AssetCollection : IAssetCollection
{
    private readonly List<AssetDescriptor> _descriptors = new();
    private readonly List<CustomPrefab> _prefabs = new();

    public void AddDescriptor(AssetDescriptor item)
    {
        _descriptors.Add(item);
    }

    public List<AssetDescriptor> GetAssetDescriptors()
    {
        return _descriptors;
    }

    public void AddCustomPrefab(CustomPrefab customPrefab)
    {
        _prefabs.Add(customPrefab);
    }

    public List<CustomPrefab> GetCustomPrefabs()
    {
        return _prefabs;
    }
}

public interface IAssetCollection
{
    void AddDescriptor(AssetDescriptor descriptor);
    List<AssetDescriptor> GetAssetDescriptors();

    void AddCustomPrefab(CustomPrefab customPrefab);
    List<CustomPrefab> GetCustomPrefabs();
}