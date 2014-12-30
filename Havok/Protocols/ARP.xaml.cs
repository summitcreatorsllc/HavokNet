using System;
using System.Collections.ObjectModel;
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
using HavokNet.OSI;
using HavokNet.Common;

namespace Havok.Protocols
{
    /// <summary>
    /// Interaction logic for ARP.xaml
    /// </summary>
    public partial class ARP : ProtocolBase
    {
        public ARP(NetClient client) : base(client)
        {
            InitializeComponent();
            DataContext = Client.Arp;
            ArpCache = new ObservableCollection<HavokNet.Protocols.ARP.ArpCacheEntry>();
            Client.Arp.ArpReplyReceived += Arp_ArpReplyReceived;
            Client.Arp.ArpCacheChanged += Arp_ArpCacheChanged;
            BindingOperations.EnableCollectionSynchronization(ArpCache, _syncLock);

            arpCache.ItemsSource = ArpCache;
        }

        public bool _syncLock;

        public ObservableCollection<HavokNet.Protocols.ARP.ArpCacheEntry> ArpCache { get; set; }
        void Arp_ArpCacheChanged(List<HavokNet.Protocols.ARP.ArpCacheEntry> cache)
        {
            foreach (HavokNet.Protocols.ARP.ArpCacheEntry entry in cache)
            {
                if (!ArpCache.Contains(entry))
                {
                    ArpCache.Add(entry);
                }
            }

            foreach (HavokNet.Protocols.ARP.ArpCacheEntry entry in ArpCache)
            {
                if (!cache.Contains(entry))
                {
                    ArpCache.Remove(entry);
                }
            }
        }

        

        void Arp_ArpReplyReceived(IPv4Address ip, MacAddress mac)
        {
            IPv4Address myip = null;

            string myText = "";
            this.Dispatcher.Invoke(() =>
                {
                    myText = ipToResolve.Text;
                });

            try
            {
                myip = new IPv4Address(myText);
                if (ip.AsString == myip.AsString || !IPv4Address.IsInSameSubnet(myip, ip, Client.Configuration.SubnetMask))
                {
                    this.Dispatcher.Invoke(() =>
                        {
                            arpAnswers.Items.Add(mac.ToString());
                        });
                }
            }
            catch
            { }
        }

        private IPv4Address lastArp = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IPv4Address ip = null;

            if (lastArp != null)
            {
                arpAnswers.Items.Clear();
            }
            try
            {
                ip = new IPv4Address(ipToResolve.Text);
                lastArp = ip;

                if (IPv4Address.IsInSameSubnet(Client.Configuration.IpAddress, ip, Client.Configuration.SubnetMask))
                {
                    Client.Arp.SendArpResolutionPacket(ip);
                }
                else
                {
                    Client.Arp.SendArpResolutionPacket(Client.Configuration.DefaultGateway);
                }
            }
            catch
            {
                MessageBox.Show("\"" + ipToResolve.Text + "\" is not a valid IP address.");
            }
        }

        private void addStaticEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            IPv4Address ip = null;
            MacAddress mac = null;

            try
            {
                ip = new IPv4Address(staticEntryIp.Text);
            }
            catch { }
            try
            {
                mac = new MacAddress(staticEntryMac.Text);
            }
            catch { }

            if (ip == null && mac == null)
            {
                MessageBox.Show("You did not enter a valid IP and MAC address.");
            }
            else if (ip == null)
            {
                MessageBox.Show("\"" + staticEntryIp.Text + "\" is not a valid IP address.");
            }
            else if (mac == null)
            {
                MessageBox.Show("\"" + staticEntryMac.Text + "\" is not a valid MAC address.");
            }
            else
            {
                // We are good to go
                Client.Arp.AddStaticEntry(ip, mac);
            }
        }

        private void clearCacheBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear the ARP cache of all Static and Dynamic entries?", "Confirm Cache Clear", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                lock (Client.Arp.ArpCache)
                {
                    Client.Arp.ArpCache.Clear();
                    arpCache.ItemsSource = null;
                }
            }
        }

        private void ipToResolve_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) Button_Click(null, null);
        }

        private void staticEntryIp_KeyDown(object sender, KeyEventArgs e)
        {
            IPv4Address ip = null;
            MacAddress mac = null;

            try
            {
                ip = new IPv4Address(staticEntryIp.Text);
            }
            catch { }
            try
            {
                mac = new MacAddress(staticEntryMac.Text);
            }
            catch { }

            if (ip != null && mac != null) addStaticEntryBtn_Click(null, null);
        }

        private void sendGratuitousArpReply_Click(object sender, RoutedEventArgs e)
        {
            Client.Arp.SendGratuitousArpReply();
        }

        private void sendGratuitousArpRequest_Click(object sender, RoutedEventArgs e)
        {
            Client.Arp.SendGratuitousArpRequest();
        }
    }
}
