namespace SMLHelper.V2.Patchers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;

    internal class KnownTechPatcher
    {
        private static readonly Func<TechType, string> AsStringFunction = (t) => t.AsString();

        internal static List<TechType> UnlockedAtStart = new List<TechType>();
        internal static IDictionary<TechType, KnownTech.AnalysisTech> AnalysisTech = new SelfCheckingDictionary<TechType, KnownTech.AnalysisTech>("AnalysisTech", AsStringFunction);
        internal static IDictionary<TechType, KnownTech.CompoundTech> CompoundTech = new SelfCheckingDictionary<TechType, KnownTech.CompoundTech>("CompoundTech", AsStringFunction);
        internal static IDictionary<TechType, List<TechType>> RemoveFromSpecificTechs = new SelfCheckingDictionary<TechType, List<TechType>>("RemoveFromSpecificTechs", AsStringFunction);
        internal static List<TechType> RemovalTechs = new List<TechType>();

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
                    KnownTech.Add(techType, false);
            }

            List<KnownTech.AnalysisTech> analysisTech = KnownTech.analysisTech;
            IEnumerable<KnownTech.AnalysisTech> techToAdd = AnalysisTech.Values.Where(a => !analysisTech.Any(a2 => a.techType == a2.techType));

            foreach (KnownTech.AnalysisTech tech in analysisTech)
            { 
                foreach(TechType techType in RemovalTechs)
                {
                    if(tech.unlockTechTypes.Contains(techType))
                        tech.unlockTechTypes.Remove(techType);
                }

                if(RemoveFromSpecificTechs.TryGetValue(tech.techType, out var types))
                {
                    foreach(TechType type in types)
                        if(tech.unlockTechTypes.Contains(type))
                            tech.unlockTechTypes.Remove(type);
                }
            }
            
            foreach (KnownTech.AnalysisTech tech in analysisTech)
            {
                if (UnlockSound == null && tech.unlockSound != null && tech.techType == TechType.Lead)
                    UnlockSound = tech.unlockSound;

                foreach (KnownTech.AnalysisTech customTech in AnalysisTech.Values)
                {
                    if (tech.techType == customTech.techType)
                    {
                        if (customTech.unlockTechTypes != null)
                            tech.unlockTechTypes.AddRange(customTech.unlockTechTypes.Where((x)=> !tech.unlockTechTypes.Contains(x)));

                        if (customTech.unlockSound != null)
                            tech.unlockSound = customTech.unlockSound;

                        if (customTech.unlockPopup != null)
                            tech.unlockPopup = customTech.unlockPopup;

                        if (customTech.unlockMessage != string.Empty)
                            tech.unlockMessage = customTech.unlockMessage;
                    }
                }
            }

            foreach (KnownTech.AnalysisTech tech in techToAdd)
            {
                if (tech == null)
                    continue;

                if (tech.unlockSound == null)
                    tech.unlockSound = UnlockSound;

                if (!KnownTech.Contains(tech.techType))
                    analysisTech.Add(tech);
            }


            List<KnownTech.CompoundTech> compoundTech = KnownTech.compoundTech;
            IEnumerable<KnownTech.CompoundTech> compoundTechToAdd = CompoundTech.Values.Where(a => !compoundTech.Any(a2 => a.techType == a2.techType));

            foreach (KnownTech.CompoundTech tech in compoundTech)
            {
                foreach (KnownTech.CompoundTech customTech in CompoundTech.Values)
                {
                    tech.dependencies = customTech.dependencies;
                }
            }

            foreach (KnownTech.CompoundTech tech in compoundTechToAdd)
            {
                if (tech == null)
                    continue;

                if (!KnownTech.Contains(tech.techType))
                    compoundTech.Add(tech);
            }
        }
    }
}
