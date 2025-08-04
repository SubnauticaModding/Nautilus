using BepInEx.Logging;
using Nautilus.Utility;
using Nautilus.Utility.ModMessages;

namespace Nautilus.Patchers;

internal static class FindMyUpdatesPatcher
{
    internal static void Patch()
    {
        ModMessageSystem.SendGlobal("FindMyUpdates", "https://raw.githubusercontent.com/SubnauticaModding/Nautilus/refs/heads/master/Version.json");
        InternalLogger.Log("FindMyUpdatesPatcher is done.", LogLevel.Debug);
    }
}