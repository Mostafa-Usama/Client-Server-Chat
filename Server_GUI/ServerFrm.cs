using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Server_GUI
{
    public partial class ServerFrm : Form
    {
        NetworkStream ns;
        ManualResetEvent mre = new ManualResetEvent(false);
        byte[] data = new byte[1024];
        Socket server;
        int count =0;
        List<Socket> clients = new List<Socket>();
        int PortNum = 9050;
        IPEndPoint remoteEp;
        Socket serv;
        List<int> users = new List<int>();

        public ServerFrm()
        {
            InitializeComponent();
            remoteEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PortNum);
            
            
        }
        
        Thread AdvertiseThread;
        public void Advetise()
        {
            // Advertise for a server operating on port 5000, note that the advetisments themselves are 
            //disseminated on port 5001 using the udp socket
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // I used my loopback address beacuse I'm tring offline in home
            //If U R trying in FCI use a real broadcast Address
            IPEndPoint broadCast = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5001);

            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            //every 10 sec
            while (true)
            {
                s.SendTo(Encoding.UTF8.GetBytes(Dns.GetHostName() + ":" + PortNum.ToString()), broadCast);
                Thread.Sleep(10000);
            }
        }

        void AcceptConn(IAsyncResult iar)
        {
            
            serv = server.EndAccept(iar);
           // broadcast();
            mre.Set();
            clients.Add(serv);
            users.Add(++count);
            //listBox2.Items.Add(serv.RemoteEndPoint);
            listBox1.Invoke((MethodInvoker)delegate
            {
                listBox1.Items.Add("Client " + count.ToString());
            });

            //new Thread(new ParameterizedThreadStart(ReadData)).Start(serv);
            new Thread(new ParameterizedThreadStart(r)).Start(serv);
        }

        void recv(IAsyncResult iar) {
            try
            {

                while(true){
                    byte[] receiveData = new byte[1024];
                    receiveData = (byte[])iar.AsyncState;
                    //Converting byte[] to string
                    ASCIIEncoding ascencoding = new ASCIIEncoding();
                    string response = ascencoding.GetString(receiveData);

                    //Adding message to listbox
                    listBox2.Items.Add(response);

                    //Loop !
                    // receiveData = new byte[1024];
                    ns.BeginRead(receiveData, 0, receiveData.Length,
                                  new AsyncCallback(recv), receiveData);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

       public void r(object ns)
        {
            Socket ser = ns as Socket;
            NetworkStream n = new NetworkStream(serv);
            while (true)
            {
                byte[] receiveData = new byte[1024];
                n.Read(receiveData, 0, receiveData.Length);
                //Converting byte[] to string
                ASCIIEncoding ascencoding = new ASCIIEncoding();
                string response = ascencoding.GetString(receiveData);
                int index = Find(ser);
                //Adding message to listbox
                textBox1.Clear();
                Invoke((MethodInvoker)delegate
              {
                  listBox2.Items.Add("Client " + index.ToString() + ": " + response);
                  
              });
                //Loop !
                // receiveData = new byte[1024];
                //ns.BeginRead(receiveData, 0, receiveData.Length,
                  //            new AsyncCallback(recv), receiveData);
            }

        }
       int Find(Socket s)
       {
           for (int i = 0; i < clients.Count; i++)
           {
               if (clients[i].RemoteEndPoint.ToString() == s.RemoteEndPoint.ToString())
               {
                   return i+1;
               }
           }
           return -1;
       }


       void broadcast()
       {
            ASCIIEncoding ascencoding = new ASCIIEncoding();


           for (int i = 0; i < clients.Count; i++)
           {

               for (int j = 0; j < clients.Count; j++)
               {
                   if (i == j)
                       continue;

                   byte[] buffer = new byte[1024];
                   string ip = clients[j].RemoteEndPoint.ToString();
                   buffer = ascencoding.GetBytes(ip);
                   clients[i].Send(buffer);
               }
           }
       }

    /*    public void ReadData(object s)
        {
            serv = s as Socket;
            ns = new NetworkStream(serv);
            data = new byte[1024];
            ns.BeginRead(data, 0, data.Length, new AsyncCallback(recv), data);
            //serv.BeginReceive(data, 0, data.Length, SocketFlags.None,
                              //new AsyncCallback(recv), data);

        }
*/
        private void button1_Click(object sender, EventArgs e)
        {
             AdvertiseThread = new Thread(new ThreadStart(Advetise));

            AdvertiseThread.Priority = ThreadPriority.Lowest;

            AdvertiseThread.Start();


            // lets do our main work, serving
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            server.Bind(remoteEp);

            server.Listen(-1);

            new Thread(new ThreadStart(start)).Start();

            button1.Enabled = false;
        
        }

        void start()
        {
            while (true)
            {
                Invoke((MethodInvoker)delegate
             {
                 textBox3.Text = "Server is online, waiting for clients...";
             });
                  mre.Reset();

                server.BeginAccept(new AsyncCallback(AcceptConn), server);

                // wait to complete handling 
                mre.WaitOne();
            }
        }
      
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();

            Environment.Exit(Environment.ExitCode);
  
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string msg = textBox1.Text;
            ASCIIEncoding ascencoding = new ASCIIEncoding();
            for (int i = 0; i < clients.Count; i++)
            {
                
                byte [] buffer = new byte[1024];
                buffer = ascencoding.GetBytes(msg);
                clients[i].Send(buffer);
                
            }
            listBox2.Items.Add("Server sent: " + msg);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                byte [] buffer = new byte[1024];
                ASCIIEncoding ascencoding = new ASCIIEncoding();
                buffer = ascencoding.GetBytes(textBox2.Text);
                int index = listBox1.SelectedIndex;
                Socket c = clients[index];
                c.Send(buffer);
                listBox2.Items.Add("Server sent to client "+(index+1).ToString()+": "+textBox2.Text);
                textBox2.Text = "";
            }
            catch(Exception){

                MessageBox.Show("Select a client to send a private message");
            }
        }

    }
}