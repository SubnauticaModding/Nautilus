using System;

namespace SMLHelper.V2.Options
{
    /// <summary>
    /// Attribute used to signify a method to run whenever the <see cref="UnityEngine.GameObject"/> for the <see cref="ModOption"/>
    /// corresponding to this member is created.
    /// </summary>
    /// <remarks>
    /// Can be specified multiple times to call multiple methods.
    /// <para>
    /// The specified method can optionally take the following parameters in any order:<br/>
    /// - <see cref="object"/> sender: The sender of the event<br/>
    /// - <see cref="Interfaces.IModOptionEventArgs"/> eventArgs: The generalized event arguments of the event<br/>
    /// - <see cref="GameObjectCreatedEventArgs"/> gameObjectCreatedEventArgs: The <see cref="GameObjectCreatedEventArgs"/>
    ///   for the event
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using SMLHelper.V2.Json;
    /// using SMLHelper.V2.Options;
    /// using QModManager.Utility;
    /// using UnityEngine;
    /// 
    /// [Menu("SMLHelper Example Mod")]
    /// public class Config : ConfigFile
    /// {
    ///     [Label("My checkbox"), OnGameObjectCreated(nameof(MyGameObjectCreatedEvent))]
    ///     public bool ToggleValue;
    /// 
    ///     private void MyGameObjectCreatedEvent(GameObjectCreatedEventArgs e)
    ///    {
    ///        Logger.Log(Logger.Level.Info, "GameObject was created");
    ///        Logger.Log(Logger.Level.Info, $"{e.Id}: {e.GameObject}");
    ///    }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="MenuAttribute"/>
    /// <seealso cref="LabelAttribute"/>
    /// <seealso cref="Interfaces.IModOptionEventArgs"/>
    /// <seealso cref="GameObjectCreatedEventArgs"/>
    /// <seealso cref="OnChangeAttribute"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class OnGameObjectCreatedAttribute : Attribute
    {
        /// <summary>
        /// The name of the method to run when the <see cref="UnityEngine.GameObject"/> for the <see cref="ModOption"/> corresponding
        /// to this member is created.
        /// The method must be a member of the same class.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Signifies a method to run whenever the <see cref="UnityEngine.GameObject"/> for the <see cref="ModOption"/> corresponding
        /// to this member is created.
        /// The method must be a member of the same class.
        /// </summary>
        /// <param name="methodName">The name of the method within the same class to run.</param>
        public OnGameObjectCreatedAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
