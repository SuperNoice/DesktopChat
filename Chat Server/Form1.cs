using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chat_Server
{
    public partial class Form1 : Form
    {
        IPEndPoint localAdress;
        Socket localSocket;
        List<Socket> users;

        byte[] data;

        string message;

        public Form1()
        {
            InitializeComponent();

            data = new byte[255];
            users = new List<Socket>();

            localAdress = new IPEndPoint(IPAddress.Parse("192.168.88.254"), 15901);

            localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            localSocket.Bind(localAdress);

            Start();

        }

        async void Start()
        {
            await Task.Run(() => StartChatServer());
        }

        async void connectUsers()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Socket user = localSocket.Accept();
                    if (!users.Contains(user))
                    {
                        logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "[" + DateTime.Now.ToString() + "] " + "Установлено подключение: " + user.RemoteEndPoint.ToString() + "\n"; });
                        users.Add(user);
                    }
                }
            });

        }

        void StartChatServer()
        {
            Thread.Sleep(1000);

            logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "[" + DateTime.Now.ToString() + "] " + "Сервер работает на адресе: " + localAdress.ToString() + "\n"; });

            localSocket.Listen(10);

            logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "[" + DateTime.Now.ToString() + "] " + "Послушивание портов включено\n"; });

            connectUsers();

            while (true)
            {
                Thread.Sleep(100);

                for (int i = 0; i < users.Count; ++i)
                {
                    Socket user = users[i];

                    try
                    {
                        byte[] tmp = new byte[1];
                        user.Send(tmp, 0, 0);
                    }
                    catch (SocketException e)
                    {
                        logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "[" + DateTime.Now.ToString() + "] " + "Соединение разорванно: " + user.RemoteEndPoint.ToString() + "\n"; });
                        users.Remove(user);
                        continue;
                    }

                    if (user.Connected == false)
                    {
                        logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "[" + DateTime.Now.ToString() + "] " + "Соединение разорванно: " + user.RemoteEndPoint.ToString() + "\n"; });
                        users.Remove(user);
                        continue;
                    }

                    if (user.Available == 0) continue;
                    int bytes = 0;
                    message = "";
                    do
                    {
                        bytes = user.Receive(data);
                        message += Encoding.UTF8.GetString(data, 0, bytes);
                    } while (user.Available > 0);

                    logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "[" + DateTime.Now.ToString() + "] " + message + "\n"; });

                    byte[] sendData = Encoding.UTF8.GetBytes(message);
                    for (int ptr = 0; ptr < users.Count; ++ptr)
                    {
                        Socket toUser = users[ptr];

                        if (user == toUser) continue;

                        if (toUser.Connected == false)
                        {
                            logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "[" + DateTime.Now.ToString() + "] " + "Соединение разорванно: " + user.RemoteEndPoint.ToString() + "\n"; });
                            users.Remove(toUser);
                            continue;
                        }

                        try { toUser.Send(sendData); }
                        catch (Exception)
                        {
                            logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "[" + DateTime.Now.ToString() + "] " + "Соединение разорванно: " + user.RemoteEndPoint.ToString() + "\n"; });
                            users.Remove(toUser);
                            continue;
                        }

                    }
                }
            }
        }

        private void logRichTextBox_TextChanged(object sender, EventArgs e)
        {
            logRichTextBox.SelectionStart = logRichTextBox.Text.Length;
            logRichTextBox.ScrollToCaret();
        }
    }
}
