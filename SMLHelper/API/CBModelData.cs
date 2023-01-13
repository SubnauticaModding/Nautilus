
using UnityEngine;

namespace SMLHelper.API;

public class CBModelData
{
    /// <summary>
    /// The custom skin for the item.<br/>
    /// This property is optional and will default to the standard texture for batteries or power cells.
    /// </summary>
    public Texture2D CustomTexture { get; set; }

    /// <summary>
    /// The custom bump texture for the item.<br/>
    /// This property is optional and will default to the standard bump texture for batteries or power cells.
    /// </summary>
    public Texture2D CustomNormalMap { get; set; }

    /// <summary>
    /// The custom Spec Texture for the item.<br/>
    /// This property is optional and will default to the standard spec texture for batteries or power cells.
    /// </summary>
    public Texture2D CustomSpecMap { get; set; }

    /// <summary>
    /// The custom lighting texture for the item.<br/>
    /// This property is optional and will default to the standard illum texture for batteries or power cells.
    /// </summary>
    public Texture2D CustomIllumMap { get; set; }

    /// <summary>
    /// The custom lighting strength for the item.<br/>
    /// This property is will default to 1.0f if the <see cref="CustomIllumMap"/> is set but will use the default value for batteries or power cells if no <see cref="CustomIllumMap"/> is set.
    /// </summary>
    public float CustomIllumStrength { get; set; } = 1.0f;

    /// <summary>
    /// Change this value if you want your item to use the Ion battery or powercell model as its base.
    /// </summary>
    public bool UseIonModelsAsBase { get; set; } = false;
}