using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NppPlugin.External
{
    /// <summary>
    /// Exposes various external C/C++ methods for Windows.
    /// </summary>
    /// <remarks>All methods exposed reside in user32.dll.</remarks>
    public abstract class Windows
    {
        #region Types   
        public enum Languages
        {
            L_TEXT, L_PHP, L_C, L_CPP, L_CS, L_OBJC, L_JAVA, L_RC,
            L_HTML, L_XML, L_MAKEFILE, L_PASCAL, L_BATCH, L_INI, L_ASCII, L_USER,
            L_ASP, L_SQL, L_VB, L_JS, L_CSS, L_PERL, L_PYTHON, L_LUA,
            L_TEX, L_FORTRAN, L_BASH, L_FLASH, L_NSIS, L_TCL, L_LISP, L_SCHEME,
            L_ASM, L_DIFF, L_PROPS, L_PS, L_RUBY, L_SMALLTALK, L_VHDL, L_KIX, L_AU3,
            L_CAML, L_ADA, L_VERILOG, L_MATLAB, L_HASKELL, L_INNO, L_SEARCHRESULT,
            L_CMAKE, L_YAML, L_COBOL, L_GUI4CLI, L_D, L_POWERSHELL, L_R, L_JSP,
            L_COFFEESCRIPT,
            // The end of enumated language type, so it should be always at the end
            L_EXTERNAL
        }
        public enum Versions
        {
            WV_UNKNOWN, WV_WIN32S, WV_95, WV_98, WV_ME, WV_NT, WV_W2K,
            WV_XP, WV_S2003, WV_XPX64, WV_VISTA, WV_WIN7, WV_WIN8, WV_WIN81
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion
        #region Constants             
        public const int MAX_PATH = 260;
        public const int MF_BYCOMMAND = 0;
        public const int MF_CHECKED = 8;
        public const int MF_UNCHECKED = 0;
        public const int MF_BYPOSITION = 0x00000400;
        public const int WM_CREATE = 1;
        #endregion
        #region Static Methods
        internal static int CheckMenuItem(IntPtr hmenu, int uIDCheckItem, bool check)
        {
            int result = -1;
            if (hmenu != IntPtr.Zero)
            {
                try
                {
                    result = Windows.CheckMenuItem(hmenu, uIDCheckItem,
                                                 (check ? Windows.MF_CHECKED : Windows.MF_UNCHECKED) | Windows.MF_BYPOSITION);
                }
                catch (Exception e)
                {
                    // TODO: Implement exception handling   
                }
            }
            return result;
        }
        #region External Methods  
        [DllImport("user32")]
        private static extern int CheckMenuItem(IntPtr hmenu, int uIDCheckItem, int uCheck);
        [DllImport("user32", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32")]
        public static extern int GetMenuItemCount(IntPtr hWnd);
        [DllImport("user32")]
        public static extern int GetMenuString(IntPtr HWnd, uint id, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpString, int count, uint flags);
        [DllImport("user32")]
        public static extern IntPtr GetSubMenu(IntPtr hWnd, int pos);
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref Rect lpRect);
        [DllImport("user32")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, out int lParam);
        [DllImport("user32")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam);
        [DllImport("user32")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, string lParam);
        [DllImport("user32")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);
        [DllImport("user32")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        #endregion
        #endregion
    }
}
