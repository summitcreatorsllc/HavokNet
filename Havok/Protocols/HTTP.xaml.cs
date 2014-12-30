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
using HavokNet.Stack;
using HavokNet.Protocols.HTTP;
using System.IO;

namespace Havok.Protocols
{
    /// <summary>
    /// Interaction logic for HTTP.xaml
    /// </summary>
    public partial class HTTP : ProtocolBase
    {
        public HTTP(NetClient client) : base(client)
        {
            InitializeComponent();
        }

        private void webAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) Button_Click(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string address = webAddress.Text;
            Task.Run(() =>
                {
                    HttpResponse response = Client.Http.Get(address);
                    string filename = Path.GetTempFileName() + ".html";
                    File.WriteAllText(filename, response.DataAsString);
                    this.Dispatcher.Invoke(() =>
                        {
                            resultsFrame.Source = new Uri(filename);
                        });
                });
        }
    }
}
