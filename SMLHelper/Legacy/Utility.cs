#pragma warning disable CS0618 // Type or member is obsolete
using UnityEngine;
using System.Collections.Generic;

namespace SMLHelper
{
    public class Utility
    {
        public readonly static Dictionary<CraftScheme, CraftTree.Type> CraftSchemeMap = new Dictionary<CraftScheme, CraftTree.Type>
        {
            { CraftScheme.Constructor, CraftTree.Type.Constructor },
            { CraftScheme.CyclopsFabricator, CraftTree.Type.CyclopsFabricator },
            { CraftScheme.Fabricator, CraftTree.Type.Fabricator },
            { CraftScheme.MapRoom, CraftTree.Type.MapRoom },
            { CraftScheme.SeamothUpgrades, CraftTree.Type.SeamothUpgrades },
            { CraftScheme.Workbench, CraftTree.Type.Workbench },
        };

        public static void AddBasicComponents(ref GameObject _object, string classId)
        {
            var rb = _object.AddComponent<Rigidbody>();
            _object.AddComponent<PrefabIdentifier>().ClassId = classId;
            _object.AddComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
            var rend = _object.GetComponentInChildren<Renderer>();
            rend.material.shader = Shader.Find("MarmosetUBER");
            var applier = _object.AddComponent<SkyApplier>();
            applier.renderers = new Renderer[] { rend };
            applier.anchorSky = Skies.Auto;
            var forces = _object.AddComponent<WorldForces>();
            forces.useRigidbody = rb;
        }

        public static string GetCurrentSaveDataDir()
        {
            return SaveLoadManager.temporarySavePath;
        }
    }
}
