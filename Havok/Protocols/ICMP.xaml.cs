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
using HavokNet.Stack;
using HavokNet.Common;
using HavokNet.Protocols.ICMP;

namespace Havok.Protocols
{
    /// <summary>
    /// Interaction logic for ICMP.xaml
    /// </summary>
    public partial class ICMP : ProtocolBase
    {
        public ICMP(NetClient client) : base(client)
        {
            InitializeComponent();
        }

        private void pingBtn_Click(object sender, RoutedEventArgs e)
        {
            echoReplies.Items.Clear();

            byte ttl = 0;
            if (!byte.TryParse(pingTtlTxt.Text, out ttl))
            {
                MessageBox.Show("\"" + pingTtlTxt.Text + "\" is not a valid Ttl (Time-to-Live).");
                return;
            }

            ushort bytes = 0;
            if (!ushort.TryParse(pingBytesTxt.Text, out bytes))
            {
                MessageBox.Show("\"" + pingBytesTxt.Text + "\" is not a valid Bytes value.");
                return;
            }

            int repeat = 0;
            if (!int.TryParse(pingRepeatTxt.Text, out repeat))
            {
                MessageBox.Show("\"" + pingRepeatTxt.Text + "\" is not a valid repeat value.");
                return;
            }

            int timeout = 0;
            if (!int.TryParse(pingTimeoutTxt.Text, out timeout))
            {
                MessageBox.Show("\"" + pingTimeoutTxt.Text + "\" is not a valid timeout value.");
                return;
            }

            int delay = 0;
            if (!int.TryParse(pingDelayTxt.Text, out delay))
            {
                MessageBox.Show("\"" + pingDelayTxt.Text + "\" is not a valid delay value.");
                return;
            }

            Client.Icmp.CurrentPings.Clear();
            echoReplies.Items.Clear();

            string val = pingDestinationTxt.Text;
            try
            {
                IPv4Address ip = new IPv4Address(val);
                Task.Run(() =>
                    {
                        Client.Icmp.Ping(ip, PingSettings.FromConfig(ttl, bytes, timeout, repeat, delay), (result) =>
                            {
                                AddPingResult(result);
                            });
                    });
            }
            catch
            {
                Task.Run(() =>
                {
                    Client.Icmp.Ping(val, PingSettings.FromConfig(ttl, bytes, timeout, repeat, delay), (result) =>
                    {
                        AddPingResult(result);
                    });
                });
            }
        }

        private void AddPingResult(HavokNet.Protocols.ICMP.PingResult result)
        {
            this.Dispatcher.Invoke(() =>
                {
                    echoReplies.Items.Add(result.ToString());
                });
        }
    }
}
