using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using Microsoft.Speech.Recognition;
using System.Runtime.InteropServices;

namespace testereco
{
    public partial class Form1 : Form
    {
        Timer timer = new Timer();
        int timerInt = 20;

        DateTime dateTime;

        // Eventos do rato
        private const int SCROLL = 0x800;
        private const int CLICK_DIREITO_BAIXO = 0x08;
        private const int CLICK_DIREITO_CIMA = 0x10;
        private const int CLICK_ESQUERDO_BAIXO = 0x02;
        private const int CLICK_ESQUERDO_CIMA = 0x04;

        SpeechRecognitionEngine sr;
        string ProcWindow;
        int count = 1;

        int scrollControl = 0, zoomControl = 0;
        double scroll, scrollOld, scrollAtual;
        double coefScroll = .1;

        int rato = 0;
        double moveX, moveY, cursorX, cursorY;
        double coefRato = 0.05;

        int altura, largura;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int cButtons, uint dwExtraInfo);

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

            sr.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(voz_reconhecida);

            sr.RecognizeAsync(RecognizeMode.Multiple);

            altura = Screen.FromControl(this).Bounds.Height;
            largura = Screen.FromControl(this).Bounds.Width;

            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = timerInt;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (rato == 1)
            {
                cursorX += moveX;
                if (cursorX > largura) cursorX = largura;
                if (cursorX < 0) cursorX = 0;

                cursorY += moveY;
                if (cursorY > altura) cursorY = altura;
                if (cursorY < 0) cursorY = 0;

                Cursor.Position = new Point((int)cursorX, (int)cursorY);
                TimeSpan timeSpent = (DateTime.Now - dateTime);
            }

