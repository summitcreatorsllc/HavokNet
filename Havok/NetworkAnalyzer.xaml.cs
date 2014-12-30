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

namespace Havok
{
    /// <summary>
    /// Interaction logic for NetworkAnalyzer.xaml
    /// </summary>
    public partial class NetworkAnalyzer : UserControl
    {
        public NetworkAnalyzer(HavokNet.Stack.NetClient client)
        {
            InitializeComponent();
            Client = client;
            LoadTabs();
        }

        public HavokNet.Stack.NetClient Client { get; set; }

        #region "Loading Tab Stuff"

        private void LoadTabs()
        {
            // Load and add Firewall
            FirewallPage = new Protocols.Firewall(Client);
            tabViewer.Items.Add(GetItemFromPage(FirewallPage));

            // Load and add ARP
            ArpPage = new Protocols.ARP(Client);
            tabViewer.Items.Add(GetItemFromPage(ArpPage));

            // Load and add ICMP
            IcmpPage = new Protocols.ICMP(Client);
            tabViewer.Items.Add(GetItemFromPage(IcmpPage));

            // Load and add DNS
            DnsPage = new Protocols.DNS(Client);
            tabViewer.Items.Add(GetItemFromPage(DnsPage));

            // Load and add HTTP
            HttpPage = new Protocols.HTTP(Client);
            tabViewer.Items.Add(GetItemFromPage(HttpPage));
        }

        private TabItem GetItemFromPage(Page pg)
        {
            TabItem item = new TabItem();
            item.Header =pg.Title;
            
            Frame frm = new Frame();
            frm.Content = pg;
            frm.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            item.Content = frm;
            return item;
        }

        #endregion

        #region "The Tab Pages"

        public Protocols.Firewall FirewallPage { get; set; }
        public Protocols.ARP ArpPage { get; set; }
        public Protocols.ICMP IcmpPage { get; set; }
        public Protocols.DNS DnsPage { get; set; }
        public Protocols.HTTP HttpPage { get; set; }

        #endregion
    }
}
