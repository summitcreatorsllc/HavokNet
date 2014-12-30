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
using HavokNet;
using HavokNet.Common;
using HavokNet.Stack;

namespace Havok
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        #region "Client Control"

        public MainWindow()
        {
            InitializeComponent();
            Analyzers = new List<NetworkAnalyzer>();
        }

        public List<NetworkAnalyzer> Analyzers { get; set; }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (NetworkAnalyzer client in Analyzers)
            {
                try
                {
                    client.Client.Stop();
                }
                catch { }
            }
        }

        #endregion

        private void addIdentityBtn_Click(object sender, RoutedEventArgs e)
        {
            IdentityEditor editor = new IdentityEditor();
            editor.ShowDialog();
            if (editor.DidChoose)
            {
                NetworkAnalyzer analyzer = new NetworkAnalyzer(editor.ChoosenClient);
                analyzer.Client.Start();
                identityGrid.ItemsSource = null;
                Analyzers.Add(analyzer);
                identityGrid.ItemsSource = Analyzers;
                identityGrid.SelectedItem = Analyzers.Last();
            }
        }

        private void removeIdentityBtn_Click(object sender, RoutedEventArgs e)
        {
            if (identityGrid.SelectedItem == null) return;
            var client = identityGrid.SelectedItem as NetworkAnalyzer;
            if (client == null) return;

            client.Client.Stop();
            Analyzers.Remove(client);
            identityGrid.ItemsSource = null;
            identityGrid.ItemsSource = Analyzers;
        }

        private void identityGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                analyzerGrid.Children.Clear();
                analyzerGrid.Children.Add(Analyzers[identityGrid.SelectedIndex]);
            }
            catch { }
        }
    }
}
