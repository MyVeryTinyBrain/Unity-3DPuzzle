using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class Win32Unity
{
    public static bool SetCursorPos(Vector2 position)
    {
#if UNITY_STANDALONE_WIN
        return Win32.SetCursorPos((int)position.x, (int)position.y) == 0;
#else
        return false;
#endif
    }

    public static bool GetCursorPos(out Vector2 position)
    {
#if UNITY_STANDALONE_WIN
        Win32.POINT point;
        bool result = Win32.GetCursorPos(out point);
        position.x = point.X;
        position.y = point.Y;
        return result;
#else
        position = Vector2.zero;
        return false;
#endif
    }

    public static bool RepeatCursor(out bool repeated, int limitX = 10, int limitY = 10)
    {
        repeated = false;
#if UNITY_STANDALONE_WIN
        Vector2 mouse;
        if (!Win32Unity.GetCursorPos(out mouse))
            return false;
        // 화면의 크기
        Rect window;
        if (!Win32Unity.GetWindowRect(out window))
            return false;
        Vector2 limit = new Vector2(limitX, limitY);
        Vector2 newMouse = mouse;
        // 마우스 커서가 화면의 경계에 근접하면,
        // 새로운 마우스 커서 위치를 설정합니다.
        if (mouse.x < window.xMin + limit.x)
            newMouse.x = window.xMax - limit.x;
        else if (mouse.x > window.xMax - limit.x)
            newMouse.x = window.xMin + limit.x;
        if (mouse.y < window.yMin + limit.y)
            newMouse.y = window.yMax - limit.y;
        else if (mouse.y > window.yMax - limit.y)
            newMouse.y = window.yMin + limit.y;
        // 이전의 마우스 위치와 다르면,
        // 마우스 커서를 새 위치로 이동시킵니다.
        if (mouse != newMouse)
        {
            if (!Win32Unity.SetCursorPos(newMouse))
                return false;
            repeated = true;
        }
        return true;
#else
        return false;
#endif
    }

    public static bool GetWindowRect(out Rect rect)
    {
#if UNITY_STANDALONE_WIN
        IntPtr ptr = Process.GetCurrentProcess().MainWindowHandle;
        Win32.RECT refRect;
        bool result = Win32.GetWindowRect(ptr, out refRect);
        rect = Rect.MinMaxRect(refRect.Left, refRect.Top, refRect.Right, refRect.Bottom);
        return result;
#else
        rect = new Rect();
        return false;
#endif
    }

    public static void SendKeyPress(Win32KeyCode keyCode)
    {
#if UNITY_STANDALONE_WIN
        Win32.INPUT input = new Win32.INPUT
        {
            Type = 1
        };
        input.Data.Keyboard = new Win32.KEYBDINPUT()
        {
            Vk = (ushort)keyCode,
            Scan = 0,
            Flags = 0,
            Time = 0,
            ExtraInfo = IntPtr.Zero,
        };

        Win32.INPUT input2 = new Win32.INPUT
        {
            Type = 1
        };
        input2.Data.Keyboard = new Win32.KEYBDINPUT()
        {
            Vk = (ushort)keyCode,
            Scan = 0,
            Flags = 2,
            Time = 0,
            ExtraInfo = IntPtr.Zero
        };
        Win32.INPUT[] inputs = new Win32.INPUT[] { input, input2 };
        if (Win32.SendInput(2, inputs, Marshal.SizeOf(typeof(Win32.INPUT))) == 0)
            throw new Exception();
#endif
    }

    public static void SendKeyDown(Win32KeyCode keyCode)
    {
#if UNITY_STANDALONE_WIN
        Win32.INPUT input = new Win32.INPUT
        {
            Type = 1
        };
        input.Data.Keyboard = new Win32.KEYBDINPUT();
        input.Data.Keyboard.Vk = (ushort)keyCode;
        input.Data.Keyboard.Scan = 0;
        input.Data.Keyboard.Flags = 0;
        input.Data.Keyboard.Time = 0;
        input.Data.Keyboard.ExtraInfo = IntPtr.Zero;
        Win32.INPUT[] inputs = new Win32.INPUT[] { input };
        if (Win32.SendInput(1, inputs, Marshal.SizeOf(typeof(Win32.INPUT))) == 0)
        {
            throw new Exception();
        }
#endif
    }

    public static void SendKeyUp(Win32KeyCode keyCode)
    {
#if UNITY_STANDALONE_WIN
        Win32.INPUT input = new Win32.INPUT
        {
            Type = 1
        };
        input.Data.Keyboard = new Win32.KEYBDINPUT();
        input.Data.Keyboard.Vk = (ushort)keyCode;
        input.Data.Keyboard.Scan = 0;
        input.Data.Keyboard.Flags = 2;
        input.Data.Keyboard.Time = 0;
        input.Data.Keyboard.ExtraInfo = IntPtr.Zero;
        Win32.INPUT[] inputs = new Win32.INPUT[] { input };
        if (Win32.SendInput(1, inputs, Marshal.SizeOf(typeof(Win32.INPUT))) == 0)
            throw new Exception();
#endif
    }

    public static void SendMouseEvent(int x, int y, params Win32MouseEvent[] events)
    {
#if UNITY_STANDALONE_WIN
        Win32.INPUT input = new Win32.INPUT
        {
            Type = 0
        };
        input.Data.Mouse.X = 0;
        input.Data.Mouse.Y = 0;
        input.Data.Mouse.Flags = 0;
        foreach (Win32MouseEvent mouseEvent in events)
            input.Data.Mouse.Flags |= (uint)mouseEvent;
        input.Data.Mouse.MouseData = 0;
        input.Data.Mouse.ExtraInfo = IntPtr.Zero;
        input.Data.Mouse.Time = 0;
        Win32.INPUT[] inputs = new Win32.INPUT[] { input };
        if (Win32.SendInput(1, inputs, Marshal.SizeOf(typeof(Win32.INPUT))) == 0)
        {
            throw new Exception();
        }
#endif
    }

#if UNITY_STANDALONE_WIN
    public class Win32
    {
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public HARDWAREINPUT Hardware;
            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }
    }
