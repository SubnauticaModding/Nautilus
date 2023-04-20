using UnityEngine;

namespace Nautilus.Assets;

/// <summary>
/// Custom Model data that Nautilus will use to apply custom looks to certain prefabs.
/// </summary>
public class CustomModelData
{
    //TODO: Expand on this to include keyword settings and other settings for Renderers/Shaders/Materials


    /// <summary>
    /// Name of the model to target with these changes.
    /// </summary>
    public string TargetPath { get; init; } = "";

    /// <summary>
    /// The custom skin for the item.<br/>
    /// This property is optional and will default to the standard texture for batteries or power cells.
    /// </summary>
    public Texture2D CustomTexture { get; init; }

    /// <summary>
    /// The custom bump texture for the item.<br/>
    /// This property is optional and will default to the standard bump texture for batteries or power cells.
    /// </summary>
    public Texture2D CustomNormalMap { get; init; }

    /// <summary>
    /// The custom Spec Texture for the item.<br/>
    /// This property is optional and will default to the standard spec texture for batteries or power cells.
    /// </summary>
    public Texture2D CustomSpecMap { get; init; }

    /// <summary>
    /// The custom lighting texture for the item.<br/>
    /// This property is optional and will default to the standard illum texture for batteries or power cells.
    /// </summary>
    public Texture2D CustomIllumMap { get; init; }

    /// <summary>
    /// The custom lighting strength for the item.<br/>
    /// This property is will default to 1.0f if the <see cref="CustomIllumMap"/> is set but will use the default value for batteries or power cells if no <see cref="CustomIllumMap"/> is set.
    /// </summary>
    public float CustomIllumStrength { get; init; } = 1.0f;

    /// <summary>
    /// A class with some common target names
    /// </summary>
    public static class Targets
    {
        //TODO: ADD MORE Targets.... Maybe a dictionary to get target list based on techtype?
#pragma warning disable 1591
        #region Buildables

        public static string Fabricator = "submarine_fabricator_01/fabricator_01";
        public static string Radio = "Mesh";
        public static string MedicalCabinetDoor = "Door";
        public static string MedicalCabinetBase = "Base";
        public static string MedicalCabinetMedKit = "MedKit/Mesh";

        #endregion

        #region Craftables

        public static string Battery = "battery_01";
        public static string IonBattery = "battery_ion";

        public static string Tank = "model/Oxygen_tank";
        public static string DoubleTank = "model/High_Capacity_tank";


        public static string Knife = "knife_01";
        public static string Welder = "welder_scaled/welder/welder_geos";
        public static string LaserCutter = "Laser_Cutter/geos/Laser_cutter_geo";

        public static string Gravsphere = "gravSphere_anim/Gravsphere";
        public static string Gravsphere_tower_01 = "gravSphere_anim/Gravsphere/Gravsphere_tower_01";
        public static string Gravsphere_tower_02 = "gravSphere_anim/Gravsphere/Gravsphere_tower_02";
        public static string Gravsphere_tower_03 = "gravSphere_anim/Gravsphere/Gravsphere_tower_03";
        public static string Gravsphere_tower_04 = "gravSphere_anim/Gravsphere/Gravsphere_tower_04";
        public static string Gravsphere_tower_05 = "gravSphere_anim/Gravsphere/Gravsphere_tower_05";

        public static string Beacon = "model/beacon/beacon_geo1";
        public static string BeaconFP = "model_FP/beacon_fp/beacon_geo1";

        #endregion
#pragma warning restore
    }
}