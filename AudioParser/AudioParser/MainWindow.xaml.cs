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
using AudioParser.Properties;
using System.Text.RegularExpressions;

namespace AudioParser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Parser.OnUpdateData += new Parser.NewDataHandler(NewData);

            Parser.Init();

            SetDevices();
            SetSerialPorts();

            tbEthernetAdr.Text = Settings.Default.ServerIp;
            tbEthernetPort.Text = Settings.Default.ServerPort;

            Parser.Start();
        }

        private void SetDevices()
        {
            Parser.Close();
            cbDevices.ItemsSource = Devices.GetDevices();

            cbDevices.SelectedValue = Settings.Default.Device;
            if (cbDevices.SelectedItem == null && cbDevices.Items.Count > 0)
                cbDevices.SelectedIndex = 0;

            Parser.Start();
        }

        private void SetSerialPorts()
        {
            cbComPorts.ItemsSource = Port.GetPorts();

            cbComPorts.SelectedValue = Settings.Default.SerialPort;
            if (cbComPorts.SelectedItem == null && cbComPorts.Items.Count > 0)
                cbComPorts.SelectedIndex = 0;
        }

        private void NewData(NewDataEventArgs e)
        {
            colorBox.SetColor(e.R, e.G, e.B);
        }

        private void cbDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string device = e.AddedItems[0].ToString();
                Devices.Connect(device);

                Settings.Default.Device = device;
                Settings.Default.Save();
            }
        }

        private void tbEthernetAdr_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!IsRightInputText(e.Text, @"[\d.]"))
                e.Handled = true;  
        }

        private void tbEthernetPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsRightInputText(e.Text, @"\d"))
                e.Handled = true;
        }

        private void tbEthernetAdr_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.ServerIp = tbEthernetAdr.Text;
            Settings.Default.Save();
        }

        private void tbEthernetPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.ServerPort = tbEthernetPort.Text;
            Settings.Default.Save();
        }

        private bool IsRightInputText(string text, string query)
        {
            Regex regex = new Regex(query);
            return regex.IsMatch(text);
        }

        private void cbComPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if ((bool)cbComPort.IsChecked)
                {
                    if (Parser.PortSetParam(e.AddedItems[0].ToString()))
                        cbComPort.Background = ColorConverter("#1000FF00");
                    else
                        cbComPort.Background = ColorConverter("#AAFF0000");
                }

                Settings.Default.SerialPort = e.AddedItems[0].ToString();
                Settings.Default.Save();
            }
        }

        private void cbComPort_Checked(object sender, RoutedEventArgs e)
        {
            if (Parser.PortOpen())
                cbComPort.Background = ColorConverter("#1000FF00");
            else
                cbComPort.Background = ColorConverter("#AAFF0000");
        }

        private void cbComPort_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!Parser.PortClose())
            {
                cbComPort.Background = Brushes.White;
            }
        }

        private void cbEthernet_Checked(object sender, RoutedEventArgs e)
        {
            string adr = tbEthernetAdr.Text;
            string portStr = tbEthernetPort.Text;
            int portInt = 0;
            bool rightAdr = IsRightInputText(adr, @"\d.\d.\d.\d");
            bool rightPort = false;

            if (portStr.Length > 0)
            {
                portInt = Convert.ToInt32(portStr);
                if (portInt > 0)
                    rightPort = true;
            }

            if (rightPort && rightAdr)
            {
                portInt = Convert.ToInt32(portStr);
                if (portInt > 0)
                {
                    if (Parser.NetworkConnect(adr, portInt))
                        cbEthernet.Background = ColorConverter("#1000FF00");
                    else
                        cbEthernet.Background = ColorConverter("#AAFF0000");

                    tbEthernetAdr.Background = ColorConverter("#1000FF00");
                    tbEthernetPort.Background = ColorConverter("#1000FF00");
                    tbEthernetAdr.IsEnabled = false;
                    tbEthernetPort.IsEnabled = false;
                }
            }
            else
            {
                (sender as CheckBox).IsChecked = false;

                if (!rightAdr)
                    tbEthernetAdr.Background = ColorConverter("#10FF0000");
                if (!rightPort)
                    tbEthernetPort.Background = ColorConverter("#10FF0000");
            }
        }

        private void cbEthernet_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!Parser.NetworkDisconnect())
            {
                cbEthernet.Background = Brushes.White;
                tbEthernetAdr.Background = Brushes.White;
                tbEthernetPort.Background = Brushes.White;
                tbEthernetAdr.IsEnabled = true;
                tbEthernetPort.IsEnabled = true;
            }
        }

        private Brush ColorConverter(string colorHEX)
        {
            var converter = new BrushConverter();
            return (Brush)converter.ConvertFromString(colorHEX);
        }

        private void miUpdateDevice_Click(object sender, RoutedEventArgs e)
        {
            SetDevices();
        }

        private void miUpdateSerialPorts_Click(object sender, RoutedEventArgs e)
        {
            SetSerialPorts();
        }
    }
}
