using Caliburn.Micro;
using gui.Utils;
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
        private Visibility _emptyTextBlockVisibility = Visibility.Visible;

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
                Globals.IoTAgent.DevicesLoadedEvent += OnAgentDevicesLoaded;
            }
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
