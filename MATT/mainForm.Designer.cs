namespace MATT
{
    partial class mainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.inputPanel = new System.Windows.Forms.Panel();
            this.eqnPromptLbl = new System.Windows.Forms.Label();
            this.equationTB = new System.Windows.Forms.TextBox();
            this.solveBtn = new System.Windows.Forms.Button();
            this.optionsPanel = new System.Windows.Forms.Panel();
            this.linkLabelPanel = new System.Windows.Forms.Panel();
            this.otherOptionsLbl = new System.Windows.Forms.Label();
            this.instructionsRTB = new System.Windows.Forms.RichTextBox();
            this.inputPanel.SuspendLayout();
            this.optionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // inputPanel
            // 
            this.inputPanel.Controls.Add(this.eqnPromptLbl);
            this.inputPanel.Controls.Add(this.equationTB);
            this.inputPanel.Controls.Add(this.solveBtn);
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.inputPanel.Location = new System.Drawing.Point(0, 0);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Size = new System.Drawing.Size(768, 58);
            this.inputPanel.TabIndex = 0;
            // 
            // eqnPromptLbl
            // 
            this.eqnPromptLbl.AutoSize = true;
            this.eqnPromptLbl.Location = new System.Drawing.Point(12, 22);
            this.eqnPromptLbl.Name = "eqnPromptLbl";
            this.eqnPromptLbl.Size = new System.Drawing.Size(171, 17);
            this.eqnPromptLbl.TabIndex = 2;
            this.eqnPromptLbl.Text = "Please enter an equation:";
            // 
            // equationTB
            // 
            this.equationTB.Location = new System.Drawing.Point(183, 20);
            this.equationTB.Name = "equationTB";
            this.equationTB.Size = new System.Drawing.Size(459, 22);
            this.equationTB.TabIndex = 1;
            // 
            // solveBtn
            // 
            this.solveBtn.Location = new System.Drawing.Point(648, 19);
            this.solveBtn.Name = "solveBtn";
            this.solveBtn.Size = new System.Drawing.Size(75, 23);
            this.solveBtn.TabIndex = 0;
            this.solveBtn.Text = "Solve";
            this.solveBtn.UseVisualStyleBackColor = true;
            this.solveBtn.Click += new System.EventHandler(this.solveBtn_Click);
            // 
            // optionsPanel
            // 
            this.optionsPanel.Controls.Add(this.linkLabelPanel);
            this.optionsPanel.Controls.Add(this.otherOptionsLbl);
            this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.optionsPanel.Location = new System.Drawing.Point(0, 479);
            this.optionsPanel.Name = "optionsPanel";
            this.optionsPanel.Size = new System.Drawing.Size(768, 113);
            this.optionsPanel.TabIndex = 1;
            // 
            // linkLabelPanel
            // 
            this.linkLabelPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.linkLabelPanel.Location = new System.Drawing.Point(0, 23);
            this.linkLabelPanel.Name = "linkLabelPanel";
            this.linkLabelPanel.Size = new System.Drawing.Size(768, 90);
            this.linkLabelPanel.TabIndex = 1;
            // 
            // otherOptionsLbl
            // 
            this.otherOptionsLbl.AutoSize = true;
            this.otherOptionsLbl.Location = new System.Drawing.Point(4, 3);
            this.otherOptionsLbl.Name = "otherOptionsLbl";
            this.otherOptionsLbl.Size = new System.Drawing.Size(176, 17);
            this.otherOptionsLbl.TabIndex = 0;
            this.otherOptionsLbl.Text = "Solutions for this equation:";
            // 
            // instructionsRTB
            // 
            this.instructionsRTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.instructionsRTB.Location = new System.Drawing.Point(0, 58);
            this.instructionsRTB.Name = "instructionsRTB";
            this.instructionsRTB.Size = new System.Drawing.Size(768, 421);
            this.instructionsRTB.TabIndex = 2;
            this.instructionsRTB.Text = "Hi, I\'m MATT. Enter an equation above to get started.";
            // 
            // mainForm
            // 
            this.AcceptButton = this.solveBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 592);
            this.Controls.Add(this.instructionsRTB);
            this.Controls.Add(this.optionsPanel);
            this.Controls.Add(this.inputPanel);
            this.Name = "mainForm";
            this.Text = "Mathematics, Algebra, and Trigonometry Tutor (MATT)";
            this.inputPanel.ResumeLayout(false);
            this.inputPanel.PerformLayout();
            this.optionsPanel.ResumeLayout(false);
            this.optionsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel inputPanel;
        private System.Windows.Forms.Label eqnPromptLbl;
        private System.Windows.Forms.TextBox equationTB;
        private System.Windows.Forms.Button solveBtn;
        private System.Windows.Forms.Panel optionsPanel;
        private System.Windows.Forms.Label otherOptionsLbl;
        private System.Windows.Forms.RichTextBox instructionsRTB;
        private System.Windows.Forms.Panel linkLabelPanel;
    }
}

