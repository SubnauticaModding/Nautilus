namespace SMLHelper.Assets
{
    using UnityEngine;

    /// <summary>
    /// A lightweight class used to specify the position of a Coordinated Spawn and optionally set its rotation.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="eulerAngles"></param>
    public record SpawnLocation(Vector3 position, Vector3 eulerAngles = default);
}
