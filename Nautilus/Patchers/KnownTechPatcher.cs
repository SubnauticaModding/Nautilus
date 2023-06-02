using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Nautilus.Utility;

namespace Nautilus.Patchers;

internal class KnownTechPatcher
{
    private static readonly Func<TechType, string> AsStringFunction = (t) => t.AsString();

    internal static List<TechType> UnlockedAtStart = new();
    internal static IDictionary<TechType, KnownTech.AnalysisTech> AnalysisTech = new SelfCheckingDictionary<TechType, KnownTech.AnalysisTech>("AnalysisTech", AsStringFunction);
    internal static IDictionary<TechType, KnownTech.CompoundTech> CompoundTech = new SelfCheckingDictionary<TechType, KnownTech.CompoundTech>("CompoundTech", AsStringFunction);
    internal static IDictionary<TechType, List<TechType>> RemoveFromSpecificTechs = new SelfCheckingDictionary<TechType, List<TechType>>("RemoveFromSpecificTechs", AsStringFunction);
    internal static List<TechType> RemovalTechs = new();

    private static FMODAsset UnlockSound;

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(KnownTech), nameof(KnownTech.Initialize)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(KnownTechPatcher), nameof(KnownTechPatcher.InitializePostfix))));
    }

    internal static void InitializePostfix()
    {
        foreach(TechType techType in UnlockedAtStart)
        {
            if (!KnownTech.Contains(techType))
            {
                KnownTech.Add(techType, false);
            }
        }

        List<KnownTech.AnalysisTech> analysisTech = KnownTech.analysisTech;
        IEnumerable<KnownTech.AnalysisTech> techToAdd = AnalysisTech.Values.Where(a => !analysisTech.Any(a2 => a.techType == a2.techType));

        foreach (KnownTech.AnalysisTech tech in analysisTech)
        { 
            foreach(TechType techType in RemovalTechs)
            {
                if(tech.unlockTechTypes.Contains(techType))
                {
                    tech.unlockTechTypes.Remove(techType);
                }
            }

            if(RemoveFromSpecificTechs.TryGetValue(tech.techType, out List<TechType> types))
            {
                foreach(TechType type in types)
                {
                    if(tech.unlockTechTypes.Contains(type))
                    {
                        tech.unlockTechTypes.Remove(type);
                    }
                }
            }
        }
            
        foreach (KnownTech.AnalysisTech tech in analysisTech)
        {
            if (UnlockSound == null && tech.unlockSound != null && tech.techType == TechType.CreepvinePiece)
            {
                UnlockSound = tech.unlockSound;
            }

            foreach (KnownTech.AnalysisTech customTech in AnalysisTech.Values)
            {
                if (tech.techType == customTech.techType)
                {
                    if (customTech.unlockTechTypes != null)
                    {
                        tech.unlockTechTypes.AddRange(customTech.unlockTechTypes.Where((x)=> !tech.unlockTechTypes.Contains(x)));
                    }

                    if (customTech.unlockSound != null)
                    {
                        tech.unlockSound = customTech.unlockSound;
                    }

                    if (customTech.unlockPopup != null)
                    {
                        tech.unlockPopup = customTech.unlockPopup;
                    }

                    if (customTech.unlockMessage != string.Empty)
                    {
                        tech.unlockMessage = customTech.unlockMessage;
                    }
                }
            }
        }

        List<KnownTech.AnalysisTech> validatedTechsToAdd = KnownTech.ValidateAnalysisTech(new(techToAdd));
        foreach (KnownTech.AnalysisTech tech in validatedTechsToAdd)
        {
            if (tech == null)
            {
                continue;
            }

            if (tech.unlockSound == null)
            {
                tech.unlockSound = UnlockSound;
            }

            if (!KnownTech.Contains(tech.techType))
            {
                analysisTech.Add(tech);
            }
        }

        List <KnownTech.CompoundTech> validatedCompoundTeches = KnownTech.ValidateCompoundTech(new(CompoundTech.Values));
        foreach (KnownTech.CompoundTech customTech in validatedCompoundTeches)
        {
            if (customTech == null) // Safety check
            {
                continue;
            }

            // Only add the new compound tech if it isn't unlocked yet 
            if (!KnownTech.Contains(customTech.techType))
            {
                KnownTech.compoundTech.Add(customTech);
            }

            // If a compound tech already exists, set the dependencies correctly.
            KnownTech.CompoundTech foundTech = KnownTech.compoundTech.Find(tech => tech.techType == customTech.techType);
            if (foundTech != null)
            {
                foundTech.dependencies = customTech.dependencies;
            }
        }
    }
}