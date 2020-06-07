using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Chat_Client
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            //string ver = "1.0.1", downloadUpdateExeLink = "http://www.dbsite.ru/chat/update.exe";     //release
            string ver = "1.0.1", downloadUpdateExeLink = "http://localhost/chat/update.exe";           //test

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            System.IO.File.WriteAllText("version.dat", ver);

            try
            {
                Process update = Process.Start("update.exe", Process.GetCurrentProcess().Id.ToString());
                update.WaitForExit();
            }
            catch (Exception)
            {
                if (MessageBox.Show("update.exe не найден!\nСкачать?", "Ошибка!", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    WebClient client = new WebClient();
                    client.DownloadFile(downloadUpdateExeLink, "update.exe");

                    Process update = Process.Start("update.exe", Process.GetCurrentProcess().Id.ToString());
                    update.WaitForExit();
                }

            }

            Application.Run(new Form1());
        }
    }
}
