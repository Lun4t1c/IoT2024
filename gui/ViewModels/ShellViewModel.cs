using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gui.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        #region Properties
        private DevicesListViewModel _devicesList = null;

        public DevicesListViewModel DevicesList
        {
            get
            {
                if (_devicesList == null)
                    _devicesList = new DevicesListViewModel();
                return _devicesList;
            }
        }
        #endregion


        #region Constructor
        public ShellViewModel()
        {

        }
        #endregion


        #region Methods
        private async void ActivateDevicesList()
        {
            await ActivateItemAsync(DevicesList);
        }
        #endregion

        public void DevicesListButton()
        {
            ActivateDevicesList();
        }
    }
}
