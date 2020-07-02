using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SVC
{
    public partial class Rozmowa : Form
    {

        public static  Thread k, n, F1;
        public static DarrenLee.LiveStream.Audio.Sender sender = new DarrenLee.LiveStream.Audio.Sender();


        private UInt32 sekundy = 0;

        public static System.Windows.Forms.Timer timer1;
        private DateTime dt = new DateTime();

        private static Rozmowa _instance;

        WaveOut waveOut = new WaveOut();
        public static int Volume;

        private void time_count()
        {
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (sekundy % 10 == 0) // aplikacja podczas trwania polaczenia sprawdza stan odbiorcy co 10 sekund 
                if (!Available(Form1.Client))
                {
                    timer1.Stop();
                    Nadawanie(false);
                    Form1.End_Converstation();
                    
                    MessageBox.Show("Utracono polaczenie");
                    
                    Application.OpenForms["Form1"].Show();

                    this.Close();
                    Hide_window();
                }


            sekundy++;
            label2.Text = "Czas: " + dt.AddSeconds(sekundy).ToString("HH:mm:ss");
        }

        public static void Nadawanie(bool tn)
        {
            if (tn)
                sender.Send(Form1.Client, 13000);
            else
            {
                sender.Disconnect();
            }
            
        }

        public bool Available(string subnet) // metoda od sprawdzania stanu odbiorcy oparta o sprawdzanie pingu
        {
            Ping myPing;
            PingReply reply;

            try
            {
                myPing = new Ping();
                reply = myPing.Send(Form1.Client);

                if (reply.Status == IPStatus.Success)
                    return true;
                else
                    return false;
            }
            catch (Exception e) { Console.WriteLine("Disconnected"); }


            return true;
        }


        public Rozmowa()
        {
            InitializeComponent();

            this.Size = new Size(450, 260);

            _instance = this;

            this.ShowInTaskbar = false;

           // Form1.Timer1.Stop();

            trackBar1.Value = Form1.Volume;

            label1.Text += Form1.Client;

            time_count();

            Nadawanie(true);

            F1 = new Thread(Form1.Listening);
            F1.Start();
            
        } 
        

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e) // zakonczenie polaczenia
        {
            timer1.Stop();
            Nadawanie(false);
            Form1.End_Converstation();
            MessageBox.Show("Koniec Rozmowy");

            Application.OpenForms["Form1"].Show();

            this.Close();
            Hide_window();
        }

        public static void Close_connection()
        {
            Nadawanie(false);
            Form1.End_Converstation();
        }

        private void trackBar1_Scroll(object sender, EventArgs e) // regulacja glosnosci 
        {
            waveOut.Volume = (float)trackBar1.Value / (float)10;
            Volume = trackBar1.Value;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                    Rozmowa.Nadawanie(false);
            }
            else
            {
                    Rozmowa.Nadawanie(true);
            }
        }

        public static void Hide_window()
        {
            _instance.Invoke((MethodInvoker)delegate
           {
               _instance.Close();
           });
            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
