namespace Nautilus.Handlers;

/// <summary>
/// A handler class for dealing with the game's UnityEngine.InputSystem implementation.
/// </summary>
public static class GameInputHandler
{
    /// <summary>
    /// A super class that then refers to all valid buttons for a respective device.
    /// </summary>
    public static class Paths
    {
        /// <summary>
        /// A class containing all valid binding paths for keyboards.
        /// </summary>
        public static class Keyboard
        {
            /// <summary>
            /// Represents the 'Escape' in the Keyboard.
            /// </summary>
            public const string Escape  = "/Keyboard/escape";

            /// <summary>
            /// Represents the 'Space' in the Keyboard.
            /// </summary>
            public const string Space  = "/Keyboard/space";

            /// <summary>
            /// Represents the 'Enter' in the Keyboard.
            /// </summary>
            public const string Enter  = "/Keyboard/enter";

            /// <summary>
            /// Represents the 'Tab' in the Keyboard.
            /// </summary>
            public const string Tab  = "/Keyboard/tab";

            /// <summary>
            /// Represents the (`) in the Keyboard.
            /// </summary>
            public const string Backquote  = "/Keyboard/backquote";

            /// <summary>
            /// Represents the (') in the Keyboard.
            /// </summary>
            public const string Quote  = "/Keyboard/quote";

            /// <summary>
            /// Represents the (;) in the Keyboard.
            /// </summary>
            public const string Semicolon  = "/Keyboard/semicolon";

            /// <summary>
            /// Represents the (,) in the Keyboard.
            /// </summary>
            public const string Comma  = "/Keyboard/comma";

            /// <summary>
            /// Represents the (.) in the Keyboard.
            /// </summary>
            public const string Period  = "/Keyboard/period";

            /// <summary>
            /// Represents the (/) in the Keyboard.
            /// </summary>
            public const string Slash  = "/Keyboard/slash";

            /// <summary>
            /// Represents the (\) in the Keyboard.
            /// </summary>
            public const string Backslash  = "/Keyboard/backslash";

            /// <summary>
            /// Represents the '[' in the Keyboard.
            /// </summary>
            public const string LeftBracket  = "/Keyboard/leftBracket";

            /// <summary>
            /// Represents the ']' in the Keyboard.
            /// </summary>
            public const string RightBracket  = "/Keyboard/rightBracket";

            /// <summary>
            /// Represents the (-) in the Keyboard.
            /// </summary>
            public const string Minus  = "/Keyboard/minus";

            /// <summary>
            /// Represents the (=) in the Keyboard.
            /// </summary>
            public new const string Equals  = "/Keyboard/equals";

            /// <summary>
            /// Represents the 'Up Arrow' in the Keyboard.
            /// </summary>
            public const string UpArrow  = "/Keyboard/upArrow";

            /// <summary>
            /// Represents the 'Down Arrow' in the Keyboard.
            /// </summary>
            public const string DownArrow  = "/Keyboard/downArrow";

            /// <summary>
            /// Represents the 'Left Arrow' in the Keyboard.
            /// </summary>
            public const string LeftArrow  = "/Keyboard/leftArrow";

            /// <summary>
            /// Represents the 'Right Arrow' in the Keyboard.
            /// </summary>
            public const string RightArrow  = "/Keyboard/rightArrow";

            /// <summary>
            /// Represents the 'A' in the Keyboard.
            /// </summary>
            public const string A  = "/Keyboard/a";

            /// <summary>
            /// Represents the 'B' in the Keyboard.
            /// </summary>
            public const string B  = "/Keyboard/b";

            /// <summary>
            /// Represents the 'C' in the Keyboard.
            /// </summary>
            public const string C  = "/Keyboard/c";

            /// <summary>
            /// Represents the 'D' in the Keyboard.
            /// </summary>
            public const string D  = "/Keyboard/d";

            /// <summary>
            /// Represents the 'E' in the Keyboard.
            /// </summary>
            public const string E  = "/Keyboard/e";

            /// <summary>
            /// Represents the 'F' in the Keyboard.
            /// </summary>
            public const string F  = "/Keyboard/f";

