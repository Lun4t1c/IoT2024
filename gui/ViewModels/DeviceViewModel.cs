using Caliburn.Micro;
using IoTAgentLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gui.ViewModels
{
    public class DeviceViewModel : Screen
    {
        #region Properties
        private VirtualDevice _virtualDevice;

        public VirtualDevice VirtualDevice
        {
            get { return _virtualDevice; }
            set { _virtualDevice = value; NotifyOfPropertyChange(() => VirtualDevice); }
        }

        #endregion


        #region Constructor
        public DeviceViewModel(VirtualDevice virtualDevice)
        {
            VirtualDevice = virtualDevice;
        }
        #endregion


        #region Methods

        #endregion


        #region Button clicks

        #endregion
    }
}
