using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NppPlugin.DllExport;
using NppPlugin.External;

namespace NppPlugin
{
    /// <summary>
    /// Methods which are called by Notepad++ to initialize and communicate with the plug-in.
    /// </summary>
    public abstract class EntryPoint
    {
        #region Types
        #region Delegates
        public delegate void delNotified(Notification notification);
        #endregion
        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct Header
        {
            /* Compatible with Windows NMHDR.
             * hwndFrom is really an environment specific window handle or pointer
             * but most clients of Scintilla.h do not have this type visible. */
            public IntPtr hwndFrom;
            public uint idFrom;
            public uint code;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct Notification
        {
            public Header nmhdr;
            public int position;                /* SCN_STYLENEEDED, SCN_MODIFIED, SCN_DWELLSTART, SCN_DWELLEND */
            public int ch;                      /* SCN_CHARADDED, SCN_KEY */
            public int modifiers;               /* SCN_KEY */
            public int modificationType;        /* SCN_MODIFIED */
            public IntPtr text;                 /* SCN_MODIFIED, SCN_USERLISTSELECTION, SCN_AUTOCSELECTION */
            public int length;                  /* SCN_MODIFIED */
            public int linesAdded;              /* SCN_MODIFIED */
            public int message;                 /* SCN_MACRORECORD */
            public uint wParam;                 /* SCN_MACRORECORD */
            public int lParam;                  /* SCN_MACRORECORD */
            public int line;                    /* SCN_MODIFIED */
            public int foldLevelNow;            /* SCN_MODIFIED */
            public int foldLevelPrev;           /* SCN_MODIFIED */
            public int margin;                  /* SCN_MARGINCLICK */
            public int listType;                /* SCN_USERLISTSELECTION */
            public int x;                       /* SCN_DWELLSTART, SCN_DWELLEND */
            public int y;                       /* SCN_DWELLSTART, SCN_DWELLEND */
            public int token;                   /* SCN_MODIFIED with SC_MOD_CONTAINER */
            public int annotationLinesAdded;    /* SC_MOD_CHANGEANNOTATION */
            public int updated;                 /* SCN_UPDATEUI */
        }
        #endregion
        #endregion
        #region Static Members
        public static event delNotified Notified = null;
        private static IntPtr pointer = IntPtr.Zero;
        private static Plugin plugin = null;
        #endregion
        #region Static Methods    
        EntryPoint()
        {
            Notepad.ShutDown += EntryPoint.Notepad_ShutDown;
        }

        /// <summary>
        /// Event notification for many Notepad++ events.
        /// </summary>
        /// <param name="notifyCode">Pointer to a struct which contains notification information.</param>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static void beNotified(IntPtr notifyCode)
        {
            Notification nc = (Notification)Marshal.PtrToStructure(notifyCode, typeof(Notification));
            if (EntryPoint.Notified != null)
                EntryPoint.Notified(nc);
            return;
        }

        /// <summary>
        /// Sends all custom menu items to Notepad++.
        /// </summary>
        /// <param name="items">Number of custom menu items</param>
        /// <returns>Pointer to a collection of custom menu items</returns>
        /// <remarks>Called by Notepad++ during initialization.</remarks>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static IntPtr getFuncsArray(ref int count)
        {
            Notepad.FuncItems items = EntryPoint.plugin.GetMenuItems();
            count = items.Items.Count;
            return items.NativePointer;
        }

        /// <summary>
        /// Sends custom plug-in name to Notepad++.
        /// </summary>
        /// <returns>Pointer to string which represents the plug-in name</returns> 
        /// <remarks>Called by Notepad++ during initialization.</remarks>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        internal static IntPtr getName()
        {
            pointer = Marshal.StringToHGlobalUni(Plugin.PLUGIN_NAME);
            return pointer;
        }

        /// <summary>
        /// Tells Notepad++ whether our plug-in is Unicode-based or not.
        /// </summary>
        /// <returns>True if we are a unicode plug-in, False otherwise.</returns>      
        /// <remarks>Called by Notepad++ during initialization.</remarks>
        [DllExport(CallingConvention=CallingConvention.Cdecl)]
        private static bool isUnicode()
        {
            return true;
        }

        /// <summary>
        /// Entry point for the plug-in
        /// </summary>
        /// <param name="data">Structure representing various Notepad++ parameters</param>       
        /// <remarks>Called by Notepad++ during initialization.</remarks>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        private static void setInfo(External.Notepad.Data data)
        {
            Notepad.Initialize(data);
            Scintilla.InitializeSettings();
            EntryPoint.plugin = new Plugin();
            return;
        }

        /// <summary>
        /// Message notification for many Notepad++ messages.
        /// </summary>
        /// <param name="Message">Message ID</param>
        /// <param name="wParam">Message parameter</param>
        /// <param name="lParam">Message parameter</param>
        /// <returns>Error code</returns>
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }
        #endregion

        #region Static Event Methods
        private static void Notepad_ShutDown(object sender, EventArgs e)
        {
            Marshal.FreeHGlobal(EntryPoint.pointer);
            return;
        }
        #endregion
    }
}
