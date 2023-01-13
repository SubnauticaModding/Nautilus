namespace SMLHelper.Assets;

using System;
using SMLHelper.Handlers;

public class PrefabInfo : IEquatable<PrefabInfo>
{
    public TechType TechType { get; set; }
    public string ClassID { get; set; }
    public string PrefabPath { get; set; }

    public static PrefabInfo WithTechType(string classId, string displayName, string description)
    {
        return new PrefabInfo
        {
            TechType = EnumHandler.AddEntry<TechType>(classId).WithPdaInfo(displayName, description),
            ClassID = classId,
            PrefabPath = classId + "Prefab"
        };
    }

    public PrefabInfo WithIcon(Atlas.Sprite sprite)
    {
        ModSprite.Add(SpriteManager.Group.None, TechType.ToString(), sprite);
        return this;
    }

    public PrefabInfo WithPrefabPath(string prefabPath)
    {
        PrefabPath = prefabPath;
        return this;
    }

    #region Equatability
    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is PrefabInfo prefabInfo && Equals(prefabInfo);
    }

    /// <inheritdoc/>
    public bool Equals(PrefabInfo other)
    {
        if(ReferenceEquals(null, other))
            return false;
        if(ReferenceEquals(this, other))
            return true;
        return TechType == other.TechType &&
               string.Equals(ClassID, other.ClassID, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(PrefabPath, other.PrefabPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)TechType;
            hashCode = (hashCode * 397) ^ (ClassID != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(ClassID) : 0);
            hashCode = (hashCode * 397) ^ (PrefabPath != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(PrefabPath) : 0);
            return hashCode;
        }
    }

    /// <summary>
    /// Indicates whether two <see cref="PrefabInfo"/> instances are equal.
    /// </summary>
    /// <param name="p1">The first instance to compare.</param>
    /// <param name="p2">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the <see cref="PrefabInfo"/> instances are equal; otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="operator!="/>
    /// <seealso cref="Equals(PrefabInfo)"/>
    public static bool operator ==(PrefabInfo p1, PrefabInfo p2) => p1!.Equals(p2);

    /// <summary>
    /// Indicates whether two <see cref="PrefabInfo"/> instances are not equal.
    /// </summary>
    /// <param name="p1">The first instance to compare.</param>
    /// <param name="p2">The second instance to compare.</param>
    /// <returns><see langword="true"/> if the <see cref="PrefabInfo"/> instances are not equal; otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="operator=="/>
    /// <seealso cref="Equals(PrefabInfo)"/>
    public static bool operator !=(PrefabInfo p1, PrefabInfo p2) => !(p1 == p2);

    #endregion
}