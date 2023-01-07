namespace SMLHelper.Utility
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A small collection of prefab related utilities.
    /// </summary>
    public static class PrefabUtils
    {
        /// <summary>
        /// Adds and configures the following components on the gameobject passed by reference:<para/>
        /// - <see cref="Rigidbody"/>
        /// - <see cref="LargeWorldEntity"/>
        /// - <see cref="Renderer"/>
        /// - <see cref="SkyApplier"/>
        /// - <see cref="WorldForces"/>
        /// </summary>
        /// <param name="_object"></param>
        /// <param name="classId"></param>
        public static void AddBasicComponents(ref GameObject _object, string classId)
        {
            Rigidbody rb = _object.AddComponent<Rigidbody>();
            _object.AddComponent<PrefabIdentifier>().ClassId = classId;
            _object.AddComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
            Renderer rend = _object.GetComponentInChildren<Renderer>();
            rend.material.shader = Shader.Find("MarmosetUBER");
            SkyApplier applier = _object.AddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { rend };
            applier.anchorSky = Skies.Auto;
            WorldForces forces = _object.AddComponent<WorldForces>();
            forces.useRigidbody = rb;
        }
    }
}
