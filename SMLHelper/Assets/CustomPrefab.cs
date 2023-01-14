namespace SMLHelper.Assets;

using System;
using System.Collections;
using System.Collections.Generic;
using SMLHelper.Crafting;
using SMLHelper.DependencyInjection;
using UnityEngine;
using UWE;

#if SUBNAUTICA
using RecipeData = Crafting.TechData;
#endif


public abstract class CustomPrefab: ModPrefabRoot, IEquatable<CustomPrefab>
{
    public List<Spawnable.SpawnLocation> CoordinatedSpawns { get; set; }
    public List<LootDistributionData.BiomeData> BiomesToSpawnIn { get; set; }
    public WorldEntityInfo WorldEntityInfo { get; set; }

    public RecipeData Recipe { get; set; }

    public override PrefabInfo PrefabInfo { get; protected set; }

    #region Equatable
    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is CustomPrefab customPrefab && PrefabInfo.Equals(customPrefab.PrefabInfo);
    }

    /// <inheritdoc/>
    public bool Equals(CustomPrefab other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return PrefabInfo.Equals(other.PrefabInfo);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return PrefabInfo.GetHashCode();
        }
    }

    /// <summary>
    /// Indicates whether two <see cref="CustomPrefab"/> instances are equal.
    /// </summary>
    /// <param name="c1">The first instance to compare.</param>
    /// <param name="c2">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the <see cref="CustomPrefab"/> instances are equal; otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="operator!="/>
    /// <seealso cref="Equals(CustomPrefab)"/>
    public static bool operator ==(CustomPrefab c1, CustomPrefab c2) => c1!.Equals(c2);
    
    /// <summary>
    /// Indicates whether two <see cref="CustomPrefab"/> instances are equal.
    /// </summary>
    /// <param name="c1">The first instance to compare.</param>
    /// <param name="c2">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the <see cref="CustomPrefab"/> instances are equal; otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="operator!="/>
    /// <seealso cref="Equals(CustomPrefab)"/>
    public static bool operator !=(CustomPrefab c1, CustomPrefab c2) => !(c1 == c2);
    #endregion
}