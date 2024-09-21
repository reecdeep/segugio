
using System.Windows.Forms;

namespace segugio
{
    partial class MainForm
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.logViewer = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_refresh = new System.Windows.Forms.Button();
            this.comboProcessList = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboCommandLine = new System.Windows.Forms.ComboBox();
            this.button_Launch = new System.Windows.Forms.Button();
            this.fileChooser = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_chooseFile = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.yarasMatchedBox = new System.Windows.Forms.TextBox();
            this.scannedProcessesBox = new System.Windows.Forms.TextBox();
            this.programStatusBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 575);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Log Viewer";
            // 
            // logViewer
            // 
            this.logViewer.Location = new System.Drawing.Point(12, 591);
            this.logViewer.Name = "logViewer";
            this.logViewer.Size = new System.Drawing.Size(913, 118);
            this.logViewer.TabIndex = 4;
            this.logViewer.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_refresh);
            this.groupBox1.Controls.Add(this.comboProcessList);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.comboCommandLine);
            this.groupBox1.Controls.Add(this.button_Launch);
            this.groupBox1.Controls.Add(this.fileChooser);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.button_chooseFile);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(19, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(504, 127);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Start New Process";
            // 
            // button_refresh
            // 
            this.button_refresh.BackgroundImage = global::segugio.Properties.Resources._118801_refresh_icon;
            this.button_refresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_refresh.Location = new System.Drawing.Point(359, 89);
            this.button_refresh.Name = "button_refresh";
            this.button_refresh.Size = new System.Drawing.Size(35, 32);
            this.button_refresh.TabIndex = 19;
            this.button_refresh.UseVisualStyleBackColor = true;
            this.button_refresh.Click += new System.EventHandler(this.button_refresh_Click);
            // 
            // comboProcessList
            // 
            this.comboProcessList.Enabled = false;
            this.comboProcessList.FormattingEnabled = true;
            this.comboProcessList.Location = new System.Drawing.Point(88, 94);
            this.comboProcessList.Name = "comboProcessList";
            this.comboProcessList.Size = new System.Drawing.Size(265, 21);
            this.comboProcessList.TabIndex = 18;
            this.comboProcessList.SelectedIndexChanged += new System.EventHandler(this.comboProcessList_SelectedIndexChanged);
            this.comboProcessList.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.ComboBox_Format);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 97);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Parent process";
            // 
            // comboCommandLine
            // 
            this.comboCommandLine.FormattingEnabled = true;
            this.comboCommandLine.Location = new System.Drawing.Point(88, 62);
            this.comboCommandLine.Name = "comboCommandLine";
            this.comboCommandLine.Size = new System.Drawing.Size(306, 21);
            this.comboCommandLine.TabIndex = 16;
            this.comboCommandLine.TextChanged += new System.EventHandler(this.comboCommandLine_TextChanged);
            this.comboCommandLine.SelectedIndexChanged += new System.EventHandler(this.comboCommandLine_SelectedIndexChanged);
            // 
            // button_Launch
            // 
            this.button_Launch.BackgroundImage = global::segugio.Properties.Resources.segugio_main_logo;
            this.button_Launch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_Launch.Enabled = false;
            this.button_Launch.Location = new System.Drawing.Point(400, 23);
            this.button_Launch.Name = "button_Launch";
            this.button_Launch.Size = new System.Drawing.Size(98, 98);
            this.button_Launch.TabIndex = 1;
            this.button_Launch.UseVisualStyleBackColor = true;
            this.button_Launch.Click += new System.EventHandler(this.button_Click_launchFileStartScan);
            // 
            // fileChooser
            // 
            this.fileChooser.Location = new System.Drawing.Point(88, 29);
            this.fileChooser.Name = "fileChooser";
            this.fileChooser.ReadOnly = true;
            this.fileChooser.Size = new System.Drawing.Size(265, 20);
            this.fileChooser.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Command Line";
            // 
            // button_chooseFile
            // 
            this.button_chooseFile.BackgroundImage = global::segugio.Properties.Resources._85334_file_open_icon;
            this.button_chooseFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button_chooseFile.Location = new System.Drawing.Point(359, 24);
            this.button_chooseFile.Name = "button_chooseFile";
            this.button_chooseFile.Size = new System.Drawing.Size(32, 32);
            this.button_chooseFile.TabIndex = 11;
            this.button_chooseFile.UseVisualStyleBackColor = true;
            this.button_chooseFile.Click += new System.EventHandler(this.button_Click_chooseFile);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "File to launch";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.treeView1);
            this.groupBox3.Location = new System.Drawing.Point(12, 145);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(919, 427);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Process Info";
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(7, 19);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(906, 402);
            this.treeView1.TabIndex = 29;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.yarasMatchedBox);
            this.groupBox2.Controls.Add(this.scannedProcessesBox);
            this.groupBox2.Controls.Add(this.programStatusBox);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(529, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(402, 127);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Overall Analysis";
            // 
            // yarasMatchedBox
            // 
            this.yarasMatchedBox.ForeColor = System.Drawing.Color.Red;
            this.yarasMatchedBox.Location = new System.Drawing.Point(69, 53);
            this.yarasMatchedBox.Multiline = true;
            this.yarasMatchedBox.Name = "yarasMatchedBox";
            this.yarasMatchedBox.ReadOnly = true;
            this.yarasMatchedBox.Size = new System.Drawing.Size(327, 68);
            this.yarasMatchedBox.TabIndex = 19;
            // 
            // scannedProcessesBox
            // 
            this.scannedProcessesBox.Location = new System.Drawing.Point(296, 25);
            this.scannedProcessesBox.Name = "scannedProcessesBox";
            this.scannedProcessesBox.ReadOnly = true;
            this.scannedProcessesBox.Size = new System.Drawing.Size(100, 20);
            this.scannedProcessesBox.TabIndex = 18;
            // 
            // programStatusBox
            // 
            this.programStatusBox.Location = new System.Drawing.Point(58, 25);
            this.programStatusBox.Name = "programStatusBox";
            this.programStatusBox.ReadOnly = true;
            this.programStatusBox.Size = new System.Drawing.Size(122, 20);
            this.programStatusBox.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(186, 28);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(104, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Scanned processes:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Matches:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Status:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(943, 721);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.logViewer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(959, 760);
            this.MinimumSize = new System.Drawing.Size(959, 760);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Segugio";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button_Launch;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.RichTextBox logViewer;
        //CustomTreeView treeView1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox fileChooser;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button_chooseFile;
        private TreeView treeView1;
        private Label label2;
        private Label label3;
        private ComboBox comboCommandLine;
        private ComboBox comboProcessList;
        private Label label6;
        private GroupBox groupBox2;
        private Label label4;
        private Label label5;
        private Label label7;
        private TextBox scannedProcessesBox;
        private TextBox programStatusBox;
        private TextBox yarasMatchedBox;
        private Button button_refresh;
    }
}

