using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SVC
{
    public partial class Dialog : Form
    {
        public static bool cls;
        public static int Result;
        private static UInt16 sekundy = 20;
        private static System.Windows.Forms.Timer Timer1;

        private System.Media.SoundPlayer player = new System.Media.SoundPlayer(SVC.Properties.Resources.Phone_ring);

        public void Waiting_Time() // time out po ktorym pojawia sie komunikat o nie odebranym polaczeniu 
        {
            Timer1 = new System.Windows.Forms.Timer();
            Timer1.Tick += new EventHandler(Timer1_tick);
            Timer1.Interval = 1000;
            Timer1.Start();
            sekundy = 20;
        }

        private void Timer1_tick(object sender, EventArgs e)
        {
            if (sekundy <= 0)
            {
                Result = 0;
                Timer1.Stop();
                player.Stop();
                MessageBox.Show("Nie odebrano");
                this.Dispose();
            }
            else
            {
                sekundy--;
            }
        }
        

        public Dialog()
        {
            InitializeComponent();

            this.ShowInTaskbar = false;
            this.BringToFront();

            cls = false;
            Result = 0;
            label1.Text = Form1.Client;

            Waiting_Time();

            player.PlayLooping();
        }

        private void button1_Click(object sender, EventArgs e) // Akceptuj
        {
            Timer1.Stop();
            player.Stop();
            Result = 1;
            this.Dispose();
        }

        private void button2_Click(object sender, EventArgs e) // Odrzuc
        {
            Timer1.Stop();
            player.Stop();
            Result = 2;
            this.Dispose();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
