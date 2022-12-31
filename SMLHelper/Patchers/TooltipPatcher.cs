namespace SMLHelper.Patchers
{
    using HarmonyLib;
    using SMLHelper.Handlers;
    using SMLHelper.Patchers.EnumPatching;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;
    using SMLHelper.Utility;

    internal class TooltipPatcher
    {
        internal static bool DisableEnumIsDefinedPatch = false;
        private static List<TechType> vanillaTechTypes = new();

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

            if (IsVanillaTechType(techType))
#if SUBNAUTICA
                WriteModName(sb, "Subnautica");
#elif BELOWZERO
                WriteModName(sb, "BelowZero");
#endif
            else if (TechTypePatcher.cacheManager.ContainsKey(techType))
            {
                WriteModNameFromTechType(sb, techType);
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
        internal static void WriteModNameFromTechType(StringBuilder sb, TechType type)
        {
            // if (MissingTechTypes.Contains(type)) WriteModNameError(sb, "Mod Missing");
            // This is for something else I am going to do

            if (TechTypeHandler.TechTypesAddedBy.TryGetValue(type, out Assembly assembly))
            {
                string modName = assembly.GetName().Name;

                if (string.IsNullOrEmpty(modName))
                {
                    WriteModNameError(sb, "Unknown Mod", "Mod could not be determined");
                }
                else
                {
                    WriteModName(sb, modName);
                }
            }
            else
            {
                WriteModNameError(sb, "Unknown Mod", "Assembly could not be determined");
            }
        }
        internal static void WriteSpace(StringBuilder sb)
        {
            sb.AppendFormat("\n<size=19></size>");
        }

        internal static bool IsVanillaTechType(TechType type)
        {
            if (vanillaTechTypes is {Count: 0})
            {
                List<TechType> allTechTypes = (System.Enum.GetValues(typeof(TechType)) as TechType[])!.ToList();
                allTechTypes.RemoveAll(tt => TechTypePatcher.cacheManager.ModdedKeys.Contains(tt));
                vanillaTechTypes = allTechTypes;
            }

            return vanillaTechTypes.Contains(type);
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

            string configPath = Path.Combine(Path.Combine(BepInEx.Paths.ConfigPath, Assembly.GetExecutingAssembly().GetName().Name), "ExtraItemInfo.txt");

            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, "Mod name (default)");
                ExtraItemInfoOption = ExtraItemInfo.ModName;

                return;
            }

            string fileContents = File.ReadAllText(configPath);

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
                    InternalLogger.Log("Error reading ExtraItemInfo.txt configuration file. Defaulted to mod name.", LogLevel.Warn);
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
