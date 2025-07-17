using HarmonyLib;

namespace Nautilus.Patchers;
internal class uGUI_CraftingMenuPatcher
{
    internal static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(uGUI_CraftingMenuPatcher));
    }

    [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.IsGrid))]
    [HarmonyPrefix]
    private static bool ShouldGridPostfix(uGUI_CraftingMenu.Node node, ref bool __result)
    {
        __result = ShouldGrid();
        return false;

        bool ShouldGrid()
        {
            var craftings = 0;
            var tabs = 0;

            foreach (var child in node)
            {
                if (child.action == TreeAction.Expand)
                {
                    tabs++;
                }
                else if (child.action == TreeAction.Craft)
                {
                    craftings++;
                }
            }

            return craftings > tabs;
        }
    }

    [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.Collapse))]
    [HarmonyPostfix]
    private static void CollapsePostfix(uGUI_CraftingMenu.Node parent)
    {
        if (!Initializer.ConfigFile.hideRootCraftNodes)
            return;

        if (parent == null) 
            return;

        if (parent.action != TreeAction.Craft) 
            return;

        parent.icon.SetActive(false);
    }

}
