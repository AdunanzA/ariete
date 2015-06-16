namespace Ariete.WinFormUI
{
    partial class CustomMessageBox
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
            this.btnGoGuida = new System.Windows.Forms.Button();
            this.btnChiudiAriete = new System.Windows.Forms.Button();
            this.rtbxMessage = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // btnGoGuida
            // 
            this.btnGoGuida.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGoGuida.AutoSize = true;
            this.btnGoGuida.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGoGuida.Location = new System.Drawing.Point(429, 254);
            this.btnGoGuida.Margin = new System.Windows.Forms.Padding(4);
            this.btnGoGuida.Name = "btnGoGuida";
            this.btnGoGuida.Size = new System.Drawing.Size(198, 29);
            this.btnGoGuida.TabIndex = 0;
            this.btnGoGuida.Text = "Richiedi IP pubblico Gratuito";
            this.btnGoGuida.UseVisualStyleBackColor = true;
            this.btnGoGuida.Click += new System.EventHandler(this.btnGoGuida_Click);
            // 
            // btnChiudiAriete
            // 
            this.btnChiudiAriete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChiudiAriete.AutoSize = true;
            this.btnChiudiAriete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChiudiAriete.Location = new System.Drawing.Point(319, 254);
            this.btnChiudiAriete.Margin = new System.Windows.Forms.Padding(4);
            this.btnChiudiAriete.Name = "btnChiudiAriete";
            this.btnChiudiAriete.Size = new System.Drawing.Size(102, 29);
            this.btnChiudiAriete.TabIndex = 1;
            this.btnChiudiAriete.Text = "Chiudi Ariete";
            this.btnChiudiAriete.UseVisualStyleBackColor = true;
            this.btnChiudiAriete.Click += new System.EventHandler(this.btnChiudiAriete_Click);
            // 
            // rtbxMessage
            // 
            this.rtbxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbxMessage.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.rtbxMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbxMessage.Location = new System.Drawing.Point(16, 15);
            this.rtbxMessage.Name = "rtbxMessage";
            this.rtbxMessage.ReadOnly = true;
            this.rtbxMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbxMessage.Size = new System.Drawing.Size(604, 228);
            this.rtbxMessage.TabIndex = 4;
            this.rtbxMessage.Text = "";
            this.rtbxMessage.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtbxMessage_LinkClicked);
            // 
            // CustomMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(636, 308);
            this.Controls.Add(this.rtbxMessage);
            this.Controls.Add(this.btnChiudiAriete);
            this.Controls.Add(this.btnGoGuida);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CustomMessageBox";
            this.Padding = new System.Windows.Forms.Padding(13, 12, 13, 12);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CustomMessageBox";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGoGuida;
        private System.Windows.Forms.Button btnChiudiAriete;
        private System.Windows.Forms.RichTextBox rtbxMessage;
    }
}