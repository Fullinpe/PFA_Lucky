using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PFA_Lucky
{
    public partial class login : Form
    {
        private bool freelogin = false;

        public static string s_id = null, mgr = null;

        public login()
        {
            InitializeComponent();
            button2.Hide();
            var strs = UtilsDB.selectDB(
                "SELECT x.OPER_device,x.S_ID,members.MGR FROM (SELECT OPER_device,S_ID FROM `logs` WHERE `KEY`=(SELECT MAX(`KEY`) FROM `logs` WHERE S_ID=(SELECT S_ID FROM `logs` WHERE `KEY`=(SELECT MAX(`KEY`) FROM `logs` WHERE OPER_device='" +
                UtilsDB.addr_Mac +
                "' AND TYPE_operation='桌面登录')) AND TYPE_operation='桌面登录')) AS x LEFT JOIN members ON members.S_ID=x.S_ID");
            if (!UtilsDB.addr_Mac.Equals("00-00-00-00-00-00") && strs.Count == 1)
            {
                button2.Show();
                textBox1.Hide();
                label2.Text = "设备已绑定，可直接进入";
                label1.Hide();
                s_id = strs[0][1];
                mgr = strs[0][2];
                button1.Text = "进入主面板";
                freelogin = true;
            }
            else if (UtilsDB.addr_Mac.Equals("00-00-00-00-00-00"))
                MessageBox.Show("mac错误", "登录失败1");
        }

        private void login_lucky()
        {
            Hide();
            var form1 = new Form1(this);
            form1.Activate();
            form1.Focus();
            form1.Show();
            ShowInTaskbar = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (freelogin)
                login_lucky();
            else if (UtilsDB.selectDB(
                         "SELECT admin.S_ID FROM admin WHERE admin.S_ID='" + textBox1.Text +
                         "' AND admin.`Password`='" + textBox2.Text + "'").Count ==
                     1)
            {
                if (!UtilsDB.addr_Mac.Equals("00-00-00-00-00-00") && UtilsDB.changeDB(
                    "INSERT INTO `logs` (S_ID,TYPE_operation,`COMMENT`,OPER_device) VALUES ('" +
                    textBox1.Text + "','桌面登录','comment','" + UtilsDB.addr_Mac + "')") == 1)
                {
                    s_id = textBox1.Text;
                    mgr = UtilsDB.selectDB("SELECT MGR FROM members WHERE S_ID='" + s_id + "'")[0][0];
                    login_lucky();
                }
                else
                    MessageBox.Show("mac错误", "登录失败");
            }
            else
            {
                MessageBox.Show("密码错误！", "登录失败");
            }
        }

        private void login_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (UtilsDB.selectDB(
                    "SELECT admin.S_ID FROM admin WHERE admin.S_ID='" + s_id +
                    "' AND admin.`Password`='" + textBox2.Text + "'").Count ==
                1)
            {
                if(UtilsDB.changeDB("DELETE FROM `logs` WHERE OPER_device = '" + UtilsDB.addr_Mac + "'")==0)
                    MessageBox.Show("与服务器通讯错误！请重新打开应用", "解绑失败");
                else
                {
                    button2.Hide();
                    s_id = null;
                    mgr = null;
                    freelogin = false;
                    textBox1.Show();
                    label2.Text = "输入登录密码：";
                    label1.Show();
                    button1.Text = "登录PFA队务系统";
                }
            }else
                MessageBox.Show("密码错误！", "解绑失败");
            
        }
    }
}