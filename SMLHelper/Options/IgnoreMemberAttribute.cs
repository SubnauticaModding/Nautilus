using System;

namespace SMLHelper.V2.Options
{
    /// <summary>
    /// Attribute used to signify the given property, field or method should be ignored when generating your mod options menu.
    /// </summary>
    /// <remarks>
    /// It is also possible to ignore any members that don't have any relevant attributes by setting the
    /// <see cref="MenuAttribute.IgnoreUnattributedMembers"/> property to <see langword="true"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// using SMLHelper.V2.Json;
    /// using SMLHelper.V2.Options;
    /// 
    /// [Menu("My Options Menu")]
    /// public class Config : ConfigFile
    /// {
    ///     [Label("My Cool Button)]
    ///     public static void MyCoolButton(object sender, ButtonClickedEventArgs e)
    ///     {
    ///         Logger.Log(Logger.Level.Info, "Button was clicked!");
    ///         Logger.Log(Logger.Level.Info, e.Id.ToString());
    ///     }
    ///     
    ///     [IgnoreMember]
    ///     public int FieldNotDisplayedInMenu;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="MenuAttribute"/>
    /// <seealso cref="LabelAttribute"/>
    /// <seealso cref="Json.ConfigFile"/>
    /// <seealso cref="MenuAttribute.IgnoreUnattributedMembers"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class IgnoreMemberAttribute : Attribute { }
}
