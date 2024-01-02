using UnityEngine;

namespace Nautilus.Assets;

/// <summary>
/// Defines the spawn location with world position and optional euler angles. Used in the Coordinated Spawns system.
/// </summary>
/// <param name="Position">The world position.</param>
/// <param name="EulerAngles">Euler angles for the rotation the spawned object will appear with.</param>
public record SpawnLocation(Vector3 Position, Vector3 EulerAngles = default)
{
    /// <summary>
    /// The scale that the object is spawned at. If default (0, 0, 0) will be resolved to (1, 1, 1).
    /// </summary>
    public Vector3 Scale { get; init; }
    
    /// <summary>
    /// Defines the spawn location with world position and optional euler angles. Used in the Coordinated Spawns system.
    /// </summary>
    /// <param name="Position">The world position.</param>
    /// <param name="EulerAngles">Euler angles for the rotation the spawned object will appear with.</param>
    /// <param name="Scale">The scale that the object is spawned at. If default (0, 0, 0) will be resolved to (1, 1, 1).</param>
    public SpawnLocation(Vector3 Position, Vector3 EulerAngles, Vector3 Scale) : this(Position, EulerAngles)
    {
        this.Scale = Scale;
    }
}