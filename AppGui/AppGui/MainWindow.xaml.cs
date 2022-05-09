

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


        public MainWindow()
        {
            InitializeComponent();
            //início da comunicação com o VLC

            IPEndPoint socketAddress = new IPEndPoint(IPAddress.Parse("192.168.41.230"), 54165);
            //  var vlcServerProcess = System.Diagnostics.Process.Start(@"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe", "-I rc --rc-host " + socketAddress.ToString());
            // var vlcServerProcess = System.Diagnostics.Process.Start(@"C:\Program Files\VideoLAN\VLC\vlc.exe", "-I rc --rc-host " + socketAddress.ToString());
            var vlcServerProcess = System.Diagnostics.Process.Start(@"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe", "-I rc --rc-host " + socketAddress.ToString());

            //"C:\Program Files\VideoLAN\VLC\vlc.exe"

            try
            {
                vlcRcSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                vlcRcSocket.Connect(socketAddress);
                // start another thread to look for responses and display them
                Task listener = System.Threading.Tasks.Task.Factory.StartNew(() => Receive(vlcRcSocket));

                Console.WriteLine("Connected. Enter VLC commands.");
                //Send(vlcRcSocket, "enqueue file:///C:/Users/asus/Desktop/playlist.xspf");
                Send(vlcRcSocket, "enqueue file:///C:/Users/tonya/Music/Linkin Park - Numb.mp3");

            }
            finally
            {
                //   vlcServerProcess.Kill();
            }

            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            // NEW 16 april 2020
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("APP", "TTS", "User1", "na", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode
            // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GUI");
            Send_Tts("Olá. Espero que esteja tudo bem contigo. Estou pronta para receber ordens. Após iniciares o filme, poderás colocá-lo em pausa, alterar o volume e a sua velocidade. Se gostares muito de algum vídeo em especial, é só dizeres que queres repetir. Mas, se não gostares, podes também colocar o vídeo seguinte.");
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

            App.Current.Dispatcher.Invoke(() =>
            {
                switch ((string)json.recognized[0].ToString())
                {
                    case "PLAY":
                        Send_Tts("O filme vai começar. Boa sessão.");
                        Task.Delay(20000);
                        Send(vlcRcSocket, "play");
                        n = 1;
                        break;
                    case "PAUSE":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {

                            if (n % 2 == 0 && n != 0)
                            {
                                Send_Tts("O filme vai continuar.");
                                Task.Delay(15000);
                                Send(vlcRcSocket, "pause");
                                ++n;
                            }
                            else
                            {
                                Send(vlcRcSocket, "pause");
                                Send_Tts("O filme está parado. Quando quiseres, diz para continuar.");
                                ++n;
                            }
                        }
                        break;
                    case "NEXT":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "next");
                        }
                        break;
                    case "PREV":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "prev");
                        }
                        break;
                    case "VOLUP":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "volup 10");
                        }
                        break;
                    case "VOLDOWN":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "voldown 10");
                        }
                        break;
                    case "FON":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "f [on]");
                        }
                        break;
                    case "FAST":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "faster");
                        }
                        break;
                    case "SLOW":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "slower");
                        }
                        break;
                    case "VNORMAL":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "normal");
                        }
                        break;
                    case "REPEATON":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "repeat [on]");
                            Send_Tts("O filme está em repetição");
                        }

                        break;
                    case "REPEATOFF":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "repeat [off]");
                            Send_Tts("A repetição do filme está desativada");
                        }
                        break;
                    case "LOOP":
                        if (n == 0)
                        {
                            Send_Tts("Primeiro tens que iniciar o filme.");
                        }
                        else
                        {
                            Send(vlcRcSocket, "loop [on]");
                            Send_Tts("A repetição contínua dos filmes foi ativada");
                        }
                        break;
                    case "QUIT":
                        Send(vlcRcSocket, "quit");
                        Send_Tts("Espero que tenhas gostado do filme. Até à próxima");
                        break;
                }
            });

            //  new 16 april 2020
            mmic.Send(lce.NewContextRequest());

            string json2 = ""; // "{ \"synthesize\": [";
            json2 += (string)json.recognized[0].ToString() + " ";
            json2 += (string)json.recognized[1].ToString() + " DONE.";
            //json2 += "] }";
            /*
             foreach (var resultSemantic in e.Result.Semantics)
            {
                json += "\"" + resultSemantic.Value.Value + "\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";
            */
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);
        }
    }
}