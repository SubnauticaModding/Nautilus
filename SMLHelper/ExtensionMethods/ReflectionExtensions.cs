namespace SMLHelper.V2.ExtensionMethods
{
    using QModManager.API;
    using System.Reflection;

    internal static class ReflectionExtensions
    {
        internal static IQMod GetQMod(this MemberInfo memberInfo)
            => QModServices.Main.GetMod(memberInfo.DeclaringType.Assembly);
    }
}