            /// <summary>
            /// Represents the 'G' in the Keyboard.
            /// </summary>
            public const string G  = "/Keyboard/g";

            /// <summary>
            /// Represents the 'H' in the Keyboard.
            /// </summary>
            public const string H  = "/Keyboard/h";

            /// <summary>
            /// Represents the 'I' in the Keyboard.
            /// </summary>
            public const string I  = "/Keyboard/i";

            /// <summary>
            /// Represents the 'J' in the Keyboard.
            /// </summary>
            public const string J  = "/Keyboard/j";

            /// <summary>
            /// Represents the 'K' in the Keyboard.
            /// </summary>
            public const string K  = "/Keyboard/k";

            /// <summary>
            /// Represents the 'L' in the Keyboard.
            /// </summary>
            public const string L  = "/Keyboard/l";

            /// <summary>
            /// Represents the 'M' in the Keyboard.
            /// </summary>
            public const string M  = "/Keyboard/m";

            /// <summary>
            /// Represents the 'N' in the Keyboard.
            /// </summary>
            public const string N  = "/Keyboard/n";

            /// <summary>
            /// Represents the 'O' in the Keyboard.
            /// </summary>
            public const string O  = "/Keyboard/o";

            /// <summary>
            /// Represents the 'P' in the Keyboard.
            /// </summary>
            public const string P  = "/Keyboard/p";

            /// <summary>
            /// Represents the 'Q' in the Keyboard.
            /// </summary>
            public const string Q  = "/Keyboard/q";

            /// <summary>
            /// Represents the 'R' in the Keyboard.
            /// </summary>
            public const string R  = "/Keyboard/r";

            /// <summary>
            /// Represents the 'S' in the Keyboard.
            /// </summary>
            public const string S  = "/Keyboard/s";

            /// <summary>
            /// Represents the 'T' in the Keyboard.
            /// </summary>
            public const string T  = "/Keyboard/t";

            /// <summary>
            /// Represents the 'U' in the Keyboard.
            /// </summary>
            public const string U  = "/Keyboard/u";

            /// <summary>
            /// Represents the 'V' in the Keyboard.
            /// </summary>
            public const string V  = "/Keyboard/v";

            /// <summary>
            /// Represents the 'W' in the Keyboard.
            /// </summary>
            public const string W  = "/Keyboard/w";

            /// <summary>
            /// Represents the 'X' in the Keyboard.
            /// </summary>
            public const string X  = "/Keyboard/x";

            /// <summary>
            /// Represents the 'Y' in the Keyboard.
            /// </summary>
            public const string Y  = "/Keyboard/y";

            /// <summary>
            /// Represents the 'Z' in the Keyboard.
            /// </summary>
            public const string Z  = "/Keyboard/z";

            /// <summary>
            /// Represents the '1' in the Keyboard.
            /// </summary>
            public const string Key1 = "/Keyboard/1";

            /// <summary>
            /// Represents the '2' in the Keyboard.
            /// </summary>
            public const string Key2 = "/Keyboard/2";

            /// <summary>
            /// Represents the '3' in the Keyboard.
            /// </summary>
            public const string Key3 = "/Keyboard/3";

            /// <summary>
            /// Represents the '4' in the Keyboard.
            /// </summary>
            public const string Key4 = "/Keyboard/4";

            /// <summary>
            /// Represents the '5' in the Keyboard.
            /// </summary>
            public const string Key5 = "/Keyboard/5";

            /// <summary>
            /// Represents the '6' in the Keyboard.
            /// </summary>
            public const string Key6 = "/Keyboard/6";

            /// <summary>
            /// Represents the '7' in the Keyboard.
            /// </summary>
            public const string Key7 = "/Keyboard/7";

            /// <summary>
            /// Represents the '8' in the Keyboard.
            /// </summary>
            public const string Key8 = "/Keyboard/8";

            /// <summary>
            /// Represents the '9' in the Keyboard.
            /// </summary>
            public const string Key9 = "/Keyboard/9";

            /// <summary>
            /// Represents the '0' in the Keyboard.
            /// </summary>
            public const string Key0 = "/Keyboard/0";

            /// <summary>
            /// Represents the 'Shift' in the Keyboard.
            /// </summary>
            public const string Shift = "/Keyboard/shift";

