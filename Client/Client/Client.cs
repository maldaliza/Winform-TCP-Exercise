using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Client : Form
    {
        TcpClient client;

        NetworkStream stream;
        StreamWriter writer;

        string server;
        int port;

        bool connected = false;

        public Client()
        {
            InitializeComponent();
        }

        private void Client_Load(object sender, EventArgs e)
        {
            // Server IP/Port Setting
            server = ConfigurationManager.AppSettings["Server"];
            port = 3000;

            Thread connectThread = new Thread(new ThreadStart(Connect));
            connectThread.Start();
        }

        private void Connect()
        {
            connected = false;

            while (true)
            {
                try
                {
                    if (client != null) client.Close();
                    if (writer != null) writer.Close();

                    /*
                     * Client Setting
                     */
                    client = new TcpClient(server, port);

                    Invoke(new MethodInvoker(delegate ()
                    {
                        listTextBox.AppendText("Client 접속 완료!" + Environment.NewLine);
                    }));

                    /*
                     * Stream Create
                     */
                    stream = client.GetStream();
                    writer = new StreamWriter(stream);

                    if (client.Connected)
                    {
                        connected = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            /*
             * Receive Start
             */
            Receive();
        }

        private void Receive()
        {
            try
            {
                while (connected)
                {
                    byte[] buffer = new byte[1024];
                    int bytes = stream.Read(buffer, 0, buffer.Length);

                    if (bytes > 0)
                    {
                        string message = Encoding.Default.GetString(buffer, 0, bytes);

                        Invoke(new MethodInvoker(delegate ()
                        {
                            messageTextBox.AppendText("[" + GetDateTime() + "][Receive] " + message + Environment.NewLine);
                        }));
                    }
                    else
                    {
                        Connect();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void sendMessageBtn_Click(object sender, EventArgs e)
        {
            string message = sendTextBox.Text;
            messageTextBox.AppendText("[" + GetDateTime() + "][Send] " + message + Environment.NewLine);

            if (connected != false)
            {
                writer.Write(message);
                writer.Flush();
            }

            sendTextBox.Clear();
        }

        private object GetDateTime()
        {
            DateTime NowDate = DateTime.Now;
            return NowDate.ToString("yyyy-MM-dd HH:mm:ss") + ":" + NowDate.Millisecond.ToString("000");
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            connected = false;
            if (writer != null) writer.Close();
            if (client != null) client.Close();

            Process.GetCurrentProcess().Kill();
        }
    }
}
