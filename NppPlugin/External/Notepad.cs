using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NppPlugin.External
{
    /// <summary>
    /// Facilitates usage of Notepad++ C/C++ functionality.
    /// </summary>
    /// <remarks>Manages custom forms and menu items.</remarks>
    public abstract class Notepad
    {
        #region Types
        #region Classes
        public class FuncItems : IDisposable
        {
            List<FuncItem> _funcItems;
            int _sizeFuncItem;
            List<IntPtr> _shortCutKeys;
            IntPtr _nativePointer;
            bool _disposed = false;

            public FuncItems()
            {
                _funcItems = new List<FuncItem>();
                _sizeFuncItem = Marshal.SizeOf(typeof(FuncItem));
                _shortCutKeys = new List<IntPtr>();
            }

            [DllImport("kernel32")]
            static extern void RtlMoveMemory(IntPtr Destination, IntPtr Source, int Length);
            public void Add(FuncItem funcItem)
            {
                int oldSize = _funcItems.Count * _sizeFuncItem;
                _funcItems.Add(funcItem);
                int newSize = _funcItems.Count * _sizeFuncItem;
                IntPtr newPointer = Marshal.AllocHGlobal(newSize);

                if (_nativePointer != IntPtr.Zero)
                {
                    RtlMoveMemory(newPointer, _nativePointer, oldSize);
                    Marshal.FreeHGlobal(_nativePointer);
                }
                IntPtr ptrPosNewItem = (IntPtr)((int)newPointer + oldSize);
                byte[] aB = Encoding.Unicode.GetBytes(funcItem._itemName + "\0");
                Marshal.Copy(aB, 0, ptrPosNewItem, aB.Length);
                ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + 128);
                IntPtr p = (funcItem._pFunc != null) ? Marshal.GetFunctionPointerForDelegate(funcItem._pFunc) : IntPtr.Zero;
                Marshal.WriteIntPtr(ptrPosNewItem, p);
                ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + IntPtr.Size);
                Marshal.WriteInt32(ptrPosNewItem, funcItem._cmdID);
                ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + 4);
                Marshal.WriteInt32(ptrPosNewItem, Convert.ToInt32(funcItem._init2Check));
                ptrPosNewItem = (IntPtr)((int)ptrPosNewItem + 4);
                if (funcItem._pShKey._key != 0)
                {
                    IntPtr newShortCutKey = Marshal.AllocHGlobal(4);
                    Marshal.StructureToPtr(funcItem._pShKey, newShortCutKey, false);
                    Marshal.WriteIntPtr(ptrPosNewItem, newShortCutKey);
                }
                else Marshal.WriteIntPtr(ptrPosNewItem, IntPtr.Zero);

                _nativePointer = newPointer;
            }
            public IntPtr NativePointer { get { return _nativePointer; } }
            public List<FuncItem> Items { get { return _funcItems; } }

            public void Dispose()
            {
                if (!_disposed)
                {
                    foreach (IntPtr ptr in _shortCutKeys) Marshal.FreeHGlobal(ptr);
                    if (_nativePointer != IntPtr.Zero) Marshal.FreeHGlobal(_nativePointer);
                    _disposed = true;
                }
            }
            ~FuncItems()
            {
                Dispose();
            }
        }
        #endregion
        #region Delegates
        public delegate void delOnClick();
        #endregion
        #region Enumerations     
        public enum MenuCommands : uint
        {
            IDM = 40000,

            IDM_FILE = (IDM + 1000),
            IDM_FILE_NEW = (IDM_FILE + 1),
            IDM_FILE_OPEN = (IDM_FILE + 2),
            IDM_FILE_CLOSE = (IDM_FILE + 3),
            IDM_FILE_CLOSEALL = (IDM_FILE + 4),
            IDM_FILE_CLOSEALL_BUT_CURRENT = (IDM_FILE + 5),
            IDM_FILE_SAVE = (IDM_FILE + 6),
            IDM_FILE_SAVEALL = (IDM_FILE + 7),
            IDM_FILE_SAVEAS = (IDM_FILE + 8),
            //IDM_FILE_ASIAN_LANG              = (IDM_FILE + 9), 
            IDM_FILE_PRINT = (IDM_FILE + 10),
            IDM_FILE_PRINTNOW = 1001,
            IDM_FILE_EXIT = (IDM_FILE + 11),
            IDM_FILE_LOADSESSION = (IDM_FILE + 12),
            IDM_FILE_SAVESESSION = (IDM_FILE + 13),
            IDM_FILE_RELOAD = (IDM_FILE + 14),
            IDM_FILE_SAVECOPYAS = (IDM_FILE + 15),
            IDM_FILE_DELETE = (IDM_FILE + 16),
            IDM_FILE_RENAME = (IDM_FILE + 17),

            // A mettre à jour si on ajoute nouveau menu item dans le menu "File"
            IDM_FILEMENU_LASTONE = IDM_FILE_RENAME,

            IDM_EDIT = (IDM + 2000),
            IDM_EDIT_CUT = (IDM_EDIT + 1),
            IDM_EDIT_COPY = (IDM_EDIT + 2),
            IDM_EDIT_UNDO = (IDM_EDIT + 3),
            IDM_EDIT_REDO = (IDM_EDIT + 4),
            IDM_EDIT_PASTE = (IDM_EDIT + 5),
            IDM_EDIT_DELETE = (IDM_EDIT + 6),
            IDM_EDIT_SELECTALL = (IDM_EDIT + 7),

            IDM_EDIT_INS_TAB = (IDM_EDIT + 8),
            IDM_EDIT_RMV_TAB = (IDM_EDIT + 9),
            IDM_EDIT_DUP_LINE = (IDM_EDIT + 10),
            IDM_EDIT_TRANSPOSE_LINE = (IDM_EDIT + 11),
            IDM_EDIT_SPLIT_LINES = (IDM_EDIT + 12),
            IDM_EDIT_JOIN_LINES = (IDM_EDIT + 13),
            IDM_EDIT_LINE_UP = (IDM_EDIT + 14),
            IDM_EDIT_LINE_DOWN = (IDM_EDIT + 15),
            IDM_EDIT_UPPERCASE = (IDM_EDIT + 16),
            IDM_EDIT_LOWERCASE = (IDM_EDIT + 17),

            // Menu macro
            IDM_MACRO_STARTRECORDINGMACRO = (IDM_EDIT + 18),
            IDM_MACRO_STOPRECORDINGMACRO = (IDM_EDIT + 19),
            IDM_MACRO_PLAYBACKRECORDEDMACRO = (IDM_EDIT + 21),
            //-----------

            IDM_EDIT_BLOCK_COMMENT = (IDM_EDIT + 22),
            IDM_EDIT_STREAM_COMMENT = (IDM_EDIT + 23),
            IDM_EDIT_TRIMTRAILING = (IDM_EDIT + 24),
            IDM_EDIT_TRIMLINEHEAD = (IDM_EDIT + 42),
            IDM_EDIT_TRIM_BOTH = (IDM_EDIT + 43),
            IDM_EDIT_EOL2WS = (IDM_EDIT + 44),
            IDM_EDIT_TRIMALL = (IDM_EDIT + 45),
            IDM_EDIT_TAB2SW = (IDM_EDIT + 46),
            IDM_EDIT_SW2TAB = (IDM_EDIT + 47),

            // Menu macro
            IDM_MACRO_SAVECURRENTMACRO = (IDM_EDIT + 25),
            //-----------

            IDM_EDIT_RTL = (IDM_EDIT + 26),
            IDM_EDIT_LTR = (IDM_EDIT + 27),
            IDM_EDIT_SETREADONLY = (IDM_EDIT + 28),
            IDM_EDIT_FULLPATHTOCLIP = (IDM_EDIT + 29),
            IDM_EDIT_FILENAMETOCLIP = (IDM_EDIT + 30),
            IDM_EDIT_CURRENTDIRTOCLIP = (IDM_EDIT + 31),

            // Menu macro
            IDM_MACRO_RUNMULTIMACRODLG = (IDM_EDIT + 32),
            //-----------

            IDM_EDIT_CLEARREADONLY = (IDM_EDIT + 33),
            IDM_EDIT_COLUMNMODE = (IDM_EDIT + 34),
            IDM_EDIT_BLOCK_COMMENT_SET = (IDM_EDIT + 35),
            IDM_EDIT_BLOCK_UNCOMMENT = (IDM_EDIT + 36),

            IDM_EDIT_AUTOCOMPLETE = (50000 + 0),
            IDM_EDIT_AUTOCOMPLETE_CURRENTFILE = (50000 + 1),
            IDM_EDIT_FUNCCALLTIP = (50000 + 2),

            //Belong to MENU FILE
            IDM_OPEN_ALL_RECENT_FILE = (IDM_EDIT + 40),
            IDM_CLEAN_RECENT_FILE_LIST = (IDM_EDIT + 41),

            IDM_SEARCH = (IDM + 3000),

            IDM_SEARCH_FIND = (IDM_SEARCH + 1),
            IDM_SEARCH_FINDNEXT = (IDM_SEARCH + 2),
            IDM_SEARCH_REPLACE = (IDM_SEARCH + 3),
            IDM_SEARCH_GOTOLINE = (IDM_SEARCH + 4),
            IDM_SEARCH_TOGGLE_BOOKMARK = (IDM_SEARCH + 5),
            IDM_SEARCH_NEXT_BOOKMARK = (IDM_SEARCH + 6),
            IDM_SEARCH_PREV_BOOKMARK = (IDM_SEARCH + 7),
            IDM_SEARCH_CLEAR_BOOKMARKS = (IDM_SEARCH + 8),
            IDM_SEARCH_GOTOMATCHINGBRACE = (IDM_SEARCH + 9),
            IDM_SEARCH_FINDPREV = (IDM_SEARCH + 10),
            IDM_SEARCH_FINDINCREMENT = (IDM_SEARCH + 11),
            IDM_SEARCH_FINDINFILES = (IDM_SEARCH + 13),
            IDM_SEARCH_VOLATILE_FINDNEXT = (IDM_SEARCH + 14),
            IDM_SEARCH_VOLATILE_FINDPREV = (IDM_SEARCH + 15),
            IDM_SEARCH_CUTMARKEDLINES = (IDM_SEARCH + 18),
            IDM_SEARCH_COPYMARKEDLINES = (IDM_SEARCH + 19),
            IDM_SEARCH_PASTEMARKEDLINES = (IDM_SEARCH + 20),
            IDM_SEARCH_DELETEMARKEDLINES = (IDM_SEARCH + 21),
            IDM_SEARCH_MARKALLEXT1 = (IDM_SEARCH + 22),
            IDM_SEARCH_UNMARKALLEXT1 = (IDM_SEARCH + 23),
            IDM_SEARCH_MARKALLEXT2 = (IDM_SEARCH + 24),
            IDM_SEARCH_UNMARKALLEXT2 = (IDM_SEARCH + 25),
            IDM_SEARCH_MARKALLEXT3 = (IDM_SEARCH + 26),
            IDM_SEARCH_UNMARKALLEXT3 = (IDM_SEARCH + 27),
            IDM_SEARCH_MARKALLEXT4 = (IDM_SEARCH + 28),
            IDM_SEARCH_UNMARKALLEXT4 = (IDM_SEARCH + 29),
            IDM_SEARCH_MARKALLEXT5 = (IDM_SEARCH + 30),
            IDM_SEARCH_UNMARKALLEXT5 = (IDM_SEARCH + 31),
            IDM_SEARCH_CLEARALLMARKS = (IDM_SEARCH + 32),

            IDM_SEARCH_GOPREVMARKER1 = (IDM_SEARCH + 33),
            IDM_SEARCH_GOPREVMARKER2 = (IDM_SEARCH + 34),
            IDM_SEARCH_GOPREVMARKER3 = (IDM_SEARCH + 35),
            IDM_SEARCH_GOPREVMARKER4 = (IDM_SEARCH + 36),
            IDM_SEARCH_GOPREVMARKER5 = (IDM_SEARCH + 37),
            IDM_SEARCH_GOPREVMARKER_DEF = (IDM_SEARCH + 38),

            IDM_SEARCH_GONEXTMARKER1 = (IDM_SEARCH + 39),
            IDM_SEARCH_GONEXTMARKER2 = (IDM_SEARCH + 40),
            IDM_SEARCH_GONEXTMARKER3 = (IDM_SEARCH + 41),
            IDM_SEARCH_GONEXTMARKER4 = (IDM_SEARCH + 42),
            IDM_SEARCH_GONEXTMARKER5 = (IDM_SEARCH + 43),
            IDM_SEARCH_GONEXTMARKER_DEF = (IDM_SEARCH + 44),

            IDM_FOCUS_ON_FOUND_RESULTS = (IDM_SEARCH + 45),
            IDM_SEARCH_GOTONEXTFOUND = (IDM_SEARCH + 46),
            IDM_SEARCH_GOTOPREVFOUND = (IDM_SEARCH + 47),

            IDM_SEARCH_SETANDFINDNEXT = (IDM_SEARCH + 48),
            IDM_SEARCH_SETANDFINDPREV = (IDM_SEARCH + 49),
            IDM_SEARCH_INVERSEMARKS = (IDM_SEARCH + 50),

            IDM_VIEW = (IDM + 4000),
            //IDM_VIEW_TOOLBAR_HIDE            = (IDM_VIEW + 1),
            IDM_VIEW_TOOLBAR_REDUCE = (IDM_VIEW + 2),
            IDM_VIEW_TOOLBAR_ENLARGE = (IDM_VIEW + 3),
            IDM_VIEW_TOOLBAR_STANDARD = (IDM_VIEW + 4),
            IDM_VIEW_REDUCETABBAR = (IDM_VIEW + 5),
            IDM_VIEW_LOCKTABBAR = (IDM_VIEW + 6),
            IDM_VIEW_DRAWTABBAR_TOPBAR = (IDM_VIEW + 7),
            IDM_VIEW_DRAWTABBAR_INACIVETAB = (IDM_VIEW + 8),
            IDM_VIEW_POSTIT = (IDM_VIEW + 9),
            IDM_VIEW_TOGGLE_FOLDALL = (IDM_VIEW + 10),
            IDM_VIEW_USER_DLG = (IDM_VIEW + 11),
            IDM_VIEW_LINENUMBER = (IDM_VIEW + 12),
            IDM_VIEW_SYMBOLMARGIN = (IDM_VIEW + 13),
            IDM_VIEW_FOLDERMAGIN = (IDM_VIEW + 14),
            IDM_VIEW_FOLDERMAGIN_SIMPLE = (IDM_VIEW + 15),
            IDM_VIEW_FOLDERMAGIN_ARROW = (IDM_VIEW + 16),
            IDM_VIEW_FOLDERMAGIN_CIRCLE = (IDM_VIEW + 17),
            IDM_VIEW_FOLDERMAGIN_BOX = (IDM_VIEW + 18),
            IDM_VIEW_ALL_CHARACTERS = (IDM_VIEW + 19),
            IDM_VIEW_INDENT_GUIDE = (IDM_VIEW + 20),
            IDM_VIEW_CURLINE_HILITING = (IDM_VIEW + 21),
            IDM_VIEW_WRAP = (IDM_VIEW + 22),
            IDM_VIEW_ZOOMIN = (IDM_VIEW + 23),
            IDM_VIEW_ZOOMOUT = (IDM_VIEW + 24),
            IDM_VIEW_TAB_SPACE = (IDM_VIEW + 25),
            IDM_VIEW_EOL = (IDM_VIEW + 26),
            IDM_VIEW_EDGELINE = (IDM_VIEW + 27),
            IDM_VIEW_EDGEBACKGROUND = (IDM_VIEW + 28),
            IDM_VIEW_TOGGLE_UNFOLDALL = (IDM_VIEW + 29),
            IDM_VIEW_FOLD_CURRENT = (IDM_VIEW + 30),
            IDM_VIEW_UNFOLD_CURRENT = (IDM_VIEW + 31),
            IDM_VIEW_FULLSCREENTOGGLE = (IDM_VIEW + 32),
            IDM_VIEW_ZOOMRESTORE = (IDM_VIEW + 33),
            IDM_VIEW_ALWAYSONTOP = (IDM_VIEW + 34),
            IDM_VIEW_SYNSCROLLV = (IDM_VIEW + 35),
            IDM_VIEW_SYNSCROLLH = (IDM_VIEW + 36),
            IDM_VIEW_EDGENONE = (IDM_VIEW + 37),
            IDM_VIEW_DRAWTABBAR_CLOSEBOTTUN = (IDM_VIEW + 38),
            IDM_VIEW_DRAWTABBAR_DBCLK2CLOSE = (IDM_VIEW + 39),
            IDM_VIEW_REFRESHTABAR = (IDM_VIEW + 40),
            IDM_VIEW_WRAP_SYMBOL = (IDM_VIEW + 41),
            IDM_VIEW_HIDELINES = (IDM_VIEW + 42),
            IDM_VIEW_DRAWTABBAR_VERTICAL = (IDM_VIEW + 43),
            IDM_VIEW_DRAWTABBAR_MULTILINE = (IDM_VIEW + 44),
            IDM_VIEW_DOCCHANGEMARGIN = (IDM_VIEW + 45),
            IDM_VIEW_LWDEF = (IDM_VIEW + 46),
            IDM_VIEW_LWALIGN = (IDM_VIEW + 47),
            IDM_VIEW_LWINDENT = (IDM_VIEW + 48),
            IDM_VIEW_SUMMARY = (IDM_VIEW + 49),

            IDM_VIEW_FOLD = (IDM_VIEW + 50),
            IDM_VIEW_FOLD_1 = (IDM_VIEW_FOLD + 1),
            IDM_VIEW_FOLD_2 = (IDM_VIEW_FOLD + 2),
            IDM_VIEW_FOLD_3 = (IDM_VIEW_FOLD + 3),
            IDM_VIEW_FOLD_4 = (IDM_VIEW_FOLD + 4),
            IDM_VIEW_FOLD_5 = (IDM_VIEW_FOLD + 5),
            IDM_VIEW_FOLD_6 = (IDM_VIEW_FOLD + 6),
            IDM_VIEW_FOLD_7 = (IDM_VIEW_FOLD + 7),
            IDM_VIEW_FOLD_8 = (IDM_VIEW_FOLD + 8),

            IDM_VIEW_UNFOLD = (IDM_VIEW + 60),
            IDM_VIEW_UNFOLD_1 = (IDM_VIEW_UNFOLD + 1),
            IDM_VIEW_UNFOLD_2 = (IDM_VIEW_UNFOLD + 2),
            IDM_VIEW_UNFOLD_3 = (IDM_VIEW_UNFOLD + 3),
            IDM_VIEW_UNFOLD_4 = (IDM_VIEW_UNFOLD + 4),
            IDM_VIEW_UNFOLD_5 = (IDM_VIEW_UNFOLD + 5),
            IDM_VIEW_UNFOLD_6 = (IDM_VIEW_UNFOLD + 6),
            IDM_VIEW_UNFOLD_7 = (IDM_VIEW_UNFOLD + 7),
            IDM_VIEW_UNFOLD_8 = (IDM_VIEW_UNFOLD + 8),

            IDM_VIEW_GOTO_ANOTHER_VIEW = 10001,
            IDM_VIEW_CLONE_TO_ANOTHER_VIEW = 10002,
            IDM_VIEW_GOTO_NEW_INSTANCE = 10003,
            IDM_VIEW_LOAD_IN_NEW_INSTANCE = 10004,

            IDM_VIEW_SWITCHTO_OTHER_VIEW = (IDM_VIEW + 72),

            IDM_FORMAT = (IDM + 5000),
            IDM_FORMAT_TODOS = (IDM_FORMAT + 1),
            IDM_FORMAT_TOUNIX = (IDM_FORMAT + 2),
            IDM_FORMAT_TOMAC = (IDM_FORMAT + 3),
            IDM_FORMAT_ANSI = (IDM_FORMAT + 4),
            IDM_FORMAT_UTF_8 = (IDM_FORMAT + 5),
            IDM_FORMAT_UCS_2BE = (IDM_FORMAT + 6),
            IDM_FORMAT_UCS_2LE = (IDM_FORMAT + 7),
            IDM_FORMAT_AS_UTF_8 = (IDM_FORMAT + 8),
            IDM_FORMAT_CONV2_ANSI = (IDM_FORMAT + 9),
            IDM_FORMAT_CONV2_AS_UTF_8 = (IDM_FORMAT + 10),
            IDM_FORMAT_CONV2_UTF_8 = (IDM_FORMAT + 11),
            IDM_FORMAT_CONV2_UCS_2BE = (IDM_FORMAT + 12),
            IDM_FORMAT_CONV2_UCS_2LE = (IDM_FORMAT + 13),

            IDM_FORMAT_ENCODE = (IDM_FORMAT + 20),
            IDM_FORMAT_WIN_1250 = (IDM_FORMAT_ENCODE + 0),
            IDM_FORMAT_WIN_1251 = (IDM_FORMAT_ENCODE + 1),
            IDM_FORMAT_WIN_1252 = (IDM_FORMAT_ENCODE + 2),
            IDM_FORMAT_WIN_1253 = (IDM_FORMAT_ENCODE + 3),
            IDM_FORMAT_WIN_1254 = (IDM_FORMAT_ENCODE + 4),
            IDM_FORMAT_WIN_1255 = (IDM_FORMAT_ENCODE + 5),
            IDM_FORMAT_WIN_1256 = (IDM_FORMAT_ENCODE + 6),
            IDM_FORMAT_WIN_1257 = (IDM_FORMAT_ENCODE + 7),
            IDM_FORMAT_WIN_1258 = (IDM_FORMAT_ENCODE + 8),
            IDM_FORMAT_ISO_8859_1 = (IDM_FORMAT_ENCODE + 9),
            IDM_FORMAT_ISO_8859_2 = (IDM_FORMAT_ENCODE + 10),
            IDM_FORMAT_ISO_8859_3 = (IDM_FORMAT_ENCODE + 11),
            IDM_FORMAT_ISO_8859_4 = (IDM_FORMAT_ENCODE + 12),
            IDM_FORMAT_ISO_8859_5 = (IDM_FORMAT_ENCODE + 13),
            IDM_FORMAT_ISO_8859_6 = (IDM_FORMAT_ENCODE + 14),
            IDM_FORMAT_ISO_8859_7 = (IDM_FORMAT_ENCODE + 15),
            IDM_FORMAT_ISO_8859_8 = (IDM_FORMAT_ENCODE + 16),
            IDM_FORMAT_ISO_8859_9 = (IDM_FORMAT_ENCODE + 17),
            IDM_FORMAT_ISO_8859_10 = (IDM_FORMAT_ENCODE + 18),
            IDM_FORMAT_ISO_8859_11 = (IDM_FORMAT_ENCODE + 19),
            IDM_FORMAT_ISO_8859_13 = (IDM_FORMAT_ENCODE + 20),
            IDM_FORMAT_ISO_8859_14 = (IDM_FORMAT_ENCODE + 21),
            IDM_FORMAT_ISO_8859_15 = (IDM_FORMAT_ENCODE + 22),
            IDM_FORMAT_ISO_8859_16 = (IDM_FORMAT_ENCODE + 23),
            IDM_FORMAT_DOS_437 = (IDM_FORMAT_ENCODE + 24),
            IDM_FORMAT_DOS_720 = (IDM_FORMAT_ENCODE + 25),
            IDM_FORMAT_DOS_737 = (IDM_FORMAT_ENCODE + 26),
            IDM_FORMAT_DOS_775 = (IDM_FORMAT_ENCODE + 27),
            IDM_FORMAT_DOS_850 = (IDM_FORMAT_ENCODE + 28),
            IDM_FORMAT_DOS_852 = (IDM_FORMAT_ENCODE + 29),
            IDM_FORMAT_DOS_855 = (IDM_FORMAT_ENCODE + 30),
            IDM_FORMAT_DOS_857 = (IDM_FORMAT_ENCODE + 31),
            IDM_FORMAT_DOS_858 = (IDM_FORMAT_ENCODE + 32),
            IDM_FORMAT_DOS_860 = (IDM_FORMAT_ENCODE + 33),
            IDM_FORMAT_DOS_861 = (IDM_FORMAT_ENCODE + 34),
            IDM_FORMAT_DOS_862 = (IDM_FORMAT_ENCODE + 35),
            IDM_FORMAT_DOS_863 = (IDM_FORMAT_ENCODE + 36),
            IDM_FORMAT_DOS_865 = (IDM_FORMAT_ENCODE + 37),
            IDM_FORMAT_DOS_866 = (IDM_FORMAT_ENCODE + 38),
            IDM_FORMAT_DOS_869 = (IDM_FORMAT_ENCODE + 39),
            IDM_FORMAT_BIG5 = (IDM_FORMAT_ENCODE + 40),
            IDM_FORMAT_GB2312 = (IDM_FORMAT_ENCODE + 41),
            IDM_FORMAT_SHIFT_JIS = (IDM_FORMAT_ENCODE + 42),
            IDM_FORMAT_KOREAN_WIN = (IDM_FORMAT_ENCODE + 43),
            IDM_FORMAT_EUC_KR = (IDM_FORMAT_ENCODE + 44),
            IDM_FORMAT_TIS_620 = (IDM_FORMAT_ENCODE + 45),
            IDM_FORMAT_MAC_CYRILLIC = (IDM_FORMAT_ENCODE + 46),
            IDM_FORMAT_KOI8U_CYRILLIC = (IDM_FORMAT_ENCODE + 47),
            IDM_FORMAT_KOI8R_CYRILLIC = (IDM_FORMAT_ENCODE + 48),
            IDM_FORMAT_ENCODE_END = IDM_FORMAT_KOI8R_CYRILLIC,

            //#define    IDM_FORMAT_CONVERT            200

            IDM_LANG = (IDM + 6000),
            IDM_LANGSTYLE_CONFIG_DLG = (IDM_LANG + 1),
            IDM_LANG_C = (IDM_LANG + 2),
            IDM_LANG_CPP = (IDM_LANG + 3),
            IDM_LANG_JAVA = (IDM_LANG + 4),
            IDM_LANG_HTML = (IDM_LANG + 5),
            IDM_LANG_XML = (IDM_LANG + 6),
            IDM_LANG_JS = (IDM_LANG + 7),
            IDM_LANG_PHP = (IDM_LANG + 8),
            IDM_LANG_ASP = (IDM_LANG + 9),
            IDM_LANG_CSS = (IDM_LANG + 10),
            IDM_LANG_PASCAL = (IDM_LANG + 11),
            IDM_LANG_PYTHON = (IDM_LANG + 12),
            IDM_LANG_PERL = (IDM_LANG + 13),
            IDM_LANG_OBJC = (IDM_LANG + 14),
            IDM_LANG_ASCII = (IDM_LANG + 15),
            IDM_LANG_TEXT = (IDM_LANG + 16),
            IDM_LANG_RC = (IDM_LANG + 17),
            IDM_LANG_MAKEFILE = (IDM_LANG + 18),
            IDM_LANG_INI = (IDM_LANG + 19),
            IDM_LANG_SQL = (IDM_LANG + 20),
            IDM_LANG_VB = (IDM_LANG + 21),
            IDM_LANG_BATCH = (IDM_LANG + 22),
            IDM_LANG_CS = (IDM_LANG + 23),
            IDM_LANG_LUA = (IDM_LANG + 24),
            IDM_LANG_TEX = (IDM_LANG + 25),
            IDM_LANG_FORTRAN = (IDM_LANG + 26),
            IDM_LANG_BASH = (IDM_LANG + 27),
            IDM_LANG_FLASH = (IDM_LANG + 28),
            IDM_LANG_NSIS = (IDM_LANG + 29),
            IDM_LANG_TCL = (IDM_LANG + 30),
            IDM_LANG_LISP = (IDM_LANG + 31),
            IDM_LANG_SCHEME = (IDM_LANG + 32),
            IDM_LANG_ASM = (IDM_LANG + 33),
            IDM_LANG_DIFF = (IDM_LANG + 34),
            IDM_LANG_PROPS = (IDM_LANG + 35),
            IDM_LANG_PS = (IDM_LANG + 36),
            IDM_LANG_RUBY = (IDM_LANG + 37),
            IDM_LANG_SMALLTALK = (IDM_LANG + 38),
            IDM_LANG_VHDL = (IDM_LANG + 39),
            IDM_LANG_CAML = (IDM_LANG + 40),
            IDM_LANG_KIX = (IDM_LANG + 41),
            IDM_LANG_ADA = (IDM_LANG + 42),
            IDM_LANG_VERILOG = (IDM_LANG + 43),
            IDM_LANG_AU3 = (IDM_LANG + 44),
            IDM_LANG_MATLAB = (IDM_LANG + 45),
            IDM_LANG_HASKELL = (IDM_LANG + 46),
            IDM_LANG_INNO = (IDM_LANG + 47),
            IDM_LANG_CMAKE = (IDM_LANG + 48),
            IDM_LANG_YAML = (IDM_LANG + 49),
            IDM_LANG_COBOL = (IDM_LANG + 50),
            IDM_LANG_D = (IDM_LANG + 51),
            IDM_LANG_GUI4CLI = (IDM_LANG + 52),
            IDM_LANG_POWERSHELL = (IDM_LANG + 53),
            IDM_LANG_R = (IDM_LANG + 54),
            IDM_LANG_JSP = (IDM_LANG + 55),

            IDM_LANG_EXTERNAL = (IDM_LANG + 65),
            IDM_LANG_EXTERNAL_LIMIT = (IDM_LANG + 79),

            IDM_LANG_USER = (IDM_LANG + 80),     //46080
            IDM_LANG_USER_LIMIT = (IDM_LANG + 110),    //46110


            IDM_ABOUT = (IDM + 7000),
            IDM_HOMESWEETHOME = (IDM_ABOUT + 1),
            IDM_PROJECTPAGE = (IDM_ABOUT + 2),
            IDM_ONLINEHELP = (IDM_ABOUT + 3),
            IDM_FORUM = (IDM_ABOUT + 4),
            IDM_PLUGINSHOME = (IDM_ABOUT + 5),
            IDM_UPDATE_NPP = (IDM_ABOUT + 6),
            IDM_WIKIFAQ = (IDM_ABOUT + 7),
            IDM_HELP = (IDM_ABOUT + 8),


            IDM_SETTING = (IDM + 8000),
            IDM_SETTING_TAB_SIZE = (IDM_SETTING + 1),
            IDM_SETTING_TAB_REPLCESPACE = (IDM_SETTING + 2),
            IDM_SETTING_HISTORY_SIZE = (IDM_SETTING + 3),
            IDM_SETTING_EDGE_SIZE = (IDM_SETTING + 4),
            IDM_SETTING_IMPORTPLUGIN = (IDM_SETTING + 5),
            IDM_SETTING_IMPORTSTYLETHEMS = (IDM_SETTING + 6),
            IDM_SETTING_TRAYICON = (IDM_SETTING + 8),
            IDM_SETTING_SHORTCUT_MAPPER = (IDM_SETTING + 9),
            IDM_SETTING_REMEMBER_LAST_SESSION = (IDM_SETTING + 10),
            IDM_SETTING_PREFERECE = (IDM_SETTING + 11),
            IDM_SETTING_AUTOCNBCHAR = (IDM_SETTING + 15),
            IDM_SETTING_SHORTCUT_MAPPER_MACRO = (IDM_SETTING + 16),
            IDM_SETTING_SHORTCUT_MAPPER_RUN = (IDM_SETTING + 17),
            IDM_SETTING_EDITCONTEXTMENU = (IDM_SETTING + 18),

            IDM_EXECUTE = (IDM + 9000),

            IDM_SYSTRAYPOPUP = (IDM + 3100),
            IDM_SYSTRAYPOPUP_ACTIVATE = (IDM_SYSTRAYPOPUP + 1),
            IDM_SYSTRAYPOPUP_NEWDOC = (IDM_SYSTRAYPOPUP + 2),
            IDM_SYSTRAYPOPUP_NEW_AND_PASTE = (IDM_SYSTRAYPOPUP + 3),
            IDM_SYSTRAYPOPUP_OPENFILE = (IDM_SYSTRAYPOPUP + 4),
            IDM_SYSTRAYPOPUP_CLOSE = (IDM_SYSTRAYPOPUP + 5)
        }
        #endregion
        #region Flags   
        [Flags]
        public enum DialogMessages : uint
        {
            IDB_CLOSE_DOWN = 137,
            IDB_CLOSE_UP = 138,
            IDD_CONTAINER_DLG = 139,

            IDC_TAB_CONT = 1027,
            IDC_CLIENT_TAB = 1028,
            IDC_BTN_CAPTION = 1050,

            DMM_MSG = 0x5000,
            DMM_CLOSE = (DMM_MSG + 1),
            DMM_DOCK = (DMM_MSG + 2),
            DMM_FLOAT = (DMM_MSG + 3),
            DMM_DOCKALL = (DMM_MSG + 4),
            DMM_FLOATALL = (DMM_MSG + 5),
            DMM_MOVE = (DMM_MSG + 6),
            DMM_UPDATEDISPINFO = (DMM_MSG + 7),
            DMM_GETIMAGELIST = (DMM_MSG + 8),
            DMM_GETICONPOS = (DMM_MSG + 9),
            DMM_DROPDATA = (DMM_MSG + 10),
            DMM_MOVE_SPLITTER = (DMM_MSG + 11),
            DMM_CANCEL_MOVE = (DMM_MSG + 12),
            DMM_LBUTTONUP = (DMM_MSG + 13),

            DMN_FIRST = 1050,
            DMN_CLOSE = (DMN_FIRST + 1),
            //nmhdr.code = DWORD(DMN_CLOSE, 0));
            //nmhdr.hwndFrom = hwndNpp;
            //nmhdr.idFrom = ctrlIdNpp;

            DMN_DOCK = (DMN_FIRST + 2),
            DMN_FLOAT = (DMN_FIRST + 3)
            //nmhdr.code = DWORD(DMN_XXX, int newContainer);
            //nmhdr.hwndFrom = hwndNpp;
            //nmhdr.idFrom = ctrlIdNpp;
        }
        [Flags]
        private enum Messages : uint
        {
            //Here you can find how to use these messages : http://notepad-plus.sourceforge.net/uk/plugins-HOWTO.php 
            NPPMSG = (0x400/*WM_USER*/ + 1000),

            GETCURRENTSCINTILLA = (NPPMSG + 4),
            NPPM_GETCURRENTLANGTYPE = (NPPMSG + 5),
            NPPM_SETCURRENTLANGTYPE = (NPPMSG + 6),

            NPPM_GETNBOPENFILES = (NPPMSG + 7),
            ALL_OPEN_FILES = 0,
            PRIMARY_VIEW = 1,
            SECOND_VIEW = 2,

            NPPM_GETOPENFILENAMES = (NPPMSG + 8),

            NPPM_MODELESSDIALOG = (NPPMSG + 12),
            MODELESSDIALOGADD = 0,
            MODELESSDIALOGREMOVE = 1,

            NPPM_GETNBSESSIONFILES = (NPPMSG + 13),
            NPPM_GETSESSIONFILES = (NPPMSG + 14),
            NPPM_SAVESESSION = (NPPMSG + 15),
            NPPM_SAVECURRENTSESSION = (NPPMSG + 16),
            //struct sessionInfo {
            //    TCHAR* sessionFilePathName;
            //    int nbFile;
            //    TCHAR** files;
            //};

            NPPM_GETOPENFILENAMESPRIMARY = (NPPMSG + 17),
            NPPM_GETOPENFILENAMESSECOND = (NPPMSG + 18),

            NPPM_CREATESCINTILLAHANDLE = (NPPMSG + 20),
            NPPM_DESTROYSCINTILLAHANDLE = (NPPMSG + 21),
            NPPM_GETNBUSERLANG = (NPPMSG + 22),

            NPPM_GETCURRENTDOCINDEX = (NPPMSG + 23),
            MAIN_VIEW = 0,
            SUB_VIEW = 1,

            NPPM_SETSTATUSBAR = (NPPMSG + 24),
            STATUSBAR_DOC_TYPE = 0,
            STATUSBAR_DOC_SIZE = 1,
            STATUSBAR_CUR_POS = 2,
            STATUSBAR_EOF_FORMAT = 3,
            STATUSBAR_UNICODE_TYPE = 4,
            STATUSBAR_TYPING_MODE = 5,

            GETMENUHANDLE = (NPPMSG + 25),
            PLUGINMENU = 0,
            NPPMAINMENU = 1,
            // INT NPPM_GETMENUHANDLE(INT menuChoice, 0)
            // Return: menu handle (HMENU) of choice (plugin menu handle or Notepad++ main menu handle)

            NPPM_ENCODESCI = (NPPMSG + 26),
            //ascii file to unicode
            //int NPPM_ENCODESCI(MAIN_VIEW/SUB_VIEW, 0)
            //return new unicodeMode

            NPPM_DECODESCI = (NPPMSG + 27),
            //unicode file to ascii
            //int NPPM_DECODESCI(MAIN_VIEW/SUB_VIEW, 0)
            //return old unicodeMode

            NPPM_ACTIVATEDOC = (NPPMSG + 28),
            //void NPPM_ACTIVATEDOC(int view, int index2Activate)

            NPPM_LAUNCHFINDINFILESDLG = (NPPMSG + 29),
            //void NPPM_LAUNCHFINDINFILESDLG(TCHAR * dir2Search, TCHAR * filtre)

            DMMSHOW = (NPPMSG + 30),
            DMMHIDE = (NPPMSG + 31),
            NPPM_DMMUPDATEDISPINFO = (NPPMSG + 32),
            //void NPPM_DMMxxx(0, tTbData->Handle)

            DMMREGASDCKDLG = (NPPMSG + 33),
            //void NPPM_DMMREGASDCKDLG(0, &tTbData)

            NPPM_LOADSESSION = (NPPMSG + 34),
            //void NPPM_LOADSESSION(0, const TCHAR* file name)

            NPPM_DMMVIEWOTHERTAB = (NPPMSG + 35),
            //void WM_DMM_VIEWOTHERTAB(0, tTbData->Name)

            RELOADFILE = (NPPMSG + 36),
            //BOOL NPPM_RELOADFILE(BOOL withAlert, TCHAR *filePathName2Reload)

            NPPM_SWITCHTOFILE = (NPPMSG + 37),
            //BOOL NPPM_SWITCHTOFILE(0, TCHAR *filePathName2switch)

            NPPM_SAVECURRENTFILE = (NPPMSG + 38),
            //BOOL NPPM_SAVECURRENTFILE(0, 0)

            NPPM_SAVEALLFILES = (NPPMSG + 39),
            //BOOL NPPM_SAVEALLFILES(0, 0)

            NPPM_SETMENUITEMCHECK = (NPPMSG + 40),
            //void WM_PIMENU_CHECK(UINT    funcItem[X]._cmdID, TRUE/FALSE)

            NPPM_ADDTOOLBARICON = (NPPMSG + 41),
            //void WM_ADDTOOLBARICON(UINT funcItem[X]._cmdID, toolbarIcons icon)
            //struct toolbarIcons {
            //    HBITMAP    hToolbarBmp;
            //    HICON    hToolbarIcon;
            //};

            NPPM_GETWINDOWSVERSION = (NPPMSG + 42),
            //winVer NPPM_GETWINDOWSVERSION(0, 0)

            NPPM_DMMGETPLUGINHWNDBYNAME = (NPPMSG + 43),
            //HWND WM_DMM_GETPLUGINHWNDBYNAME(const TCHAR *windowName, const TCHAR *moduleName)
            // if moduleName is NULL, then return value is NULL
            // if windowName is NULL, then the first found window handle which matches with the moduleName will be returned

            NPPM_MAKECURRENTBUFFERDIRTY = (NPPMSG + 44),
            //BOOL NPPM_MAKECURRENTBUFFERDIRTY(0, 0)

            NPPM_GETENABLETHEMETEXTUREFUNC = (NPPMSG + 45),
            //BOOL NPPM_GETENABLETHEMETEXTUREFUNC(0, 0)

            NPPM_GETPLUGINSCONFIGDIR = (NPPMSG + 46),
            //void NPPM_GETPLUGINSCONFIGDIR(int strLen, TCHAR *str)

            NPPM_MSGTOPLUGIN = (NPPMSG + 47),
            //BOOL NPPM_MSGTOPLUGIN(TCHAR *destModuleName, CommunicationInfo *info)
            // return value is TRUE when the message arrive to the destination plugins.
            // if destModule or info is NULL, then return value is FALSE
            //struct CommunicationInfo {
            //    long internalMsg;
            //    const TCHAR * srcModuleName;
            //    void * info; // defined by plugin
            //};

            NPPM_MENUCOMMAND = (NPPMSG + 48),
            //void NPPM_MENUCOMMAND(0, int cmdID)
            // uncomment //#include "menuCmdID.h"
            // in the beginning of this file then use the command symbols defined in "menuCmdID.h" file
            // to access all the Notepad++ menu command items

            NPPM_TRIGGERTABBARCONTEXTMENU = (NPPMSG + 49),
            //void NPPM_TRIGGERTABBARCONTEXTMENU(int view, int index2Activate)

            NPPM_GETNPPVERSION = (NPPMSG + 50),
            // int NPPM_GETNPPVERSION(0, 0)
            // return version 
            // ex : v4.6
            // HIWORD(version) == 4
            // LOWORD(version) == 6

            NPPM_HIDETABBAR = (NPPMSG + 51),
            // BOOL NPPM_HIDETABBAR(0, BOOL hideOrNot)
            // if hideOrNot is set as TRUE then tab bar will be hidden
            // otherwise it'll be shown.
            // return value : the old status value

            NPPM_ISTABBARHIDDEN = (NPPMSG + 52),
            // BOOL NPPM_ISTABBARHIDDEN(0, 0)
            // returned value : TRUE if tab bar is hidden, otherwise FALSE

            NPPM_GETPOSFROMBUFFERID = (NPPMSG + 57),
            // INT NPPM_GETPOSFROMBUFFERID(INT bufferID, 0)
            // Return VIEW|INDEX from a buffer ID. -1 if the bufferID non existing
            // if priorityView set to SUB_VIEW, then SUB_VIEW will be search firstly
            //
            // VIEW takes 2 highest bits and INDEX (0 based) takes the rest (30 bits) 
            // Here's the values for the view :
            //  MAIN_VIEW 0
            //  SUB_VIEW  1

            NPPM_GETFULLPATHFROMBUFFERID = (NPPMSG + 58),
            // INT NPPM_GETFULLPATHFROMBUFFERID(INT bufferID, TCHAR *fullFilePath)
            // Get full path file name from a bufferID. 
            // Return -1 if the bufferID non existing, otherwise the number of TCHAR copied/to copy
            // User should call it with fullFilePath be NULL to get the number of TCHAR (not including the nul character),
            // allocate fullFilePath with the return values + 1, then call it again to get  full path file name

            NPPM_GETBUFFERIDFROMPOS = (NPPMSG + 59),
            // INT NPPM_GETBUFFERIDFROMPOS(INT index, INT iView)
            //wParam: Position of document
            //lParam: View to use, 0 = Main, 1 = Secondary
            //Returns 0 if invalid

            NPPM_GETCURRENTBUFFERID = (NPPMSG + 60),
            // INT NPPM_GETCURRENTBUFFERID(0, 0)
            //Returns active Buffer

            NPPM_RELOADBUFFERID = (NPPMSG + 61),
            // VOID NPPM_RELOADBUFFERID(0, 0)
            //Reloads Buffer
            //wParam: Buffer to reload
            //lParam: 0 if no alert, else alert


            NPPM_GETBUFFERLANGTYPE = (NPPMSG + 64),
            // INT NPPM_GETBUFFERLANGTYPE(INT bufferID, 0)
            //wParam: BufferID to get LangType from
            //lParam: 0
            //Returns as int, see LangType. -1 on error

            NPPM_SETBUFFERLANGTYPE = (NPPMSG + 65),
            // BOOL NPPM_SETBUFFERLANGTYPE(INT bufferID, INT langType)
            //wParam: BufferID to set LangType of
            //lParam: LangType
            //Returns TRUE on success, FALSE otherwise
            //use int, see LangType for possible values
            //L_USER and L_EXTERNAL are not supported

            NPPM_GETBUFFERENCODING = (NPPMSG + 66),
            // INT NPPM_GETBUFFERENCODING(INT bufferID, 0)
            //wParam: BufferID to get encoding from
            //lParam: 0
            //returns as int, see UniMode. -1 on error

            NPPM_SETBUFFERENCODING = (NPPMSG + 67),
            // BOOL NPPM_SETBUFFERENCODING(INT bufferID, INT encoding)
            //wParam: BufferID to set encoding of
            //lParam: format
            //Returns TRUE on success, FALSE otherwise
            //use int, see UniMode
            //Can only be done on new, unedited files

            NPPM_GETBUFFERFORMAT = (NPPMSG + 68),
            // INT NPPM_GETBUFFERFORMAT(INT bufferID, 0)
            //wParam: BufferID to get format from
            //lParam: 0
            //returns as int, see formatType. -1 on error

            NPPM_SETBUFFERFORMAT = (NPPMSG + 69),
            // BOOL NPPM_SETBUFFERFORMAT(INT bufferID, INT format)
            //wParam: BufferID to set format of
            //lParam: format
            //Returns TRUE on success, FALSE otherwise
            //use int, see formatType

            /*
            NPPM_ADDREBAR = (NPPMSG + 57),
            // BOOL NPPM_ADDREBAR(0, REBARBANDINFO *)
            // Returns assigned ID in wID value of struct pointer
            NPPM_UPDATEREBAR = (NPPMSG + 58),
            // BOOL NPPM_ADDREBAR(INT ID, REBARBANDINFO *)
            //Use ID assigned with NPPM_ADDREBAR
            NPPM_REMOVEREBAR = (NPPMSG + 59),
            // BOOL NPPM_ADDREBAR(INT ID, 0)
            //Use ID assigned with NPPM_ADDREBAR
            */

            NPPM_HIDETOOLBAR = (NPPMSG + 70),
            // BOOL NPPM_HIDETOOLBAR(0, BOOL hideOrNot)
            // if hideOrNot is set as TRUE then tool bar will be hidden
            // otherwise it'll be shown.
            // return value : the old status value

            NPPM_ISTOOLBARHIDDEN = (NPPMSG + 71),
            // BOOL NPPM_ISTOOLBARHIDDEN(0, 0)
            // returned value : TRUE if tool bar is hidden, otherwise FALSE

            NPPM_HIDEMENU = (NPPMSG + 72),
            // BOOL NPPM_HIDEMENU(0, BOOL hideOrNot)
            // if hideOrNot is set as TRUE then menu will be hidden
            // otherwise it'll be shown.
            // return value : the old status value

            NPPM_ISMENUHIDDEN = (NPPMSG + 73),
            // BOOL NPPM_ISMENUHIDDEN(0, 0)
            // returned value : TRUE if menu is hidden, otherwise FALSE

            NPPM_HIDESTATUSBAR = (NPPMSG + 74),
            // BOOL NPPM_HIDESTATUSBAR(0, BOOL hideOrNot)
            // if hideOrNot is set as TRUE then STATUSBAR will be hidden
            // otherwise it'll be shown.
            // return value : the old status value

            NPPM_ISSTATUSBARHIDDEN = (NPPMSG + 75),
            // BOOL NPPM_ISSTATUSBARHIDDEN(0, 0)
            // returned value : TRUE if STATUSBAR is hidden, otherwise FALSE

            NPPM_GETSHORTCUTBYCMDID = (NPPMSG + 76),
            // BOOL NPPM_GETSHORTCUTBYCMDID(int cmdID, ShortcutKey *sk)
            // get your plugin command current mapped shortcut into sk via cmdID
            // You may need it after getting NPPN_READY notification
            // returned value : TRUE if this function call is successful and shorcut is enable, otherwise FALSE

            NPPM_DOOPEN = (NPPMSG + 77),
            // BOOL NPPM_DOOPEN(0, const TCHAR *fullPathName2Open)
            // fullPathName2Open indicates the full file path name to be opened.
            // The return value is TRUE (1) if the operation is successful, otherwise FALSE (0).

            NPPM_SAVECURRENTFILEAS = (NPPMSG + 78),
            // BOOL NPPM_SAVECURRENTFILEAS (BOOL asCopy, const TCHAR* filename)

            NPPM_GETCURRENTNATIVELANGENCODING = (NPPMSG + 79),
            // INT NPPM_GETCURRENTNATIVELANGENCODING(0, 0)
            // returned value : the current native language enconding

            NPPM_ALLOCATESUPPORTED = (NPPMSG + 80),
            // returns TRUE if NPPM_ALLOCATECMDID is supported
            // Use to identify if subclassing is necessary

            ALLOCATECMDID = (NPPMSG + 81),
            // BOOL NPPM_ALLOCATECMDID(int numberRequested, int* startNumber)
            // sets startNumber to the initial command ID if successful
            // Returns: TRUE if successful, FALSE otherwise. startNumber will also be set to 0 if unsuccessful

            NPPM_ALLOCATEMARKER = (NPPMSG + 82),
            // BOOL NPPM_ALLOCATEMARKER(int numberRequested, int* startNumber)
            // sets startNumber to the initial command ID if successful
            // Allocates a marker number to a plugin
            // Returns: TRUE if successful, FALSE otherwise. startNumber will also be set to 0 if unsuccessful

            NPPM_GETLANGUAGENAME = (NPPMSG + 83),
            // INT NPPM_GETLANGUAGENAME(int langType, TCHAR *langName)
            // Get programing language name from the given language type (LangType)
            // Return value is the number of copied character / number of character to copy (\0 is not included)
            // You should call this function 2 times - the first time you pass langName as NULL to get the number of characters to copy.
            // You allocate a buffer of the length of (the number of characters + 1) then call NPPM_GETLANGUAGENAME function the 2nd time 
            // by passing allocated buffer as argument langName

            NPPM_GETLANGUAGEDESC = (NPPMSG + 84),
            // INT NPPM_GETLANGUAGEDESC(int langType, TCHAR *langDesc)
            // Get programing language short description from the given language type (LangType)
            // Return value is the number of copied character / number of character to copy (\0 is not included)
            // You should call this function 2 times - the first time you pass langDesc as NULL to get the number of characters to copy.
            // You allocate a buffer of the length of (the number of characters + 1) then call NPPM_GETLANGUAGEDESC function the 2nd time 
            // by passing allocated buffer as argument langDesc

            NPPM_SHOWDOCSWITCHER = (NPPMSG + 85),
            // VOID NPPM_ISDOCSWITCHERSHOWN(0, BOOL toShowOrNot)
            // Send this message to show or hide doc switcher.
            // if toShowOrNot is TRUE then show doc switcher, otherwise hide it.

            NPPM_ISDOCSWITCHERSHOWN = (NPPMSG + 86),
            // BOOL NPPM_ISDOCSWITCHERSHOWN(0, 0)
            // Check to see if doc switcher is shown.

            NPPM_GETAPPDATAPLUGINSALLOWED = (NPPMSG + 87),
            // BOOL NPPM_GETAPPDATAPLUGINSALLOWED(0, 0)
            // Check to see if loading plugins from "%APPDATA%\Notepad++\plugins" is allowed.

            NPPM_GETCURRENTVIEW = (NPPMSG + 88),
            // INT NPPM_GETCURRENTVIEW(0, 0)
            // Return: current edit view of Notepad++. Only 2 possible values: 0 = Main, 1 = Secondary

            NPPM_DOCSWITCHERDISABLECOLUMN = (NPPMSG + 89),
            // VOID NPPM_DOCSWITCHERDISABLECOLUMN(0, BOOL disableOrNot)
            // Disable or enable extension column of doc switcher

            NPPM_GETEDITORDEFAULTFOREGROUNDCOLOR = (NPPMSG + 90),
            // INT NPPM_GETEDITORDEFAULTFOREGROUNDCOLOR(0, 0)
            // Return: current editor default foreground color. You should convert the returned value in COLORREF

            NPPM_GETEDITORDEFAULTBACKGROUNDCOLOR = (NPPMSG + 91),
            // INT NPPM_GETEDITORDEFAULTBACKGROUNDCOLOR(0, 0)
            // Return: current editor default background color. You should convert the returned value in COLORREF

            RUNCOMMAND_USER = (0x400/*WM_USER*/ + 3000),
            GETFULLCURRENTPATH = (RUNCOMMAND_USER + FULL_CURRENT_PATH),
            NPPM_GETCURRENTDIRECTORY = (RUNCOMMAND_USER + CURRENT_DIRECTORY),
            NPPM_GETFILENAME = (RUNCOMMAND_USER + FILE_NAME),
            NPPM_GETNAMEPART = (RUNCOMMAND_USER + NAME_PART),
            NPPM_GETEXTPART = (RUNCOMMAND_USER + EXT_PART),
            NPPM_GETCURRENTWORD = (RUNCOMMAND_USER + CURRENT_WORD),
            NPPM_GETNPPDIRECTORY = (RUNCOMMAND_USER + NPP_DIRECTORY),
            // BOOL NPPM_GETXXXXXXXXXXXXXXXX(size_t strLen, TCHAR *str)
            // where str is the allocated TCHAR array,
            //         strLen is the allocated array size
            // The return value is TRUE when get generic_string operation success
            // Otherwise (allocated array size is too small) FALSE

            GETCURRENTLINE = (RUNCOMMAND_USER + CURRENT_LINE),
            // INT NPPM_GETCURRENTLINE(0, 0)
            // return the caret current position line
            NPPM_GETCURRENTCOLUMN = (RUNCOMMAND_USER + CURRENT_COLUMN),
            // INT NPPM_GETCURRENTCOLUMN(0, 0)
            // return the caret current position column
            VAR_NOT_RECOGNIZED = 0,
            FULL_CURRENT_PATH = 1,
            CURRENT_DIRECTORY = 2,
            FILE_NAME = 3,
            NAME_PART = 4,
            EXT_PART = 5,
            CURRENT_WORD = 6,
            NPP_DIRECTORY = 7,
            CURRENT_LINE = 8,
            CURRENT_COLUMN = 9,

            // Notification code
            NPPN_FIRST = 1000,
            READY = (NPPN_FIRST + 1), // To notify plugins that all the procedures of launchment of notepad++ are done.
                                      //scnNotification->nmhdr.code = NPPN_READY;
                                      //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                      //scnNotification->nmhdr.idFrom = 0;

            NPPN_TBMODIFICATION = (NPPN_FIRST + 2), // To notify plugins that toolbar icons can be registered
                                                    //scnNotification->nmhdr.code = NPPN_TB_MODIFICATION;
                                                    //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                    //scnNotification->nmhdr.idFrom = 0;

            NPPN_FILEBEFORECLOSE = (NPPN_FIRST + 3), // To notify plugins that the current file is about to be closed
                                                     //scnNotification->nmhdr.code = NPPN_FILEBEFORECLOSE;
                                                     //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                     //scnNotification->nmhdr.idFrom = BufferID;

            NPPN_FILEOPENED = (NPPN_FIRST + 4), // To notify plugins that the current file is just opened
                                                //scnNotification->nmhdr.code = NPPN_FILEOPENED;
                                                //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                //scnNotification->nmhdr.idFrom = BufferID;

            NPPN_FILECLOSED = (NPPN_FIRST + 5), // To notify plugins that the current file is just closed
                                                //scnNotification->nmhdr.code = NPPN_FILECLOSED;
                                                //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                //scnNotification->nmhdr.idFrom = BufferID;

            NPPN_FILEBEFOREOPEN = (NPPN_FIRST + 6), // To notify plugins that the current file is about to be opened
                                                    //scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
                                                    //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                    //scnNotification->nmhdr.idFrom = BufferID;

            NPPN_FILEBEFORESAVE = (NPPN_FIRST + 7), // To notify plugins that the current file is about to be saved
                                                    //scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
                                                    //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                    //scnNotification->nmhdr.idFrom = BufferID;

            NPPN_FILESAVED = (NPPN_FIRST + 8), // To notify plugins that the current file is just saved
                                               //scnNotification->nmhdr.code = NPPN_FILESAVED;
                                               //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                               //scnNotification->nmhdr.idFrom = BufferID;

            SHUTDOWN = (NPPN_FIRST + 9), // To notify plugins that Notepad++ is about to be shutdowned.
                                         //scnNotification->nmhdr.code = NPPN_SHUTDOWN;
                                         //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                         //scnNotification->nmhdr.idFrom = 0;

            BUFFERACTIVATED = (NPPN_FIRST + 10), // To notify plugins that a buffer was activated (put to foreground).
                                                 //scnNotification->nmhdr.code = NPPN_BUFFERACTIVATED;
                                                 //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                 //scnNotification->nmhdr.idFrom = activatedBufferID;

            NPPN_LANGCHANGED = (NPPN_FIRST + 11), // To notify plugins that the language in the current doc is just changed.
                                                  //scnNotification->nmhdr.code = NPPN_LANGCHANGED;
                                                  //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                  //scnNotification->nmhdr.idFrom = currentBufferID;

            NPPN_WORDSTYLESUPDATED = (NPPN_FIRST + 12), // To notify plugins that user initiated a WordStyleDlg change.
                                                        //scnNotification->nmhdr.code = NPPN_WORDSTYLESUPDATED;
                                                        //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                        //scnNotification->nmhdr.idFrom = currentBufferID;

            NPPN_SHORTCUTREMAPPED = (NPPN_FIRST + 13), // To notify plugins that plugin command shortcut is remapped.
                                                       //scnNotification->nmhdr.code = NPPN_SHORTCUTSREMAPPED;
                                                       //scnNotification->nmhdr.hwndFrom = ShortcutKeyStructurePointer;
                                                       //scnNotification->nmhdr.idFrom = cmdID;
                                                       //where ShortcutKeyStructurePointer is pointer of struct ShortcutKey:
                                                       //struct ShortcutKey {
                                                       //    bool _isCtrl;
                                                       //    bool _isAlt;
                                                       //    bool _isShift;
                                                       //    UCHAR _key;
                                                       //};

            NPPN_FILEBEFORELOAD = (NPPN_FIRST + 14), // To notify plugins that the current file is about to be loaded
                                                     //scnNotification->nmhdr.code = NPPN_FILEBEFOREOPEN;
                                                     //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                     //scnNotification->nmhdr.idFrom = NULL;

            NPPN_FILELOADFAILED = (NPPN_FIRST + 15),  // To notify plugins that file open operation failed
                                                      //scnNotification->nmhdr.code = NPPN_FILEOPENFAILED;
                                                      //scnNotification->nmhdr.hwndFrom = hwndNpp;
                                                      //scnNotification->nmhdr.idFrom = BufferID;

            NPPN_READONLYCHANGED = (NPPN_FIRST + 16),  // To notify plugins that current document change the readonly status,
                                                       //scnNotification->nmhdr.code = NPPN_READONLYCHANGED;
                                                       //scnNotification->nmhdr.hwndFrom = bufferID;
                                                       //scnNotification->nmhdr.idFrom = docStatus;
                                                       // where bufferID is BufferID
                                                       //       docStatus can be combined by DOCSTAUS_READONLY and DOCSTAUS_BUFFERDIRTY

            DOCSTAUS_READONLY = 1,
            DOCSTAUS_BUFFERDIRTY = 2,

            NPPN_DOCORDERCHANGED = (NPPN_FIRST + 16),  // To notify plugins that document order is changed
                                                       //scnNotification->nmhdr.code = NPPN_DOCORDERCHANGED;
                                                       //scnNotification->nmhdr.hwndFrom = newIndex;
                                                       //scnNotification->nmhdr.idFrom = BufferID;

            NPPN_SNAPSHOTDIRTYFILELOADED = (NPPN_FIRST + 18),  // To notify plugins that a snapshot dirty file is loaded on startup
                                                               //scnNotification->nmhdr.code = NPPN_SNAPSHOTDIRTYFILELOADED;
                                                               //scnNotification->nmhdr.hwndFrom = NULL;
                                                               //scnNotification->nmhdr.idFrom = BufferID;
        }
        [Flags]
        public enum WindowOptions : uint
        {
            // styles for containers
            //CAPTION_TOP                = 1,
            //CAPTION_BOTTOM            = 0,

            // defines for docking manager
            CONT_LEFT = 0,
            CONT_RIGHT = 1,
            CONT_TOP = 2,
            CONT_BOTTOM = 3,
            DOCKCONT_MAX = 4,

            // mask params for plugins of internal dialogs
            DWS_ICONTAB = 0x00000001,            // Icon for tabs are available
            DWS_ICONBAR = 0x00000002,            // Icon for icon bar are available (currently not supported)
            DWS_ADDINFO = 0x00000004,            // Additional information are in use
            DWS_PARAMSALL = (DWS_ICONTAB | DWS_ICONBAR | DWS_ADDINFO),

            // default docking values for first call of plugin
            DWS_DF_CONT_LEFT = (CONT_LEFT << 28),    // default docking on left
            DWS_DF_CONT_RIGHT = (CONT_RIGHT << 28),    // default docking on right
            DWS_DF_CONT_TOP = (CONT_TOP << 28),        // default docking on top
            DWS_DF_CONT_BOTTOM = (CONT_BOTTOM << 28),    // default docking on bottom
            DWS_DF_FLOATING = 0x80000000            // default state is floating
        }
        #endregion
        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct Data
        {
            public IntPtr _nppHandle;
            public IntPtr _scintillaMainHandle;
            public IntPtr _scintillaSecondHandle;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FuncItem
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string _itemName;
            public delOnClick _pFunc;
            public int _cmdID;
            public bool _init2Check;
            public ShortcutKey _pShKey;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct ShortcutKey
        {
            public ShortcutKey(bool isCtrl, bool isAlt, bool isShift, Keys key)
            {
                // the types 'bool' and 'char' have a size of 1 byte only!
                _isCtrl = Convert.ToByte(isCtrl);
                _isAlt = Convert.ToByte(isAlt);
                _isShift = Convert.ToByte(isShift);
                _key = Convert.ToByte(key);
            }
            public byte _isCtrl;
            public byte _isAlt;
            public byte _isShift;
            public byte _key;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct Window
        {
            public IntPtr Handle;            // HWND: client Window Handle
            public string Name;            // TCHAR*: name of plugin (shown in window)
            public int ID;                // int: a funcItem provides the function pointer to start a dialog. Please parse here these ID
                                          // user modifications
            public WindowOptions Options;                // UINT: mask params: look to above defines
            public uint Icon;            // HICON: icon for tabs
            public string pszAddInfo;        // TCHAR*: for plugin to display additional informations
                                             // internal data, do not use !!!
            public Windows.Rect rcFloat;            // RECT: floating position
            public int iPrevCont;           // int: stores the privious container (toggling between float and dock)
            public string PluginName;    // const TCHAR*: it's the plugin file name. It's used to identify the plugin
        }
        #endregion
        #endregion
        #region Static Properties
        internal static string CurrentDocumentPath
        {
            get
            {
                StringBuilder path = new StringBuilder(Windows.MAX_PATH);
                Windows.SendMessage(Handles._nppHandle, (uint)Notepad.Messages.GETFULLCURRENTPATH, 0, path);
                return path.ToString();
            }
        }
        internal static int CurrentLine
            => Windows.SendMessage(Handles._nppHandle, (uint)Notepad.Messages.GETCURRENTLINE, 0, 0).ToInt32();
        private static Data Handles { get; set; }
        internal static IntPtr PluginMenuHandle => Windows.SendMessage(Handles._nppHandle,
            (uint)Notepad.Messages.GETMENUHANDLE, (int)Notepad.Messages.PLUGINMENU, 0);
        internal static IntPtr Scintilla
        {
            get
            {
                int curScintilla;
                Windows.SendMessage(Handles._nppHandle, (uint)Notepad.Messages.GETCURRENTSCINTILLA, 0, out curScintilla);
                return (curScintilla == 0) ? Handles._scintillaMainHandle : Handles._scintillaSecondHandle;
            }
        }
        internal static bool ShuttingDown { get; private set; }
        #endregion   
        #region Static Members                                
        internal static event EventHandler BufferActivated = null;
        internal static event EventHandler Ready = null;
        internal static event EventHandler ShutDown = null;
        #endregion
        #region Static Methods
        static Notepad()
        {
            Notepad.ShuttingDown = false;
            EntryPoint.Notified += Notepad.Notified;
            return;
        }
        internal static void Initialize(Data handle)
        {
            Notepad.Handles = handle;
        }
        internal static Notepad.FuncItem AllocateCommandID(string command_name,
            Notepad.delOnClick functionPointer, Notepad.ShortcutKey shortcut = new Notepad.ShortcutKey(),
            bool start_checked = false)
        {
            // do some pointer magic
            int[] handle_value = new int[1];
            GCHandle handle = GCHandle.Alloc(handle_value, GCHandleType.Pinned);
            IntPtr pointer = IntPtr.Zero;
            try
            {
                pointer = handle.AddrOfPinnedObject();
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            Windows.SendMessage(Notepad.Handles._nppHandle, (uint)Notepad.Messages.ALLOCATECMDID, 1, pointer);
            return Notepad.SetCommand(handle_value[0], command_name, functionPointer, shortcut, start_checked);
        }

        internal static IntPtr GetSubMenu(string plugin_name)
        {
            IntPtr npp_menu_handle = Notepad.PluginMenuHandle;
            if (npp_menu_handle != IntPtr.Zero)
            {
                StringBuilder menu_text = new StringBuilder(20);
                int count = Windows.GetMenuItemCount(npp_menu_handle);
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        Windows.GetMenuString(npp_menu_handle, (uint)i, menu_text, 20, Windows.MF_BYPOSITION);
                    }
                    catch (Exception e)
                    {
                        // TODO: implement exception handling
                        continue;
                    }
                    if (menu_text.ToString().Contains(plugin_name))
                    {
                        IntPtr sub_menu_handle = Windows.GetSubMenu(npp_menu_handle, i);
                        return sub_menu_handle;
                    }
                }
            }
            return IntPtr.Zero;
        }
        internal static void RegisterForm(Form form, Icon icon, int id, string name, Notepad.WindowOptions options, string plugin_name)
        {
            try
            {
                Notepad.Window data = new Notepad.Window
                {
                    Handle = form.Handle,
                    Icon = icon == null ? 0 : (uint)icon.Handle,
                    ID = id,
                    Name = name,
                    Options = options,
                    PluginName = plugin_name
                };
                IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(data));
                Marshal.StructureToPtr(data, pointer, false);
                Windows.SendMessage(Notepad.Handles._nppHandle, (uint)Notepad.Messages.DMMREGASDCKDLG, 0, pointer);
            }
            catch (Exception e)
            {
                // TODO: implement exception handling
            }
            return;
        }
        internal static void ReloadFile(string path)
        {
            Windows.SendMessage(Notepad.Handles._nppHandle, (uint)Notepad.Messages.RELOADFILE, 0, path);
            return;
        }
        private static Notepad.FuncItem SetCommand(int item, string command_name,
            Notepad.delOnClick function_pointer, Notepad.ShortcutKey shortcut = new Notepad.ShortcutKey(),
            bool start_checked = false)
        {
            Notepad.FuncItem func_item = new Notepad.FuncItem
            {
                _cmdID = (int)item,
                _itemName = command_name
            };
            if (function_pointer != null)
                func_item._pFunc = new Notepad.delOnClick(function_pointer);
            if (shortcut._key != 0)
                func_item._pShKey = shortcut;
            func_item._init2Check = start_checked;
            return func_item;
        }
        internal static void SetFormVisible(IntPtr handle, bool visible)
        {
            Windows.SendMessage(Handles._nppHandle, (uint)(visible ? Notepad.Messages.DMMSHOW : Notepad.Messages.DMMHIDE),
                0, handle);
            return;
        }
        #endregion
        #region Static Events
        private static void Notified(EntryPoint.Notification notification)
        {
            switch (notification.nmhdr.code)
            {
                default:
                    break;
                case (uint)Notepad.Messages.READY:
                    if (Notepad.Ready != null)
                        Notepad.Ready(null, EventArgs.Empty);
                    break;
                case (uint)Notepad.Messages.BUFFERACTIVATED:
                    if (Notepad.BufferActivated != null)
                        Notepad.BufferActivated(null, EventArgs.Empty);
                    break;
                case (uint)Notepad.Messages.SHUTDOWN:
                    Notepad.ShuttingDown = true;
                    if (Notepad.ShutDown != null)
                        Notepad.ShutDown(null, EventArgs.Empty);
                    break;
            }
            return;
        }
        #endregion
    }
}