            /// <summary>
            /// Represents the 'Alt' in the Keyboard.
            /// </summary>
            public const string Alt = "/Keyboard/alt";

            /// <summary>
            /// Represents the 'Control' in the Keyboard.
            /// </summary>
            public const string Ctrl = "/Keyboard/ctrl";

            /// <summary>
            /// Represents the 'Left System' in the Keyboard.
            /// </summary>
            public const string LeftMeta = "/Keyboard/leftMeta";

            /// <summary>
            /// Represents the 'Right System' in the Keyboard.
            /// </summary>
            public const string RightMeta = "/Keyboard/rightMeta";

            /// <summary>
            /// Represents the 'Context Menu' in the Keyboard.
            /// </summary>
            public const string ContextMenu = "/Keyboard/contextMenu";

            /// <summary>
            /// Represents the 'Backspace' in the Keyboard.
            /// </summary>
            public const string Backspace = "/Keyboard/backspace";

            /// <summary>
            /// Represents the 'Page Down' in the Keyboard.
            /// </summary>
            public const string PageDown = "/Keyboard/pageDown";

            /// <summary>
            /// Represents the 'Page Up' in the Keyboard.
            /// </summary>
            public const string PageUp = "/Keyboard/pageUp";

            /// <summary>
            /// Represents the 'Home' in the Keyboard.
            /// </summary>
            public const string Home = "/Keyboard/home";

            /// <summary>
            /// Represents the 'End' in the Keyboard.
            /// </summary>
            public const string End = "/Keyboard/end";

            /// <summary>
            /// Represents the 'Insert' in the Keyboard.
            /// </summary>
            public const string Insert = "/Keyboard/insert";

            /// <summary>
            /// Represents the 'Delete' in the Keyboard.
            /// </summary>
            public const string Delete = "/Keyboard/delete";

            /// <summary>
            /// Represents the 'Caps Lock' in the Keyboard.
            /// </summary>
            public const string CapsLock = "/Keyboard/capsLock";

            /// <summary>
            /// Represents the 'Num Lock' in the Keyboard.
            /// </summary>
            public const string NumLock = "/Keyboard/numLock";

            /// <summary>
            /// Represents the 'Print Screen' in the Keyboard.
            /// </summary>
            public const string PrintScreen = "/Keyboard/printScreen";

            /// <summary>
            /// Represents the 'Scroll Lock' in the Keyboard.
            /// </summary>
            public const string ScrollLock = "/Keyboard/scrollLock";

            /// <summary>
            /// Represents the 'Pause/Break' in the Keyboard.
            /// </summary>
            public const string Pause = "/Keyboard/pause";

            /// <summary>
            /// Represents the 'Numpad Enter' in the Keyboard.
            /// </summary>
            public const string NumpadEnter = "/Keyboard/numpadEnter";

            /// <summary>
            /// Represents the 'Numpad /' in the Keyboard.
            /// </summary>
            public const string NumpadDivide = "/Keyboard/numpadDivide";

            /// <summary>
            /// Represents the 'Numpad *' in the Keyboard.
            /// </summary>
            public const string NumpadMultiply = "/Keyboard/numpadMultiply";

            /// <summary>
            /// Represents the 'Numpad +' in the Keyboard.
            /// </summary>
            public const string NumpadPlus = "/Keyboard/numpadPlus";

            /// <summary>
            /// Represents the 'Numpad -' in the Keyboard.
            /// </summary>
            public const string NumpadMinus = "/Keyboard/numpadMinus";

            /// <summary>
            /// Represents the 'Numpad .' in the Keyboard.
            /// </summary>
            public const string NumpadPeriod = "/Keyboard/numpadPeriod";

            /// <summary>
            /// Represents the 'Numpad =' in the Keyboard.
            /// </summary>
            public const string NumpadEquals = "/Keyboard/numpadEquals";

            /// <summary>
            /// Represents the 'Numpad 1' in the Keyboard.
            /// </summary>
            public const string Numpad1 = "/Keyboard/numpad1";

            /// <summary>
            /// Represents the 'Numpad 2' in the Keyboard.
            /// </summary>
            public const string Numpad2 = "/Keyboard/numpad2";

