namespace SMLHelper.V2.Handlers
{
    using Commands;
    using HarmonyLib;
    using Interfaces;
    using SMLHelper.V2.Patchers;
    using System;
    using System.Reflection;

    /// <summary>
    /// A handler class for registering your custom console commands.
    /// </summary>
    public class ConsoleCommandsHandler : IConsoleCommandHandler
    {
        /// <summary>
        /// Main entry point for all calls to this handler.
        /// </summary>
        public static IConsoleCommandHandler Main { get; } = new ConsoleCommandsHandler();

        /// <summary>
        /// Registers your custom console command.
        /// </summary>
        /// <remarks>
        /// <para>Target method must be <see langword="static"/>.</para>
        /// 
        /// <para>The command can take parameters and will respect optional parameters as outlined in the method's signature.<br/>
        /// Supported parameter types: <see cref="string"/>, <see cref="bool"/>, <see cref="int"/>, <see cref="float"/>,
        /// <see cref="double"/>.</para>
        /// 
        /// <para>If the method has a return type, it will be printed to both the screen and the log.</para>
        /// </remarks>
        /// <param name="command">The case-insensitive command to register.</param>
        /// <param name="declaringType">The declaring type that holds the method to call when the command is entered.</param>
        /// <param name="methodName">The name of the method to call within the declaring type when the command is entered. 
        /// Method must be <see langword="static"/>.</param>
        /// <param name="parameters">The parameter types the method receives, for targeting overloads.</param>
        /// <seealso cref="ConsoleCommandAttribute"/>
        void IConsoleCommandHandler.RegisterConsoleCommand(string command, Type declaringType, string methodName, Type[] parameters)
        {
            MethodInfo targetMethod = parameters == null
                ? AccessTools.Method(declaringType, methodName)
                : AccessTools.Method(declaringType, methodName, parameters);
            ConsoleCommandsPatcher.AddCustomCommand(command, targetMethod);
        }

        /// <summary>
        /// Registers your custom console command.
        /// </summary>
        /// <remarks>
        /// <para>Target method must be <see langword="static"/>.</para>
        /// 
        /// <para>The command can take parameters and will respect optional parameters as outlined in the method's signature.<br/>
        /// Supported parameter types: <see cref="string"/>, <see cref="bool"/>, <see cref="int"/>, <see cref="float"/>,
        /// <see cref="double"/>.</para>
        /// 
        /// <para>If the method has a return type, it will be printed to both the screen and the log.</para>
        /// </remarks>
        /// <param name="command">The case-insensitive command to register.</param>
        /// <param name="declaringType">The declaring type that holds the method to call when the command is entered.</param>
        /// <param name="methodName">The name of the method to call within the declaring type when the command is entered. 
        /// Method must be <see langword="static"/>.</param>
        /// <param name="parameters">The parameter types the method receives, for targeting overloads.</param>
        /// <seealso cref="ConsoleCommandAttribute"/>
        public static void RegisterConsoleCommand(string command, Type declaringType, string methodName, Type[] parameters = null)
            => Main.RegisterConsoleCommand(command, declaringType, methodName, parameters);
    }
}
