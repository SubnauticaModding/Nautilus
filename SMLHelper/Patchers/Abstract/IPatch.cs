namespace SMLHelper.V2.Patchers.Abstract
{
    using Harmony;

    internal interface IPatch
    {
        void Patch(HarmonyInstance harmony);
    }
}
