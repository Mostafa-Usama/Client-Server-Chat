using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client_GUI
{
   
    public partial class ClientFrm : Form
    {
       
        private byte[] data = new byte[1024];
        private int size = 1024;
        Socket newsock;

        public ClientFrm()
        {
            InitializeComponent();
        }

    

        private void btnConnect_Click(object sender, EventArgs e)
        {
            newsock = new Socket(AddressFamily.InterNetwork,
                   SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
            newsock.BeginConnect(iep, new AsyncCallback(Connected), newsock);
        }


        void Connected(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            try
            {
                client.EndConnect(iar);
                byte[]buffer = new byte[1024];
                
                Invoke((MethodInvoker)delegate
               {
                   btnConnect.Enabled = false;
               });
                    textBox1.Text = "Connected to: " + client.RemoteEndPoint.ToString();
                    //client.BeginReceive(data, 0, size, SocketFlags.None, new AsyncCallback(ReceiveData), data);
                    new Thread(new ParameterizedThreadStart(r)).Start(client);
               
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        void r(object s)
        {
            Socket sock = s as Socket;
            NetworkStream n = new NetworkStream(sock);
            while (true)
            {
                byte[] receiveData = new byte[1024];
                n.Read(receiveData, 0, receiveData.Length);
                //Converting byte[] to string
                ASCIIEncoding ascencoding = new ASCIIEncoding();
                string response = ascencoding.GetString(receiveData);
                
                //Adding message to listbox
                MessageList.Items.Add("Server sent: "+response);

                //Loop !
                // receiveData = new byte[1024];
                //ns.BeginRead(receiveData, 0, receiveData.Length,
                //            new AsyncCallback(recv), receiveData)
            }
        }

        void ReceiveData(IAsyncResult iar)
        {
            byte [] buffer= (byte[])iar.AsyncState;
            //int recv = remote.EndReceive(iar);
            string stringData = Encoding.ASCII.GetString(buffer);
            Invoke((MethodInvoker)delegate
               {
                   MessageList.Items.Add(stringData);
                   
               });
          
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ASCIIEncoding ascencoding = new ASCIIEncoding();
            byte[] sendmess = new byte[1500];
            sendmess = ascencoding.GetBytes(textMessage.Text);
            newsock.Send(sendmess);
            MessageList.Items.Add("You Said:" + textMessage.Text);
            textMessage.Text = "";
        }
    }
}
