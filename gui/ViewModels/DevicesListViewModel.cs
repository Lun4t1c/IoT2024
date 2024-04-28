using Caliburn.Micro;
using gui.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gui.ViewModels
{
    public class DevicesListViewModel : Screen
    {
        #region Properties
        private BindableCollection<DeviceViewModel> _devicesViewModels = new BindableCollection<DeviceViewModel>();

        public BindableCollection<DeviceViewModel> DevicesViewModels
        {
            get { return _devicesViewModels; }
            set { _devicesViewModels = value; NotifyOfPropertyChange(() => DevicesViewModels); }
        }

        #endregion


        #region Constructor
        public DevicesListViewModel()
        {
            foreach (var device in Globals.IoTAgent.Devices)
            {
                DevicesViewModels.Add(new DeviceViewModel(device));
            }
        }
        #endregion


        #region Methods

        #endregion
    }
}
