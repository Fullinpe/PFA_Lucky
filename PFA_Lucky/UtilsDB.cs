using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PFA_Lucky
{
    public class UtilsDB
    {
        static string conn_ = "server=119.27.176.211;port=10005;user=customer;password=9876541233;database=together;";

        // static string connStr = "server=127.0.0.1;port=3306;user=customer;password=9876541233;database=together;";
        static string connStr = "server=2409:8a34:a33:f9e0:8430:7d75:ca95:ad12;port=3306;user=customer;password=9876541233;database=together;";

        public static string addr_Mac = "";

        public static bool linksta = true;

        static UtilsDB()
        {
            try
            {
                connStr= "server="+addrDB("SELECT address_ipv6 FROM addr")+";port=3306;user=customer;password=9876541233;database=together;";
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    addr_Mac = BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes());
                    if (addr_Mac != "")
                        break;
                }
            }
            catch (Exception ex)
            {
                addr_Mac = "00-00-00-00-00-00";
                Debug.WriteLine(ex.Message);
            }
        }

        public static string addrDB(string sql)
        {
            string ret = null;
            MySqlConnection conn = new MySqlConnection(conn_);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn); //生成命令构造器对象。
                MySqlDataReader rdr = cmd.ExecuteReader();
                try
                {
                    if (rdr.Read())
                        ret = rdr[0].ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), @"错误信息_addr");
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"连接失败", @"错误信息_addr");
            }
            finally
            {
                conn.Close();
            }

            return ret;
        }

        //查询字符串   数据库
        public static List<string[]> selectDB(string sql)
        {
            if (sql == "")
                sql = "SELECT * FROM together.members;";
            List<string[]> list = new List<string[]>();
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                Debug.WriteLine("已经建立连接");
                MySqlCommand cmd = new MySqlCommand(sql, conn); //生成命令构造器对象。
                MySqlDataReader rdr = cmd.ExecuteReader();
                try
                {
                    int temp = 0;
                    while (rdr.Read()) //Read()函数设计的时候是按行查询，查完一行换下一行。
                    {
                        string[] strings = new string[rdr.FieldCount];
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            strings[i] = rdr[i].ToString();
                        }

                        list.Add(strings);
                        temp++;
                    }

                    return list;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), @"错误信息");
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"连接失败", @"错误信息");
                linksta = false;
            }
            finally
            {
                conn.Close();
            }

            return list;
        }

        //增删改  数据库
        public static int changeDB(string sql, params MySqlParameter[] mySqlParameter)
        {
            int ret = 0;
            if (sql == "")
                return ret;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                Debug.WriteLine("已经建立连接");
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                foreach (var par in mySqlParameter)
                {
                    cmd.Parameters.Add(par);
                }

                ret = cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show(@"连接失败", @"错误信息");
                linksta = false;
            }
            finally
            {
                conn.Close();
            }

            return ret;
        }

        public static List<byte[]> getbolbDB(string sql, List<int> geti)
        {
            List<byte[]> ret = new List<byte[]>();
            if (sql == "")
                return ret;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                Debug.WriteLine("已经建立连接");
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                var rdr = cmd.ExecuteReader();

                int temp = 0;
                while (rdr.Read())
                {
                    if (geti.Contains(temp))
                    {
                        long len = rdr.GetBytes(0, 0, null, 0, 0);
                        ret.Add(new byte[len]);
                        rdr.GetBytes(0, 0, ret.Last(), 0, (int) len);
                    }

                    temp++;
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"连接失败", @"错误信息");
                linksta = false;
            }
            finally
            {
                conn.Close();
            }

            return ret;
        }

        public static List<byte[]> getpicsDB(string sql, List<int> geti, out List<string> dates)
        {
            List<byte[]> ret = new List<byte[]>();
            dates = new List<string>();
            if (sql == "")
                return ret;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
                Debug.WriteLine("已经建立连接");
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                var rdr = cmd.ExecuteReader();

                int temp = 0;
                while (rdr.Read())
                {
                    if (geti.Contains(temp))
                    {
                        long len = rdr.GetBytes(0, 0, null, 0, 0);
                        ret.Add(new byte[len]);
                        dates.Add(rdr.GetString(1));
                        rdr.GetBytes(0, 0, ret.Last(), 0, (int) len);
                    }

                    temp++;
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"连接失败", @"错误信息");
                linksta = false;
            }
            finally
            {
                conn.Close();
            }

            return ret;
        }

        public delegate void Willclose();

        public static void DeleteItself(Willclose willclose)
        {
            string bat = @"@echo off
                           :tryagain
                           del %1
                           if exist %1 goto tryagain
                           ren %2 PFA_Lucky.exe
                           del %0";
            File.WriteAllText("killme.bat", bat); //写bat文件
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "killme.bat";
            psi.Arguments = "\"" + Environment.GetCommandLineArgs()[0] + "\" \"" + Environment.GetCommandLineArgs()[0] +
                            0 + "\" \"" + Environment.GetCommandLineArgs()[0];
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            willclose();
            Process.Start(psi);
            Application.Exit();
        }
    }
}