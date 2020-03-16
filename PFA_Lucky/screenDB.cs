using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Management;
using System.Windows.Forms;

namespace PFA_Lucky
{
    public class screenDB
    {
        public static Bitmap GetScreenCapture()
        {
            float factor = (float)ppxI / 96;
            Rectangle tScreenRect = new Rectangle(0, 0, (int) (Screen.PrimaryScreen.Bounds.Width * factor),
                (int) (Screen.PrimaryScreen.Bounds.Height * factor));
            Bitmap tSrcBmp = new Bitmap(tScreenRect.Width, tScreenRect.Height); // 用于屏幕原始图片保存
            Graphics gp = Graphics.FromImage(tSrcBmp);
            gp.CopyFromScreen(0, 0, 0, 0, tScreenRect.Size);
            gp.DrawImage(tSrcBmp, 0, 0, tScreenRect, GraphicsUnit.Pixel);
            return tSrcBmp;
        }

        public static int ppxI = 0; // dpi for x

        static screenDB()
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
    }
}