            if (scrollControl == 1 || zoomControl == 1)
            {
                int scrollTm;
                uint X = (uint)Cursor.Position.X;
                uint Y = (uint)Cursor.Position.Y;
                scrollAtual += scroll;

                if (Math.Abs(scrollAtual) < 1)
                {
                    scrollTm = 0;
                }
                else
                {
                    scrollTm = (int)scrollAtual;
                    scrollAtual -= (int)scrollAtual;
                }
                mouse_event(SCROLL, X, Y, scrollTm, 0);
            }
        }

        void voz_reconhecida(object sender, SpeechRecognizedEventArgs e)
        {
            listBox1.Items.Insert(0, e.Result.Text + "  (confiança= " + e.Result.Confidence + ")");
            if (e.Result.Confidence > 0.8)
            {
                switch (e.Result.Text)
                {

                    // Cenários do rato
                    //Cliques
                    case "Clicar":
                        uint X = (uint)Cursor.Position.X;
                        uint Y = (uint)Cursor.Position.Y;
                        mouse_event(CLICK_ESQUERDO_BAIXO | CLICK_ESQUERDO_CIMA, X, Y, 0, 0);
                        dateTime = DateTime.Now;
                        break;

                    case "Clicar direito":
                        uint XX = (uint)Cursor.Position.X;
                        uint YY = (uint)Cursor.Position.Y;
                        mouse_event(CLICK_DIREITO_BAIXO | CLICK_DIREITO_CIMA, XX, YY, 0, 0);
                        dateTime = DateTime.Now;
                        break;

                    // Navegação
                    case "Ativar rato":
                        rato = 1;
                        scrollControl = 0;
                        zoomControl = 0;
                        moveX = moveY = 0;

                        cursorX = Cursor.Position.X;
                        cursorY = Cursor.Position.Y;
                        break;

                    case "Direita":
                        if (rato == 1)
                        {
                            if (moveX == 0 && moveY == 0)
                            {
                                moveX = largura * timerInt * altura / 1000;
                            }
                            else if (moveX == 0 && moveY != 0)
                            {
                                moveX = Math.Abs(moveY);
                            }
                            else if (moveX < 0)
                            {
                                moveX *= -1;
                            }
                            else if (moveX > 0 && moveY != 0)
                            {
                                moveY *= 0.5;
                            }
                            dateTime = DateTime.Now;
                        }
                        break;

                    case "Esquerda":
                        if (rato == 1)
                        {
                            if (moveX == 0 && moveY == 0)
                            {
                                moveX = (largura * timerInt * coefRato / 1000) * -1;
                            }
                            else if (moveX == 0 && moveY != 0)
                            {
                                moveX = Math.Abs(moveY) * -1;
                            }
                            else if (moveX > 0)
                            {
                                moveX *= -1;
                            }
                            else if (moveX < 0 && moveY != 0)
                            {
                                moveY *= 0.5;
                            }
                        }

                        dateTime = DateTime.Now;
                        break;

                    case "Cima":
                        if (rato == 1)
                        {
                            if (moveY == 0 && moveX == 0)
                            {
                                moveY = altura * timerInt * coefRato / 1000 * -1;
                            }
                            else if (moveY == 0 && moveX != 0)
                            {
                                moveY = Math.Abs(moveX) * -1;
                            }
                            else if (moveY > 0)
                            {
                                moveY *= -1;
                            }
                            else if (moveY < 0 && moveX != 0)
                            {
                                moveX *= 0.5;
                            }
                        }
                        if (scrollControl == 1)
                        {
                            if (scroll == 0 && scrollOld == 0)
                            {
                                scroll = altura * timerInt * coefScroll / 1000;
                            }
                            else if (scroll == 0 && scrollOld != 0)
                            {
                                scroll = Math.Abs(scrollOld);
                            }
                            else
                            {
                                scroll = Math.Abs(scroll);
                            }
                        }

                        dateTime = DateTime.Now;
                        break;

                    case "Baixo":
                        if (rato == 1)
                        {
                            if (moveY == 0 && moveX == 0)
                            {
                                moveY = altura * timerInt * coefRato / 1000;
                            }
                            else if (moveY == 0 && moveX != 0)
                            {
                                moveY = Math.Abs(moveX);
                            }
                            else if (moveY < 0)
                            {
                                moveY *= -1;
                            }
                            else if (moveY > 0 && moveX != 0)
                            {
                                moveX *= 0.5;
                            }
                        }
                        if (scrollControl == 1)
                        {
                            if (scroll == 0 && scrollOld == 0)
                            {
                                scroll = altura * timerInt * coefScroll / 1000 * -1;
                            }
                            else if (scroll == 0 && scrollOld != 0)
                            {
                                scroll = Math.Abs(scrollOld) * -1;
                            }
                            else
                            {
                                scroll = Math.Abs(scroll) * -1;
                            }
                        }

                        dateTime = DateTime.Now;

                        break;

                    case "Rápido":
                        if (rato == 1)
                        {
                            moveX *= 3;
                            moveY *= 3;
                        }
                        if (scrollControl == 1) scroll *= 3;

                        dateTime = DateTime.Now;
                        break;

                    case "Devagar":
                        if (rato == 1)
                        {
                            moveX /= 3;
                            moveY /= 3;
                        }
                        if (scrollControl == 1) scroll /= 3;

                        dateTime = DateTime.Now;


                        break;

                    case "Parar":
                        if (rato == 1)
                        {
                            cursorX -= .7 * moveX * 1000 / timerInt;
                            cursorY -= .7 * moveY * 1000 / timerInt;

                            moveX = moveY = 0;
                            dateTime = DateTime.Now;
                        }

                        if (scrollControl == 1 || zoomControl == 1)
                        {
                            scrollOld = scroll;
                            scroll = 0;
                        }
                        break;

                    case "Ativar scrol":
                        rato = 0;
                        scrollControl = 1;
                        zoomControl = 0;
                        scroll = 0;
                        scrollOld = 0;
                        break;

                    // Manipulação de processos
                    case "Abrir sapo":
                        System.Diagnostics.Process.Start(@"www.sapo.pt");
                        break;

                    case "Fechar crome":
                        ProcWindow = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                        Close();
                        break;

                    case "Fechar programa":
                        Close();
                        break;

                    case "Mudar programa":
                        SendKeys.Send("%{TAB " + count + "}");
                        count += 1;
                        break;

                    case "Minimiza":
                        if (WindowState == FormWindowState.Normal)
                        {
                            WindowState = FormWindowState.Minimized;
                        }
                        break;

                    case "Aparece":
                        if (WindowState == FormWindowState.Minimized)
                        {
                            WindowState = FormWindowState.Normal;
                        }
                        break;

                    case "Abrir editor de texto":
                        System.Diagnostics.Process.Start(@"C:\Program Files\Microsoft Office\Office14\Winword.exe");
                        break;
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void StopWindow()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName(ProcWindow);
            foreach (System.Diagnostics.Process proc in procs)
            {
                proc.CloseMainWindow();
            }
        }
    }
}