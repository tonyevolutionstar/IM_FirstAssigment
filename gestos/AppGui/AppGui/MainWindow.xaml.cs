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

            IPEndPoint socketAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43325);
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

        int n = 0;
        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);
            Send(vlcRcSocket, "enqueue " + dir_music);
            //Send(vlcRcSocket, "play");
            string e_message_music = "Primeiro tens de abrir a música.";


            App.Current.Dispatcher.Invoke(() =>
            {


  
                //Console.WriteLine("APP invoker " + (string)json.recognized[1].ToString());


                switch ((string)json.recognized[0].ToString())
                {
                    case "PLAY":
                        Console.WriteLine(dir_music);
                        Send(vlcRcSocket, "enqueue " + dir_music);
                        Send_Tts("Iniciar música");
                        //Task.Delay(20000);
                        Send(vlcRcSocket, "play volume 50.0");
                        n = 1;
                        break;
                    case "PAUSE":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            if (n % 2 == 0 && n != 0)
                            {
                                Send_Tts("A continuar");
                                //Task.Delay(15000);
                                Send(vlcRcSocket, "pause");
                                ++n;
                            }
                            else
                            {
                                Send(vlcRcSocket, "pause");
                                Send_Tts("Em pausa");
                                ++n;
                            }
                        }
                        break;
                    case "RESUME":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "play");
                            Send_Tts("A música recomeçou");
                        }
                        break;
                    case "NEXT":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "next");
                            Send_Tts("Música seguinte");
                        }
                        break;
                    case "PREV":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "prev");
                            Send_Tts("Música anterior");
                        }
                        break;
                    case "VOLUP":
                        Send(vlcRcSocket, "volup 10.0");
                        break;
                    case "VOLDOWN":
                        Send(vlcRcSocket, "voldown 10.0");
                        break;
                    case "FAST":
                        Send(vlcRcSocket, "faster");
                        Send_Tts("Velocidade rápida");
                        break;
                    case "SLOW":
                        Send(vlcRcSocket, "slower");
                        Send_Tts("Velocidade lenta");
                        break;
                    case "VNORMAL":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "normal");
                            Send_Tts("Velocidade normal");
                        }
                        break;
                    case "REPEATON":
                        Send(vlcRcSocket, "repeat on");
                        Send_Tts("Repetir música");
                        break;
                    case "REPEATOFF":
                        Send(vlcRcSocket, "repeat off");
                        Send_Tts("Repetição cancelada");
                        break;
                    case "MUTE":
                    case "mutemusic":

                        if (mute_n % 2 == 0)
                        {
                            Send(vlcRcSocket, "volume 0");
                        }
                        else
                        {
                            Send(vlcRcSocket, "volume 150.0");
                        }

                        mute_n += 1;

                        break;
                    case "RANDOM":
                        Send(vlcRcSocket, "random on");
                        Send_Tts("Modo aleatório ativo");

                        break;
                    case "RANDOMOFF":
                        Send(vlcRcSocket, "random off");
                        Send_Tts("Modo aleatório inativo");
                        break;
                    case "MUSIC_NAME":
                        Send(vlcRcSocket, "get_title");
                        byte[] b3 = new byte[100];
                        int k3 = vlcRcSocket.Receive(b3);
                        string szReceived = Encoding.UTF8.GetString(b3, 0, k3);
                        while (szReceived.StartsWith("status change:"))
                        {
                            Send(vlcRcSocket, "get_title");
                            b3 = new byte[100];
                            k3 = vlcRcSocket.Receive(b3);
                            szReceived = Encoding.UTF8.GetString(b3, 0, k3);
                        }
                        Send_Tts("O nome desta música é " + szReceived.Replace(".mp3", ""));

                        break;
                    case "TIME_MUSIC":
                        Send(vlcRcSocket, "get_length");
                        byte[] b2 = new byte[100];
                        int k2 = vlcRcSocket.Receive(b2);
                        String length = Encoding.ASCII.GetString(b2, 0, k2);

                        Console.WriteLine(length);
                        int seconds = int.Parse(length);


                        TimeSpan t = TimeSpan.FromSeconds(seconds);
                        string value = "A música tem ";
                        if (t.Hours != 0)
                        {
                            value += t.Hours + " horas ";
                        }
                        if (t.Minutes != 0)
                        {
                            value += t.Minutes + " minutos ";
                        }
                        if (t.Seconds != 0)
                        {
                            value += t.Seconds + " segundos";
                        }


                        Send_Tts(value);

                        break;
                    case "MUSIC_SELECTED":

                        string command = "goto " + (string)json.recognized[1].ToString();
                        Send(vlcRcSocket, command);

                        string to_speech = "Musica mudada";

                        Send(vlcRcSocket, "volume 10.0");
                        Send_Tts(to_speech);

                        Send(vlcRcSocket, "volume 200.0");

                        break;
                    case "QUIT":
                        Send(vlcRcSocket, "quit " + dir_music);
                        Send_Tts("Até à próxima");
                        break;

                    case "proxmusicR":
                        Send(vlcRcSocket, "next");
                        break;
                    case "prevmusicL":
                        Send(vlcRcSocket, "prev");
                        break;

                }

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
                    case "randommusic":
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