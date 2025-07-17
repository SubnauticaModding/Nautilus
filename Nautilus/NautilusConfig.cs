using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Nautilus.Patchers;

namespace Nautilus;
[ConfigFile("NautilusConfig")]
internal class NautilusConfig : ConfigFile
{
    [Toggle(Label = "Enable debug logs")]
    [OnChange(nameof(OnDebugLogChange))]
    public bool enableDebugLogs = false;

    [Toggle(Label = "Enable mod databank entries", Tooltip = "If enabled, a 'Mods' tab is added to the databank which holds PDA entries with in-game description of mods.")]
    public bool enableModDatabankEntries = true;

    [Choice(Label = "Extra item info", Options = new[] { "Mod name (default)", "Mod name and item ID", "Nothing", }, Tooltip = "Determines what information is displayed under item tooltips.")]
    [OnChange(nameof(OnItemInfoChange))]
    public string extraItemInfo = "Mod name (default)";

    [Toggle(Label = "Hide adjacent craft nodes", Tooltip = "When opening a tab node, hide all craft nodes from the same level as the opened tab node. This helps against overlapping nodes, but some users may not like how it looks.")]
    public bool hideRootCraftNodes = true;
    
    private void OnDebugLogChange(ToggleChangedEventArgs args)
    {
        Utility.InternalLogger.SetDebugging(args.Value);
    }
    private void OnItemInfoChange(ChoiceChangedEventArgs<string> args)
    {
        TooltipPatcher.RefreshExtraItemInfo(args.Value);
    }
}
