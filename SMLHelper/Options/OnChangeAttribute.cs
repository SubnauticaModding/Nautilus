namespace SMLHelper.V2.Options
{
    using Interfaces;
    using Json;
    using System;

    /// <summary>
    /// Attribute used to signify a method to run whenever the member this attribute is applied to changes.
    /// The method must be a member of the same class.
    /// </summary>
    /// <remarks>
    /// Can be specified mutliple times to call multiple methods.
    /// <para>
    /// The specified method can take the following parameters in any order:<br/>
    /// - <see cref="object"/> sender: The sender of the event<br/>
    /// - <see cref="IModOptionEventArgs"/> eventArgs: The generalized event arguments of the event<br/>
    /// - <see cref="ChoiceChangedEventArgs"/> choiceChangedEventArgs: Only when the member corresponds to a
    ///   <see cref="ModChoiceOption"/><br/>
    /// - <see cref="KeybindChangedEventArgs"/> keybindChangedEventArgs: Only when the member correspends to a
    ///   <see cref="ModKeybindOption"/><br/>
    /// - <see cref="SliderChangedEventArgs"/> sliderChangedEventArgs: Only when the member corresponds to a
    ///   <see cref="ModSliderOption"/><br/>
    /// - <see cref="ToggleChangedEventArgs"/> toggleChangedEventArgs: Only when the member corresponds to a
    ///   <see cref="ModToggleOption"/>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using SMLHelper.V2.Interfaces;
    /// using SMLHelper.V2.Json;
    /// using SMLHelper.V2.Options;
    /// using QModManager.Utility;
    /// using UnityEngine;
    /// 
    /// [Menu("SMLHelper Example Mod")]
    /// public class Config : ConfigFile
    /// {
    ///     [Label("My checkbox"), OnChange(nameof(MyCheckboxToggleEvent)), OnChange(nameof(MyGenericValueChangedEvent))]
    ///     public bool ToggleValue;
    ///     
    ///     public void MyCheckboxToggleEvent(ToggleChangedEventArgs e)
    ///     {
    ///         Logger.Log(Logger.Level.Info, "Checkbox value was changed!");
    ///         Logger.Log(Logger.Level.Info, $"{e.Value}");
    ///     }
    /// 
    ///     private void MyGenericValueChangedEvent(IModOptionEventArgs e)
    ///     {
    ///         Logger.Log(Logger.Level.Info, "Generic value changed!");
    ///         Logger.Log(Logger.Level.Info, $"{e.Id}: {e.GetType()}");
    /// 
    ///         switch (e)
    ///         {
    ///             case KeybindChangedEventArgs keybindChangedEventArgs:
    ///                 Logger.Log(Logger.Level.Info, keybindChangedEventArgs.KeyName);
    ///                 break;
    ///             case ChoiceChangedEventArgs choiceChangedEventArgs:
    ///                 Logger.Log(Logger.Level.Info, choiceChangedEventArgs.Value);
    ///                 break;
    ///             case SliderChangedEventArgs sliderChangedEventArgs:
    ///                 Logger.Log(Logger.Level.Info, sliderChangedEventArgs.Value.ToString());
    ///                 break;
    ///             case ToggleChangedEventArgs toggleChangedEventArgs:
    ///                 Logger.Log(Logger.Level.Info, toggleChangedEventArgs.Value.ToString());
    ///                 break;
    ///         }
    ///      }
    /// </code>
    /// </example>
    /// <seealso cref="MenuAttribute"/>
    /// <seealso cref="LabelAttribute"/>
    /// <seealso cref="IModOptionEventArgs"/>
    /// <seealso cref="ChoiceChangedEventArgs"/>
    /// <seealso cref="KeybindChangedEventArgs"/>
    /// <seealso cref="SliderChangedEventArgs"/>
    /// <seealso cref="ToggleChangedEventArgs"/>
    /// <seealso cref="ConfigFile"/>
    /// <seealso cref="OnGameObjectCreatedAttribute"/>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class OnChangeAttribute : Attribute
    {
        /// <summary>
        /// The name of the method to run whenever the member this attribute is applied to changes.
        /// The method must be a member of the same class.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Signifies a method to run whenever the member this attribute is applied to changes.
        /// The method must be a member of the same class.
        /// </summary>
        /// <param name="methodName">The name of the method within the same class to run.</param>
        public OnChangeAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
