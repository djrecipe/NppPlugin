using System.Windows.Forms;
using System;
using System.Drawing;

namespace NppPlugin.Forms
{
    /// <summary>
    /// A splash screen style dialog box to indicate pending activity.
    /// </summary>
    public class formWait : Form
    {
        private static formWait form = null;
        private static Point GetStartingLocation()
        {
            IntPtr window = External.Windows.FindWindow("Notepad++", null);
            External.Windows.Rect rect = new External.Windows.Rect();
            if (window == IntPtr.Zero || !External.Windows.GetWindowRect(window, ref rect))
            {
                Rectangle r = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                rect.Left = r.Left;
                rect.Top = r.Top;
                rect.Bottom = r.Bottom;
                rect.Right = r.Right;
            }
            int npp_width = Math.Abs(rect.Right - rect.Left);
            int npp_height = Math.Abs(rect.Bottom - rect.Top);
            return new Point(rect.Left + npp_width / 2 - formWait.form.Width/2, rect.Top + npp_height / 2 - formWait.form.Height/2);
        }
        public static void HideForm()
        {
            if (formWait.form == null || !formWait.form.IsHandleCreated)
                return;
            try
            {
                formWait.form.Invoke((MethodInvoker)delegate
                {
                    if (formWait.form.Visible)
                        formWait.form.Close();
                });
                formWait.form = null;
            }
            catch (Exception e)
            {
                // TODO: 11/23/15 implement exception handling
            }
            return;
        }
        public static void ShowForm(IWin32Window owner = null, bool start_center = false)
        {
            if (formWait.form != null)
                return;
            try
            {
                formWait.form = new formWait();
                formWait.form.Location = formWait.GetStartingLocation();
                if(start_center)
                    formWait.form.StartPosition = FormStartPosition.CenterScreen;
                formWait.form.Show(owner);
                formWait.form.Refresh();
            }
            catch (Exception e)
            {
            }
            return;
        }
        private System.ComponentModel.IContainer components = null;
        private Label lblPlugin;
        private Label lblWait;
        private formWait()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.lblWait = new System.Windows.Forms.Label();
            this.lblPlugin = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblWait
            // 
            this.lblWait.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWait.Location = new System.Drawing.Point(29, 28);
            this.lblWait.Margin = new System.Windows.Forms.Padding(20);
            this.lblWait.Name = "lblWait";
            this.lblWait.Size = new System.Drawing.Size(130, 30);
            this.lblWait.TabIndex = 0;
            this.lblWait.Text = "Please Wait...";
            this.lblWait.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPlugin
            // 
            this.lblPlugin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlugin.Location = new System.Drawing.Point(14, 2);
            this.lblPlugin.Margin = new System.Windows.Forms.Padding(5);
            this.lblPlugin.Name = "lblPlugin";
            this.lblPlugin.Size = new System.Drawing.Size(160, 15);
            this.lblPlugin.TabIndex = 1;
            this.lblPlugin.Text = "Notepad++ Plug-in";
            this.lblPlugin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // formWait
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(189, 86);
            this.ControlBox = false;
            this.Controls.Add(this.lblPlugin);
            this.Controls.Add(this.lblWait);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "formWait";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.ResumeLayout(false);

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
