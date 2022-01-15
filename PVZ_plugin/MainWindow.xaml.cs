using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PVZ_plugin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // PVZ PID
        private static int iPid = 0;

        // The flag controls is enabled.
        private bool _isEnabledControls = false;

        // private bool IsAutoCollect { get; set; } = false;

        /// <summary>
        /// Thread locked sun value.
        /// </summary>
        private Thread _trdLockedSunValue;

        public static string GameTitle { get; set; } = "植物大战僵尸中文版";
        
        private readonly object _lockIsGameRunning = new object();
        public bool IsGameRunning { get; set; } = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ThemedWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Set app title.
            Title = Properties.Resources.ApplicationTitle;
            // Title = "When you first run app, please click \"Check the game is running or not\" button.";
            DisabledControls();
            CheckGameIsRunning();
        }

        /// <summary>
        /// Disabled all controls that can't be interact.
        /// </summary>
        private void DisabledControls()
        {
            btnModify.IsEnabled = false;
            isLockedSunValue.IsEnabled = false;
            txtSunValue.IsEnabled = false;

            txtGoldValue.IsEnabled = false;
            btnModifyGoldValue.IsEnabled = false;
        }

        private void BtnChcekGameIsRunning_Click(object sender, RoutedEventArgs e)
        {
            int i = Helper.GetProcessPidByName("PlantsVsZombies");
            if (i == 0)
            {
                new InfoWindow("Sorry, Plants vs zombies is not now running!").Show();
            }
            else
            {
                new InfoWindow("Plants vs zombies is now running!").Show();
            }
            // CheckGameIsRunning();
        }

        /// <summary>
        /// Check the game is running or not.
        /// </summary>
        private void CheckGameIsRunning()
        {
            Task.Run(() => 
            { 
                while (true)
                {
                    iPid = Helper.GetProcessPidByName("PlantsVsZombies");
                    if (iPid == 0)
                    {
                        lock (_lockIsGameRunning)
                            IsGameRunning = false;
                        Dispatcher.Invoke(() =>
                        {
                            if (_isEnabledControls)
                            {
                                DisabledControls();
                                _trdLockedSunValue?.Abort();
                                _isEnabledControls = false;
                            }
                        });
                    }
                    else
                    {
                        lock (_lockIsGameRunning)
                            IsGameRunning = true;
                        Dispatcher.Invoke(() =>
                        {
                            if (!_isEnabledControls)
                            {
                                EnableControls();
                                _isEnabledControls = true;
                            }
                        });
                    }
                    Thread.Sleep(1000);
                }
            });

            /*iPid = Helper.GetProcessPidByName("PlantsVsZombies");
            // IntPtr ptrHandleWindow = WinApi.FindWindow(null, GameTitle);
            if (iPid == 0)
            {
                MessageBoxResult result = MessageBox.Show("Plants vs zombies isn't running. If you are sure game is running, then you should input game title by your own. Do you want to input game title and check again?", "Sorry", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    InputWindow window = new InputWindow();
                    if (window.ShowDialog() == true)
                    {
                        CheckGameIsRunning();
                    }
                }
            }
            else
            {
                IsGameRunning = true;
                EnableControls();
            }*/
        }

        /// <summary>
        /// Enable all controls.
        /// </summary>
        private void EnableControls()
        {
            btnModify.IsEnabled = true;
            isLockedSunValue.IsEnabled = true;
            txtSunValue.IsEnabled = true;

            txtGoldValue.IsEnabled = true;
            btnModifyGoldValue.IsEnabled = true;
        }

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            ModifySunValue();
        }

        private void ModifySunValue()
        {
            lock (_lockIsGameRunning) 
            {
                if (!IsGameRunning) { return; }
            }
            bool isParseSuccessful = int.TryParse(txtSunValue?.Text, out int sunValue);
            txtSunValue.Text = sunValue.ToString();
            if (iPid == 0) { return; }
            IntPtr ptrPvzHandle = WinApi.OpenProcess(0x1F0FFF/*Highest permission*/, false, iPid);
            // int sunValueOffsetOne = Marshal.ReadInt32(Address.BASE_ADDRESS, 0) + 0x768;
            // int sunAddress = Marshal.ReadInt32(sunValueOffsetOne, 0) + 0x5560;
            int sunValueOffsetOne = Helper.ReadMemoryValue(Address.BASE_ADDRESS, iPid) + 0x768;
            int sunAddress = Helper.ReadMemoryValue(sunValueOffsetOne, iPid) + 0x5560;
            // Marshal.WriteInt32(new IntPtr(sunAddress), sunValue);
            bool isWriteSuccessful = WinApi.WriteProcessMemory(ptrPvzHandle, new IntPtr(sunAddress), new int[] { sunValue }, 4, IntPtr.Zero);
            WinApi.CloseHandle(ptrPvzHandle);
        }

        private void IsLockedSunValue_Click(object sender, RoutedEventArgs e)
        {
            if (iPid == 0) { return; }

            // Unavailable or false
            if (isLockedSunValue is null || !(bool)isLockedSunValue.IsChecked)
            {
                _trdLockedSunValue?.Abort();
            }
            else
            {
                if (iPid == 0)
                {
                    return;
                }
                _trdLockedSunValue = new Thread(() =>
                {
                    int? sunValue = null;
                    while (true)
                    {
                        IntPtr ptrPvzHandle = WinApi.OpenProcess(0x1F0FFF/*Highest permission*/, false, iPid);
                        int sunValueOffsetOne = Helper.ReadMemoryValue(Address.BASE_ADDRESS, iPid) + 0x768;
                        int sunAddress = Helper.ReadMemoryValue(sunValueOffsetOne, iPid) + 0x5560;
                        sunValue = sunValue is null ? Helper.ReadMemoryValue(sunAddress, iPid) : sunValue;
                        WinApi.WriteProcessMemory(ptrPvzHandle, new IntPtr(sunAddress), new int[] { (int)sunValue }, 4, IntPtr.Zero);
                        WinApi.CloseHandle(ptrPvzHandle);
                        Thread.Sleep(100);
                    }
                })
                {
                    IsBackground = true
                };
                _trdLockedSunValue.Start();
            }
        }

        private void BtnModifyGoldValue_Click(object sender, RoutedEventArgs e)
        {
            ModifyGoldValue();
        }

        private void ModifyGoldValue()
        {
            // Check game is running or not.
            lock (_lockIsGameRunning)
            {
                if (!IsGameRunning) { return; }
            }
            bool isParseSuccessful = int.TryParse(txtGoldValue?.Text, out int goldValue);
            txtGoldValue.Text = goldValue.ToString();
            if (iPid == 0) { return; }
            IntPtr ptrPvzHandle = WinApi.OpenProcess(0x1F0FFF/*Highest permission*/, false, iPid);
            // int sunValueOffsetOne = Marshal.ReadInt32(Address.BASE_ADDRESS, 0) + 0x768;
            // int sunAddress = Marshal.ReadInt32(sunValueOffsetOne, 0) + 0x5560;
            int goldValueOffsetOne = Helper.ReadMemoryValue(Address.BASE_ADDRESS, iPid) + 0x82C;
            int goldAddress = Helper.ReadMemoryValue(goldValueOffsetOne, iPid) + 0x28;
            // Marshal.WriteInt32(new IntPtr(sunAddress), sunValue);
            bool isWriteSuccessful = WinApi.WriteProcessMemory(ptrPvzHandle, new IntPtr(goldAddress), new int[] { goldValue / 10 }, 4, IntPtr.Zero);
            WinApi.CloseHandle(ptrPvzHandle);
        }

        private void Window_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            } 
            else if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
