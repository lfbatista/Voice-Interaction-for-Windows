using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;


using Microsoft.Speech.Recognition;

namespace testereco
{
    public partial class Form1 : Form
    {
        SpeechRecognitionEngine sr;
        public Form1()
        {
            InitializeComponent();
           
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {       
            sr = new SpeechRecognitionEngine(new CultureInfo("pt-PT"));
            sr.SetInputToDefaultAudioDevice();
            
            Grammar gr = new Grammar("palavras.xml");
            sr.LoadGrammar(gr);

            // Register a handler for the SpeechRecognized event.
            sr.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(voz_reconhecida);
            sr.SpeechHypothesized += delegate(object sender2, SpeechHypothesizedEventArgs eventArgs)
            {
                listBox2.Items.Insert(0, eventArgs.Result.Text);
            };

            sr.RecognizeAsync(RecognizeMode.Multiple);
           
        }

        void voz_reconhecida(object sender, SpeechRecognizedEventArgs e)
        {
            listBox1.Items.Insert(0,e.Result.Text+"  «Confiança="+e.Result.Confidence+"»");
           
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
