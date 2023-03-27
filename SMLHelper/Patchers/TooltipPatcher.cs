namespace SMLHelper.Patchers
{
    using HarmonyLib;
    using Handlers;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;
    using Utility;
    using BepInEx.Logging;
    using System;

    internal class TooltipPatcher
    {
        internal static bool DisableEnumIsDefinedPatch = false;

        internal static void Patch(Harmony harmony)
        {
            Initialize();

            MethodInfo buildTech = AccessTools.Method(typeof(TooltipFactory), nameof(TooltipFactory.BuildTech));
            MethodInfo itemCommons = AccessTools.Method(typeof(TooltipFactory), nameof(TooltipFactory.ItemCommons));
            MethodInfo recipe = AccessTools.Method(typeof(TooltipFactory), nameof(TooltipFactory.CraftRecipe));
            HarmonyMethod customTooltip = new(AccessTools.Method(typeof(TooltipPatcher), nameof(TooltipPatcher.CustomTooltip)));
            HarmonyMethod techTypePostfix = new(AccessTools.Method(typeof(TooltipPatcher), nameof(TooltipPatcher.TechTypePostfix)));

            harmony.Patch(itemCommons, postfix: customTooltip);
            harmony.Patch(recipe, postfix: techTypePostfix);
            harmony.Patch(buildTech, postfix: techTypePostfix);

            InternalLogger.Log("TooltipPatcher is done.", LogLevel.Debug);
        }

        internal static void CustomTooltip(StringBuilder sb, TechType techType)
        {
            if (ExtraItemInfoOption == ExtraItemInfo.Nothing)
            {
                return;
            }

            if (ExtraItemInfoOption == ExtraItemInfo.ModNameAndItemID)
            {
                WriteTechType(sb, techType);
            }
            else
            {
                WriteSpace(sb);
            }

            if (techType.IsDefinedByDefault())
#if SUBNAUTICA
                WriteModName(sb, "Subnautica");
#elif BELOWZERO
                WriteModName(sb, "BelowZero");
#endif
            else if (EnumHandler.TryGetOwnerAssembly(techType, out Assembly assembly))
            {
                WriteModNameFromAssembly(sb, assembly);
            }
            else
            {
                WriteModNameError(sb, "Unknown Mod", "Item added without SMLHelper");
            }
        }
        
        internal static void WriteTechType(StringBuilder sb, TechType techType)
        {
            sb.AppendFormat("\n\n<size=19><color=#808080FF>{0} ({1})</color></size>", techType.AsString(), (int)techType);
        }
        internal static void WriteModName(StringBuilder sb, string text)
        {
            sb.AppendFormat("\n<size=23><color=#00ffffff>{0}</color></size>", text);
        }
        internal static void WriteModNameError(StringBuilder sb, string text, string reason)
        {
            sb.AppendFormat("\n<size=23><color=#ff0000ff>{0}</color></size>\n<size=17><color=#808080FF>({1})</color></size>", text, reason);
        }
        internal static void WriteModNameFromAssembly(StringBuilder sb, Assembly assembly)
        {
            string modName = assembly.GetName().Name;

            if(string.IsNullOrEmpty(modName))
            {
                WriteModNameError(sb, "Unknown Mod", "Mod could not be determined");
            }
            else
            {
                WriteModName(sb, modName);
            }
        }
        internal static void WriteSpace(StringBuilder sb)
        {
            sb.AppendFormat("\n<size=19></size>");
        }

#region Options

        internal enum ExtraItemInfo
        {
            ModName,
            ModNameAndItemID,
            Nothing
        }

        internal static ExtraItemInfo ExtraItemInfoOption { get; private set; }

        internal static void SetExtraItemInfo(ExtraItemInfo value)
        {
            string configPath = Path.Combine(Path.Combine(BepInEx.Paths.ConfigPath, Assembly.GetExecutingAssembly().GetName().Name), "ExtraItemInfo.txt");

            string text;
            switch (value)
            {
                case ExtraItemInfo.ModName:
                    text = "Mod name (default)";
                    break;
                case ExtraItemInfo.ModNameAndItemID:
                    text = "Mod name and item ID";
                    break;
                case ExtraItemInfo.Nothing:
                    text = "Nothing";
                    break;
                default:
                    return;
            }

            File.WriteAllText(configPath, text);
            ExtraItemInfoOption = value;
        }

        internal static bool Initialized = false;

        internal static void Initialize()
        {
            if (Initialized)
            {
                return;
            }

            Initialized = true;
            
            var smlFolder = Path.Combine(BepInEx.Paths.ConfigPath, Assembly.GetExecutingAssembly().GetName().Name);
            Directory.CreateDirectory(smlFolder);

            var configPath = Path.Combine(smlFolder, "ExtraItemInfo.txt");

            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, "Mod name (default)");
                ExtraItemInfoOption = ExtraItemInfo.ModName;

                return;
            }

            var fileContents = File.ReadAllText(configPath);

            switch (fileContents)
            {
                case "Mod name (default)":
                    ExtraItemInfoOption = ExtraItemInfo.ModName;
                    InternalLogger.Log($"Extra item info set to: {fileContents}", LogLevel.Info);
                    break;
                case "Mod name and item ID":
                    ExtraItemInfoOption = ExtraItemInfo.ModNameAndItemID;
                    InternalLogger.Log($"Extra item info set to: {fileContents}", LogLevel.Info);
                    break;
                case "Nothing":
                    ExtraItemInfoOption = ExtraItemInfo.Nothing;
                    InternalLogger.Log($"Extra item info set to: {fileContents}", LogLevel.Info);
                    break;
                default:
                    File.WriteAllText(configPath, "Mod name (default)");
                    ExtraItemInfoOption = ExtraItemInfo.ModName;
                    InternalLogger.Log("Error reading ExtraItemInfo.txt configuration file. Defaulted to mod name.", LogLevel.Warning);
                    break;
            }
        }

        #endregion

        #region Patches

        internal static void TechTypePostfix(TechType techType, TooltipData data)
        {
            CustomTooltip(data.prefix, techType);
        }
#endregion
    }
}