            /// <summary>
            /// Represents the 'Numpad 3' in the Keyboard.
            /// </summary>
            public const string Numpad3 = "/Keyboard/numpad3";

            /// <summary>
            /// Represents the 'Numpad 4' in the Keyboard.
            /// </summary>
            public const string Numpad4 = "/Keyboard/numpad4";

            /// <summary>
            /// Represents the 'Numpad 5' in the Keyboard.
            /// </summary>
            public const string Numpad5 = "/Keyboard/numpad5";

            /// <summary>
            /// Represents the 'Numpad 6' in the Keyboard.
            /// </summary>
            public const string Numpad6 = "/Keyboard/numpad6";

            /// <summary>
            /// Represents the 'Numpad 7' in the Keyboard.
            /// </summary>
            public const string Numpad7 = "/Keyboard/numpad7";

            /// <summary>
            /// Represents the 'Numpad 8' in the Keyboard.
            /// </summary>
            public const string Numpad8 = "/Keyboard/numpad8";

            /// <summary>
            /// Represents the 'Numpad 9' in the Keyboard.
            /// </summary>
            public const string Numpad9 = "/Keyboard/numpad9";

            /// <summary>
            /// Represents the 'Numpad 0' in the Keyboard.
            /// </summary>
            public const string Numpad0 = "/Keyboard/numpad0";

            /// <summary>
            /// Represents the 'F1' in the Keyboard.
            /// </summary>
            public const string F1 = "/Keyboard/f1";

            /// <summary>
            /// Represents the 'F2' in the Keyboard.
            /// </summary>
            public const string F2 = "/Keyboard/f2";

            /// <summary>
            /// Represents the 'F3' in the Keyboard.
            /// </summary>
            public const string F3 = "/Keyboard/f3";

            /// <summary>
            /// Represents the 'F4' in the Keyboard.
            /// </summary>
            public const string F4 = "/Keyboard/f4";

            /// <summary>
            /// Represents the 'F5' in the Keyboard.
            /// </summary>
            public const string F5 = "/Keyboard/f5";

            /// <summary>
            /// Represents the 'F6' in the Keyboard.
            /// </summary>
            public const string F6 = "/Keyboard/f6";

            /// <summary>
            /// Represents the 'F7' in the Keyboard.
            /// </summary>
            public const string F7 = "/Keyboard/f7";

            /// <summary>
            /// Represents the 'F8' in the Keyboard.
            /// </summary>
            public const string F8 = "/Keyboard/f8";

            /// <summary>
            /// Represents the 'F9' in the Keyboard.
            /// </summary>
            public const string F9 = "/Keyboard/f9";

            /// <summary>
            /// Represents the 'F10' in the Keyboard.
            /// </summary>
            public const string F10 = "/Keyboard/f10";

            /// <summary>
            /// Represents the 'F11' in the Keyboard.
            /// </summary>
            public const string F11 = "/Keyboard/f11";

            /// <summary>
            /// Represents the 'F12' in the Keyboard.
            /// </summary>
            public const string F12 = "/Keyboard/f12";

            /// <summary>
            /// Represents the 'OEM1' in the Keyboard.
            /// </summary>
            public const string Oem1 = "/Keyboard/OEM1";

            /// <summary>
            /// Represents the 'OEM2' in the Keyboard.
            /// </summary>
            public const string Oem2 = "/Keyboard/OEM2";

            /// <summary>
            /// Represents the 'OEM3' in the Keyboard.
            /// </summary>
            public const string Oem3 = "/Keyboard/OEM3";

            /// <summary>
            /// Represents the 'OEM4' in the Keyboard.
            /// </summary>
            public const string Oem4 = "/Keyboard/OEM4";

            /// <summary>
            /// Represents the 'OEM5' in the Keyboard.
            /// </summary>
            public const string Oem5 = "/Keyboard/OEM5";

            /// <summary>
            /// Represents the 'IMESelected' in the Keyboard.
            /// </summary>
            public const string ImeSelected = "/Keyboard/IMESelected";
        }

