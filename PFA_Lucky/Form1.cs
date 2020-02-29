using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
            // button1.Text = GetMacByNetworkInterface();
            string[][] strs = UtilsDB.selectDB("", "");
            button1.Text = strs[1][4];
            // String connetStr = "server=119.27.176.211;port=10005;user=customer;password=1234569877;database=lucky;";
            // MySqlConnection conn = new MySqlConnection(connetStr);
            // try
            // {
            //     conn.Open();
            //     Debug.WriteLine("已经建立连接");
            //     //TODO：在这里使用代码对数据库进行增删查改
            //     string sqlStr = "SELECT * FROM lucky.`题库`;";
            //     MySqlCommand cmd = new MySqlCommand(sqlStr, conn); //生成命令构造器对象。
            //     // cmd.Parameters.AddWithValue("答案", "ii");
            //     MySqlDataReader rdr = cmd.ExecuteReader();
            //     try
            //     {
            //         while (rdr.Read()) //Read()函数设计的时候是按行查询，查完一行换下一行。
            //         {
            //             string[] strings = new string[5];
            //             for (int i = 0; i < strings.Length; i++)
            //             {
            //                 strings[i] = rdr[i].ToString();
            //             }
            //
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         MessageBox.Show(ex.ToString(), @"错误信息");
            //     }
            // }
            // catch (Exception)
            // {
            //     Debug.WriteLine("连接失败");
            //     MessageBox.Show(@"连接失败", @"错误信息");
            // }
            // finally
            // {
            //     conn.Close();
            // }
        }

        public static string GetMacByNetworkInterface()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    return BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return "00-00-00-00-00-00";
        }
    }
}