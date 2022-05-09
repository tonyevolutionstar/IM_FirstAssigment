using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGui
{
    public class Vlc
    {
        private string dir_vlc = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";
        Process p;
        
        string dir_music = @"C:\Users\tonya\Music\";
        public Vlc()
        {
            p = new Process();
          
            //var DDIR = System.IO.Directory.GetCurrentDirectory();
            //Console.WriteLine("Dir " + System.AppContext.BaseDirectory);
            //

            //p.StartInfo.Arguments = "\"C:\\Users\\tonya\\Music\\Anthrax-GotTheTime.mp3\""; //C:\Users\tonya\Music\Anthrax-GotTheTime.mp3

            //p.StartInfo.Arguments = System.AppContext.BaseDirectory + "\\music\\Anthrax-GotTheTime.mp3\""; //C:\Users\tonya\Music\Anthrax-GotTheTime.mp3

        }

        private static void Log(string v)
        {
            Console.WriteLine(v);
        }

        public string play_music(string name)
        {
            return "play " + dir_music + name + ".mp3";
        }

        public void add_file_dir()
        {
            string[] files = Directory.GetFiles(dir_music);
            foreach (string file in files)
                Console.WriteLine(Path.GetFileName(file));

            System.IO.StreamWriter TestWriter = new System.IO.StreamWriter("test1.grxml");

            TestWriter.Write("<?xml version=\"1.0\" encoding=\"utf - 8\"?><grammar mode=\"voice\" version=\"1.0\" root=\"main\"><rule id=\"main\"><one-of>");

            foreach (string file in files)
            {
                string name = Path.GetFileName(file).ToString();
                string musician = name.Split('-')[0];
                string music = name.Split('-')[1].Split('.')[0];
                Console.WriteLine(musician + " " + music);
                TestWriter.Write("<item>{0}<tag>out={1}</tag></item>", musician + " " + music, musician + " " + music);
            }

            TestWriter.Write("</one-of></rule></grammar>");
            TestWriter.Close();
        }


        public void send_commands_vlc(string cmd)
        {
            p.StartInfo.FileName = dir_vlc;
            p.StartInfo.Arguments = cmd;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            p.OutputDataReceived += (sender2, e2) =>
            {
                if (e2.Data != null)
                {
                    Log("Received output data: " + e2.Data);
                }
                else
                    Log("Received output data: NULL");
            };

            p.ErrorDataReceived += (sender2, e2) =>
            {
                if (e2.Data != null)
                {
                    Log("Received Error data: " + e2.Data);
                }
                else
                    Log("Received Error data: NULL");
            };

            p.Start();

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            p.StandardInput.WriteLine("get_time");
            p.StandardInput.Flush();

            p.Kill();

        }

    }
}
