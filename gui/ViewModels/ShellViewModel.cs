﻿using Caliburn.Micro;
using gui.Utils;
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
        private string _serverStatusString = "Server: Disconnected";
        private string _serverConnectionString = "";

        public DevicesListViewModel DevicesList
        {
            get
            {
                if (_devicesList == null)
                    _devicesList = new DevicesListViewModel();
                return _devicesList;
            }
        }

        public string ServerStatusString
        {
            get { return _serverStatusString; }
            set { _serverStatusString = value; NotifyOfPropertyChange(() => ServerStatusString); }
        }

        public string ServerConnectionString
        {
            get { return _serverConnectionString; }
            set { _serverConnectionString = value; NotifyOfPropertyChange(() => ServerConnectionString); }
        }
        #endregion


        #region Constructor
        public ShellViewModel()
        {
            Globals.IoTAgent.ServerConnectedEvent += OnAgentServerConnected;
        }
        #endregion


        #region Methods
        private void ConnectWithServer()
        {
            Globals.IoTAgent.ConnectWithServer(ServerConnectionString);
        }

        private async void ActivateDevicesList()
        {
            await ActivateItemAsync(DevicesList);
        }
        #endregion


        #region Button clicks
        public void ConnectButton()
        {
            ConnectWithServer();
        }

        public void DevicesListButton()
        {
            ActivateDevicesList();
        }
        #endregion


        #region Event handlers
        private void OnAgentServerConnected(object? sender, EventArgs e)
        {
            ServerStatusString = "Server: Connected";
        }
        #endregion
    }
}