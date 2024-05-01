﻿using Caliburn.Micro;
using IoTAgentLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

            VirtualDevice.ProductionStateChangedEvent += (_, _) => NotifyOfPropertyChange(() => VirtualDevice);
            VirtualDevice.WorkorderIdChangedEvent += (_, _) => NotifyOfPropertyChange(() => VirtualDevice);
            VirtualDevice.ProductionRateChangedEvent += (_, _) => NotifyOfPropertyChange(() => VirtualDevice);
            VirtualDevice.GoodCountChangedEvent += (_, _) => NotifyOfPropertyChange(() => VirtualDevice);
            VirtualDevice.BadCountChangedEvent += (_, _) => NotifyOfPropertyChange(() => VirtualDevice);
            VirtualDevice.TemperatureChangedEvent += (_, _) => NotifyOfPropertyChange(() => VirtualDevice);
            VirtualDevice.DeviceErrorsChangedEvent += (_, _) => NotifyOfPropertyChange(() => VirtualDevice);
        }
        #endregion


        #region Methods
        private async void SetProductionRate()
        {
            Utils.Globals.IoTAgent.SetProductionRateInDevice(VirtualDevice, 50);
        }
        #endregion


        #region Button clicks
        public void SetProductionRateButton()
        {
            SetProductionRate();
        }
        #endregion
    }
}
