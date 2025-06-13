using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Net.NetworkInformation;
using System.Net;
using System;
using System.Management;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using NetScanner;
using System.Threading;

namespace Анализ_Сети
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();


            Btn_scannerOpener_Click(Btn_scannerOpener, null);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;


            staticIP_container_isActive(false);
            DNS_container_isActive(false);


            //Проверка на подключение к сети
            UpdateNetStatus();

            Btn_scanner_startScan_Click(Btn_scanner_startScan, null);
        }


        #region network information requests

        ///<Summary> Возвращает IP адрес шлюза первого активного сетевого интерфейса.
        /// Иначе возвращает null
        /// </Summary>
        private IPAddress GetGatewayAddress()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    GatewayIPAddressInformationCollection gateways = ni.GetIPProperties().GatewayAddresses;

                    foreach (GatewayIPAddressInformation gateway in gateways)
                    {
                        if (gateway.Address != null)
                        {
                            return gateway.Address;
                        }
                    }
                }
            }
            return null;
        }

        bool isOnline = false;
        private async void UpdateNetStatus()
        {
            while (true)
            {
                if (GetGatewayAddress() != null)
                {
                    ellipse_netstatus.Fill = new SolidColorBrush(Color.FromArgb(255, 35, 144, 52));
                    lbl_netstatus_container__Header.Text = "В сети";
                    isOnline = true;
                }
                else
                {
                    ellipse_netstatus.Fill = new SolidColorBrush(Color.FromArgb(255, 101, 101, 101));
                    lbl_netstatus_container__Header.Text = "Не в сети";
                    isOnline = false;
                }
                await Task.Delay(15000);
            }
        }

        private IPAddress GetIPAddress()
        {

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    IPInterfaceProperties ipInfo = ni.GetIPProperties();
                    
                    if (ipInfo != null)
                    {
                        UnicastIPAddressInformationCollection unicastAddresses = ipInfo.UnicastAddresses;

                        foreach (UnicastIPAddressInformation uni in unicastAddresses)
                        {
                            if (uni.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                return uni.Address;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private IPAddress GetSubnetMask()
        {

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    IPInterfaceProperties ipInfo = ni.GetIPProperties();

                    if (ipInfo != null)
                    {
                        UnicastIPAddressInformationCollection unicastAddresses = ipInfo.UnicastAddresses;

                        foreach (UnicastIPAddressInformation uni in unicastAddresses)
                        {
                            if (uni.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                return uni.IPv4Mask;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private string GetInterfaceName()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up
                    && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
                    && ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                    && ni.Supports(NetworkInterfaceComponent.IPv4))
                {
                    return ni.Name;
                }
            }
            return null;
        }

        #endregion


        #region network set requests

        private void SetStaticIP(string interfaceName, string ipAddress, string mask, string gateway)
        {
            ManagementClass niConfig = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection niConfigs = niConfig.GetInstances();

            foreach (ManagementObject ni in niConfigs)
            {
                if (ni["Description"] != null
                    && ni["Description"].ToString().Equals(interfaceName, StringComparison.OrdinalIgnoreCase)
                    && (bool)ni["IPEnabled"])
                {

                    //УСТАНОВКА IP И МАСКИ
                    ManagementBaseObject setIP;
                    ManagementBaseObject newIP = ni.GetMethodParameters("EnableStatic");

                    newIP["IPAddress"] = new string[] { ipAddress };
                    newIP["SubnetMask"] = new string[] { mask };

                    setIP = ni.InvokeMethod("EnableStatic", newIP, null);

                    if ((uint)setIP["ReturnValue"] != 0)
                    {
                        throw new Exception($"Не удалось установить статический IP-адрес. Код ошибки: {setIP["ReturnValue"]}");
                    }

                    //УСТАНОВКА ШЛЮЗА
                    ManagementBaseObject setGateway;
                    ManagementBaseObject newGateway = ni.GetMethodParameters("SetGateways");
                    newGateway["DefaultIPGateway"] = new string[] { gateway };
                    newGateway["GatewayCostMetric"] = new int[] { 1 };

                    setGateway = ni.InvokeMethod("SetGateways", newGateway, null);

                    if ((uint)setGateway["ReturnValue"] != 0)
                    {
                        throw new Exception($"Не удалось установить шлюз по умолчанию. Код ошибки: {setGateway["ReturnValue"]}");
                    }

                    break;
                }
            }
        }

        private void SetDNS(string interfaceName, string primaryDNSServer, string secondaryDNSServer)
        {
            ManagementClass niConfig = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection niConfigs = niConfig.GetInstances();

            foreach (ManagementObject ni in niConfigs)
            {
                if (ni["Description"] != null && ni["Description"].ToString().Equals(interfaceName, StringComparison.OrdinalIgnoreCase) && (bool)ni["IPEnabled"])
                {
                    ManagementBaseObject setDNS;
                    ManagementBaseObject dnsParams = ni.GetMethodParameters("SetDNSServerSearchOrder");

                    dnsParams["DNSServerSearchOrder"] = new string[] {primaryDNSServer, secondaryDNSServer};
                    setDNS = ni.InvokeMethod("SetDNSServerSearchOrder", dnsParams, null);

                    if ((uint)setDNS["ReturnValue"] != 0)
                    {
                        throw new Exception($"Не удалось установить DNS-серверы. Код ошибки: {setDNS["ReturnValue"]}");
                    }
                    break;
                }
            }
        }

        #endregion





        #region LEFT PANEL navigation buttons

        bool expectation = false;
        private async void Btn_menuOpener_Click(object sender, RoutedEventArgs e)
        {
            if (!expectation && Grid_window__leftPanel.ActualWidth < 309)
            {
                while (!expectation && Grid_window__leftPanel.ActualWidth < 309)
                {
                    expectation = true;
                    Grid_window__leftPanel.Width += (310 - Grid_window__leftPanel.ActualWidth) / 5;
                    await Task.Delay(10);
                    expectation = false;
                }
                Grid_window__leftPanel.Width = 310;
            }
            else if (!expectation && Grid_window__leftPanel.ActualWidth > 97)
            {
                while (!expectation && Grid_window__leftPanel.ActualWidth > 97)
                {
                    expectation = true;
                    Grid_window__leftPanel.Width -= (Grid_window__leftPanel.ActualWidth - 96) / 5;
                    await Task.Delay(10);
                    expectation = false;
                }
                Grid_window__leftPanel.Width = 96;
            }
        }


        private void Btn_scannerOpener_Click(object sender, RoutedEventArgs e)
        {
            HideWindowGrids();
            Grid_window__scanner.Visibility = Visibility.Visible;
            if (Grid_window__leftPanel.ActualWidth > 96 && this.WindowState != WindowState.Maximized) Btn_menuOpener_Click(Btn_menuOpener, null);
        }


        private async void Btn_editOpener_Click(object sender, RoutedEventArgs e)
        {
            HideWindowGrids();
            Grid_window__editor.Visibility = Visibility.Visible;
            if (Grid_window__leftPanel.ActualWidth > 96 && this.WindowState != WindowState.Maximized) Btn_menuOpener_Click(Btn_menuOpener, null);

            if (!IsAdministrator())
            {
                Grid_navigationButtons__warning.IsEnabled = false;
                Grid_navigationButtons__warning.Opacity = 0;
                await Task.Delay(1000);
                ShowNotificationAnimation();
            }
        }


        private void HideWindowGrids()
        {
            Grid_window__editor.Visibility = Visibility.Hidden;
            Grid_window__scanner.Visibility = Visibility.Hidden;
        }
        

        private void Btn_infoOpener_Click(object sender, RoutedEventArgs e)
        {
            InformationWindow nw = new InformationWindow();
            nw.Show();
        }

        #endregion


        

        #region SCANNER

        private void plugs_color_set(byte num)
        {
            Border_scanner__NetworkInterface_name_plug.Background = new SolidColorBrush(Color.FromArgb(255, num, num, num));
            Border_scanner_IPv4Plug_Header.Background = new SolidColorBrush(Color.FromArgb(255, num, num, num));
            Border_scanner_IPv4Plug.Background = new SolidColorBrush(Color.FromArgb(255, num, num, num));
            Border_scanner_GatewayPlug_Header.Background = new SolidColorBrush(Color.FromArgb(255, num, num, num));
            Border_scanner_GatewayPlug.Background = new SolidColorBrush(Color.FromArgb(255, num, num, num));
            Border_scanner_MaskPlug_Header.Background = new SolidColorBrush(Color.FromArgb(255, num, num, num));
            Border_scanner_MaskPlug.Background = new SolidColorBrush(Color.FromArgb(255, num, num, num));
        }
        private async void scanner_plugs_loading_animation()
        {
            Thickness currentMargin = Grid_scanner_plugs.Margin;
            Thickness newMargin = new Thickness(0, currentMargin.Top, currentMargin.Right, currentMargin.Bottom);
            Grid_scanner_plugs.Margin = newMargin;
            Grid_scanner_plugs.Opacity = 1;


            for (byte blackout = 62; blackout > 43; blackout--)
            {
                plugs_color_set(blackout);
                await Task.Delay(50);
            }
            for (byte highlighting = 42; highlighting < 63; highlighting++)
            {
                plugs_color_set(highlighting);
                await Task.Delay(50);
            }
            await Task.Delay(150);

        }

        private async void scanner_plugs_fade_animation()
        {
            while (Grid_scanner_plugs.Opacity > 0)
            {
                lbl_scanner__NetworkInterface_name.Opacity += 0.1;
                Grid_scanner_plugs.Opacity -= 0.1;
                await Task.Delay(2);
            }

            Thickness currentMargin = Grid_scanner_plugs.Margin;
            // 2. Создаем новый объект Thickness, в котором изменяем Left, а остальные значения берем из текущего Margin
            Thickness newMargin = new Thickness(600, currentMargin.Top, currentMargin.Right, currentMargin.Bottom);
            // 3. Применяем новый Margin к Grid
            Grid_scanner_plugs.Margin = newMargin;
        }

        private void ShowMessageBox(string Header, string Comment)
        {
            Notification messageBox = new Notification(Header, Comment);
            messageBox.Show();
        }

        private async void Btn_scanner_startScan_Click(object sender, RoutedEventArgs e)
        {
            byte TTE = 0;

            if (isOnline)
            {
                Btn_scanner_startScan.IsEnabled = false;

                while (TTE < 5)
                {
                    scanner_plugs_loading_animation();
                    try
                    {
                        lbl_scanner__NetworkInterface_name.Content = GetInterfaceName();
                        tb_IPv4_output_bg__text.Text = Convert.ToString(GetIPAddress());
                        tb_mask_output_bg__text.Text = Convert.ToString(GetSubnetMask());
                        tb_gateway_output_bg__text.Text = Convert.ToString(GetGatewayAddress());
                        break;
                    }
                    catch (Exception ex)
                    {
                        ShowMessageBox("Ошибка", $"Не удалось обнаружить адаптер. Сообщение ошибки: {ex.Message}");
                    }
                    await Task.Delay(2400);
                    TTE++;
                }

                if (lbl_scanner__NetworkInterface_name.Content == null
                    && tb_IPv4_output_bg__text.Text == null
                    && tb_mask_output_bg__text == null
                    && tb_gateway_output_bg__text.Text == null)
                {
                    ShowMessageBox("Ошибка", $"Не удалось обнаружить адаптер.\nВозможные решения: перезапустить сеть, обновить статус в приложении");
                }
                scanner_plugs_fade_animation();
                Btn_scanner_startScan.IsEnabled = true;
            }
            else
            {
                await Task.Delay(1000);
                ShowMessageBox("Устройство не в сети!", "Возможные решения: перезапустить сеть, сброс сети, перезапуск приложения, обновление статуса в приложении");
            }

        }


        //Clipboard
        private async void MoveClipboardNotification(int marginLeft, int marginUp, int marginRight, int marginBottom)
        {
            Thickness currentMargin = Border_scanner__Clipboard_notification.Margin;
            Thickness newMargin = new Thickness(marginLeft, marginUp, marginRight, marginBottom);
            Border_scanner__Clipboard_notification.Margin = newMargin;
            while (Border_scanner__Clipboard_notification.Opacity < 1)
            {
                Border_scanner__Clipboard_notification.Opacity += 0.2;
                await Task.Delay(1);
            }
            await Task.Delay(1000);
            while (Border_scanner__Clipboard_notification.Opacity > 0)
            {
                Border_scanner__Clipboard_notification.Opacity -= 0.1;
                await Task.Delay(20);
            }
        }
        
        private void Border_scanner__IPv4_output_background_LB(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, tb_IPv4_output_bg__text.Text);
            MoveClipboardNotification(0, -290, -240, 0);
        }

        private void Border_scanner__mask_output_background_LB(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, tb_mask_output_bg__text.Text);
            MoveClipboardNotification(0, -100, -240, 0);
        }

        private void Border_scanner__gateway_output_background_LB(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, tb_gateway_output_bg__text.Text);
            MoveClipboardNotification(0, 90, -240, 0);
        }

        #endregion




        #region grid_EDIT_container

        #region edit_container texboxes hint printers

        private void tbHintPrint(string valueToInsert, System.Windows.Controls.TextBox TextBox)
        {
            string text = TextBox.Text;
            if (text == string.Empty)
            {
                TextBox.Text = valueToInsert;
                TextBox.Foreground = new SolidColorBrush(Color.FromArgb(255, 99, 99, 99));
            }
            else if (text == valueToInsert)
            {
                TextBox.Clear();
                TextBox.Foreground = new SolidColorBrush(Color.FromArgb(255, 216, 216, 216));
            }
        }

        private void tb_staticIP_container__staticIP_LostFocus(object sender, RoutedEventArgs e)
        {
            tbHintPrint("IPv4 Адрес", tb_staticIP_container__staticIP);
        }

        private void tb_staticIP_container__staticIP_GotFocus(object sender, RoutedEventArgs e)
        {
            tbHintPrint("IPv4 Адрес", tb_staticIP_container__staticIP);
        }

        private void tb_staticIP_container__subnet_mask_GotFocus(object sender, RoutedEventArgs e)
        {
            tbHintPrint("Маска", tb_staticIP_container__subnet_mask);
        }

        private void tb_staticIP_container__subnet_mask_LostFocus(object sender, RoutedEventArgs e)
        {
            tbHintPrint("Маска", tb_staticIP_container__subnet_mask);
        }

        private void tb_staticIP_container__gateway_GotFocus(object sender, RoutedEventArgs e)
        {
            tbHintPrint("Основной шлюз", tb_staticIP_container__gateway);
        }

        private void tb_staticIP_container__gateway_LostFocus(object sender, RoutedEventArgs e)
        {
            tbHintPrint("Основной шлюз", tb_staticIP_container__gateway);
        }

        private void tb_DNS_container__primaryDNS_GotFocus(object sender, RoutedEventArgs e)
        {
            tbHintPrint("Предпочитаемый DNS", tb_DNS_container__primaryDNS);
        }

        private void tb_DNS_container__primaryDNS_LostFocus(object sender, RoutedEventArgs e)
        {
            tbHintPrint("Предпочитаемый DNS", tb_DNS_container__primaryDNS);
        }

        private void tb_DNS_container__secondaryDNS_GotFocus(object sender, RoutedEventArgs e)
        {
            tbHintPrint("Альтернативный DNS", tb_DNS_container__secondaryDNS);
        }

        private void tb_DNS_container__secondaryDNS_LostFocus(object sender, RoutedEventArgs e)
        {
            tbHintPrint("Альтернативный DNS", tb_DNS_container__secondaryDNS);
        }

        #endregion


        #region edit_container checkbox switch and textbox activation

         private void staticIP_container_isActive(bool bit)
        {
            tb_staticIP_container__staticIP.IsEnabled = bit;
            tb_staticIP_container__subnet_mask.IsEnabled = bit;
            tb_staticIP_container__gateway.IsEnabled = bit;
        }

        private void DNS_container_isActive(bool bit)
        {
            tb_DNS_container__primaryDNS.IsEnabled = bit;
            tb_DNS_container__secondaryDNS.IsEnabled = bit;
        }

        private void cb_staticIP_container_staticIP_switcher_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_staticIP_container_staticIP_switcher.IsChecked == true)
            {
                staticIP_container_isActive(false);
            }
            else if (cb_staticIP_container_staticIP_switcher.IsChecked == false)
            {
                staticIP_container_isActive(true);

            }
        }

        private void cb_DNS_container__DNS_switcher_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_DNS_container__DNS_switcher.IsChecked == true)
            {
                DNS_container_isActive(false);

            }
            else if (cb_DNS_container__DNS_switcher.IsChecked == false)
            {
                DNS_container_isActive(true);
            }
        }

        #endregion


        #region administrator validation

        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void StartAsAdministrator()
        {
            if (!IsAdministrator())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true; // Важно для запроса прав администратора
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                startInfo.Verb = "runas"; // Запускаем с правами администратора

                try
                {
                    // Запускаем новый процесс от имени администратора
                    Process.Start(startInfo);
                    // Закрываем текущий процесс
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    ShowMessageBox("Отклонено в доступе.", "Не удалось перезапустить приложение с правами администратора.\n" + ex.Message);
                }
            }
        }

        private void ShowNotificationAnimation()
        {
            var animation = new ThicknessAnimation
            {
                From = Grid_navigationButtons__adminNotification.Margin,
                To = new Thickness(
                Grid_navigationButtons__adminNotification.Margin.Left,
                Grid_navigationButtons__adminNotification.Margin.Top,
                Grid_navigationButtons__adminNotification.Margin.Right,
                90), // конечное значение Bottom
                Duration = TimeSpan.FromSeconds(0.5), // длительность анимации
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            Grid_navigationButtons__adminNotification.BeginAnimation(FrameworkElement.MarginProperty, animation);
        }

        private void ShakeGridAnimation(System.Windows.Controls.Grid grid)
        {
            // Убедимся, что у элемента есть RenderTransform типа TransformGroup или TranslateTransform
            if (grid.RenderTransform == null ||
                !(grid.RenderTransform is TranslateTransform))
            {
                grid.RenderTransform = new TranslateTransform(0, 0);
            }

            var transform = (TranslateTransform)grid.RenderTransform;

            // Создаем анимацию смещения по X
            var animation = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromSeconds(0.5), // длительность тряски
                RepeatBehavior = new RepeatBehavior(1) // выполнить один раз
            };

            // Добавляем ключевые кадры для эффекта тряски
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(0, KeyTime.FromPercent(0.0)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(-10, KeyTime.FromPercent(0.1)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(10, KeyTime.FromPercent(0.2)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(-10, KeyTime.FromPercent(0.3)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(10, KeyTime.FromPercent(0.4)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(-10, KeyTime.FromPercent(0.5)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(10, KeyTime.FromPercent(0.6)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(-10, KeyTime.FromPercent(0.7)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(10, KeyTime.FromPercent(0.8)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(-10, KeyTime.FromPercent(0.9)));
            animation.KeyFrames.Add(new SplineDoubleKeyFrame(0, KeyTime.FromPercent(1.0)));

            transform.BeginAnimation(TranslateTransform.XProperty, animation);

        }

        #endregion

        private void Btn_editor__saveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdministrator())
            {
                ShakeGridAnimation(Grid_navigationButtons__adminNotification); return;
            }
            else if (IsAdministrator() && Grid_navigationButtons__warning.IsEnabled)
            {
                ShakeGridAnimation(Grid_navigationButtons__warning); return;
            }
            
            // Настройка статического IP-адреса
            else if (IsAdministrator() && GetGatewayAddress() != null)
            {
                bool noErrors = true;

                if (cb_staticIP_container_staticIP_switcher.IsChecked == true)
                {
                    // no method yet
                }
                else if (cb_staticIP_container_staticIP_switcher.IsChecked == false && 
                    (tb_staticIP_container__staticIP.Text == "IPv4 Адрес" 
                    || tb_staticIP_container__subnet_mask.Text == "Маска" 
                    || tb_staticIP_container__gateway.Text == "Основной шлюз"))
                {
                    ShowMessageBox("Не все поля заполнены!", "Аккуратно! Данные неверны или заполнены не полностью.");
                    noErrors = false;
                }
                else
                {
                    try
                    {
                        SetStaticIP(GetInterfaceName(), tb_staticIP_container__staticIP.Text, tb_staticIP_container__subnet_mask.Text, tb_staticIP_container__gateway.Text);
                    }
                    catch (Exception ex)
                    { 
                        ShowMessageBox("Ошибка!", $"Не удалось установить статический адрес. Сообщение ошибки: {ex.Message}");
                        noErrors = false;
                    }
                }

                //настройка DNS серверов
                if (cb_staticIP_container_staticIP_switcher.IsChecked == true)
                {
                   // no method yet
                }
                else if (cb_DNS_container__DNS_switcher.IsChecked == false && 
                    (tb_DNS_container__primaryDNS.Text == "Предпочитаемый DNS" 
                    || tb_DNS_container__secondaryDNS.Text == "Альтернативный DNS"))
                {
                    ShowMessageBox("Не все поля заполнены!", "Аккуратно! Данные неверны или заполнены не полностью.");
                    noErrors = false;
                }
                else
                {
                    try
                    {
                        SetDNS(GetInterfaceName(), tb_DNS_container__primaryDNS.Text, tb_DNS_container__secondaryDNS.Text);
                    }
                    catch (Exception ex)
                    {
                        ShowMessageBox("Ошибка!", $"Не удалось установить DNS. Сообщение ошибки: {ex.Message}");
                        noErrors = false;
                    }
                }

                if (noErrors)
                {
                    ShowMessageBox("Успешно!", "Настройки применены без ошибок.");
                }
            }
        }

        private void Btn_editor__cancelChanges_Click(object sender, RoutedEventArgs e)
        {
            tb_staticIP_container__staticIP.Clear();
            tb_staticIP_container__subnet_mask.Clear();
            tb_staticIP_container__gateway.Clear();
            tb_DNS_container__primaryDNS.Clear();
            tb_DNS_container__secondaryDNS.Clear();

            tb_staticIP_container__staticIP_LostFocus(tb_staticIP_container__staticIP, null);
            tb_staticIP_container__subnet_mask_LostFocus(tb_staticIP_container__subnet_mask, null);
            tb_staticIP_container__gateway_LostFocus(tb_staticIP_container__gateway, null);
            tb_DNS_container__primaryDNS_LostFocus(tb_DNS_container__primaryDNS, null);
            tb_DNS_container__secondaryDNS_LostFocus(tb_DNS_container__secondaryDNS, null);

            cb_DNS_container__DNS_switcher.IsChecked = true;
            DNS_container_isActive(false);
            cb_staticIP_container_staticIP_switcher.IsChecked = true;
            staticIP_container_isActive(false);
        }



        private void Btn_adminNotification__restart_Click(object sender, RoutedEventArgs e)
        {
            StartAsAdministrator();
        }

        private void Btn_warning__OK_Click(object sender, RoutedEventArgs e)
        {
            Grid_navigationButtons__warning.IsEnabled = false;
            Grid_navigationButtons__warning.Opacity = 0;
        }



        #endregion

    }
}
