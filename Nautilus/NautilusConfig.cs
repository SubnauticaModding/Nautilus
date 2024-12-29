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

    [Toggle(Label = "Enable mod databank entries", Tooltip = "No seriously this needs a tooltip. I don't even know what this one does. And from a quick glance it looks like nothing")]
    public bool enableModDatabankEntries = true;//This whole option doesn't do anything now, because it didn't look like it did anything before either. 

    [Choice(Label = "Extra item info", Options = new[] { "Mod name (default)", "Mod name and item ID", "Nothing", }/*, Tooltip = "Wait there's no tooltip for this? Wild, there should probably be one"*/)]
    [OnChange(nameof(OnItemInfoChange))]
    public string extraItemInfo = "Mod name (default)";

    [Toggle(Label = "Hide Root Craft Nodes", Tooltip = "When opening a tab node, hide all craft nodes from the same level as the opened tab node. This helps against overlapping nodes, but some users may not like how it looks")]
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