#endif
}

public enum Win32MouseEvent : ushort
{
    /// <summary>
    /// Movement occurred.
    /// </summary>
    MOUSEEVENTF_MOVE = 0x0001,

    /// <summary>
    /// The left button was pressed.
    /// </summary>
    MOUSEEVENTF_LEFTDOWN = 0x0002,

    /// <summary>
    /// The left button was released.
    /// </summary>
    MOUSEEVENTF_LEFTUP = 0x0004,

    /// <summary>
    /// The right button was pressed.
    /// </summary>
    MOUSEEVENTF_RIGHTDOWN = 0x0008,

    /// <summary>
    /// The right button was released.
    /// </summary>
    MOUSEEVENTF_RIGHTUP = 0x0010,

    /// <summary>
    /// The middle button was pressed.
    /// </summary>
    MOUSEEVENTF_MIDDLEDOWN = 0x0020,

    /// <summary>
    /// The middle button was released.
    /// </summary>
    MOUSEEVENTF_MIDDLEUP = 0x0040,

    /// <summary>
    /// An X button was pressed.
    /// </summary>
    MOUSEEVENTF_XDOWN = 0x0080,

    /// <summary>
    /// An X button was released.
    /// </summary>
    MOUSEEVENTF_XUP = 0x0100,

    /// <summary>
    /// The wheel was moved, if the mouse has a wheel. The amount of movement is specified in mouseData.
    /// </summary>
    MOUSEEVENTF_WHEEL = 0x0800,

    /// <summary>
    /// The wheel was moved horizontally, if the mouse has a wheel. The amount of movement is specified in mouseData.
    /// Windows XP/2000: This value is not supported.
    /// </summary>
    MOUSEEVENTF_HWHEEL = 0x1000,

    /// <summary>
    /// The WM_MOUSEMOVE messages will not be coalesced. The default behavior is to coalesce WM_MOUSEMOVE messages.
    /// Windows XP/2000: This value is not supported.
    /// </summary>
    MOUSEEVENTF_MOVE_NOCOALESCE = 0x2000,

    /// <summary>
    /// Maps coordinates to the entire desktop. Must be used with MOUSEEVENTF_ABSOLUTE.
    /// </summary>
    MOUSEEVENTF_VIRTUALDESK = 0x4000,

    /// <summary>
    /// The dx and dy members contain normalized absolute coordinates. 
    /// If the flag is not set, dxand dy contain relative data (the change in position since the last reported position). 
    /// This flag can be set, or not set, regardless of what kind of mouse or other pointing device, if any, is connected to the system. 
    /// For further information about relative mouse motion, see the following Remarks section.
    /// </summary>
    MOUSEEVENTF_ABSOLUTE = 0x8000,

}

