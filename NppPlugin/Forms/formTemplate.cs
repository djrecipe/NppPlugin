using System;
using System.IO;
using System.Windows.Forms;
using NppPlugin.External;

namespace NppPlugin.Forms
{
    /// <summary>
    /// A template form with special support for Notepad++.
    /// </summary>
    public class formTemplate : Form
    {
        internal delegate void delSelectionChanged(int line);
        protected DataGridView gridMain;
        private ContextMenuStrip cmenMain;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem mnuitmExport;
        internal event delSelectionChanged SelectionChanged;
        protected bool IgnoreSelectionChange = false;
        public readonly int ID=-1;
        public readonly string PluginName=null;
        public formTemplate(string plugin_name, int id)
        {
            this.ID = id;
            this.PluginName = plugin_name;
            this.InitializeComponent();
            Notepad.RegisterForm(this, null, this.ID, "Example",
                Notepad.WindowOptions.DWS_DF_CONT_RIGHT | Notepad.WindowOptions.DWS_PARAMSALL, plugin_name);
            return;
        }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gridMain = new System.Windows.Forms.DataGridView();
            this.cmenMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuitmExport = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
            this.cmenMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridMain
            // 
            this.gridMain.AllowUserToAddRows = false;
            this.gridMain.AllowUserToDeleteRows = false;
            this.gridMain.AllowUserToResizeRows = false;
            this.gridMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridMain.ContextMenuStrip = this.cmenMain;
            this.gridMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridMain.Location = new System.Drawing.Point(0, 0);
            this.gridMain.Name = "gridMain";
            this.gridMain.RowHeadersVisible = false;
            this.gridMain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridMain.Size = new System.Drawing.Size(289, 614);
            this.gridMain.TabIndex = 0;
            this.gridMain.SelectionChanged += new System.EventHandler(this.gridMain_SelectionChanged);
            // 
            // cmenMain
            // 
            this.cmenMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuitmExport});
            this.cmenMain.Name = "cmenMain";
            this.cmenMain.Size = new System.Drawing.Size(108, 26);
            // 
            // mnuitmExport
            // 
            this.mnuitmExport.Name = "mnuitmExport";
            this.mnuitmExport.Size = new System.Drawing.Size(107, 22);
            this.mnuitmExport.Text = "Export";
            this.mnuitmExport.Click += new System.EventHandler(this.mnuitmExport_Click);
            // 
            // formTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 614);
            this.Controls.Add(this.gridMain);
            this.Name = "formTemplate";
            this.VisibleChanged += new System.EventHandler(this.formTemplate_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
            this.cmenMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void gridMain_SelectionChanged(object sender, EventArgs e)
        {
            if (this.IgnoreSelectionChange || this.gridMain.SelectedCells.Count < 1)
                return;
            DataGridViewCell cell = this.gridMain.SelectedCells[0];
            if (cell.RowIndex < 0)
                return;
            DataGridViewRow row = this.gridMain.Rows[cell.RowIndex];
            if (this.SelectionChanged != null)
                this.SelectionChanged((int)row.Tag);
            return;
        }

        protected virtual void mnuitmExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog fSave = new SaveFileDialog();
            fSave.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Promega";
            fSave.FileName = this.Text;
            fSave.Filter = "CSV (Comma delimited)|.csv";
            fSave.OverwritePrompt = true;
            fSave.RestoreDirectory = true;
            if(fSave.ShowDialog(this) == DialogResult.OK)
            {
                string text_out = "";
                foreach (DataGridViewRow row in this.gridMain.Rows)
                {
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        if (i > 0)
                            text_out += ", ";
                        DataGridViewCell cell = row.Cells[i];
                        text_out += cell.Value.ToString().Replace(',', ' ').Replace('\n', ' ');
                    }
                    text_out += "\n";
                }
                try
                {
                    File.WriteAllText(fSave.FileName, text_out);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to export.");
                }
            }
            return;
        }

        private void formTemplate_VisibleChanged(object sender, EventArgs e)
        {
            Windows.CheckMenuItem(Notepad.GetSubMenu(this.PluginName), this.ID, this.Visible);
        }
    }
}
