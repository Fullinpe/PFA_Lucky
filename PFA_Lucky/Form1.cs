using System;
using System.Collections;
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
using OpenCvSharp;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace PFA_Lucky
{
    public partial class Form1 : Form
    {
        private login _login;

        private List<Dictionary<string, string>> maps;

        public Form1(login _login)
        {
            this._login = _login;
            InitializeComponent();
            this.Text += "-" + login.s_id;
            maps = new List<Dictionary<string, string>>();
            var stringses =
                UtilsDB.selectDB(@"SELECT `NAME`,S_ID,OL FROM members WHERE MGR>2 ORDER BY OL DESC, MGR DESC,S_ID");
            for (int i = 0; i < stringses.Count; i++)
            {
                var map = new Dictionary<string, string>();
                map.Add("name", stringses[i][0]);
                map.Add("id", stringses[i][1]);
                map.Add("online", stringses[i][2]);
                maps.Add(map);
            }

            for (int i = 0; i < maps.Count; i++)
            {
                listBox1.Items.Add("oi");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Bitmap bitmap = UtilsPic.GetScreenCapture();
            Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
            Cv2.NamedWindow("mat", 0);
            Cv2.ImShow("mat", mat);
            Cv2.WaitKey(0);


            // saveFileDialog1.ShowDialog(this);
            // bitmap.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            // Image img = Image.FromHbitmap(bitmap.GetHbitmap());
            // pictureBox1.Image = img;
            // pictureBox1.Show();
            // pictureBox1.Refresh();

            // MessageBox.Show("" + UtilsDB.changeDB(
            //     "INSERT INTO together.pictures(pictures.SERIER_PIC,pictures.S_ID,pictures.OPER_device,pictures.BLOB_PIC) VALUES('1','1713206317',@macData,@blobData);",
            //     new MySqlParameter("@blobData", UtilsScreen.Bitmap2Byte(bitmap)),
            //     new MySqlParameter("@macData", UtilsDB.addr_Mac)));
        }

        private bool isexit = false;

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !isexit)
            {
                e.Cancel = true;
                ShowInTaskbar = false;
                Hide();
            }
            else
                notifyIcon1.Visible = false;
        }


        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("退出？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                notifyIcon1.Visible = false;
                isexit = true;
                Close();
                _login.Close();
            }
        }


        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowInTaskbar = true;
                Show();
                Activate();
                Focus();
            }
        }

        private void 打开面板ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1_MouseClick(sender,
                new MouseEventArgs(MouseButtons.Left, 1, MousePosition.X, MousePosition.Y, 0));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            退出ToolStripMenuItem_Click(sender, e);
        }

        List<Mat> mats = new List<Mat>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Interval = new Random().Next(7000) + 3000;
            // notifyIcon1.ShowBalloonTip(5000,"title","message"+timer1.Interval,ToolTipIcon.Warning);
            for (int i = 0; i < mats.Count - 1; i++)
            {
                mats.Remove(mats.First());
            }

            mats.Add(OpenCvSharp.Extensions.BitmapConverter.ToMat(UtilsPic.GetScreenCapture()));


            // Cv2.NamedWindow("mat"+timer1.Interval,WindowMode.Normal);
            // Cv2.ImShow("mat"+timer1.Interval,mat);
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            int index = e.Index; //获取当前要进行绘制的行的序号，从0开始。
            Graphics g = e.Graphics; //获取Graphics对象。

            Rectangle bound = e.Bounds;

            string text = listBox1.Items[index].ToString(); //获取当前要绘制的行的显示文本。
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                //如果当前行为选中行。
                //绘制选中时要显示的蓝色边框。
                g.DrawRectangle(Pens.Black, bound.Left, bound.Top, bound.Width - 1, bound.Height - 1);
                Rectangle rect = new Rectangle(bound.Left, bound.Top, bound.Width, bound.Height);
                //绘制选中时要显示的蓝色背景。
                g.FillRectangle(Brushes.Red, rect);

                //绘制显示文本。
                TextRenderer.DrawText(g, maps[index]["name"], this.Font, rect, Color.Black,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
            else
            {
                Rectangle rect = new Rectangle(bound.Left, (bound.Top), bound.Width, bound.Height);
                if (int.Parse(maps[index]["online"]) > 0)
                {
                    g.FillRectangle(Brushes.ForestGreen, rect);
                }
                else if (int.Parse(maps[index]["online"]) < 0)
                {
                    g.FillRectangle(Brushes.Pink, rect);
                }
                else
                {
                    g.FillRectangle(Brushes.Beige, rect);
                }

                TextRenderer.DrawText(g, maps[index]["name"], this.Font, rect, Color.Black,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
        }

        private void listBox1_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 50;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}