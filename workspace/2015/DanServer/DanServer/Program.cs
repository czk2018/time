using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DanServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string file = "C:\\Windows\\Temp\\QDW4HF6_VDER08_SWE56G";
            if (File.Exists(file) == false)
            {
                FileStream fs22 = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
                BinaryWriter bw = new BinaryWriter(fs22);
                bw.Write(1);

                bw.Close();
                fs22.Close();
            }
            else
            {
                FileStream fs23 = new FileStream(file, FileMode.Open, FileAccess.ReadWrite);
                BinaryReader br23 = new BinaryReader(fs23);
                BinaryWriter bw = new BinaryWriter(fs23);
                try
                {
                    int c = br23.ReadInt32();
                    if (c > 200)
                    {
                        return;
                    }
                    c++;
                    bw.Seek(0, SeekOrigin.Begin);
                    bw.Write(c);
                }
                catch (EndOfStreamException ex)
                {
                    bw.Close();
                    br23.Close();
                    fs23.Close();
                }
                bw.Close();
                br23.Close();
                fs23.Close();
            }

            try
            {
                bool createNew;
                using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, Application.ProductName, out createNew))
                {
                    if (createNew)
                    {
                        Application.Run(new Form1());
                    }
                    else
                    {
                        MessageBox.Show("应用程序已经在运行中...");
                        System.Threading.Thread.Sleep(1000);
                        System.Environment.Exit(1);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("application -> " + e.ToString());
            }
        }
    }
}
