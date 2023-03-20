using UnityEngine;

namespace SMLHelper.Assets;

/// <summary>
/// Returns the list of <see cref="SpawnLocation"/>s that specify the prefab's Coordinated Spawns.<br/>
/// By default this will be null.
/// </summary>
/// <param name="Position">The world position.</param>
/// <param name="EulerAngles">Euler angles for the rotation the spawned object will appear with.</param>
public record SpawnLocation(Vector3 Position, Vector3 EulerAngles = default);