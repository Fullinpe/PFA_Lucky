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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

        private int serier = 0;
        private bool isworking = false, ispause = false;
        private int worktime = 0;

        private string title = "";
        Stopwatch stopwatch = new Stopwatch();

        public Form1(login _login)
        {
            SynchronizationContext synchronizationContext = SynchronizationContext.Current;
            this._login = _login;
            InitializeComponent();
            this.Text += " - " + login.s_id;
            title = Text;
            maps = new List<Dictionary<string, string>>();
            var stringses =
                UtilsDB.selectDB("SELECT `NAME`,S_ID,OL FROM members WHERE MGR>2 AND S_ID <>'" + login.s_id +
                                 "' ORDER BY OL DESC, MGR DESC,S_ID");
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

            new Thread(() =>
            {
                while (!login.over)
                {
                    //todo:查询需求、查询在线状况（自己和成员）、上传截图、
                    if (isworking && !ispause)
                    {
                        var strs = UtilsDB.selectDB(
                            "SELECT NUM_PIC,`NAME`,MSG FROM members JOIN (SELECT NUM_PIC,LOOKER,MSG FROM pictures WHERE MSG<>'' AND S_ID='" +
                            login.s_id + "') AS x ON x.LOOKER=members.S_ID");
                        if (strs.Count >= 1)
                        {
                            UtilsDB.changeDB("UPDATE pictures SET MSG='' WHERE NUM_PIC=" + strs[0][0]);
                            synchronizationContext.Post((obj) =>
                            {
                                notifyIcon1.BalloonTipTitle = strs[0][1];
                                notifyIcon1.BalloonTipText = strs[0][2];
                                notifyIcon1.ShowBalloonTip(5000);
                            }, null);
                        }

                        var temp = UtilsDB.selectDB(
                            "SELECT MAX(NUM_PIC) FROM pictures WHERE S_ID='" + login.s_id + "' AND SERIER_PIC=0");
                        if (temp.Count == 1 && temp[0][0] != "")
                        {
                            UtilsDB.changeDB(
                                "UPDATE pictures SET SERIER_PIC=" + serier + " , BLOB_PIC=@blobData WHERE NUM_PIC=" +
                                temp[0][0],
                                new MySqlParameter("@blobData", UtilsPic.Bitmap2Byte(UtilsPic.GetScreenCapture())));
                        }

                        //TODO:修改时间间隔
                        if (worktime % 50 == 0)
                        {
                            UtilsDB.changeDB(
                                "INSERT INTO together.pictures(pictures.SERIER_PIC,pictures.S_ID,pictures.OPER_device,pictures.BLOB_PIC) VALUES('" +
                                serier + "','" + login.s_id + "',@macData,@blobData);",
                                new MySqlParameter("@blobData", UtilsPic.Bitmap2Byte(UtilsPic.GetScreenCapture())),
                                new MySqlParameter("@macData", UtilsDB.addr_Mac));
                        }
                    }

                    if (worktime % 2 == 0)
                    {
                        if (isworking)
                        {
                            UtilsDB.changeDB("UPDATE members SET OL=5 WHERE S_ID='" + login.s_id + "'");
                        }

                        synchronizationContext.Post((obj) =>
                        {
                            listBox1.BeginUpdate();
                            maps = new List<Dictionary<string, string>>();
                            var sts =
                                UtilsDB.selectDB("SELECT `NAME`,S_ID,OL FROM members WHERE MGR>2 AND S_ID <>'" +
                                                 login.s_id +
                                                 "' ORDER BY OL DESC, MGR DESC,S_ID");
                            int total_ol = 0;
                            for (int i = 0; i < sts.Count; i++)
                            {
                                var map = new Dictionary<string, string>();
                                map.Add("name", sts[i][0]);
                                map.Add("id", sts[i][1]);
                                map.Add("online", sts[i][2]);
                                total_ol += int.Parse(sts[i][2]) > 0 ? 1 : 0;
                                label1.Text = "在线人数：" + (total_ol + (!ispause && isworking ? 1 : 0));
                                maps.Add(map);
                            }

                            listBox1.EndUpdate();
                        }, null);
                    }


                    worktime++;
                    synchronizationContext.Post((state) => { label2.Text = "当前时长：" + state; },
                        $"{stopwatch.Elapsed.Hours}:{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds}");
                    Thread.Sleep(900);
                }
            }).Start();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (isworking)
                MessageBox.Show(
                    $"正在计时--{stopwatch.Elapsed.Hours}:{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds}");
            else
            {
                Text = title + " - working";
                stopwatch.Restart();
                isworking = true;
                serier = int.Parse(UtilsDB.selectDB("SELECT MAX(SERIER_PIC) FROM pictures WHERE S_ID='" + login.s_id +
                                                    "'")[0][0]) + 1;
            }

            // Bitmap bitmap = UtilsPic.GetScreenCapture();
            // Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
            // Cv2.NamedWindow("mat", 0);
            // Cv2.ImShow("mat", mat);
            // Cv2.WaitKey(0);
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !login.over)
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
            if (isworking)
            {
                bool temp = isworking && stopwatch.Elapsed.TotalSeconds >= 30.0;
                if (MessageBox.Show(
                    "确定要结束本次打卡计时？\n" +
                    (temp
                        ? "本次将获得时长: " + (int) stopwatch.Elapsed.TotalSeconds + " 分钟"
                        : "\n无法获得时长  原因：\n计时未达到30分钟最低时间\n"), "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    if (temp)
                        UtilsDB.changeDB("UPDATE members SET OL_TOTAL=OL_TOTAL+IF(OL>0," +
                                         (int) stopwatch.Elapsed.TotalSeconds +
                                         ",0) WHERE S_ID='" + login.s_id + "';");
                    stopwatch.Reset();
                    if (ispause)
                        button2.Text = "休息一下";
                    isworking = false;
                    ispause = false;
                    Text = title;
                }else
                    return;
            }
            if (MessageBox.Show("退出？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                notifyIcon1.Visible = false;
                login.over = true;
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

            if (int.Parse(maps[index]["online"]) > 0)
            {
                Rectangle button = new Rectangle(listBox1.GetItemRectangle(index).X + 200,
                    listBox1.GetItemRectangle(index).Y + 15, 30, 20);
                g.FillRectangle(Brushes.Gray, button);
                TextRenderer.DrawText(g, "查看", this.Font, button, Color.White,
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


        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.X, e.Y);
            Rectangle rect = new Rectangle(listBox1.GetItemRectangle(index).X + 200,
                listBox1.GetItemRectangle(index).Y + 15, 30, 20);
            if (int.Parse(maps[index]["online"]) > 0 && index != -1 && rect.X <= e.X && e.X < rect.X + rect.Width &&
                (rect.Y <= e.Y && e.Y < rect.Y + rect.Height))
            {
                listBox1.SelectedIndex = index;
                if (listBox1.SelectedIndex != -1)
                {
                    var form2 = new Form2(maps[index]["id"], true);
                    form2.Activate();
                    form2.Focus();
                    form2.ShowDialog();

                    // var bytes = UtilsDB.getbolbDB("SELECT BLOB_PIC FROM pictures");
                    // Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(
                    //     (Bitmap) Image.FromStream(new MemoryStream(bytes)));
                    // Cv2.NamedWindow("mat", 0);
                    // Cv2.ImShow("mat", mat);
                    // // Cv2.WaitKey(0);
                }
            }
            else if (index != -1)
            {
                //TODO:ceshi
                 var form2 = new Form2(maps[index]["id"], false);
                // var form2 = new Form2(login.s_id, true);
                form2.Activate();
                form2.Focus();
                form2.ShowDialog();
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            //TODO:气泡，战队总时长，今日总时长
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!ispause && isworking && stopwatch.Elapsed.TotalSeconds >= 30.0)
            {
                ispause = true;
                stopwatch.Stop();
                button2.Text = "继续计时";
            }
            else if (stopwatch.Elapsed.TotalSeconds < 30.0)
            {
                MessageBox.Show("需要连续工作大于半小时，才可暂停\n直接结束，将不会有时长记录");
            }
            else if (ispause)
            {
                ispause = false;
                stopwatch.Start();
                button2.Text = "休息一下";
            }
        }

        //TODO:秒改分
        private void button3_Click(object sender, EventArgs e)
        {
            if (isworking)
            {
                bool temp = isworking && stopwatch.Elapsed.TotalSeconds >= 30.0;
                if (MessageBox.Show(
                    "确定要结束本次打卡计时？\n" +
                    (temp
                        ? "本次将获得时长: " + (int) stopwatch.Elapsed.TotalSeconds + " 分钟"
                        : "\n无法获得时长  原因：\n计时未达到30分钟最低时间\n"), "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    if (temp)
                        UtilsDB.changeDB("UPDATE members SET OL_TOTAL=OL_TOTAL+IF(OL>0," +
                                         (int) stopwatch.Elapsed.TotalSeconds +
                                         ",0) WHERE S_ID='" + login.s_id + "';");
                    stopwatch.Reset();
                    if (ispause)
                        button2.Text = "休息一下";
                    isworking = false;
                    ispause = false;
                    Text = title;
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // notifyIcon1.BalloonTipTitle = "94830";
            // notifyIcon1.BalloonTipText = "03uw09pa";
            // notifyIcon1.ShowBalloonTip(5000);
        }
    }
}