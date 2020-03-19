using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PFA_Lucky
{
    public class UtilsDB
    {
        static string connStr = "server=127.0.0.1;port=3306;user=customer;password=9876541233;database=together;";

        public static string addr_Mac = "";

        static UtilsDB()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    addr_Mac = BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes());
                    if(addr_Mac!="")
                        break;
                }
            }
            catch (Exception ex)
            {
                addr_Mac = "00-00-00-00-00-00";
                Debug.WriteLine(ex.Message);
            }

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
                //TODO：在这里使用代码对数据库进行增删查改
                MySqlCommand cmd = new MySqlCommand(sql, conn); //生成命令构造器对象。
                // cmd.Parameters.AddWithValue("答案", "ii");
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
            }
            finally
            {
                conn.Close();
            }

            return ret;
        }
    }
}