public enum Win32KeyCode : ushort
{
    #region Media

    /// <summary>
    /// Next track if a song is playing
    /// </summary>
    MEDIA_NEXT_TRACK = 0xb0,

    /// <summary>
    /// Play pause
    /// </summary>
    MEDIA_PLAY_PAUSE = 0xb3,

    /// <summary>
    /// Previous track
    /// </summary>
    MEDIA_PREV_TRACK = 0xb1,

    /// <summary>
    /// Stop
    /// </summary>
    MEDIA_STOP = 0xb2,

    #endregion

    #region math

    /// <summary>Key "+"</summary>
    ADD = 0x6b,
    /// <summary>
    /// "*" key
    /// </summary>
    MULTIPLY = 0x6a,

    /// <summary>
    /// "/" key
    /// </summary>
    DIVIDE = 0x6f,

    /// <summary>
    /// Subtract key "-"
    /// </summary>
    SUBTRACT = 0x6d,

    #endregion

    #region Browser
    /// <summary>
    /// Go Back
    /// </summary>
    BROWSER_BACK = 0xa6,
    /// <summary>
    /// Favorites
    /// </summary>
    BROWSER_FAVORITES = 0xab,
    /// <summary>
    /// Forward
    /// </summary>
    BROWSER_FORWARD = 0xa7,
    /// <summary>
    /// Home
    /// </summary>
    BROWSER_HOME = 0xac,
    /// <summary>
    /// Refresh
    /// </summary>
    BROWSER_REFRESH = 0xa8,
    /// <summary>
    /// browser search
    /// </summary>
    BROWSER_SEARCH = 170,
    /// <summary>
    /// Stop
    /// </summary>
    BROWSER_STOP = 0xa9,
    #endregion

    #region Numpad numbers
    /// <summary>
    /// 
    /// </summary>
    NUMPAD0 = 0x60,
    /// <summary>
    /// 
    /// </summary>
    NUMPAD1 = 0x61,
    /// <summary>
    /// 
    /// </summary>
    NUMPAD2 = 0x62,
    /// <summary>
    /// 
    /// </summary>
    NUMPAD3 = 0x63,
    /// <summary>
    /// 
    /// </summary>
    NUMPAD4 = 100,
    /// <summary>
    /// 
    /// </summary>
    NUMPAD5 = 0x65,
    /// <summary>
    /// 
    /// </summary>
    NUMPAD6 = 0x66,
    /// <summary>
    /// 
    /// </summary>
    NUMPAD7 = 0x67,
    /// <summary>
    /// 
    /// </summary>
    NUMPAD8 = 0x68,
    /// <summary>
    /// 
    /// </summary>
    NUMPAD9 = 0x69,

    #endregion

    #region Fkeys
    /// <summary>
    /// F1
    /// </summary>
    F1 = 0x70,
    /// <summary>
    /// F10
    /// </summary>
    F10 = 0x79,
    /// <summary>
    /// 
    /// </summary>
    F11 = 0x7a,
    /// <summary>
    /// 
    /// </summary>
    F12 = 0x7b,
    /// <summary>
    /// 
    /// </summary>
    F13 = 0x7c,
    /// <summary>
    /// 
    /// </summary>
    F14 = 0x7d,
    /// <summary>
    /// 
    /// </summary>
    F15 = 0x7e,
    /// <summary>
    /// 
    /// </summary>
    F16 = 0x7f,
    /// <summary>
    /// 
    /// </summary>
    F17 = 0x80,
    /// <summary>
    /// 
    /// </summary>
    F18 = 0x81,
    /// <summary>
    /// 
    /// </summary>
    F19 = 130,
    /// <summary>
    /// 
    /// </summary>
    F2 = 0x71,
    /// <summary>
    /// 
    /// </summary>
    F20 = 0x83,
    /// <summary>
    /// 
    /// </summary>
    F21 = 0x84,
    /// <summary>
    /// 
    /// </summary>
    F22 = 0x85,
    /// <summary>
    /// 
    /// </summary>
    F23 = 0x86,
    /// <summary>
    /// 
    /// </summary>
    F24 = 0x87,
    /// <summary>
    /// 
    /// </summary>
    F3 = 0x72,
    /// <summary>
    /// 
    /// </summary>
    F4 = 0x73,
    /// <summary>
    /// 
    /// </summary>
    F5 = 0x74,
    /// <summary>
    /// 
    /// </summary>
    F6 = 0x75,
    /// <summary>
    /// 
    /// </summary>
    F7 = 0x76,
    /// <summary>
    /// 
    /// </summary>
    F8 = 0x77,
    /// <summary>
    /// 
    /// </summary>
    F9 = 120,

