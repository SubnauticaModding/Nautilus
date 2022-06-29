using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Reflection;
namespace SMLHelper.V2.Patchers
{
    internal class PlayerPatcher
    {
        internal static bool FloatingOriginEnabled = false;
        private static bool isInit = false;
        private static readonly string configpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "FloatingOriginConfig.txt");
        internal static void SetFloatingOriginEnabled(bool value)
        {
            File.WriteAllText(configpath, value.ToString());
            FloatingOriginEnabled = value;
        }
        internal static void InitFloatingOriginEnabled()
        {
            if(isInit is true)
            {
                return;
            }
            isInit = true;
            if (!File.Exists(configpath))
            {
                File.Create(configpath).Dispose();
                File.WriteAllText(configpath, FloatingOriginEnabled.ToString());
            }
            if (FloatingOriginEnabled is false)
            { 
                try
                {
                    var contents = File.ReadAllText(configpath);
                    FloatingOriginEnabled = bool.Parse(contents);
                } catch(Exception)
                {
                    FloatingOriginEnabled = false;
                    File.WriteAllText(configpath, false.ToString());
                    Logger.Log("FloatingOriginEnabled couldn't be read from file, defaulting to False.");
                }
            }
        }
            [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
            [PatchUtils.Postfix]
            internal static void Player_Awake_Postfix(Player __instance)
            {
            if (FloatingOriginEnabled)
            {
                __instance.gameObject.EnsureComponent<FloatingOrigin>().ReferenceObject = __instance.transform;
            }
            }
            [HarmonyPatch(typeof(Player), nameof(Player.SetPosition), new Type[] { typeof(Vector3) })]
            [PatchUtils.Prefix]
            internal static bool Player_SetPosition_Prefix(ref Vector3 wsPos)
            {
                wsPos -= FloatingOrigin.CurrentOffset;
                return true;
            }
        internal static void Patch(Harmony h)
        {
            PatchUtils.PatchClass(h);
            Logger.Log("Patched Player");
        }
        
    }
}
