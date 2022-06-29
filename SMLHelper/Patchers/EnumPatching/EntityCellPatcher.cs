using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
namespace SMLHelper.V2.Patchers.EnumPatching
{
    internal class EntityCellPatcher
    {
        [HarmonyPatch(typeof(EntityCell),nameof(EntityCell.SerializeWaiterDataAsync))]
        internal static class EntityCell_SWDA_Patch
        {
            [HarmonyTranspiler]
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> list_)
            {
                var list = new List<CodeInstruction>(list_);
                for(var i = 0;i < list.Count;i++)
                {
                    if (i == 51)
                    {
                        list[i].operand = typeof(EntityCell_SWDA_Patch).GetMethod(nameof(EntityCell_SWDA_Patch.op_Implicit_Replace));
                    }
                }
                return list.AsEnumerable();
            }
            internal static bool op_Implicit_Replace(UnityEngine.Object obj)
            {
                if(obj is LargeWorldEntity)
                {
                    var obj_ = obj as LargeWorldEntity;
                    var block = LargeWorldStreamer.main.GetBlock(obj_.transform.position);
                    var key = block / LargeWorldStreamer.main.blocksPerBatch;
                    if(BiomeThings.Variables.Biomes.Exists(biome => biome.BatchIds.Contains(key)))
                    {
                        return false;
                    }
                }
                return obj != null;
            }
        }
    }
}
