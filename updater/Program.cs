using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace update
{
    static class Program
    {
        
        [STAThread]
        static void Main(string[] args)
        {
            int appId;
            try
            {
                appId = Convert.ToInt32(args[0]);
            }
            catch (Exception)
            {
                return;
            }

            //string downloadVersionLink = "http://www.dbsite.ru/chat/version.dat";     //release
            //string downloadExeLink = "http://www.dbsite.ru/chat/ChatClient.exe";

            string downloadVersionLink = "http://localhost/chat/version.dat";           //test
            string downloadExeLink = "http://localhost/chat/ChatClient.exe";
            
            string ver = System.IO.File.ReadAllText("version.dat");

            WebClient client = new WebClient();
            string remoteVersion;
            try { remoteVersion = client.DownloadString(downloadVersionLink); }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка обновления! verionDownload\n" + e.ToString(), "Ошибка!");
                return;
            }

            if (!ver.Equals(remoteVersion))
            {
                if (MessageBox.Show("Доступна новая версия! Версия " + ver + " => " + remoteVersion + "\nСкачать?", "Обновление!", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    try { client.DownloadFile(downloadExeLink, "ChatClientTemp.exe"); }
                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка обновления! downloadClient\n" + e.ToString(), "Ошибка!");
                        return;
                    }

                    try
                    {
                        Process.GetProcessById(appId).Kill();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка обновления! KillClient\n" + e.ToString(), "Ошибка!");
                        return;
                    }

                    try
                    {
                        Process del = Process.Start("cmd.exe", "/c dir & del ChatClient.exe");
                        del.WaitForExit();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка обновления! DeleteClient\n" + e.ToString(), "Ошибка!");
                        return;
                    }

                    try
                    {

                        System.IO.File.Move("ChatClientTemp.exe", "ChatClient.exe");

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка обновления! MoveClient" + e.ToString(), "Ошибка!");
                        return;
                    }

                    try
                    {


                        Process.Start("ChatClient.exe");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Ошибка обновления! StartClient\n" + e.ToString(), "Ошибка!");
                        return;
                    }


                }
                else return;

            }

        }
    }
}
