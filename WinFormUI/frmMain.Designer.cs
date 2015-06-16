namespace Ariete.WinFormUI
{
    partial class frmMain
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
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.rtbSummary = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.button5 = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblDesc = new System.Windows.Forms.Label();
            this.lblHeadline = new System.Windows.Forms.Label();
            this.rtxtbLog = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // rtbSummary
            // 
            this.rtbSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbSummary.ForeColor = System.Drawing.Color.Brown;
            this.rtbSummary.Location = new System.Drawing.Point(0, 0);
            this.rtbSummary.Name = "rtbSummary";
            this.rtbSummary.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbSummary.Size = new System.Drawing.Size(180, 453);
            this.rtbSummary.TabIndex = 1;
            this.rtbSummary.Text = "";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.rtbSummary);
            this.splitContainer1.Size = new System.Drawing.Size(704, 453);
            this.splitContainer1.SplitterDistance = 520;
            this.splitContainer1.TabIndex = 2;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.splitContainer2.Panel1.Controls.Add(this.button5);
            this.splitContainer2.Panel1.Controls.Add(this.btnStart);
            this.splitContainer2.Panel1.Controls.Add(this.pictureBox1);
            this.splitContainer2.Panel1.Controls.Add(this.lblDesc);
            this.splitContainer2.Panel1.Controls.Add(this.lblHeadline);
            this.splitContainer2.Panel1.Cursor = System.Windows.Forms.Cursors.Default;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.rtxtbLog);
            this.splitContainer2.Size = new System.Drawing.Size(520, 453);
            this.splitContainer2.SplitterDistance = 305;
            this.splitContainer2.SplitterWidth = 8;
            this.splitContainer2.TabIndex = 1;
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button5.AutoEllipsis = true;
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.Location = new System.Drawing.Point(12, 268);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(94, 23);
            this.button5.TabIndex = 8;
            this.button5.Text = "Copia Log";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.AutoSize = true;
            this.btnStart.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnStart.Location = new System.Drawing.Point(385, 249);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(115, 42);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Avvia";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.ImageLocation = "http://forum.adunanza.net/images/adu/misc/adu_logo.jpg";
            this.pictureBox1.Location = new System.Drawing.Point(5, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(244, 84);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // lblDesc
            // 
            this.lblDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDesc.AutoSize = true;
            this.lblDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDesc.Location = new System.Drawing.Point(3, 178);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(331, 16);
            this.lblDesc.TabIndex = 1;
            this.lblDesc.Text = "Cliccare sul pulsante \"Avvia\" per avviare la procedura.";
            // 
            // lblHeadline
            // 
            this.lblHeadline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHeadline.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblHeadline.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeadline.Location = new System.Drawing.Point(3, 91);
            this.lblHeadline.Name = "lblHeadline";
            this.lblHeadline.Size = new System.Drawing.Size(497, 87);
            this.lblHeadline.TabIndex = 0;
            this.lblHeadline.Text = "Ariete configurera\' automaticamente tutte le porte per utilizzare eMule AdunanzA " +
    "al meglio.";
            // 
            // rtxtbLog
            // 
            this.rtxtbLog.AutoWordSelection = true;
            this.rtxtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtxtbLog.HideSelection = false;
            this.rtxtbLog.Location = new System.Drawing.Point(0, 0);
            this.rtxtbLog.Name = "rtxtbLog";
            this.rtxtbLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtxtbLog.Size = new System.Drawing.Size(520, 140);
            this.rtxtbLog.TabIndex = 0;
            this.rtxtbLog.Text = "";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 453);
            this.Controls.Add(this.splitContainer1);
            this.Name = "frmMain";
            this.Text = "Ariete by AduTeam";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.RichTextBox rtbSummary;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox rtxtbLog;
        private System.Windows.Forms.Label lblHeadline;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button button5;
    }
}

