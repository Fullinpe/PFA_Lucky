using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Windows.Forms;
using OpenCvSharp;

namespace PFA_Lucky
{
    public class UtilsPic
    {
        public static int ppxI = 0; // dpi for x

        static UtilsPic()
        {
            using (ManagementClass mc = new ManagementClass("Win32_DesktopMonitor"))
            {
                using (ManagementObjectCollection moc = mc.GetInstances())
                {
                    foreach (ManagementObject each in moc)
                    {
                        ppxI = int.Parse((each.Properties["PixelsPerXLogicalInch"].Value.ToString()));
                    }
                }
            }
        }
        public static Bitmap GetScreenCapture()
        {
            float factor = (float)ppxI / 96.0f;
            Rectangle tScreenRect = new Rectangle(0, 0, (int) (Screen.PrimaryScreen.Bounds.Width * factor),
                (int) (Screen.PrimaryScreen.Bounds.Height * factor));
            Bitmap tSrcBmp = new Bitmap(tScreenRect.Width, tScreenRect.Height); // 用于屏幕原始图片保存
            Graphics gp = Graphics.FromImage(tSrcBmp);
            gp.CopyFromScreen(0, 0, 0, 0, tScreenRect.Size);
            gp.DrawImage(tSrcBmp, 0, 0, tScreenRect, GraphicsUnit.Pixel);
            return tSrcBmp;
        }

        public static byte[] Bitmap2Byte(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream , ImageFormat.Jpeg);
                byte[] data = new byte[stream.Length];
                stream.Seek(0 , SeekOrigin.Begin);
                stream.Read(data ,0  , Convert.ToInt32(stream.Length));
                return data;
            }
        }
    }
}