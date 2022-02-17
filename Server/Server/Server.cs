using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Server : Form
    {
        TcpListener listener;
        TcpClient client;

        NetworkStream stream;
        StreamWriter writer;

        public Server()
        {
            InitializeComponent();
        }

        private void Server_Load(object sender, EventArgs e)
        {
            Thread listenThread = new Thread(new ThreadStart(Listen));
            listenThread.Start();
        }

        private void Listen()
        {
            while (true)
            {
                if (listener != null) listener.Stop();
                if (client != null) client.Close();
                if (writer != null) writer.Close();

                /*
                 * Listener Setting
                 */
                listener = new TcpListener(IPAddress.Any, 3000);
                listener.Start();

                Invoke(new MethodInvoker(delegate ()
                {
                    listTextBox.AppendText("Client 접속 대기 중 ..." + Environment.NewLine);
                }));

                /*
                 * Client Setting
                 */
                client = listener.AcceptTcpClient();
                if (client.Connected)
                {
                    Invoke(new MethodInvoker(delegate ()
                    {
                        listTextBox.AppendText("Client 접속 완료!" + Environment.NewLine);
                    }));
                }

                /*
                 * Stream Create
                 */
                stream = client.GetStream();
                writer = new StreamWriter(stream);

                break;
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
                while (true)
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
                        Listen();
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

            writer.Write(message);
            writer.Flush();

            sendTextBox.Clear();
        }

        private object GetDateTime()
        {
            DateTime NowDate = DateTime.Now;
            return NowDate.ToString("yyyy-MM-dd HH:mm:ss") + ":" + NowDate.Millisecond.ToString("000");
        }

        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (writer != null) writer.Close();
            if (listener != null) listener.Stop();
            if (client != null) client.Close();

            Process.GetCurrentProcess().Kill();
        }
    }
}
