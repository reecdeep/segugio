using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace segugio
{
    public class FormContextMenu:Form
    {
        public TextBox contenutoBox;
        public Button closeButton;

        public FormContextMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormContextMenu));
            this.contenutoBox = new System.Windows.Forms.TextBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // contenutoBox
            // 
            this.contenutoBox.Location = new System.Drawing.Point(12, 12);
            this.contenutoBox.Multiline = true;
            this.contenutoBox.Name = "contenutoBox";
            this.contenutoBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.contenutoBox.Size = new System.Drawing.Size(869, 432);
            this.contenutoBox.TabIndex = 0;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(414, 450);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 32);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // FormContextMenu
            // 
            this.ClientSize = new System.Drawing.Size(893, 485);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.contenutoBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormContextMenu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
