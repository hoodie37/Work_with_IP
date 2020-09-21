using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.NetworkInformation;

namespace Работа_с_IP
{
    public partial class Form1 : Form
    {
       
        public bool Connect(IPAddress ip)
        {
            Ping Pinger = new Ping();                 //вот это вот утилита ping которую он скинул на почту
            PingReply pingrep = Pinger.Send(ip, 100);

            if (pingrep.Status == IPStatus.Success) //если мы можем достучаться до этого адреса то true, иначе false
                return true;
            else return false;


        }



        public List<IPAddress> GetList(IPAddress beginIP, IPAddress endIP)
        {
            List<IPAddress> list = new List<IPAddress>(); // создаем объект лист класса List<IPAddress>
            var beginIParray = beginIP.GetAddressBytes(); // массив байтов
            var endIParray = endIP.GetAddressBytes();
            Array.Reverse(beginIParray); //изменяем порядок жлементов на обратный
            Array.Reverse(endIParray);
            var beginIPint = BitConverter.ToInt32(beginIParray, 0); //преобразуем в число
            var endIPint = BitConverter.ToInt32(endIParray, 0);
            for (; beginIPint <= endIPint; beginIPint++)
            {
                var IParray = BitConverter.GetBytes(beginIPint); //возвращает число как массив байтов
                Array.Reverse(IParray); //меняем обратно
                list.Add(new IPAddress(IParray)); //заносим в лист
            };
            return list;
        }


        void GetMac()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            string mac = "";
            foreach (NetworkInterface adapter in nics)
            {
                PhysicalAddress address = adapter.GetPhysicalAddress(); //этот кусок кода я нагло взял из интернета, тут надо знать команду ГетФизикалАдрес
                byte[] bytes = address.GetAddressBytes();
                for (int k = 0; k < bytes.Length; k++)
                {
                    mac += bytes[k].ToString("X2");
                    if (k != bytes.Length - 1)
                    {
                        mac += "-";
                    }
                }
                listBox.Items.Add(mac);
                mac = "";

            }

        }


        void info(IPAddress beginIP, IPAddress endIP)
        {
            var beginIParray = beginIP.GetAddressBytes(); // массив байтов
            var endIParray = endIP.GetAddressBytes();
            byte[] maskarray = new byte[4];             //новые массивы
            byte[] ipadressarray = new byte[4];
            byte[] broadcastiparray = new byte[4];

            bool end1 = false;
            for (int i = 0; i < 4; i++)
            {
                for (byte bit = 128; bit >= 1; bit /= 2)
                {
                    if (!end1 && (beginIParray[i] & bit) == (endIParray[i] & bit)) //если end1 стоит в позиции ложь и биты начала и конца совпадают, то
                    {
                        maskarray[i] |= bit; //бит уходит в маску
                    }
                    else
                    {
                        end1 = true;
                        maskarray[i] = (byte)(maskarray[i] & ~bit); //иначе он меняется
                    }
                }
                ipadressarray[i] = (byte)(maskarray[i] & beginIParray[i]);      //побитовая конъюнкция маски и первого айпишника(хотя можно брать любой айпишник из диапазона
                broadcastiparray[i] = (byte)(~maskarray[i] | ipadressarray[i]); //побитовая дизъюнкция инвертированной маски и айпишника сети
            }


            IPAddress ipAddress = new IPAddress(ipadressarray); //записываем всё в текст боксы
            textBIP.Text = ipAddress.ToString();

            IPAddress mask = new IPAddress(maskarray);
            textBMask.Text = mask.ToString();

            IPAddress broadcast = new IPAddress(broadcastiparray);
            textBrIP.Text = broadcast.ToString();
        }




        public Form1()
        {
            InitializeComponent();
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            string IP1 = tb1.Text;  //Берем IPшники из текстбоксов
            string IP2 = tb2.Text;
            IPAddress beginIP = IPAddress.Parse(IP1); //парсим айпишники в класс IPAddress
            IPAddress endIP = IPAddress.Parse(IP2);

            List<IPAddress> list = GetList(beginIP, endIP);
            info(beginIP, endIP);
            GetMac();

            foreach (IPAddress ip in list)
            {

                Connect(ip);
                ListViewItem lvi = new ListViewItem();
                lvi.Text = ip.ToString();
                string host = Dns.GetHostEntry(ip).HostName.ToString(); //днс этого айпишника
                lvi.SubItems.Add(host);
                if (Connect(ip) == true)
                    lvi.SubItems.Add("true");
                else lvi.SubItems.Add("false");
                listView1.Items.Add(lvi);
            }



        }
    }
}
