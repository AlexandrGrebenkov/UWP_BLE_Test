using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.UI.Core;

namespace App3.ViewModels
{
    class MainVM : BaseViewModel
    {
        object SyncObj = new object();

        ObservableCollection<DeviceInformation> _Devices;
        /// <summary>Список найденых девайсов</summary>
        public ObservableCollection<DeviceInformation> Devices
        {
            get { return _Devices; }
            set { SetProperty(ref _Devices, value); }
        }

        DeviceInformation _SelectedDevice;
        /// <summary>Выбранный прибор</summary>
        public DeviceInformation SelectedDevice
        {
            get { return _SelectedDevice; }
            set { SetProperty(ref _SelectedDevice, value); }
        }

        bool _IsScanning;
        /// <summary>Идёт сканирование BLE</summary>
        public bool IsScanning
        {
            get { return _IsScanning; }
            set { SetProperty(ref _IsScanning, value); }
        }

        DeviceWatcher deviceWatcher;

        public MainVM()
        {
            Title = "Тест BLE UWP";

            // Query for extra properties you want returned
            /* string[] requestedProperties =
             {
                 "System.Devices.GlyphIcon",
                 "System.Devices.Aep.Category",
                 "System.Devices.Aep.ContainerId",
                 "System.Devices.Aep.DeviceAddress",
                 "System.Devices.Aep.IsConnected",
                 "System.Devices.Aep.IsPaired",
                 "System.Devices.Aep.IsPresent",
                 "System.Devices.Aep.ProtocolId",
                 "System.Devices.Aep.Bluetooth.Le.IsConnectable",
                 "System.Devices.Aep.SignalStrength",
                 "System.Devices.Aep.Bluetooth.LastSeenTime",
                 "System.Devices.Aep.Bluetooth.Le.IsConnectable",
             };*/

            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

            deviceWatcher =
                        DeviceInformation.CreateWatcher(
                        BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);
            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;

            // EnumerationCompleted and Stopped are optional to implement.
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            cmdScan = new RelayCommand(() =>
            {

                if (IsScanning == true)
                {
                    deviceWatcher.Stop();
                    return;
                }

                
                if (deviceWatcher.Status == DeviceWatcherStatus.Aborted ||
                    deviceWatcher.Status == DeviceWatcherStatus.Created ||
                    deviceWatcher.Status == DeviceWatcherStatus.Stopped)
                {
                    deviceWatcher.Start();

                    Devices = new ObservableCollection<DeviceInformation>();
                    IsScanning = true;
                }
            });
        }


        public RelayCommand cmdScan { get; }




        async private void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                IsScanning = false;
            });
        }

        async private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                IsScanning = false;
                deviceWatcher.Stop();
            });
        }

        async private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                for (int i = 0; i < Devices.Count; i++)
                {
                    if (Devices[i].Id == args.Id)
                        Devices.Remove(Devices[i]);
                }
            });
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {

        }

        async private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (IsConnectable(args))
                    Devices.Add(args);
            });
        }

        bool IsConnectable(DeviceInformation deviceInformation)
        {
            if (string.IsNullOrEmpty(deviceInformation.Name))
                return false;
            // Let's make it connectable by default, we have error handles in case it doesn't work
            bool isConnectable = (bool?)deviceInformation.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"] == true;
            bool isConnected = (bool?)deviceInformation.Properties["System.Devices.Aep.IsConnected"] == true;
            return isConnectable || isConnected;
        }
    }
}
