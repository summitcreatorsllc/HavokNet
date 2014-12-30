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
using System.Windows.Shapes;
using HavokNet;
using HavokNet.Stack;
using HavokNet.Common;

namespace Havok
{
    /// <summary>
    /// Interaction logic for SpoofSettings.xaml
    /// </summary>
    public partial class IdentityEditor : MahApps.Metro.Controls.MetroWindow
    {
        public IdentityEditor()
        {
            InitializeComponent();
        }

        public bool DidChoose { get; set; }
        public NetClient ChoosenClient { get; set; }

        #region "Device and Info Management"

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new System.Threading.Thread(() => { LoadDevices(); }).Start();
        }
        private void LoadDevices()
        {
            Interfaces = NetworkCard.GetAllAvailableDevices();

            this.Dispatcher.Invoke(() =>
            {
                deviceCombobox.ItemsSource = Interfaces;
                deviceCombobox.SelectedIndex = 0;
            });
        }

        public List<NetworkCard> Interfaces { get; set; }

        #endregion

        #region "Layer 3 Setting Handlers"

        private void dhcpRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ipAddrTxtBox.Text = "";
            subnetMaskTxtBox.Text = "";
            gatewayTxtBox.Text = "";
            dnsServerTxtBox.Text = "";
            staticGrid.IsEnabled = false;
        }
        private void staticRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            staticGrid.IsEnabled = true;
        }
        private void noIpRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (staticGrid != null) staticGrid.IsEnabled = false;
        }
        private void copyDevLayer3Config_Click(object sender, RoutedEventArgs e)
        {
            if (deviceCombobox.SelectedIndex == -1) return;

            try
            {
                int index = deviceCombobox.SelectedIndex;
                NetworkCard nic = Interfaces[index];

                dhcpRadioButton.IsChecked = false;
                staticRadioButton.IsChecked = true;


                ipAddrTxtBox.Text = nic.Config.IpAddress.AsString;
                subnetMaskTxtBox.Text = nic.Config.SubnetMask.AsString;
                dnsServerTxtBox.Text = nic.Config.DnsServers[0].AsString;
                gatewayTxtBox.Text = nic.Config.DefaultGateway.AsString;
            }
            catch { }
        }

        #endregion

        #region "Layer 2 Setting Handlers"

        private void copyDevLayer2Config_Click(object sender, RoutedEventArgs e)
        {
            if (deviceCombobox.SelectedIndex == -1) return;

            try
            {
                int index = deviceCombobox.SelectedIndex;
                NetworkCard nic = Interfaces[index];
                macAddrTxtBox.Text = nic.Config.MacAddress.AsString;
            }
            catch { }
        }
        private void randomMacAddrBtn_Click(object sender, RoutedEventArgs e)
        {
            macAddrTxtBox.Text = MacAddress.Random.AsString;
            
        }

        #endregion

        #region "Save / Load / Start Btn. Handlers"

        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            NetworkCard nic = TryGetNic();
            
            HavokNet.OSI.IPv4StackConfiguration config = new HavokNet.OSI.IPv4StackConfiguration();
            config.IpAddress = new IPv4Address(ipAddrTxtBox.Text);
            config.SubnetMask = new IPv4Address(subnetMaskTxtBox.Text);
            config.DnsServers = new IPv4Address[1] { new IPv4Address(dnsServerTxtBox.Text)};
            config.DefaultGateway = new IPv4Address(gatewayTxtBox.Text);
            config.MacAddress = new MacAddress(macAddrTxtBox.Text);

            NetClient client = new NetClient(nic, config);

            switch (sessionCombobox.SelectedIndex)
            {
                case 0:
                    client.NetworkFirewall = new HavokNet.Firewall.SimpleFirewall(true, true);
                    break;
                case 1:
                    client.NetworkFirewall = new HavokNet.Firewall.SimpleFirewall(false, false);
                    break;
                case 2:
                    break;
            }

            ChoosenClient = client;
            DidChoose = true;
            this.Close();
        }

        #endregion

        #region "Loading Stuff"

        private void ShowLoading()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                mainGrid.Visibility = System.Windows.Visibility.Collapsed;
                loadingPanel.Visibility = System.Windows.Visibility.Visible;
            }));
        }

        private void HideLoading()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                mainGrid.Visibility = System.Windows.Visibility.Visible;
                loadingPanel.Visibility = System.Windows.Visibility.Collapsed;
            }));
        }

        #endregion

        #region "Get Config and NIC"

        private NetworkCard TryGetNic()
        {
            try
            {
                int index = deviceCombobox.SelectedIndex;
                return Interfaces[index];
            }
            catch
            {

                return null;
            }
        }

        #endregion
    }
}
