﻿namespace NUpdater
{
    partial class UpdateForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateForm));
            this.UpdatingAppLabel = new System.Windows.Forms.Label();
            this.UpdatingMessageLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.TotalProgressLabel = new System.Windows.Forms.Label();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.DownloadingLabel = new System.Windows.Forms.Label();
            this.UpdateWorker = new System.ComponentModel.BackgroundWorker();
            this.StartMinimizedCheckBox = new System.Windows.Forms.CheckBox();
            this.RegistryConfigurationBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RegistryConfigurationBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // UpdatingAppLabel
            // 
            this.UpdatingAppLabel.AutoSize = true;
            this.UpdatingAppLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpdatingAppLabel.Location = new System.Drawing.Point(12, 9);
            this.UpdatingAppLabel.Name = "UpdatingAppLabel";
            this.UpdatingAppLabel.Size = new System.Drawing.Size(58, 13);
            this.UpdatingAppLabel.TabIndex = 0;
            this.UpdatingAppLabel.Text = "Updating";
            // 
            // UpdatingMessageLabel
            // 
            this.UpdatingMessageLabel.Location = new System.Drawing.Point(12, 43);
            this.UpdatingMessageLabel.Name = "UpdatingMessageLabel";
            this.UpdatingMessageLabel.Size = new System.Drawing.Size(401, 32);
            this.UpdatingMessageLabel.TabIndex = 2;
            this.UpdatingMessageLabel.Text = "This may take several minutes. You can use your computer to do other tasks during" +
    " installation.";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.UpdatingMessageLabel);
            this.panel1.Controls.Add(this.UpdatingAppLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(497, 94);
            this.panel1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(419, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(78, 94);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 130);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(473, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // TotalProgressLabel
            // 
            this.TotalProgressLabel.AutoSize = true;
            this.TotalProgressLabel.Location = new System.Drawing.Point(12, 166);
            this.TotalProgressLabel.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            this.TotalProgressLabel.Name = "TotalProgressLabel";
            this.TotalProgressLabel.Size = new System.Drawing.Size(74, 13);
            this.TotalProgressLabel.TabIndex = 3;
            this.TotalProgressLabel.Text = "Total progress";
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(12, 189);
            this.progressBar2.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(473, 23);
            this.progressBar2.TabIndex = 4;
            // 
            // DownloadingLabel
            // 
            this.DownloadingLabel.AutoSize = true;
            this.DownloadingLabel.Location = new System.Drawing.Point(12, 107);
            this.DownloadingLabel.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            this.DownloadingLabel.Name = "DownloadingLabel";
            this.DownloadingLabel.Size = new System.Drawing.Size(69, 13);
            this.DownloadingLabel.TabIndex = 1;
            this.DownloadingLabel.Text = "Downloading";
            // 
            // UpdateWorker
            // 
            this.UpdateWorker.WorkerReportsProgress = true;
            this.UpdateWorker.WorkerSupportsCancellation = true;
            this.UpdateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.UpdateWorkerDo);
            this.UpdateWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.UpdateWorkerProgressChanged);
            this.UpdateWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.UpdateWorkerCompleted);
            // 
            // StartMinimizedCheckBox
            // 
            this.StartMinimizedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StartMinimizedCheckBox.AutoSize = true;
            this.StartMinimizedCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.RegistryConfigurationBindingSource, "StartMinimized", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.StartMinimizedCheckBox.Location = new System.Drawing.Point(389, 225);
            this.StartMinimizedCheckBox.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.StartMinimizedCheckBox.Name = "StartMinimizedCheckBox";
            this.StartMinimizedCheckBox.Size = new System.Drawing.Size(96, 17);
            this.StartMinimizedCheckBox.TabIndex = 5;
            this.StartMinimizedCheckBox.Text = "Start minimized";
            this.StartMinimizedCheckBox.UseVisualStyleBackColor = true;
            // 
            // RegistryConfigurationBindingSource
            // 
            this.RegistryConfigurationBindingSource.AllowNew = false;
            this.RegistryConfigurationBindingSource.DataSource = typeof(NUpdater.RegistryConfiguration);
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 250);
            this.Controls.Add(this.StartMinimizedCheckBox);
            this.Controls.Add(this.DownloadingLabel);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.TotalProgressLabel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "UpdateForm";
            this.Text = "Updating";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateFormClosing);
            this.Load += new System.EventHandler(this.UpdateFormLoad);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RegistryConfigurationBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label UpdatingAppLabel;
        private System.Windows.Forms.Label UpdatingMessageLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label TotalProgressLabel;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label DownloadingLabel;
        private System.ComponentModel.BackgroundWorker UpdateWorker;
        private System.Windows.Forms.CheckBox StartMinimizedCheckBox;
        public System.Windows.Forms.BindingSource RegistryConfigurationBindingSource;
    }
}