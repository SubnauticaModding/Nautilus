namespace SMLHelper.V2.Patchers
{
    using System.Collections.Generic;
    using SMLHelper.V2.Utility;

    internal class BioReactorPatcher
    {
        internal static IDictionary<TechType, float> CustomBioreactorCharges = new SelfCheckingDictionary<TechType, float>("CustomBioreactorCharges", TechTypeExtensions.sTechTypeComparer);

        internal static void Patch()
        {
            // Direct access to private fields made possible by https://github.com/CabbageCrow/AssemblyPublicizer/
            // See README.md for details.
            PatchUtils.PatchDictionary(BaseBioReactor.charge, CustomBioreactorCharges);

            InternalLogger.Log("BaseBioReactorPatcher is done.", LogLevel.Debug);
        }
    }
}
