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

namespace Chat_Client
{
    public partial class Form1 : Form
    {
        class Constant
        {
            public string localAdress = "127.0.0.1";            //release, test

            public string remoteAdress = "192.168.88.254";      //test
            //public string remoteAdress = "194.150.255.135";   //release
        };

        IPEndPoint localAdress, remoteAdress;
        Socket localSocket;

        byte[] data;

        string message;

        public Form1()
        {
            InitializeComponent();

            data = new byte[255];
            Constant Constant = new Constant();

            localAdress = new IPEndPoint(IPAddress.Parse(Constant.localAdress), 15899);
            remoteAdress = new IPEndPoint(IPAddress.Parse(Constant.remoteAdress), 15901);

            localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            localSocket.Bind(localAdress);

            Start();
        }

        async void Start()
        {
            await Task.Run(() => StartChat());
        }


        private void message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                nickTextBox.Text = nickTextBox.Text.Trim();
                messageTextBox.Text = messageTextBox.Text.Trim();

                if (nickTextBox.TextLength == 0)
                {
                    MessageBox.Show("Введите ник!", "Ошибка!");
                    return;
                }

                if (!localSocket.Connected && messageTextBox.Text != "/reconnect")
                {
                    MessageBox.Show("Подключитесь к серверу!", "Ошибка!");
                    return;
                }

                if (messageTextBox.TextLength != 0)
                {
                    switch (messageTextBox.Text)
                    {
                        case "/reconnect": Start(); break;

                        default:
                            byte[] sendData = Encoding.UTF8.GetBytes(nickTextBox.Text + ": " + messageTextBox.Text);
                            localSocket.Send(sendData);
                            logRichTextBox.Text += nickTextBox.Text + ": " + messageTextBox.Text + "\n";
                            break;
                    }

                }
                messageTextBox.Clear();
            }

        }

        async void getMessages()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);

                    int bytes = 0;
                    message = "";
                    do
                    {
                        try
                        {
                            bytes = localSocket.Receive(data);
                        }
                        catch (Exception)
                        {
                            logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "Сервер закрыл соединение! /reconnect для переподключения\n"; });
                            return;
                        }

                        message += Encoding.UTF8.GetString(data, 0, bytes);
                    } while (localSocket.Available > 0);

                    if (bytes == 0)
                    {
                        logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "Сервер закрыл соединение! /reconnect для переподключения\n"; });
                        return;
                    }

                    logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += message + "\n"; });
                }
            });
        }

        private void logRichTextBox_TextChanged(object sender, EventArgs e)
        {
            logRichTextBox.SelectionStart = logRichTextBox.Text.Length;
            logRichTextBox.ScrollToCaret();
        }

        void StartChat()
        {
            Thread.Sleep(1000);

            logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "Подключение к серверу...\n"; });

            try { localSocket.Connect(remoteAdress); }
            catch (Exception)
            {
                try
                {
                    localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    localSocket.Connect(remoteAdress);
                }
                catch (Exception)
                {
                    logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "Не удалось подключится к серверу! /reconnect для переподключения\n"; });
                    return;
                }
            }

            logRichTextBox.Invoke((MethodInvoker)delegate { logRichTextBox.Text += "Соединение установлено!\n"; });

            getMessages();


        }
    }
}