        /// <summary>
        /// A class containing all valid binding paths for mice.
        /// </summary>
        public static class Mouse
        {
            /// <summary>
            /// Represents the 'Scroll' in the Mouse.
            /// </summary>
            public const string ScrollUp = "/Mouse/scroll/up";
            
            /// <summary>
            /// Represents the 'Scroll' in the Mouse.
            /// </summary>
            public const string ScrollDown = "/Mouse/scroll/down";

            /// <summary>
            /// Represents the 'Left Button' in the Mouse.
            /// </summary>
            public const string LeftButton = "/Mouse/leftButton";

            /// <summary>
            /// Represents the 'Right Button' in the Mouse.
            /// </summary>
            public const string RightButton = "/Mouse/rightButton";

            /// <summary>
            /// Represents the 'Middle Button' in the Mouse.
            /// </summary>
            public const string MiddleButton = "/Mouse/middleButton";

            /// <summary>
            /// Represents the 'Northern button' in the Mouse.
            /// </summary>
            /// <remarks>Only some mouse, especially macro mouse have this button.</remarks>
            public const string ForwardButton = "/Mouse/forwardButton";

            /// <summary>
            /// Represents the 'Southern button' in the Mouse.
            /// </summary>
            /// <remarks>Only some mouse, especially macro mouse have this button.</remarks>
            public const string BackButton = "/Mouse/backButton";
        }

        /// <summary>
        /// A class containing all valid binding paths for controllers.
        /// </summary>
        public static class Gamepad
        {
            /// <summary>
            /// Represents the 'D-Pad Up' in the Gamepad.
            /// </summary>
            public const string DpadUp = "<Gamepad>/dpad/up";

            /// <summary>
            /// Represents the 'D-Pad Down' in the Gamepad.
            /// </summary>
            public const string DpadDown = "<Gamepad>/dpad/down";

            /// <summary>
            /// Represents the 'D-Pad Left' in the Gamepad.
            /// </summary>
            public const string DpadLeft = "<Gamepad>/dpad/left";

            /// <summary>
            /// Represents the 'D-Pad Right' in the Gamepad.
            /// </summary>
            public const string DpadRight = "<Gamepad>/dpad/right";

            /// <summary>
            /// Represents the 'Start' in the Gamepad.
            /// </summary>
            public const string Start = "<Gamepad>/start";

            /// <summary>
            /// Represents the 'Select' in the Gamepad.
            /// </summary>
            public const string Select = "<Gamepad>/select";

            /// <summary>
            /// Represents the 'Left Stick' in the Gamepad.
            /// </summary>
            public const string LeftStick = "<Gamepad>/leftStickPress";

            /// <summary>
            /// Represents the 'Right Stick' in the Gamepad.
            /// </summary>
            public const string RightStick = "<Gamepad>/rightStickPress";

            /// <summary>
            /// Represents the 'Left Bumper' in the Gamepad.
            /// </summary>
            public const string LeftBumper = "<Gamepad>/leftShoulder";

            /// <summary>
            /// Represents the 'Right Bumper' in the Gamepad.
            /// </summary>
            public const string RightBumper = "<Gamepad>/rightShoulder";

            /// <summary>
            /// Represents the 'A' or 'Cross' in the Gamepad.
            /// </summary>
            public const string ButtonSouth = "<Gamepad>/buttonSouth";

            /// <summary>
            /// Represents the 'B' or 'Cricle' in the Gamepad.
            /// </summary>
            public const string ButtonEas  = "<Gamepad>/buttonEast";

            /// <summary>
            /// Represents the 'X' or 'Square' in the Gamepad.
            /// </summary>
            public const string ButtonWest = "<Gamepad>/buttonWest";

            /// <summary>
            /// Represents the 'Y' 'Triangle' in the Gamepad.
            /// </summary>
            public const string ButtonNorth = "<Gamepad>/buttonNorth";

            /// <summary>
            /// Represents the 'Left Trigger' in the Gamepad.
            /// </summary>
            public const string LeftTrigger = "<Gamepad>/leftTrigger";

            /// <summary>
            /// Represents the 'Right Trigger' in the Gamepad.
            /// </summary>
            public const string RightTrigger = "<Gamepad>/rightTrigger";
        }
    }
}