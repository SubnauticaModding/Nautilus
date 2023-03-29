using UnityEngine;

namespace SMLHelper.Assets;

/// <summary>
/// Defines the spawn location with world position and optional euler angles. Used in the Coordinated Spawns system.
/// </summary>
/// <param name="Position">The world position.</param>
/// <param name="EulerAngles">Euler angles for the rotation the spawned object will appear with.</param>
public record SpawnLocation(Vector3 Position, Vector3 EulerAngles = default);