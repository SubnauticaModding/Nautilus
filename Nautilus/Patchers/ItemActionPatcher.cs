using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;

namespace Nautilus.Patchers;


internal class ItemActionPatcher
{
    internal record CustomItemAction(Action<InventoryItem> Action, string LanguageLineKey, Predicate<InventoryItem> Condition);

    internal static readonly IDictionary<TechType, CustomItemAction> MiddleClickActions = new SelfCheckingDictionary<TechType, CustomItemAction>("MiddleClickActions", t => t.AsString());
    internal static readonly IDictionary<TechType, CustomItemAction> LeftClickActions = new SelfCheckingDictionary<TechType, CustomItemAction>("LeftClickActions", t => t.AsString());
    
    private static ItemAction _customMiddleClick;
    private static ItemAction _customLeftClick;

    //Controller does not have a button
    internal const GameInput.Button ControllerMiddleClickBind = GameInput.Button.Sprint;
    
    internal static void Patch(Harmony harmony)
    {
        _customMiddleClick = EnumHandler.AddEntry<ItemAction>("CustomMiddleClick");
        _customLeftClick = EnumHandler.AddEntry<ItemAction>("CustomLeftClick");
        harmony.PatchAll(typeof(ItemActionPatcher));
        InternalLogger.Debug("ItemActionPatcher is done.");
    }

    [HarmonyPatch(typeof(uGUI_ItemsContainer))]
    [HarmonyPatch(nameof(uGUI_ItemsContainer.OnButtonDown))]
    [HarmonyPostfix]
    private static void OnButtonDown_Postfix(uGUI_ItemsContainer __instance, uGUI_ItemIcon icon, GameInput.Button button)
    {
        //This method is only called with a controller attached. Controller by default does not give events for a middle click like input
        if (GameInput.IsPrimaryDeviceGamepad() && button == ControllerMiddleClickBind)
        {
            __instance.icons.TryGetValue(icon, out InventoryItem inventoryItem);
            Inventory.main.ExecuteItemAction(_customMiddleClick, inventoryItem);
        }
    }

    [HarmonyPatch(typeof(uGUI_InventoryTab))]
    [HarmonyPatch(nameof(uGUI_InventoryTab.OnPointerClick))]
    [HarmonyPrefix]
    private static bool OnPointerClick_Prefix(InventoryItem item, int button)
    {
        if (ItemDragManager.isDragging) return true;
        if (button == 0 && LeftClickActions.ContainsKey(item.item.GetTechType()))
        {
            Inventory.main.ExecuteItemAction(_customLeftClick, item);
            return false;
        }
        //Controller binds button 2 to X (for xbox). This is the drop button for items so we don't want it
        if (!GameInput.IsPrimaryDeviceGamepad() && button == 2 && MiddleClickActions.ContainsKey(item.item.GetTechType()))
        {
            Inventory.main.ExecuteItemAction(_customMiddleClick, item);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch(nameof(Inventory.ExecuteItemAction), typeof(ItemAction), typeof(InventoryItem))]
    [HarmonyPrefix]
    private static bool ExecuteItemAction_Prefix(ItemAction action, InventoryItem item)
    {
        if (action == _customLeftClick)
        {
            HandleActionFor(LeftClickActions);
            return false;
        }
        if (action == _customMiddleClick)
        {
            HandleActionFor(MiddleClickActions);
            return false;
        }
        return true;
        
        void HandleActionFor(IDictionary<TechType, CustomItemAction> actions)
        {
            TechType itemTechType = item.item.GetTechType();
            if (actions.TryGetValue(itemTechType, out CustomItemAction customItemAction) && customItemAction.Condition(item))
            {
                customItemAction.Action(item);
            }
        }
    }

    [HarmonyPatch(typeof(TooltipFactory))]
    [HarmonyPatch(nameof(TooltipFactory.ItemActions))]
    [HarmonyPostfix]
    private static void ItemActions_Postfix(StringBuilder sb, InventoryItem item)
    {
        TechType itemTechType = item.item.GetTechType();
        if (LeftClickActions.TryGetValue(itemTechType, out CustomItemAction action) && action.Condition(item))
        {
            TooltipFactory.WriteAction(sb, TooltipFactory.stringButton0, Language.main.Get(action.LanguageLineKey));
        }
        if (MiddleClickActions.TryGetValue(itemTechType, out action) && action.Condition(item))
        {
            TooltipFactory.WriteAction(sb, GetMiddleClickGlyph(), Language.main.Get(action.LanguageLineKey));
        }
    }
    
    private static string GetMiddleClickGlyph()
    {
        //Middle click is not bound to a button like others are. Getting TooltipFactory.stringButton2 format does NOT give what is expected for keyboard :(
#if SUBNAUTICA
        if (GameInput.IsPrimaryDeviceGamepad()) return GameInput.FormatButton(ControllerMiddleClickBind);
        return "<sprite name=\"MouseButtonMiddle\" color=#ADF8FFFF>";
#elif BELOWZERO
        if (GameInput.IsPrimaryDeviceGamepad())
        {
            string bindName = GameInput.GetBindingName(ControllerMiddleClickBind, GameInput.BindingSet.Primary, true);
            string bindGlyph = uGUI.GetDisplayTextForBinding(bindName);
            return $"<color=#ADF8FFFF>{bindGlyph}</color>";
        }
        return TooltipFactory.stringButton2;
#endif
    }
}