    #endregion

    #region Other
    /// <summary>
    /// 
    /// </summary>
    OEM_1 = 0xba,
    /// <summary>
    /// 
    /// </summary>
    OEM_102 = 0xe2,
    /// <summary>
    /// 
    /// </summary>
    OEM_2 = 0xbf,
    /// <summary>
    /// 
    /// </summary>
    OEM_3 = 0xc0,
    /// <summary>
    /// 
    /// </summary>
    OEM_4 = 0xdb,
    /// <summary>
    /// 
    /// </summary>
    OEM_5 = 220,
    /// <summary>
    /// 
    /// </summary>
    OEM_6 = 0xdd,
    /// <summary>
    /// 
    /// </summary>
    OEM_7 = 0xde,
    /// <summary>
    /// 
    /// </summary>
    OEM_8 = 0xdf,
    /// <summary>
    /// 
    /// </summary>
    OEM_CLEAR = 0xfe,
    /// <summary>
    /// 
    /// </summary>
    OEM_COMMA = 0xbc,
    /// <summary>
    /// 
    /// </summary>
    OEM_MINUS = 0xbd,
    /// <summary>
    /// 
    /// </summary>
    OEM_PERIOD = 190,
    /// <summary>
    /// 
    /// </summary>
    OEM_PLUS = 0xbb,

    #endregion

    #region KEYS

    /// <summary>
    /// 
    /// </summary>
    KEY_0 = 0x30,
    /// <summary>
    /// 
    /// </summary>
    KEY_1 = 0x31,
    /// <summary>
    /// 
    /// </summary>
    KEY_2 = 50,
    /// <summary>
    /// 
    /// </summary>
    KEY_3 = 0x33,
    /// <summary>
    /// 
    /// </summary>
    KEY_4 = 0x34,
    /// <summary>
    /// 
    /// </summary>
    KEY_5 = 0x35,
    /// <summary>
    /// 
    /// </summary>
    KEY_6 = 0x36,
    /// <summary>
    /// 
    /// </summary>
    KEY_7 = 0x37,
    /// <summary>
    /// 
    /// </summary>
    KEY_8 = 0x38,
    /// <summary>
    /// 
    /// </summary>
    KEY_9 = 0x39,
    /// <summary>
    /// 
    /// </summary>
    KEY_A = 0x41,
    /// <summary>
    /// 
    /// </summary>
    KEY_B = 0x42,
    /// <summary>
    /// 
    /// </summary>
    KEY_C = 0x43,
    /// <summary>
    /// 
    /// </summary>
    KEY_D = 0x44,
    /// <summary>
    /// 
    /// </summary>
    KEY_E = 0x45,
    /// <summary>
    /// 
    /// </summary>
    KEY_F = 70,
    /// <summary>
    /// 
    /// </summary>
    KEY_G = 0x47,
    /// <summary>
    /// 
    /// </summary>
    KEY_H = 0x48,
    /// <summary>
    /// 
    /// </summary>
    KEY_I = 0x49,
    /// <summary>
    /// 
    /// </summary>
    KEY_J = 0x4a,
    /// <summary>
    /// 
    /// </summary>
    KEY_K = 0x4b,
    /// <summary>
    /// 
    /// </summary>
    KEY_L = 0x4c,
    /// <summary>
    /// 
    /// </summary>
    KEY_M = 0x4d,
    /// <summary>
    /// 
    /// </summary>
    KEY_N = 0x4e,
    /// <summary>
    /// 
    /// </summary>
    KEY_O = 0x4f,
    /// <summary>
    /// 
    /// </summary>
    KEY_P = 80,
    /// <summary>
    /// 
    /// </summary>
    KEY_Q = 0x51,
    /// <summary>
    /// 
    /// </summary>
    KEY_R = 0x52,
    /// <summary>
    /// 
    /// </summary>
    KEY_S = 0x53,
    /// <summary>
    /// 
    /// </summary>
    KEY_T = 0x54,
    /// <summary>
    /// 
    /// </summary>
    KEY_U = 0x55,
    /// <summary>
    /// 
    /// </summary>
    KEY_V = 0x56,
    /// <summary>
    /// 
    /// </summary>
    KEY_W = 0x57,
    /// <summary>
    /// 
    /// </summary>
    KEY_X = 0x58,
    /// <summary>
    /// 
    /// </summary>
    KEY_Y = 0x59,
    /// <summary>
    /// 
    /// </summary>
    KEY_Z = 90,

