using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace speechModality
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SpeechMod _sm;
        public MainWindow()
        {
            InitializeComponent();

            // generate playlist by music folder
            Generate_playlist();

            // update dynamic grammar
            Update_music_list_grammar();

            _sm = new SpeechMod();
            _sm.Recognized += _sm_Recognized;
        }

        private void _sm_Recognized(object sender, SpeechEventArg e)
        {
            result.Text = e.Text;
            confidence.Text = e.Confidence + "";
            if (e.Final) result.FontWeight = FontWeights.Bold;
            else result.FontWeight = FontWeights.Normal;
        }
        private void Update_music_list_grammar()
        {
            //Get rigth path of music list
            string main_dir = Environment.CurrentDirectory;
            string dir_music = main_dir.Replace("speechModality\\speechModality\\bin\\x64\\Debug", "music\\");



            string xml_initial = System.IO.File.ReadAllText(main_dir.Replace("bin\\x64\\Debug", "ptG_initial.grxml"));

            string dynamic_grammar_file = main_dir.Replace("bin\\x64\\Debug", "Dynamic_Grammar.grxml");
            System.IO.File.Delete(dynamic_grammar_file);

            string[] files = System.IO.Directory.GetFiles(dir_music);


            System.IO.StreamWriter TestWriter = new System.IO.StreamWriter(dynamic_grammar_file);



            TestWriter.Write("<grammar xml:lang = \"pt-PT\" version = \"1.0\" xmlns = \"http://www.w3.org/2001/06/grammar\" tag-format = \"semantics/1.0\" >\n" +
                            "<rule id = \"music\" scope=\"public\"> \n" +
                                "<item>\n" +
                                    "<one-of>\n" +
                                        "<item>coloca esta música<tag> out.action = \"MUSIC_SELECTED\";</tag></item>\n" +
                                        "<item>poe a música<tag> out.action = \"MUSIC_SELECTED\";</tag></item>\n" +
                                    "</one-of>\n" +
                                "</item>\n" +
                                "<item>\n" +
                                    "<one-of>\n");

            int nmusic = 1;
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file).ToString();

                string musician = name.Split('-')[0];

                string music = name.Split('-')[1].Split('.')[0];
                Console.WriteLine(musician + " " + music);
                TestWriter.Write("<item>" + music + "<item repeat='0-1'>" + musician + "</item><tag>out.action2='" + nmusic + "'</tag> </item>\n");
                nmusic++;
            }

            TestWriter.Write("</one-of></item></rule></grammar>");
            TestWriter.Close();
        }

        private void Generate_playlist()
        {
            //Get rigth path of music list
            string path = Environment.CurrentDirectory;
            path = path.Replace("speechModality\\speechModality\\bin\\x64\\Debug", "music\\");

            //get all files names on folder "music"
            string[] files = System.IO.Directory.GetFiles(path);


            string path_to_playlist = Environment.CurrentDirectory;
            path_to_playlist = path_to_playlist.Replace("speechModality\\speechModality\\bin\\x64\\Debug", "AppGui\\") + "playlist.xspf";
            System.IO.File.Delete(path_to_playlist);
            System.IO.StreamWriter TestWriter = new System.IO.StreamWriter(path_to_playlist);
            TestWriter.AutoFlush = true;
            string new_xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                "<playlist xmlns=\"http://xspf.org/ns/0/\" xmlns:vlc=\"http://www.videolan.org/vlc/playlist/ns/0/\" version=\"1\">\n" +
                                    "\t<title>Lista de reprodução</title>\n" +
                                    "\t<trackList>\n";
            TestWriter.Write(new_xml);

            int nmusic = 0;
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file).ToString();
                Console.WriteLine(file);


                new_xml = "\t\t<track>\n" +
                                    "\t\t<location>file:///" + file.Replace(" ","%20").Replace("\\","/") + "</location>\n" +
                                    "\t\t\t<extension application=\"http://www.videolan.org/vlc/playlist/0\">\n" +
                                        "\t\t\t\t<vlc:id>" + nmusic + "</vlc:id>\n" +
                                        "\t\t\t\t<vlc:option>file-caching=1000</vlc:option>\n" +
                                    "\t\t\t</extension>\n" +
                                    "\t\t</track>\n";
                TestWriter.Write(new_xml);
                nmusic++;
            }
            new_xml = "\t</trackList>\n" +
                            "<extension application=\"http://www.videolan.org/vlc/playlist/0\">\n";
            TestWriter.Write(new_xml);
            nmusic = 0;
            foreach (string file in files)
            {
                new_xml = "\t\t<vlc:item tid=\"" + nmusic +"\"/>\n";
                TestWriter.Write(new_xml);
                nmusic++;
            }
            new_xml = "</extension>\n" +
                            "</playlist>";

            
            TestWriter.Write(new_xml);
        }
    }
}
