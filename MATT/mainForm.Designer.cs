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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainForm));
            this.inputPanel = new System.Windows.Forms.Panel();
            this.equationTB = new System.Windows.Forms.TextBox();
            this.eqnPromptLbl = new System.Windows.Forms.Label();
            this.solveBtn = new System.Windows.Forms.Button();
            this.clearLabel = new System.Windows.Forms.Label();
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
            this.inputPanel.Controls.Add(this.equationTB);
            this.inputPanel.Controls.Add(this.eqnPromptLbl);
            this.inputPanel.Controls.Add(this.solveBtn);
            this.inputPanel.Controls.Add(this.clearLabel);
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.inputPanel.Location = new System.Drawing.Point(0, 0);
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Padding = new System.Windows.Forms.Padding(0, 15, 15, 15);
            this.inputPanel.Size = new System.Drawing.Size(868, 58);
            this.inputPanel.TabIndex = 0;
            // 
            // equationTB
            // 
            this.equationTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.equationTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.equationTB.Location = new System.Drawing.Point(199, 15);
            this.equationTB.Name = "equationTB";
            this.equationTB.Size = new System.Drawing.Size(579, 27);
            this.equationTB.TabIndex = 1;
            // 
            // eqnPromptLbl
            // 
            this.eqnPromptLbl.AutoSize = true;
            this.eqnPromptLbl.Dock = System.Windows.Forms.DockStyle.Left;
            this.eqnPromptLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eqnPromptLbl.Location = new System.Drawing.Point(0, 15);
            this.eqnPromptLbl.Name = "eqnPromptLbl";
            this.eqnPromptLbl.Size = new System.Drawing.Size(199, 20);
            this.eqnPromptLbl.TabIndex = 2;
            this.eqnPromptLbl.Text = "Please enter an equation:";
            // 
            // solveBtn
            // 
            this.solveBtn.Dock = System.Windows.Forms.DockStyle.Right;
            this.solveBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.solveBtn.Location = new System.Drawing.Point(778, 15);
            this.solveBtn.Name = "solveBtn";
            this.solveBtn.Size = new System.Drawing.Size(75, 28);
            this.solveBtn.TabIndex = 0;
            this.solveBtn.Text = "Solve";
            this.solveBtn.UseVisualStyleBackColor = true;
            this.solveBtn.Click += new System.EventHandler(this.solveBtn_Click);
            // 
            // clearLabel
            // 
            this.clearLabel.AutoSize = true;
            this.clearLabel.Location = new System.Drawing.Point(5, 35);
            this.clearLabel.Name = "clearLabel";
            this.clearLabel.Size = new System.Drawing.Size(100, 17);
            this.clearLabel.TabIndex = 3;
            this.clearLabel.Text = "                       ";
            this.clearLabel.Click += new System.EventHandler(this.clearLabel_Click);
            // 
            // optionsPanel
            // 
            this.optionsPanel.Controls.Add(this.linkLabelPanel);
            this.optionsPanel.Controls.Add(this.otherOptionsLbl);
            this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.optionsPanel.Location = new System.Drawing.Point(0, 552);
            this.optionsPanel.Name = "optionsPanel";
            this.optionsPanel.Size = new System.Drawing.Size(868, 113);
            this.optionsPanel.TabIndex = 1;
            // 
            // linkLabelPanel
            // 
            this.linkLabelPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.linkLabelPanel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelPanel.Location = new System.Drawing.Point(0, 23);
            this.linkLabelPanel.Name = "linkLabelPanel";
            this.linkLabelPanel.Size = new System.Drawing.Size(868, 90);
            this.linkLabelPanel.TabIndex = 1;
            // 
            // otherOptionsLbl
            // 
            this.otherOptionsLbl.AutoSize = true;
            this.otherOptionsLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.otherOptionsLbl.Location = new System.Drawing.Point(4, 3);
            this.otherOptionsLbl.Name = "otherOptionsLbl";
            this.otherOptionsLbl.Size = new System.Drawing.Size(208, 20);
            this.otherOptionsLbl.TabIndex = 0;
            this.otherOptionsLbl.Text = "Solutions for this equation:";
            // 
            // instructionsRTB
            // 
            this.instructionsRTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.instructionsRTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.instructionsRTB.Location = new System.Drawing.Point(0, 58);
            this.instructionsRTB.Name = "instructionsRTB";
            this.instructionsRTB.Size = new System.Drawing.Size(868, 494);
            this.instructionsRTB.TabIndex = 2;
            this.instructionsRTB.Text = "Hi, I\'m MATT! Enter an equation or expression above and I\'ll try to help you work" +
    " through the problem. If you have multiple equations, please separate them with " +
    "a semicolon (;).";
            // 
            // mainForm
            // 
            this.AcceptButton = this.solveBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(868, 665);
            this.Controls.Add(this.instructionsRTB);
            this.Controls.Add(this.optionsPanel);
            this.Controls.Add(this.inputPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
        private System.Windows.Forms.Label clearLabel;
    }
}

