using BepInEx;

namespace SMLHelper.DependencyInjection;

public static class PrefabManager
{
    public static AssetBuilder CreateBuilder(BaseUnityPlugin plugin)
    {
        return new AssetBuilder(plugin);
    }
}