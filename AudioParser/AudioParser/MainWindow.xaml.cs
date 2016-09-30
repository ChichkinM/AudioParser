using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AudioParser.Properties;
using System.Text.RegularExpressions;

namespace AudioParser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string lightGreen = "#1000FF00";
        private const string middleRed = "#AAFF0000";
        private const string lightRed = "#10FF0000";

        public MainWindow()
        {
            InitializeComponent();
            Parser.OnUpdateData += new Parser.NewDataHandler(NewData);

            Parser.Init();
            Devices.Init();

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
                        cbComPort.Background = ColorConverter(lightGreen);
                    else
                        cbComPort.Background = ColorConverter(middleRed);
                }

                Settings.Default.SerialPort = e.AddedItems[0].ToString();
                Settings.Default.Save();
            }
        }

        private void cbComPort_Checked(object sender, RoutedEventArgs e)
        {
            if (Parser.PortOpen())
                cbComPort.Background = ColorConverter(lightGreen);
            else
                cbComPort.Background = ColorConverter(middleRed);
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
                        cbEthernet.Background = ColorConverter(lightGreen);
                    else
                        cbEthernet.Background = ColorConverter(middleRed);

                    tbEthernetAdr.Background = ColorConverter(lightGreen);
                    tbEthernetPort.Background = ColorConverter(lightGreen);
                    tbEthernetAdr.IsEnabled = false;
                    tbEthernetPort.IsEnabled = false;
                }
            }
            else
            {
                (sender as CheckBox).IsChecked = false;

                if (!rightAdr)
                    tbEthernetAdr.Background = ColorConverter(lightRed);
                if (!rightPort)
                    tbEthernetPort.Background = ColorConverter(lightRed);
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

        private void miSetDef_Click(object sender, RoutedEventArgs e)
        {
            Devices.SetDefaultDevice(cbDevices.Text);
        }

        private void cbDevices_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            cmUpdateDevice.Items.Clear();

            if (!Devices.IsDefaultDevice(cbDevices.Text))
            {
                MenuItem miSetDef = new MenuItem();
                miSetDef.Header = "Устройство по умолчнию";
                miSetDef.Click += miSetDef_Click;

                cmUpdateDevice.Items.Add(miSetDef);
            }

            MenuItem miUpdateDevice = new MenuItem();
            miUpdateDevice.Header = "Обновить список";
            miUpdateDevice.Click += miUpdateDevice_Click;
            
            cmUpdateDevice.Items.Add(miUpdateDevice);
        }
    }
}
