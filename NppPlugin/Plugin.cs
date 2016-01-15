using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using NppPlugin.External;

namespace NppPlugin
{
    public class Plugin
    {
        #region Types
        /// <summary>
        /// Indices of each menu item. This enumeration must match menu item order.
        /// </summary>
        private enum MenuItems : int { Example = 0};
        #endregion
        #region Instance Members     
        #region Constants
        /// <summary>
        /// Name of the plug-in as seen by Notepad++.
        /// </summary>
        internal const string PLUGIN_NAME = "NppPluginExample";
        #endregion
        #region Controls   
        private Forms.formTemplate fExample = null;
        #endregion
        #region Notepad++
        /// <summary>
        /// Custom menu items in Notepad++ plug-in submenu.
        /// </summary>
        private readonly Notepad.FuncItems funcItems = new Notepad.FuncItems();
        #endregion       
        #endregion
        #region Instance Properties
        /// <summary>
        /// Visiblility of exceptions list dialog.
        /// </summary>
        private bool ExampleVisible
        {
            get { return this.fExample.Visible; }
            set
            {
                Notepad.SetFormVisible(this.fExample.Handle, value);
                return;
            }
        }
        #endregion
        #region Static Methods
        #region Initialization Methods
        public Plugin()
        {
            this.InitializeEvents();
            this.InitializeMenu();
            this.InitializeForms();
            return;
        }
        private void InitializeEvents()
        {
            Notepad.BufferActivated += this.Notepad_BufferActivated;
            Notepad.Ready += this.Notepad_Ready;
            Notepad.ShutDown += this.Notepad_ShutDown;
            Scintilla.Modified += this.Scintilla_Modified;
            return;
        }
        private void InitializeForms()
        {
            this.fExample = new Forms.formTemplate(PLUGIN_NAME, (int)MenuItems.Example);
            return;
        }
        private void InitializeMenu()
        {
            this.funcItems.Add(Notepad.AllocateCommandID("Example Menu Item", this.mnuitmExample_Click));
            return;
        }
        #endregion
        private void CleanUp()
        {
            Forms.formWait.HideForm();
            return;
        }

        /// <summary>
        /// Retrieves a collection of the plug-ins custom menu items.
        /// </summary>
        /// <returns></returns>
        public Notepad.FuncItems GetMenuItems()
        {
            return this.funcItems;
        }

        /// <summary>
        /// Updates state of each checkbox in Notepad++ plug-in submenu to match value of their corresponding member.
        /// </summary>
        private void UpdateCheckboxStates()
        {
            IntPtr sub_menu_handle = Notepad.GetSubMenu(PLUGIN_NAME);
            if (sub_menu_handle == IntPtr.Zero)
                return;
            Windows.CheckMenuItem(sub_menu_handle, (int)MenuItems.Example, this.fExample.Visible);
            return;
        }
        #endregion
        #region Static Events
        #region Menu Item Events
        private void mnuitmExample_Click()
        {
            this.ExampleVisible = !this.ExampleVisible;
            return;
        }
        #endregion
        #region Notepad Events
        private void Notepad_BufferActivated(object sender, EventArgs e)
        {
            return;
        }
        private void Notepad_Ready(object sender, EventArgs e)
        {
            this.UpdateCheckboxStates();
            return;
        }
        private void Notepad_ShutDown(object sender, EventArgs e)
        {
            this.CleanUp();
            return;
        }
        #endregion
        #region Scintilla Events
        private void Scintilla_Modified(object sender, EventArgs e)
        {
            return;
        }
        #endregion
        #endregion
    }
}   
