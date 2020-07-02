using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SVC
{
    public partial class Interfejs : Form
    {
        public static NetworkInterface network_interface;
        
        public Interfejs()
        {
            InitializeComponent();

            Get_interfaces();
        }

        public void Get_interfaces() // pobieranie aktywnych interfejsow sieciowych z urzadzenia
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                   ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    comboBox1.Items.Add(ni.Name);
                }
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e) // wybieranie interfejsu z listy
        {
            if (comboBox1.SelectedItem != null)
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                       ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        if (ni.Name == comboBox1.SelectedItem.ToString())
                        {
                            network_interface = ni;
                            break;
                        }

                    }
                }

                this.Close();
            }
            else {
                MessageBox.Show("Nie wybrano interfejsu sieciowego");
            }
        }

        private void button2_Click(object sender, EventArgs e) // zamykanie calej aplikacji
        {
            System.Windows.Forms.Application.Exit();

            try
            {
                Process[] proc = Process.GetProcessesByName("SVC");
                proc[0].Kill();
            }
            catch (Exception exc) { };
        }
    }
}
