using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Ariete.WinFormUI
{
    public partial class CustomMessageBox : Form
    {
        string guideurl;

        public CustomMessageBox()
        {
            InitializeComponent();
        }

        public CustomMessageBox(string title, string description, string url)
        {
            InitializeComponent();

            this.Text = title;
            this.rtbxMessage.Text = description;
            guideurl = url;
        }

        private void btnGoGuida_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo(guideurl);
            Process.Start(sInfo);
        }

        private void btnChiudiAriete_Click(object sender, EventArgs e)
        {
            //TODO: controllare se esce correttamente coi' o mecessita kill
            Application.Exit();
        }

        private void rtbxMessage_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo(guideurl);
            Process.Start(sInfo);
        }

    }
}