    #endregion

    #region volume
    /// <summary>
    /// Decrese volume
    /// </summary>
    VOLUME_DOWN = 0xae,

    /// <summary>
    /// Mute volume
    /// </summary>
    VOLUME_MUTE = 0xad,

    /// <summary>
    /// Increase volue
    /// </summary>
    VOLUME_UP = 0xaf,

    #endregion


    /// <summary>
    /// Take snapshot of the screen and place it on the clipboard
    /// </summary>
    SNAPSHOT = 0x2c,

    /// <summary>Send right click from keyboard "key that is 2 keys to the right of space bar"</summary>
    RightClick = 0x5d,

    /// <summary>
    /// Go Back or delete
    /// </summary>
    BACKSPACE = 8,

    /// <summary>
    /// Control + Break "When debuging if you step into an infinite loop this will stop debug"
    /// </summary>
    CANCEL = 3,
    /// <summary>
    /// Caps lock key to send cappital letters
    /// </summary>
    CAPS_LOCK = 20,
    /// <summary>
    /// Ctlr key
    /// </summary>
    CONTROL = 0x11,

    /// <summary>
    /// Alt key
    /// </summary>
    ALT = 18,

    /// <summary>
    /// "." key
    /// </summary>
    DECIMAL = 110,

    /// <summary>
    /// Delete Key
    /// </summary>
    DELETE = 0x2e,


    /// <summary>
    /// Arrow down key
    /// </summary>
    DOWN = 40,

    /// <summary>
    /// End key
    /// </summary>
    END = 0x23,

    /// <summary>
    /// Escape key
    /// </summary>
    ESC = 0x1b,

    /// <summary>
    /// Home key
    /// </summary>
    HOME = 0x24,

    /// <summary>
    /// Insert key
    /// </summary>
    INSERT = 0x2d,

    /// <summary>
    /// Open my computer
    /// </summary>
    LAUNCH_APP1 = 0xb6,
    /// <summary>
    /// Open calculator
    /// </summary>
    LAUNCH_APP2 = 0xb7,

    /// <summary>
    /// Open default email in my case outlook
    /// </summary>
    LAUNCH_MAIL = 180,

    /// <summary>
    /// Opend default media player (itunes, winmediaplayer, etc)
    /// </summary>
    LAUNCH_MEDIA_SELECT = 0xb5,

    /// <summary>
    /// Left control
    /// </summary>
    LCONTROL = 0xa2,

    /// <summary>
    /// Left arrow
    /// </summary>
    LEFT = 0x25,

    /// <summary>
    /// Left shift
    /// </summary>
    LSHIFT = 160,

    /// <summary>
    /// left windows key
    /// </summary>
    LWIN = 0x5b,


    /// <summary>
    /// Next "page down"
    /// </summary>
    PAGEDOWN = 0x22,

    /// <summary>
    /// Num lock to enable typing numbers
    /// </summary>
    NUMLOCK = 0x90,

    /// <summary>
    /// Page up key
    /// </summary>
    PAGE_UP = 0x21,

    /// <summary>
    /// Right control
    /// </summary>
    RCONTROL = 0xa3,

    /// <summary>
    /// Return key
    /// </summary>
    ENTER = 13,

    /// <summary>
    /// Right arrow key
    /// </summary>
    RIGHT = 0x27,

    /// <summary>
    /// Right shift
    /// </summary>
    RSHIFT = 0xa1,

    /// <summary>
    /// Right windows key
    /// </summary>
    RWIN = 0x5c,

    /// <summary>
    /// Shift key
    /// </summary>
    SHIFT = 0x10,

    /// <summary>
    /// Space back key
    /// </summary>
    SPACE_BAR = 0x20,

    /// <summary>
    /// Tab key
    /// </summary>
    TAB = 9,

    /// <summary>
    /// Up arrow key
    /// </summary>
    UP = 0x26,

}