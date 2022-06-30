using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmisharp;
using Microsoft.Speech.Recognition;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Windows.Shapes;
using System.Windows.Media;
using System.IO;
//using Newtonsoft.Json;

namespace speechModality
{
    public class SpeechMod
    {
        // changed 16 april 2020
        private static SpeechRecognitionEngine sre= new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
        private Grammar gr;

        private bool question;
        private string last_json;

        public event EventHandler<SpeechEventArg> Recognized;
        protected virtual void onRecognized(SpeechEventArg msg)
        {
            EventHandler<SpeechEventArg> handler = Recognized;
            if (handler != null)
            {
                handler(this, msg);
            }
        }

        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        //  NEW 16 april
        private static Tts tts = new Tts(sre);
        private MmiCommunication mmiReceiver;
    

        public SpeechMod()
        {
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("ASR", "FUSION", "speech-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            
            mmic = new MmiCommunication("localhost",9876,"User1", "ASR");  //PORT TO FUSION - uncomment this line to work with fusion later
            
            
            //mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic.Send(lce.NewContextRequest());

            //load pt recognizer
            //sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
            gr = new Grammar(Environment.CurrentDirectory + "\\ptG.grxml", "rootRule");

            sre.LoadGrammar(gr);

            sre.SetInputToDefaultAudioDevice();
            sre.RecognizeAsync(RecognizeMode.Multiple);
            sre.SpeechRecognized += Sre_SpeechRecognized;
            sre.SpeechHypothesized += Sre_SpeechHypothesized;


            // NEW - TTS support 16 April
            tts.Speak("Olá sou o Cone, podes iniciar a playlist");


            //  o TTS  no final indica que se recebe mensagens enviadas para TTS
            mmiReceiver = new MmiCommunication("localhost",8000, "User1", "TTS");
            mmiReceiver.Message += MmiReceived_Message;
            mmiReceiver.Start();
        }



        private void Sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Console.WriteLine(e.Result.Text);
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = false });
        }

        //
        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine(e.Result.Text);
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true });

           bool flag_true = false;

            //SEND
            // IMPORTANT TO KEEP THE FORMAT {"recognized":["SHAPE","COLOR"]}
            string json = "{ \"recognized\": [";
            foreach (var resultSemantic in e.Result.Semantics)
            {
                if (resultSemantic.Value.Count == 0)
                {
                    Console.WriteLine(" -> " + resultSemantic.Key + " , " + resultSemantic.Value.Value + "<-");
                    if (question && resultSemantic.Value.Value.ToString() == "YES") { flag_true = true; }
                    //json += "\"" + resultSemantic.Key + "\", " + "\"" + resultSemantic.Value.Value + "\",\n ";
                    json += "\"" + resultSemantic.Value.Value + "\",\n ";
                }
                else
                {

                    foreach (var key in resultSemantic.Value)
                    {
                        if (question && key.Value.Value.ToString() == "YES") { flag_true = true; }
                        Console.WriteLine(" -> " + key.Key + " , " + key.Value.Value + "<-" );
                        //json += "\"" + key.Key + "\", " + "\"" + key.Value.Value + "\",\n ";
                        json +=  "\"" + key.Value.Value + "\",\n ";

                    }

                }
                
                //json += "\"" + resultSemantic.Value.Value + "\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";
            Console.WriteLine(json);
            var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime + "", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration) + "", e.Result.Confidence, json);
            

            if (question && flag_true && e.Result.Confidence >= 0.55)
            {
                Console.Write("ENTROU NA RESPOSTA DA QUESTAO");
                Console.Write(json);
                question = false;
                mmic.Send(last_json);
                last_json = null;
            }
            else if (question && !flag_true && e.Result.Confidence >= 0.55) {
                    question = false;
                    last_json = null;
            }
            else
            {
                if (e.Result.Confidence >= 0.45 && e.Result.Confidence < 0.55) 
                {
                    question = true;
                    tts.Speak("Quis dizer " + e.Result.Text);
                    last_json = exNot;

                }
                else if (e.Result.Confidence >= 0.55)
                    mmic.Send(exNot);

            }
        }
      
        private bool check()
        {
            return true;
        }

        //  NEW 16 April 2020   - create receiver, answer to messages received
        //  Adapted from AppGUI code


        //MmiReceived_Message;

        private void MmiReceived_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);

            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;

            Console.WriteLine(com);

            tts.Speak(com);
        }
    }
}
