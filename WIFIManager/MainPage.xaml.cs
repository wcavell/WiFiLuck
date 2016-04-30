using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ACTV.Controls;
using WIFIManager.Controls;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace WIFIManager
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page,INotifyPropertyChanged
    { 
        
        private ObservableCollection<WiFiNetworkDisplay> _Displays = new ObservableCollection<WiFiNetworkDisplay>();

        public ObservableCollection<WiFiNetworkDisplay> Displays
        {
            get { return _Displays; }
            set
            {
                _Displays = value;
                OnPropertyChanged();
            }
        }
        private WiFiAdapter firstAdapter;

        private string savedProfileName = null;
        private bool Allowed = true;
        Server server=new Server();
        public static MainPage Current;
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            DataContext = this;
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded;
            Displays.Clear();
            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed)
            {
                Allowed = false;
                Toast.ShowError("不允许连接wifi，请确认已开启wifi");
            }
            else
            {
                var result =
                    await DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                if (result.Count >= 1)
                {
                    firstAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);
                    await UpdateConnectivityStatusAsync();
                    NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
                    OnRefreshRequested(null, null);
                }
                Allowed = true;
            }
           
        }

        private async Task UpdateConnectivityStatusAsync()
        {
            var connectedProfile = await firstAdapter.NetworkAdapter.GetConnectedProfileAsync();
            if (connectedProfile != null && !connectedProfile.ProfileName.Equals(savedProfileName))
            {
                savedProfileName = connectedProfile.ProfileName;
            }
            else if (connectedProfile == null && savedProfileName != null)
            {
                savedProfileName = null;
            }
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await UpdateConnectivityStatusAsync();
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
             {
                 foreach (var network in Displays)
                 {
                     await network.UpdateConnectivityLevel();
                 }
             }); 
        }

        private async void OnRefreshRequested(object sender, RefreshRequestedEventArgs e)
        {
            if (!Allowed)
            {
                Toast.ShowError("不允许连接wifi，请确认已开启wifi，重新打开应用");
                return;
            }
            await firstAdapter.ScanAsync();
            LoadingVisibility=Visibility.Visible;
            await DisplayNetworkReport(firstAdapter.NetworkReport);
           
            e?.GetDeferral();
        }
        private async Task DisplayNetworkReport(WiFiNetworkReport report)
        {
            Displays.Clear();
            if (report.AvailableNetworks != null && report.AvailableNetworks.Count > 0)
            {
                foreach (var network in report.AvailableNetworks)
                {
                    var networkDisplay = new WiFiNetworkDisplay(network, firstAdapter);
                    await networkDisplay.UpdateConnectivityLevel();
                    Displays.Add(networkDisplay);
                }
                GetServerPwds(report);
            }
        }

        private async void GetServerPwds(WiFiNetworkReport report)
        {
            try
            {
                if (report.AvailableNetworks != null && report.AvailableNetworks.Count > 0)
                {
                    var winfos = (from r in report.AvailableNetworks select new WiFiInfo(r.Bssid, r.Ssid)).ToList();
                    var result = await server.GetWifiPwds(winfos);
                    if (result != null)
                    {
                        if (result.QryaPwd.Psws != null)
                        {
                            Toast.Show("成功获取"+ result.QryaPwd.Psws.Count+"个密码");
                            foreach (var p in result.QryaPwd.Psws)
                            {
                                var item = Displays.FirstOrDefault(x => x.Bssid.ToLower() == p.Key.ToLower());
                                if (item != null)
                                {
                                    item.Pwd = p.Value.Pwd;
                                }
                            }
                        }
                    }
                    else
                    {
                        Toast.ShowError("没有密码获取");
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            LoadingVisibility = Visibility.Collapsed;
        }

        private async void OnItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedNetwork = (WiFiNetworkDisplay) e.ClickedItem;
            WiFiReconnectionKind reconnectionKind = WiFiReconnectionKind.Automatic;
            WiFiConnectionResult result;
            if (selectedNetwork.AvailableNetwork.SecuritySettings.NetworkAuthenticationType ==
                NetworkAuthenticationType.Open80211)
            {
                result = await firstAdapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind);
                ConnectResult(result);
            }
            else
            {
                if (selectedNetwork.ConnectivityLevel == "已连接")
                {
                    firstAdapter.Disconnect();
                    await selectedNetwork.UpdateConnectivityLevel();
                    return;
                }

                if (selectedNetwork.Pwd.Length >= 8)
                {
                    var credential = new PasswordCredential();
                    credential.Password = selectedNetwork.Pwd;
                    result =
                        await firstAdapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind, credential);
                    ConnectResult(result);
                }
                else
                {
                    
                    var message = new PwdControl(async (x) =>
                    {
                        var credential = new PasswordCredential();
                        credential.Password = x;
                        result =
                            await
                                firstAdapter.ConnectAsync(selectedNetwork.AvailableNetwork, reconnectionKind, credential);
                        ConnectResult(result);
                    });
                    await message.ShowAsync();
                }
            }
        }

        private void ConnectResult(WiFiConnectionResult result)
        { 
            switch (result.ConnectionStatus)
            {
                case WiFiConnectionStatus.AccessRevoked:
                    Toast.ShowError("连接失败，因为已取消访问网络。");
                    break;
                case WiFiConnectionStatus.InvalidCredential:
                    Toast.ShowError("连接失败，因为提供了无效的凭据");
                    break;
                case WiFiConnectionStatus.NetworkNotAvailable:
                    Toast.ShowError("连接失败，因为网络不可用");
                    break;
                case WiFiConnectionStatus.Success:
                    Toast.Show("连接成功");
                    break;
                case WiFiConnectionStatus.Timeout:
                    Toast.ShowError("连接失败，因为连接尝试超时");
                    break;
                case WiFiConnectionStatus.UnspecifiedFailure:
                    Toast.ShowError("连接失败，原因未列在此列表中");
                    break;
                case WiFiConnectionStatus.UnsupportedAuthenticationProtocol:
                    Toast.ShowError("连接失败，因为不支持身份验证协议");
                    break;
            }
        }

        private Visibility _visibility=Visibility.Collapsed;

        public Visibility LoadingVisibility
        {
            get { return _visibility; }
            set
            {
                _visibility = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
