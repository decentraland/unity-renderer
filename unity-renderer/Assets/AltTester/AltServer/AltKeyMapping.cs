#if ALTTESTER && ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Altom.AltTester
{
    public class AltKeyMapping
    {
        public static Dictionary<string, KeyCode> StringToKeyCode = new Dictionary<string, KeyCode>()
        {
        
        //Letters
        {"a", KeyCode.A},
        {"b", KeyCode.B},
        {"c", KeyCode.C},
        {"d", KeyCode.D},
        {"e", KeyCode.E},
        {"f", KeyCode.F},
        {"g", KeyCode.G},
        {"h", KeyCode.H},
        {"i", KeyCode.I},
        {"j", KeyCode.J},
        {"k", KeyCode.K},
        {"l", KeyCode.L},
        {"m", KeyCode.M},
        {"n", KeyCode.N},
        {"o", KeyCode.O},
        {"p", KeyCode.P},
        {"q", KeyCode.Q},
        {"r", KeyCode.R},
        {"s", KeyCode.S},
        {"t", KeyCode.T},
        {"u", KeyCode.U},
        {"v", KeyCode.V},
        {"w", KeyCode.W},
        {"x", KeyCode.X},
        {"y", KeyCode.Y},
        {"z", KeyCode.Z},
        
        //KeyPad Numbers
        {"1", KeyCode.Keypad1},
        {"2", KeyCode.Keypad2},
        {"3", KeyCode.Keypad3},
        {"4", KeyCode.Keypad4},
        {"5", KeyCode.Keypad5},
        {"6", KeyCode.Keypad6},
        {"7", KeyCode.Keypad7},
        {"8", KeyCode.Keypad8},
        {"9", KeyCode.Keypad9},
        {"0", KeyCode.Keypad0},

        //Fs
        {"F1", KeyCode.F1},
        {"F2", KeyCode.F2},
        {"F3", KeyCode.F3},
        {"F4", KeyCode.F4},
        {"F5", KeyCode.F5},
        {"F6", KeyCode.F6},
        {"F7", KeyCode.F7},
        {"F8", KeyCode.F8},
        {"F9", KeyCode.F9},
        {"F10", KeyCode.F10},
        {"F11", KeyCode.F11},
        {"F12", KeyCode.F12},
        
        //Other Symbols
        {"!", KeyCode.Exclaim}, //1
        {"\"", KeyCode.DoubleQuote},
        {"#", KeyCode.Hash}, //3
        {"$", KeyCode.Dollar}, //4
        {"&", KeyCode.Ampersand}, //7
        {"\'", KeyCode.Quote}, //remember the special forward slash rule... this isnt wrong
        {"(", KeyCode.LeftParen}, //9
        {")", KeyCode.RightParen}, //0
        {"*", KeyCode.Asterisk}, //8
        {"+", KeyCode.Plus},
        {",", KeyCode.Comma},
        {"-", KeyCode.Minus},
        {".", KeyCode.Period},
        {"/", KeyCode.Slash},
        {":", KeyCode.Colon},
        {";", KeyCode.Semicolon},
        {"<", KeyCode.Less},
        {"=", KeyCode.Equals},
        {">", KeyCode.Greater},
        {"?", KeyCode.Question},
        {"@", KeyCode.At}, //2
        {"[", KeyCode.LeftBracket},
        {"\\", KeyCode.Backslash}, //remember the special forward slash rule... this isnt wrong
        {"]", KeyCode.RightBracket},
        {"^", KeyCode.Caret}, //6
        {"_", KeyCode.Underscore},
        {"`", KeyCode.BackQuote},
        
        //Alpha Numbers
        //NOTE: we are using the UPPER CASE LETTERS Q -> P because they are nearest to the Alpha Numbers
        {"Q", KeyCode.Alpha1},
        {"W", KeyCode.Alpha2},
        {"E", KeyCode.Alpha3},
        {"R", KeyCode.Alpha4},
        {"T", KeyCode.Alpha5},
        {"Y", KeyCode.Alpha6},
        {"U", KeyCode.Alpha7},
        {"I", KeyCode.Alpha8},
        {"O", KeyCode.Alpha9},
        {"P", KeyCode.Alpha0},

        {"k.", KeyCode.KeypadPeriod},
        {"k/", KeyCode.KeypadDivide},
        {"k*", KeyCode.KeypadMultiply},
        {"k-", KeyCode.KeypadMinus},
        {"k+", KeyCode.KeypadPlus},
        {"k=", KeyCode.KeypadEquals},
        {"kenter", KeyCode.KeypadEnter},

        {"~", KeyCode.Tilde},
        {"{", KeyCode.LeftCurlyBracket},
        {"}", KeyCode.RightCurlyBracket},
        {"|", KeyCode.Pipe},
        {"%", KeyCode.Percent},
        {"bsp", KeyCode.Backspace},
        {"tab", KeyCode.Tab},
        {"clr", KeyCode.Clear},
        {"return", KeyCode.Return},
        {"pause", KeyCode.Pause},
        {"esc", KeyCode.Escape},
        {"space", KeyCode.Space},
        {"del", KeyCode.Delete},
        {"upArrow", KeyCode.UpArrow},
        {"downArrow", KeyCode.DownArrow},
        {"rightArrow", KeyCode.RightArrow},
        {"leftArrow", KeyCode.LeftArrow},
        {"ins", KeyCode.Insert},
        {"home", KeyCode.Home},
        {"end", KeyCode.End},
        {"pageUp", KeyCode.PageUp},
        {"pageDown", KeyCode.PageDown},
        {"Numlock", KeyCode.Numlock},
        {"CapsLock", KeyCode.CapsLock},
        {"ScrollLock", KeyCode.ScrollLock},
        {"RightShift", KeyCode.RightShift},
        {"LeftShift", KeyCode.LeftShift},
        {"RightControl", KeyCode.RightControl},
        {"LeftControl", KeyCode.LeftControl},
        {"RightAlt", KeyCode.RightAlt},
        {"LeftAlt", KeyCode.LeftAlt},
        {"RightCommand", KeyCode.RightCommand},
        {"RightApple", KeyCode.RightApple},
        {"LeftCommand", KeyCode.LeftCommand},
        {"LeftApple", KeyCode.LeftApple},
        {"LeftWindows", KeyCode.LeftWindows},
        {"RightWindows", KeyCode.RightWindows},
        {"AltGr", KeyCode.AltGr},
        {"Help", KeyCode.Help},
        {"Print", KeyCode.Print},
        {"SysReq", KeyCode.SysReq},
        {"Break", KeyCode.Break},
        {"Menu", KeyCode.Menu}
        };

        public static Dictionary<string, Key> StringToKey = new Dictionary<string, Key>()
        {
        
        //Letters
        {"a", Key.A},
        {"b", Key.B},
        {"c", Key.C},
        {"d", Key.D},
        {"e", Key.E},
        {"f", Key.F},
        {"g", Key.G},
        {"h", Key.H},
        {"i", Key.I},
        {"j", Key.J},
        {"k", Key.K},
        {"l", Key.L},
        {"m", Key.M},
        {"n", Key.N},
        {"o", Key.O},
        {"p", Key.P},
        {"q", Key.Q},
        {"r", Key.R},
        {"s", Key.S},
        {"t", Key.T},
        {"u", Key.U},
        {"v", Key.V},
        {"w", Key.W},
        {"x", Key.X},
        {"y", Key.Y},
        {"z", Key.Z},
        
        //KeyPad Numbers
        {"1", Key.Numpad1},
        {"2", Key.Numpad2},
        {"3", Key.Numpad3},
        {"4", Key.Numpad4},
        {"5", Key.Numpad5},
        {"6", Key.Numpad6},
        {"7", Key.Numpad7},
        {"8", Key.Numpad8},
        {"9", Key.Numpad9},
        {"0", Key.Numpad0},

        //Fs
        {"F1", Key.F1},
        {"F2", Key.F2},
        {"F3", Key.F3},
        {"F4", Key.F4},
        {"F5", Key.F5},
        {"F6", Key.F6},
        {"F7", Key.F7},
        {"F8", Key.F8},
        {"F9", Key.F9},
        {"F10", Key.F10},
        {"F11", Key.F11},
        {"F12", Key.F12},
        
        //Other Symbols
        {"!", Key.Digit1}, //Exclaim
        {"\"", Key.Quote}, //DoubleQuote
        {"#", Key.Digit3}, //Hash
        {"$", Key.Digit4}, //Dollar
        {"&", Key.Digit7}, //Ampersand
        {"\'", Key.Quote}, //remember the special forward slash rule... this isnt wrong
        {"(", Key.Digit9}, //LeftParen
        {")", Key.Digit0}, //RightParen
        {"*", Key.Digit8}, //Asterisk
        {"+", Key.NumpadPlus}, //Plus
        {",", Key.Comma},
        {"-", Key.Minus},
        {".", Key.Period},
        {"/", Key.Slash},
        {":", Key.Semicolon}, //Colon
        {";", Key.Semicolon},
        {"<", Key.Comma}, //Less
        {"=", Key.Equals},
        {">", Key.Period}, //Greater
        {"?", Key.Slash}, //Question
        {"@", Key.Digit2}, //At
        {"[", Key.LeftBracket},
        {"\\", Key.Backslash}, //remember the special forward slash rule... this isnt wrong
        {"]", Key.RightBracket},
        {"^", Key.Digit6}, //Caret
        {"_", Key.Minus}, //Underscore
        {"`", Key.Backquote},
        
        //Alpha Numbers
        //NOTE: we are using the UPPER CASE LETTERS Q -> P because they are nearest to the Alpha Numbers
        {"Q", Key.Digit1},
        {"W", Key.Digit2},
        {"E", Key.Digit3},
        {"R", Key.Digit4},
        {"T", Key.Digit5},
        {"Y", Key.Digit6},
        {"U", Key.Digit7},
        {"I", Key.Digit8},
        {"O", Key.Digit9},
        {"P", Key.Digit0},

        {"k.", Key.NumpadPeriod},
        {"k/", Key.NumpadDivide},
        {"k*", Key.NumpadMultiply},
        {"k-", Key.NumpadMinus},
        {"k+", Key.NumpadPlus},
        {"k=", Key.NumpadEquals},
        {"kenter", Key.NumpadEnter},

        {"~", Key.Backquote}, //Tilde
        {"{", Key.LeftBracket}, //LeftCurlyBracket
        {"}", Key.RightBracket}, //RightCurlyBracket
        {"|", Key.Backslash}, //Pipe
        {"%", Key.Digit5}, //Percent
        {"bsp", Key.Backspace},
        {"tab", Key.Tab},
        {"clr", Key.Delete}, //clear
        {"return", Key.Enter},
        {"pause", Key.Pause},
        {"esc", Key.Escape},
        {"space", Key.Space},
        {"del", Key.Delete},
        {"upArrow", Key.UpArrow},
        {"downArrow", Key.DownArrow},
        {"rightArrow", Key.RightArrow},
        {"leftArrow", Key.LeftArrow},
        {"ins", Key.Insert},
        {"home", Key.Home},
        {"end", Key.End},
        {"pageUp", Key.PageUp},
        {"pageDown", Key.PageDown},
        {"Numlock", Key.NumLock},
        {"CapsLock", Key.CapsLock},
        {"ScrollLock", Key.ScrollLock},
        {"RightShift", Key.RightShift},
        {"LeftShift", Key.LeftShift},
        {"RightControl", Key.RightCtrl},
        {"LeftControl", Key.LeftCtrl},
        {"RightAlt", Key.RightAlt},
        {"LeftAlt", Key.LeftAlt},
        {"RightCommand", Key.RightCommand},
        {"RightApple", Key.RightApple},
        {"LeftCommand", Key.LeftCommand},
        {"LeftApple", Key.LeftApple},
        {"LeftWindows", Key.LeftWindows},
        {"RightWindows", Key.RightWindows},
        {"AltGr", Key.AltGr},
        {"Help", Key.Enter},
        {"Print", Key.PrintScreen},
        {"SysReq", Key.PrintScreen}, //same button as above
        {"Break", Key.Pause},
        {"Menu", Key.RightCtrl}
        };

        public static Dictionary<KeyCode, ButtonControl> mouseKeyCodeToButtonControl = new Dictionary<KeyCode, ButtonControl>()
        {
            // in Mouse class there are only 5 buttons thus KeyCode.Mouse5, KeyCode.Mouse6 are not mapped
            {KeyCode.Mouse0, NewInputSystem.Mouse.leftButton},
            {KeyCode.Mouse1, NewInputSystem.Mouse.rightButton},
            {KeyCode.Mouse2, NewInputSystem.Mouse.middleButton},
            {KeyCode.Mouse3, NewInputSystem.Mouse.forwardButton},
            {KeyCode.Mouse4, NewInputSystem.Mouse.backButton}
        };

        public Dictionary<KeyCode, ButtonControl> joystickKeyCodeToGamepad;

        public AltKeyMapping(float power)
        {
            joystickKeyCodeToGamepad = new Dictionary<KeyCode, ButtonControl>()
        {
            {KeyCode.JoystickButton0, NewInputSystem.Gamepad.aButton},
            {KeyCode.JoystickButton1, NewInputSystem.Gamepad.bButton},
            {KeyCode.JoystickButton2, NewInputSystem.Gamepad.xButton},
            {KeyCode.JoystickButton3, NewInputSystem.Gamepad.yButton},
            {KeyCode.JoystickButton4, NewInputSystem.Gamepad.leftShoulder},
            {KeyCode.JoystickButton5, NewInputSystem.Gamepad.rightShoulder},
            {KeyCode.JoystickButton6, NewInputSystem.Gamepad.leftTrigger},
            {KeyCode.JoystickButton7, NewInputSystem.Gamepad.rightTrigger},
            {KeyCode.JoystickButton8, NewInputSystem.Gamepad.selectButton},
            {KeyCode.JoystickButton9, NewInputSystem.Gamepad.startButton},
            {KeyCode.JoystickButton10, NewInputSystem.Gamepad.leftStickButton},
            {KeyCode.JoystickButton11, NewInputSystem.Gamepad.rightStickButton},
            {KeyCode.JoystickButton12, NewInputSystem.Gamepad.dpad.up},
            {KeyCode.JoystickButton13, NewInputSystem.Gamepad.dpad.down},
            {KeyCode.JoystickButton14, NewInputSystem.Gamepad.dpad.left},
            {KeyCode.JoystickButton15, NewInputSystem.Gamepad.dpad.right},
            {KeyCode.JoystickButton16, power > 0 ? NewInputSystem.Gamepad.leftStick.right : NewInputSystem.Gamepad.leftStick.left},
            {KeyCode.JoystickButton17, power > 0 ? NewInputSystem.Gamepad.leftStick.up : NewInputSystem.Gamepad.leftStick.down},
            {KeyCode.JoystickButton18, power > 0 ? NewInputSystem.Gamepad.rightStick.right: NewInputSystem.Gamepad.rightStick.left},
            {KeyCode.JoystickButton19, power > 0 ? NewInputSystem.Gamepad.rightStick.up : NewInputSystem.Gamepad.rightStick.down}
        };
        }
    }
}
#endif