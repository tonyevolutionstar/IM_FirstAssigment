using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGui
{
    internal class Vlc
    {
        public Vlc()
        {

            Process p;

            p = new Process();
            p.StartInfo.FileName = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";
            var DDIR = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine("Dir " + System.AppContext.BaseDirectory);
            ///IM_FirstAssigment/AppGui/AppGui/bin/Debug/music
            //

            p.StartInfo.Arguments = "\"C:\\Users\\tonya\\Music\\Anthrax-GotTheTime.mp3\""; //C:\Users\tonya\Music\Anthrax-GotTheTime.mp3

            //p.StartInfo.Arguments = System.AppContext.BaseDirectory + "\\music\\Anthrax-GotTheTime.mp3\""; //C:\Users\tonya\Music\Anthrax-GotTheTime.mp3

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
        }

        private static void Log(string v)
        {
            Console.WriteLine(v);
        }
    
    }
}
