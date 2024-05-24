using Caliburn.Micro;
using gui.Utils;
using IoTAgentLib.Utils;
using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gui.ViewModels
{
    public class DevicesListViewModel : Screen
    {
        #region Properties
        private BindableCollection<DeviceViewModel> _devicesViewModels = new BindableCollection<DeviceViewModel>();
        private Visibility _emptyTextBlockVisibility = Visibility.Hidden;

        public Visibility EmptyTextBlockVisibility
        {
            get { return _emptyTextBlockVisibility; }
            set { _emptyTextBlockVisibility = value; NotifyOfPropertyChange(() => EmptyTextBlockVisibility); }
        }

        public BindableCollection<DeviceViewModel> DevicesViewModels
        {
            get { return _devicesViewModels; }
            set { _devicesViewModels = value; NotifyOfPropertyChange(() => DevicesViewModels); }
        }
        #endregion


        #region Constructor
        public DevicesListViewModel()
        {
            if (Globals.IoTAgent.IsConnected)
                LoadUpDevicesFromAgent();
            else
            {
                EmptyTextBlockVisibility = Visibility.Visible;
                Globals.IoTAgent.DevicesLoadedEvent += OnAgentDevicesLoaded;
            }

            Globals.IoTAgent.NewDeviceEvent += IoTAgent_NewDeviceEvent;
        }

        private void IoTAgent_NewDeviceEvent(object sender, VirtualDevice vd)
        {
            DevicesViewModels.Add(new DeviceViewModel(vd));
        }
        #endregion


        #region Methods
        private void LoadUpDevicesFromAgent()
        {
            foreach (var device in Globals.IoTAgent.Devices)
                DevicesViewModels.Add(new DeviceViewModel(device));

            if (DevicesViewModels.Count == 0)
                EmptyTextBlockVisibility = Visibility.Visible;
        }

        private void AddNewDevice()
        {
            new WindowManager().ShowDialogAsync(new NewDeviceWindowViewModel());
        }
        #endregion


        #region Button clicks
        public void NewDeviceButton()
        {
            AddNewDevice();
        }
        #endregion


        #region Event handlers
        private void OnAgentDevicesLoaded(object? sender, EventArgs e)
        {
            EmptyTextBlockVisibility = Visibility.Hidden;
            LoadUpDevicesFromAgent();
        }
        #endregion
    }
}
