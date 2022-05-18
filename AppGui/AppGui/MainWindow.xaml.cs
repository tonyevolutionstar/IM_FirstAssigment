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
        string dir_music = "file:///C:/Users/tonya/OneDrive/Ambiente de Trabalho/mestrado/IM/IM_FirstAssigment/AppGui/Testplaylist.xspf";


        public MainWindow()
        {

            InitializeComponent();
            //início da comunicação com o VLC

            IPEndPoint socketAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43525);
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
            string e_message_music = "Primeiro tens de abrir a música.";
           
            App.Current.Dispatcher.Invoke(() =>
            {
                switch ((string)json.recognized[0].ToString())
                {
                    case "PLAY":
                        Send(vlcRcSocket, "enqueue " + dir_music);
                        Send_Tts("A música vai começar. Bom aproveito");
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
                                Send_Tts("A música vai continuar.");
                                //Task.Delay(15000);
                                Send(vlcRcSocket, "pause");
                                ++n;
                            }
                            else
                            {
                                Send(vlcRcSocket, "pause");
                                Send_Tts("A música está parada. Quando quiseres, diz Cone,continua.");
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
                            Send_Tts("Passei a música para seguinte");
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
                            Send_Tts("Passei a música para anterior");
                        }
                        break;
                    case "VOLUP":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {                 
                            Send(vlcRcSocket, "volup 10.0");
                           
                        }
                        break;
                    case "VOLDOWN":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "voldown 10.0");
                  
                        }
                        break;
                    case "FAST":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "faster");
                            Send_Tts("Estou a tocar a música mais rápido");
                        }
                        break;
                    case "SLOW":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "slower");
                            Send_Tts("Estou a tocar a música mais lento");
                        }
                        break;
                    case "VNORMAL":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "normal");
                            Send_Tts("Estou a tocar a música na velocidade normal");
                        }
                        break;
                    case "REPEATON":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "repeat on");
                            Send_Tts("A música que está a tocar vai estar tocar outra vez. Para cancelar diz Cone,já não quero voltar a ouvir esta música de novo ");
                        }

                        break;
                    case "REPEATOFF":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "repeat off");
                            Send_Tts("Desativei o modo de repetição para esta música");
                        }
                        break;
                   
                    case "MUTE":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "volume 0");
                            Send_Tts("A música está em silêncio se quiseres retormar diz Cone,podes retornar o som");
                        }
                        break;
                    case "SPEAK":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "volume 10.0");
                            Send_Tts("A música está agora com som, se tiver muito alto ou muito baixo altere com os comandos de aumentar ou diminuir o som, obrigado");
                        }
            
                        break;
                    case "RANDOM":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "random on");
                            Send_Tts("A playlist vai ser tocada aleatória. Para cancelar diz Cone,podes cancelar músicas aletórias");
                        }
                        break;
                    case "RANDOMOFF":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
                            Send(vlcRcSocket, "random off");
                            Send_Tts("A playlist deixou de ser tocada aleatóriamente");
                        }
                        break;
                    case "MUSIC_NAME": 
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {

                            Send(vlcRcSocket, "get_title");
                            byte[] b3 = new byte[100];
                            int k3 = vlcRcSocket.Receive(b3);
                            string szReceived = Encoding.UTF8.GetString(b3, 0, k3);

                            Send_Tts("O nome desta música é " + szReceived);
                        }
                        
                        break;
                    case "TIME_MUSIC":
                        if (n == 0)
                        {
                            Send_Tts(e_message_music);
                        }
                        else
                        {
       

                            Send(vlcRcSocket, "get_length");
                            byte[] b2 = new byte[100];
                            int k2 = vlcRcSocket.Receive(b2);
                            String length = Encoding.ASCII.GetString(b2, 0, k2);

                            Console.WriteLine(length);
                            int seconds = int.Parse(length);

                            
                            TimeSpan t = TimeSpan.FromSeconds(seconds);
                            
                            Send_Tts("A música tem " + t.Hours + " horas " + t.Minutes + " minutos " + t.Seconds + " segundos");
 
                        }
                        break;
                    case "LEAPTIME":
                        Send(vlcRcSocket, "get_time");
                        break;
                    case "QUIT":
                        Send(vlcRcSocket, "quit " + dir_music);
                        Send_Tts("Espero que tenhas gostado de ouvir a música. Até à próxima");
                        break;
                }
            });

            //  new 16 april 2020
            mmic.Send(lce.NewContextRequest());

            string json2 = ""; // "{ \"synthesize\": [";
            json2 += (string)json.recognized[0].ToString() + " ";
            json2 += (string)json.recognized[1].ToString() + " DONE.";
       
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);
        }
    }
}