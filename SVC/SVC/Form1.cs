using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SVC
{
    public partial class Form1 : Form
    {

        public static bool Busy;
        public static bool Talk_done;
        public static bool Decline;
        public static bool Starting;
        public static string Client = null;
        public static TcpListener listener;
        public static TcpClient Client_tcp;
        public static NetworkStream network_stream;
        private static Socket Socket_tcp;
        private static UInt16 sekundy = 20;
        public static System.Windows.Forms.Timer Timer1;
        private static bool End_Count;

        public static DarrenLee.LiveStream.Audio.Receiver receiver = new DarrenLee.LiveStream.Audio.Receiver();
        public static Thread Rec;

        WaveOut waveOut = new WaveOut();
        public static int Volume;

        public static DialogResult result;

        public static Thread res;

        public static System.Media.SoundPlayer player = new System.Media.SoundPlayer(SVC.Properties.Resources.Caller_ring);
        

        public static Label label_4;

        public static Form1 _instance;

        public static string Client_con;

        // pobieranie IP z wybranego adresu sieciowego (wyswietla sie jako Twoje IP)
        public static string GetLocalIPAddress()
        {
            try
            {
                IPAddress ipa = Interfejs.network_interface.GetIPProperties().UnicastAddresses[1].Address;

                return ipa.ToString();

            } catch (Exception e) { return "0.0.0.0"; }
        }


        public static void Listening()
        {
            while (true)
            {
                //Przyjmij połączenie od klienta TCP
                Client_tcp = listener.AcceptTcpClient();

                string data = null;
                string prompt = null;

                network_stream = Client_tcp.GetStream();
                byte[] buffer = new byte[Client_tcp.ReceiveBufferSize];

                int bytes_Read = network_stream.Read(buffer, 0, Client_tcp.ReceiveBufferSize);

                data = Encoding.ASCII.GetString(buffer, 0, bytes_Read);

                // doczytaj komunikat
                prompt = data.Substring(0, 3);

                // odczytaj adres IP
                string address = data.Substring(3, data.Length - 3);
                
                Client = address;

                if (prompt == "CAL") // komunikat rozpoczecia polaczenia
                {
                    if (Application.OpenForms["Rozmowa"] != null) // sprawdzenie zajetosci linii
                    {
                        Transmit("BUS"); // jesli uzytkownik jest zajety -> wyslij komunikat BUS
                    }
                    else { // jesli nie, powiadom o polaczeniu przychodzacym
                        Starting = true;
                        
                        Dialog incoming = new Dialog();
                        incoming.ShowDialog();

                        // akceptowanie polaczenia przychodzacego
                        if (Dialog.Result == 1)
                        {
                            End_Count = true;

                            Transmit("ACC");  // wyslanie komunikatu akceptacji polaczenia

                            Client_con = address;
                            Busy = true;      // podniesienie flagi zajetosci linii
                            _instance.Hide();

                            Rec = new Thread(odbior);   // rozpoczecie odbierania danych audio
                            Rec.Start();
                            
                            Form f = new Rozmowa();     // otwarcie nowego okna rozmowy
                            f.ShowDialog();
                            f.BringToFront();
                        }

                        if (Dialog.Result == 2) // odrzucenie polaczenia
                        {
                            Busy = false;
                            Transmit("DEC");
                        }

                        if (Dialog.Result == 0) // zignorowanie, uplyniecie time out'u
                        {
                            Busy = false;
                            player.Stop();
                        }
                    }


                    

                }

                if (prompt == "ACC" && Starting) // akceptowanie polaczenia
                {
                    Timer1.Stop();

                    Starting = true;
                    label_4.Visible = false;
                    player.Stop();
                    End_Count = true;
                    Busy = true;

                    Client_con = address;
                    _instance.Hide(); 

                    Rec = new Thread(odbior);  // rozpoczecie odbierania danych audio w nowym watku
                    Rec.Start();

                    Form f = new Rozmowa(); // pojawia sie nowe okno, aktualne jest ukrywane
                    Application.Run(f);
                    if (Application.OpenForms["Rozmowa"] != null)
                    {
                        f.ShowDialog();
                        f.BringToFront();
                    }

                }

                if (prompt == "DEC" && Starting) // odrzucanie polaczenia
                {
                    Timer1.Stop();

                    IntPtr window = FindWindow(null, "Polaczenie przychodzace");
                    if (window != IntPtr.Zero)
                    {
                        SendMessage((int)window, (uint)WM_SYSCOMMAND, SC_CLOSE, 0);
                    }
                    Busy = false;
                    Starting = false;
                    Decline = true;
                    label_4.Visible = false;
                    player.Stop();
                    End_Count = true;
                    MessageBox.Show("Odrzucono");
                }

                if (prompt == "END" && Starting) // konczenie polaczenia
                {
                    Starting = false;

                    Rozmowa.Nadawanie(false); // zatrzymanie wysylanie pakietow UDP z dzwiekiem

                    Rozmowa.timer1.Stop(); // zatrzymanie liczenia czasu polaczenia

                    Talk_done = true;

                    MessageBox.Show("Zakonczono");

                    Rozmowa.Hide_window();

                    Busy = false;


                    _instance.Show();
                }

                if (prompt == "BUS" && Starting) // odebranie komunikatu o zajetoscii linii odbiorcy
                {
                    player.Stop();
                    
                    End_Count = true;
                    Decline = true;
                    label_4.Visible = false;
                    MessageBox.Show("Użytkownik jest zajęty");
                }



                }



        }


        public static void OpenDialog() // metoda otwarcia okna dialogowego polaczenia przychodzacego
        {
            Dialog incoming = new Dialog();
            incoming.ShowDialog();
        }

        public static void Transmit(string data) // wysylanie komunikatow TCP
        {
            IPEndPoint remoteEP;

            if (data == "END") // sa dwa typy klientow
                remoteEP = new IPEndPoint(IPAddress.Parse(Client_con), 13200); // klient polaczony -> z nim trwa aktualne polaczenie
            else
                remoteEP = new IPEndPoint(IPAddress.Parse(Client), 13200); // klient probujacy nawiazac polaczenie, ktore jest odrzucane

            data += GetLocalIPAddress();

            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);

            Socket_tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            

            try {
                Socket_tcp.Connect(remoteEP);
                Socket_tcp.Send(buffer);
            } catch (Exception e) { Console.WriteLine("Error!"); }
                     
            
        }

        public static void Init_listener()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 13200);
                listener.Start();
            }
            catch (Exception e) { }
        }

        public static void End_Converstation()
        {
            Transmit("END");
        }


        public static void odbior() // odbieranie danych audio przez UDP
        {
            DarrenLee.LiveStream.Audio.Receiver receiver = new DarrenLee.LiveStream.Audio.Receiver();
            receiver.Receive(System.Net.IPAddress.Any.ToString(), 13000);
        }

        public static void Waiting_Time() // obsluga timer'a
        {
            player.PlayLooping();
            Timer1 = new System.Windows.Forms.Timer();
            Timer1.Tick += new EventHandler(Timer1_tick);
            Timer1.Interval = 1000;
            Timer1.Start();
            sekundy = 20;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;

        private static void Timer1_tick(object sender, EventArgs e)
        {
            if (sekundy <= 0 && !End_Count && !Busy && !Decline)
            {
                label_4.Visible = false;
                player.Stop();
                Timer1.Stop();
                Busy = false;
                End_Count = true;

                Dialog.Result = 0;
                

                IntPtr window = FindWindow(null, "Polaczenie przychodzace");
                if (window != IntPtr.Zero)
                {
                    SendMessage((int)window, (uint)WM_SYSCOMMAND, SC_CLOSE, 0);
                }


                sekundy = 20;
                MessageBox.Show("Użytkownik nie odpowiada");
            }
            else
            {
                End_Count = false;
                sekundy--;
            }
                


        }
        

        public Form1()
        {
            InitializeComponent();

            this.Size = new Size(450, 260);

            #region
            this.Hide();
            Form i = new Interfejs();
            i.ShowDialog();
            i.BringToFront();
            #endregion



            label4.Visible = false;
            label_4 = label4;
            
            label1.Text += GetLocalIPAddress();
            trackBar1.Value = 7;
            Volume = trackBar1.Value;

            Talk_done = false;

            End_Count = false;
            Busy = false;
            Init_listener();

            _instance = this;

            Thread T = new Thread(Listening);
            T.Start();
            T.IsBackground = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Client = textBox1.Text;
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Jesli host jest osiagalny w sieci
            if (scan(Client))
            {
                Starting = true;
                label_4.Visible = true;
                Transmit("CAL");
                Decline = false;
                Waiting_Time();
            } // jesli host jest niedostepny
            else
            {
                MessageBox.Show("Adres jest nieosiągalny");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();

            try {
                Process [] proc = Process.GetProcessesByName("SVC");
                proc[0].Kill();
            } catch (Exception exc) { };
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            waveOut.Volume = (float)trackBar1.Value / (float)10;
            Volume = trackBar1.Value;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) // checkbox od wyciszenia mikrofonu
        {
            if (checkBox1.Checked)
            {
                if (Application.OpenForms["Rozmowa"] != null)
                    Rozmowa.Nadawanie(true);
            }
            else
            {
                if (Application.OpenForms["Rozmowa"] != null)
                    Rozmowa.Nadawanie(false);
            }
        }

        private void button3_Click(object sender, EventArgs e) // odswiezanie IP
        {
            label1.Text = string.Empty;
            label1.Text = "Twoje IP:" + GetLocalIPAddress();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        // scan IP
        public bool scan(string subnet)
        {
            Ping myPing;
            PingReply reply;

            try {
                myPing = new Ping();
                reply = myPing.Send(Client);

                if (reply.Status == IPStatus.Success)
                    return true;
                else
                    return false;
            } catch (Exception e) { }


            return true;

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
