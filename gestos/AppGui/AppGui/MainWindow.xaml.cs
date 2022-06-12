using mmisharp;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;


namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private static void Send(Socket socket, string command)
        {
            // send command to vlc socket, note \n is important
            byte[] commandData = UTF8Encoding.UTF8.GetBytes(String.Format("{0}\n", command));
            int sent = socket.Send(commandData);
        }

        private static void Receive(Socket socket)
        {
            do
            {
                if (socket.Connected == false)
                    break;
                // check if there is any data
                bool haveData = socket.Poll(1000000, SelectMode.SelectRead);

                if (haveData == false) continue;
                byte[] buffer = new byte[socket.ReceiveBufferSize];
                using (MemoryStream mem = new MemoryStream())
                {
                    while (haveData)
                    {
                        int received = socket.Receive(buffer);
                        mem.Write(buffer, 0, received);
                        haveData = socket.Poll(1000000, SelectMode.SelectRead);
                    }

                    Console.WriteLine(Encoding.UTF8.GetString(mem.ToArray()));
                }

            } while (true);
        }


        private MmiCommunication mmiC;

        private MmiCommunication mmiSender;
        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        Socket vlcRcSocket;
        private object tts;
        int mute_n = 0;
        int random_n = 0;

        string dir_music = "playlist.xspf";

        public MainWindow()
        {
          

            InitializeComponent();
            //início da comunicação com o VLC

            IPEndPoint socketAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43225);
            var vlcServerProcess = System.Diagnostics.Process.Start(@"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe", "-I rc --rc-host " + socketAddress.ToString());

            try
            {
                vlcRcSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                vlcRcSocket.Connect(socketAddress);
                // start another thread to look for responses and display them
                Task listener = System.Threading.Tasks.Task.Factory.StartNew(() => Receive(vlcRcSocket));

                Console.WriteLine("Connected. Enter VLC commands.");
            }
            finally
            {
                //vlcServerProcess.Kill();
            }

            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            // NEW 16 april 2020
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("APP", "TTS", "User1", "na", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode
            // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GUI");
        }

        private void Send_Tts(string send)
        {
            mmic.Send(lce.NewContextRequest());
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, send);
            mmic.Send(exNot);
        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            Send(vlcRcSocket, "enqueue " + dir_music);
            Send(vlcRcSocket, "play");

           
            App.Current.Dispatcher.Invoke(() =>
            {
                //Console.WriteLine("APP invoker " + (string)json.recognized[1].ToString());
                //Task.Delay(20000);
                switch ((string)json.recognized[1].ToString())
                {
                    case "nextmusicR":
                        Send(vlcRcSocket, "next");
                        break;
                    case "previousmusicL":
                        Send(vlcRcSocket, "prev");
                        break;
                    case "mute":
                        if (mute_n % 2== 0)
                        {
                            Send(vlcRcSocket, "volume 0");
                        }
                        else
                        {
                            Send(vlcRcSocket, "volume 150.0");
                        }

                        mute_n += 1;
                        

                        break;
                    case "random":
                        if(random_n % 2 == 0)
                        {
                            Send(vlcRcSocket, "random on");
                           
                        }
                        else
                        {
                            Send(vlcRcSocket, "random off");
                           
                        }
                        random_n += 1;

                        break;
                  
                }
            });

            //  new 16 april 2020
            mmic.Send(lce.NewContextRequest());
            string json2 = ""; // "{ \"synthesize\": [";
            json2 += (string)json.recognized[1].ToString();
            Console.WriteLine(json2);

            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);
        }
    }
}