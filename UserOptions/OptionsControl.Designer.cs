namespace UserOptions
{
    partial class OptionsControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DefaultSdkVersion = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.DefaultKeyFileName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // DefaultSdkVersion
            // 
            this.DefaultSdkVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DefaultSdkVersion.FormattingEnabled = true;
            this.DefaultSdkVersion.Items.AddRange(new object[] {
            "CRM 2011 (5.0.X)",
            "CRM 2013 (6.0.X)",
            "CRM 2013 SP1 (6.1.X)",
            "CRM 2015 (7.0.X)",
            "CRM 2015 (7.1.X)"});
            this.DefaultSdkVersion.Location = new System.Drawing.Point(151, 25);
            this.DefaultSdkVersion.Name = "DefaultSdkVersion";
            this.DefaultSdkVersion.Size = new System.Drawing.Size(163, 21);
            this.DefaultSdkVersion.TabIndex = 1;
            this.DefaultSdkVersion.SelectedIndexChanged += new System.EventHandler(this.DefaultSdkVersion_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Default CRM SDK Version";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Default Key File Name";
            // 
            // DefaultKeyFileName
            // 
            this.DefaultKeyFileName.Location = new System.Drawing.Point(151, 61);
            this.DefaultKeyFileName.Name = "DefaultKeyFileName";
            this.DefaultKeyFileName.Size = new System.Drawing.Size(134, 20);
            this.DefaultKeyFileName.TabIndex = 4;
            this.DefaultKeyFileName.Text = "MyKey";
            this.DefaultKeyFileName.TextChanged += new System.EventHandler(this.DefaultKeyFileName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(287, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = ".snk";
            // 
            // OptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.DefaultKeyFileName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DefaultSdkVersion);
            this.Name = "OptionsControl";
            this.Size = new System.Drawing.Size(373, 352);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox DefaultSdkVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DefaultKeyFileName;
        private System.Windows.Forms.Label label3;
    }
}
