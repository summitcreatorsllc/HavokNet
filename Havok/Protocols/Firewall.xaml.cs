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
using System.Timers;
using HavokNet.Stack;
using HavokNet.Common;

namespace Havok.Protocols
{
    /// <summary>
    /// Interaction logic for Firewall.xaml
    /// </summary>
    public partial class Firewall : ProtocolBase
    {
        public Firewall(HavokNet.Stack.NetClient client) : base(client)
        {
            InitializeComponent();
            firewallTypeTxt.Text = client.NetworkFirewall.GetType().Name;
            DataContext = Client.NetworkFirewall;
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = 3000;
            _timer.Start();
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdatePacketDetails();
        }

        private void UpdatePacketDetails()
        {
            List<PacketType> allTypes = Client.NetworkFirewall.PassedRx.Keys.Concat(Client.NetworkFirewall.PassedTx.Keys.Concat(Client.NetworkFirewall.DroppedRx.Keys.Concat(Client.NetworkFirewall.DroppedTx.Keys))).Distinct().ToList();
            List<FirewallProtocolReport> reports = new List<FirewallProtocolReport>();
            foreach (PacketType type in allTypes)
            {
                FirewallProtocolReport report = new FirewallProtocolReport();
                report.Protocol = type;
                report.DroppedRx = Client.NetworkFirewall.DroppedRx.ContainsKey(type) ? Client.NetworkFirewall.DroppedRx[type] : 0;
                report.DroppedTx = Client.NetworkFirewall.DroppedTx.ContainsKey(type) ? Client.NetworkFirewall.DroppedTx[type] : 0;
                report.PassedRx = Client.NetworkFirewall.PassedRx.ContainsKey(type) ? Client.NetworkFirewall.PassedRx[type] : 0;
                report.PassedTx = Client.NetworkFirewall.PassedTx.ContainsKey(type) ? Client.NetworkFirewall.PassedTx[type] : 0;
                reports.Add(report);
            }

            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    droppedPacketsRxTxt.Text = Client.NetworkFirewall.DroppedRxPacketCount.ToString();
                    droppedPacketsTxTxt.Text = Client.NetworkFirewall.DroppedTxPacketCount.ToString();
                    totalDropped.Text = Client.NetworkFirewall.TotalDroppedPackets.ToString();

                    packetBreakdown.ItemsSource = reports;
                });
            }
            catch { }
        }

        private void ProtocolBase_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePacketDetails();
        }

        public Timer _timer;

        private void changeFirewallBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
