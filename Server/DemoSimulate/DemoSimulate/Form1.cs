using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Protocol;
using ProtoBuf;
using System.IO;

namespace DemoSimulate
{
    public partial class Form1 : Form
    {
        private ClientSocket client = null;

        public Form1()
        {
            client = new ClientSocket();
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client.ConnectServer(AdressLabel.Text,13000);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CardInfo info = new CardInfo();
            info.num = 10;
            info.weight = 10;
            info.type = CardType.HeiTao;
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize<CardInfo>(ms, info);
            byte[] buf = ms.ToArray();
            client.SendMessage(101,Encoding.Default.GetString(buf));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            client.DisConnect();
        }
    }
}
