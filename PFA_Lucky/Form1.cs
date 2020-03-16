using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PFA_Lucky
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Bitmap bitmap = screenDB.GetScreenCapture();

            
            saveFileDialog1.ShowDialog(this);
            bitmap.Save(saveFileDialog1.FileName,System.Drawing.Imaging.ImageFormat.Jpeg);
            // button1.Text = "" + screenDB.ppxI;
            // Image img= Image.FromHbitmap(bitmap.GetHbitmap());
            // pictureBox1.Image = img;
            // pictureBox1.Show();
            // pictureBox1.Refresh();
        }
        
    }
}