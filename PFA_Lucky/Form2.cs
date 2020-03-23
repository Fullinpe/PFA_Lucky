using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;

namespace PFA_Lucky
{
    public partial class Form2 : Form
    {
        private bool over_thread = false, iscontinue = false, get_first = false;

        private string title = "";

        private int index_pic = 0;
        private string sid;
        private List<string[]> strt = null;

        public Form2(string sid, bool b)
        {
            SynchronizationContext synchronizationContext = SynchronizationContext.Current;
            InitializeComponent();
            this.sid = sid;
            Text += sid;
            title = Text;
            if (!b)
            {
                textBox1.Hide();
                button4.Hide();
                button5.Hide();
                List<string> dates;
                var bytes = UtilsDB.getpicsDB(
                    "SELECT BLOB_PIC,OPER_time FROM pictures WHERE S_ID='" + sid +
                    "' AND !ISNULL(BLOB_PIC) ORDER BY OPER_time DESC", new List<int>() {index_pic}, out dates);
                if (bytes.Count > 0)
                {
                    pictureBox1.Image = Image.FromStream(new MemoryStream(bytes.First()));
                    Text = title + "   时间：" + dates[0] + "   序列·" + (index_pic + 1);
                }
                else
                {
                    MessageBox.Show("暂时没有数据");
                    synchronizationContext.Post((obj) => { Close(); }, null);
                }
            }
            //在线查看
            else
            {
                UtilsDB.changeDB(
                    "INSERT INTO pictures(SERIER_PIC,S_ID,LOOKER,OPER_device) VALUES (0,'" + sid + "','" + login.s_id +
                    "','" + UtilsDB.addr_Mac + "')");
                new Thread(() =>
                {
                    strt = UtilsDB.selectDB("SELECT MAX(NUM_PIC) FROM pictures WHERE SERIER_PIC=0 AND LOOKER='" +
                                            login.s_id + "'");
                    if (strt.Count == 1)
                    {
                        int timeout = 0;
                        while (!over_thread)
                        {
                            if (!button4.Enabled &&
                                UtilsDB.selectDB("SELECT MSG FROM pictures WHERE NUM_PIC=" + strt[0][0])[0][0] == "")
                            {
                                synchronizationContext.Post((obj) =>
                                {
                                    button4.Enabled = true;
                                    textBox1.Text = "";
                                }, null);
                            }

                            if (iscontinue && get_first)
                            {
                                List<string> dat;
                                UtilsDB.changeDB("UPDATE pictures SET SERIER_PIC=0 WHERE NUM_PIC=" + strt[0][0]);
                                var byt = UtilsDB.getpicsDB(
                                    "SELECT BLOB_PIC,OPER_time FROM pictures WHERE !ISNULL(BLOB_PIC) AND NUM_PIC=" +
                                    strt[0][0], new List<int>() {0}, out dat);
                                if (byt.Count > 0)
                                {
                                    synchronizationContext.Post((obj) =>
                                    {
                                        pictureBox1.Image = Image.FromStream(new MemoryStream(byt.First()));
                                        Text = title + "时间：" + dat[0];
                                    }, null);
                                    timeout = 0;
                                }
                                else
                                {
                                    timeout++;
                                    if (timeout > 5)
                                    {
                                        MessageBox.Show("连接超时", "连接错误");
                                    }
                                }
                            }
                            else if (!get_first)
                            {
                                List<string> dat;
                                var byt = UtilsDB.getpicsDB(
                                    "SELECT BLOB_PIC,OPER_time FROM pictures WHERE !ISNULL(BLOB_PIC) AND NUM_PIC=" +
                                    strt[0][0], new List<int>() {0}, out dat);
                                if (byt.Count > 0)
                                {
                                    synchronizationContext.Post((obj) =>
                                    {
                                        pictureBox1.Image = Image.FromStream(new MemoryStream(byt.First()));
                                        Text = title + "时间：" + dat[0];
                                    }, null);
                                    get_first = true;
                                }
                                else
                                {
                                    timeout++;
                                    if (timeout > 5)
                                    {
                                        MessageBox.Show("连接超时", "连接错误");
                                        synchronizationContext.Post((obj) => { Close(); }, null);
                                    }
                                }
                            }

                            Thread.Sleep(1000);
                        }
                    }
                }).Start();
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            over_thread = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null && saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                pictureBox1.Image.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else if (pictureBox1.Image == null)
                MessageBox.Show("没有数据");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (get_first)
            {
                iscontinue = !iscontinue;
                button5.Text = iscontinue ? "暂停" : "连续";
            }
            else
                MessageBox.Show("等待第一份数据");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (index_pic > 0)
            {
                index_pic--;
                List<string> dates;
                var bytes = UtilsDB.getpicsDB(
                    "SELECT BLOB_PIC,OPER_time FROM pictures WHERE S_ID='" + sid +
                    "' AND !ISNULL(BLOB_PIC) ORDER BY OPER_time DESC", new List<int>() {index_pic}, out dates);
                if (bytes.Count > 0)
                {
                    pictureBox1.Image = Image.FromStream(new MemoryStream(bytes.First()));
                    Text = title + "   时间：" + dates[0] + "   序列·" + (index_pic + 1);
                }
                else
                {
                    MessageBox.Show("暂时没有数据");
                }
            }
            else
                MessageBox.Show("已经是第一张了");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            index_pic++;
            List<string> dates;
            var bytes = UtilsDB.getpicsDB(
                "SELECT BLOB_PIC,OPER_time FROM pictures WHERE S_ID='" + sid +
                "' AND !ISNULL(BLOB_PIC) ORDER BY OPER_time DESC", new List<int>() {index_pic}, out dates);
            if (bytes.Count > 0)
            {
                pictureBox1.Image = Image.FromStream(new MemoryStream(bytes.First()));
                Text = title + "   时间：" + dates[0] + "   序列·" + (index_pic + 1);
            }
            else
            {
                MessageBox.Show("暂时没有数据了");
                index_pic--;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (get_first &&
                UtilsDB.changeDB("UPDATE pictures SET MSG='" + textBox1.Text + "' WHERE NUM_PIC=" + strt[0][0]) > 0)
            {
                button4.Enabled = false;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                button4_Click(sender, e);
        }
    }
}