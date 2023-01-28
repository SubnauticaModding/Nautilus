using System;
using System.Collections;
using UnityEngine;

namespace SMLHelper.Assets.PrefabTemplates;

/// <summary>
/// Represents an energy source template. This template is capable of returning a Battery or a Power Cell.
/// </summary>
public class EnergySourceTemplate : PrefabTemplate
{
    /// <summary>
    /// Is this energy source a Power Cell?
    /// </summary>
    public bool IsPowerCell { get; set; }

    public bool UseIonModelAsBase { get; set; }
    
    /// <summary>
    /// Skin data for this energy source.<br/>
    /// This property is optional and will default to the standard model of the energy source.
    /// </summary>
    public CustomModelData ModelData { get; set; }

    private readonly int _energyAmount;

    /// <summary>
    /// Creates an <see cref="EnergySourceTemplate"/> instance.
    /// </summary>
    /// <param name="energyAmount">The amount of energy this source should have.</param>
    public EnergySourceTemplate(PrefabInfo info, int energyAmount) : base(info)
    {
        _energyAmount = energyAmount;
    }
    
    /// <summary>
    /// Gets the appropriate energy source prefab.
    /// </summary>
    /// <param name="gameObject">The energy source prefab is set into this argument.<br/>
    /// If the provided task result already has a game object, it will try to set the necessary components first.
    /// Otherwise; sets the standard Battery or Power Cell.</param>
    /// <returns>A coroutine operation. Must be used with either <c>yield return</c>, or <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/>.</returns>
    public override IEnumerator GetPrefabAsync(TaskResult<GameObject> gameObject)
    {
        var obj = gameObject.Get();
        if (obj)
        {
            ModifyPrefab(obj);
            yield break;
        }

        yield return CreateEnergySource(gameObject);
    }

    private IEnumerator CreateEnergySource(IOut<GameObject> gameObject)
    {
        var tt = GetReferenceType();
        var task = CraftData.GetPrefabForTechTypeAsync(tt, false);
        yield return task;

        var obj = task.GetResult();
        
        if (ModelData != null)
        {
            foreach (var renderer in obj.GetAllComponentsInChildren<Renderer>())
            {
                if (ModelData.CustomTexture != null)
                    renderer.material.SetTexture(ShaderPropertyID._MainTex, ModelData.CustomTexture);

                if (ModelData.CustomNormalMap != null)
                    renderer.material.SetTexture(ShaderPropertyID._BumpMap, ModelData.CustomNormalMap);

                if (ModelData.CustomSpecMap != null)
                    renderer.material.SetTexture(ShaderPropertyID._SpecTex, ModelData.CustomSpecMap);

                if (ModelData.CustomIllumMap != null)
                {
                    renderer.material.SetTexture(ShaderPropertyID._Illum, ModelData.CustomIllumMap);
                    renderer.material.SetFloat(ShaderPropertyID._GlowStrength, ModelData.CustomIllumStrength);
                    renderer.material.SetFloat(ShaderPropertyID._GlowStrengthNight, ModelData.CustomIllumStrength);
                }
            }
        }
        
        ModifyPrefab(obj);
        
        gameObject.Set(obj);
    }

    private TechType GetReferenceType()
    {
        var modelData = ModelData ?? new CustomModelData();
        return IsPowerCell switch
        {
            false when !UseIonModelAsBase => TechType.Battery,
            true when !UseIonModelAsBase => TechType.PowerCell,
            false when UseIonModelAsBase => TechType.PrecursorIonBattery,
            true when UseIonModelAsBase => TechType.PrecursorIonPowerCell,
            _ => throw new NotSupportedException()
        };
    }

    private void ModifyPrefab(GameObject obj)
    {
        var battery = obj.EnsureComponent<Battery>();
        battery._capacity = _energyAmount;
    }
}