namespace SMLHelper.API;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Assets;
using System.Linq;
using static BepInEx.Bootstrap.Chainloader;
using BepInEx;
using SMLHelper.Utility;

internal static class CbDatabase
{
    public const string BatteryCraftTab = "BatteryTab";
    public const string PowCellCraftTab = "PowCellTab";
    public const string ElecCraftTab = "Electronics";
    public const string ResCraftTab = "Resources";

    public static readonly string[] BatteryCraftPath = new[] { ResCraftTab, BatteryCraftTab };
    public static readonly string[] PowCellCraftPath = new[] { ResCraftTab, PowCellCraftTab };

    public static string ExecutingFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public static List<CbCore> BatteryItems { get; } = new List<CbCore>();
    public static List<CbCore> PowerCellItems { get; } = new List<CbCore>();

    public static Dictionary<TechType, CBModelData> BatteryModels { get; } = new Dictionary<TechType, CBModelData>();
    public static Dictionary<TechType, CBModelData> PowerCellModels { get; } = new Dictionary<TechType, CBModelData>();

    public static HashSet<TechType> TrackItems { get; } = new HashSet<TechType>();

    private static bool _decoModDetectionRun = false;
    private static FieldInfo enablePlaceBatteriesField { get; set; }

    public static bool PlaceBatteriesFeatureEnabled
    {
        get
        {
            if(!_decoModDetectionRun)
            {
                DecorationsModCheck();
            }

            return enablePlaceBatteriesField != null ? (bool)enablePlaceBatteriesField.GetValue(null) : false;
        }
    }

    private static void DecorationsModCheck()
    {
        PluginInfo puginInfo = PluginInfos.Values.Where((x) => x.Metadata.Name == "DecorationsMod" && x.Instance.enabled).FirstOrFallback(null);
        Assembly decorationsModAssembly = null;
        if(puginInfo == null)
        {
            _decoModDetectionRun = true;
            InternalLogger.Debug($"DecorationsMod not detected.");
            return;
        }

        decorationsModAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.Location == puginInfo.Location).FirstOrFallback(decorationsModAssembly);
        if(decorationsModAssembly == null)
        {
            InternalLogger.Debug($"DecorationsMod detected but unable to find assembly.");
            _decoModDetectionRun = true;
            return;
        }

        Type decorationsModConfig = decorationsModAssembly.GetType("DecorationsMod.ConfigSwitcher", false);
        if(decorationsModConfig == null)
        {
            InternalLogger.Debug($"DecorationsMod assembly found but unable to find DecorationsMod.ConfigSwitcher Type.");
            _decoModDetectionRun = true;
            return;
        }

        enablePlaceBatteriesField = decorationsModConfig.GetField("EnablePlaceBatteries", BindingFlags.Public | BindingFlags.Static);
        if(enablePlaceBatteriesField == null || !enablePlaceBatteriesField.IsStatic)
        {
            InternalLogger.Debug($"DecorationsMod.ConfigSwitcher Type found but unable to find Static EnablePlaceBatteries Field.");
            enablePlaceBatteriesField = null;
            _decoModDetectionRun = true;
            return;
        }
        _decoModDetectionRun = true;
    }

    private static GameObject _precursorionbattery;
    private static GameObject _precursorionpowercell;
    private static GameObject _battery;
    private static GameObject _powercell;

    public static GameObject IonBattery()
    {
        return _precursorionbattery ??= Resources.Load<GameObject>("worldentities/tools/precursorionbattery");
    }

    public static GameObject IonPowerCell()
    {
        return _precursorionpowercell ??= Resources.Load<GameObject>("worldentities/tools/precursorionpowercell");
    }

    public static GameObject Battery()
    {
        return _battery ??= Resources.Load<GameObject>("worldentities/tools/battery");
    }

    public static GameObject PowerCell()
    {
        return _powercell ??= Resources.Load<GameObject>("worldentities/tools/powercell");
    }
}