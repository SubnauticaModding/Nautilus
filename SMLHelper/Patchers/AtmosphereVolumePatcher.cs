using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
namespace SMLHelper.V2.Patchers
{
    internal class AtmosphereVolumePatcher
    {
        [HarmonyPatch(typeof(AtmosphereVolume),nameof(AtmosphereVolume.Start))]
        internal static class AtmosphereVolume_Start_Patch
        {
            [HarmonyTranspiler]
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> list_)
            {
                var list = new List<CodeInstruction>(list_);
                for(var i = 0; i < list.Count;i++)
                {
                    if (list[i].Calls(typeof(Component).GetMethod(nameof(Component.GetComponent),Type.EmptyTypes).MakeGenericMethod(typeof(Collider))))
                    {
                        list[i].operand = typeof(AtmosphereVolume_Start_Patch).GetMethod(nameof(AtmosphereVolume_Start_Patch.GetComponentReplace),System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    }
                }
                return list.AsEnumerable();
            }
            internal static unsafe Collider GetComponentReplace(Component __instance)
            {
                var collider = __instance.GetComponent<Collider>();
                if(collider is TerrainCollider)
                {
                    if(__instance.gameObject.TryGetComponent<BoxCollider>(out var boxcollider))
                    {
                        collider = boxcollider;
                    }else if(__instance.TryGetComponent<SphereCollider>(out var spherecollider))
                    {
                        collider = spherecollider;
                    }else if(__instance.TryGetComponent<CapsuleCollider>(out var capsulecollider))
                    {
                        collider = capsulecollider;
                    }
                }
                return collider;
            }
                }
            }
        }
   
