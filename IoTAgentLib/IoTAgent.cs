﻿using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using IoTAgentLib.Utils;
using Opc.Ua;
using Opc.UaFx;
using Opc.UaFx.Client;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;

namespace IoTAgentLib
{
    /// <summary>
    /// Singleton class for managing Azure Hub and production devices connection.
    /// </summary>
    public class IoTAgent
    {
        #region Properties
        /// <summary>
        /// IoTAgent's instance.
        /// </summary>
        private static IoTAgent Instance { get; set; } = null;

        /// <summary>
        /// OPC server's client.
        /// </summary>
        public OpcClient _opcClient { get; set; } = null;

        /// <summary>
        /// List of devices in system.
        /// </summary>
        public List<VirtualDevice> Devices { get; set; } = new List<VirtualDevice>();

        /// <summary>
        /// Flag for keeping information about OPC connection state.
        /// </summary>
        public bool IsConnected { get { return _opcClient != null; } }

        public event EventHandler ServerConnectedEvent;
        public event EventHandler DevicesLoadBeginEvent;
        public event EventHandler DevicesLoadedEvent;
        public event NewDeviceEventHandler NewDeviceEvent;


        #region Delegates
        public delegate void NewDeviceEventHandler(object sender, VirtualDevice vd);
        #endregion
        #endregion


        #region Constructor + Instance getter
        private IoTAgent() { }
        public static IoTAgent GetInstance()
        {
            if (Instance == null)
                Instance = new IoTAgent();
            return Instance;
        }
        #endregion


        #region Methods
        /// <summary>
        /// Connects with OPC server by creating new client.
        /// </summary>
        /// <param name="address">OPC server's address.</param>
        /// <returns><c>null</c> if succeeded and error message as string if failed.</returns>
        public async Task<Exception?> ConnectWithServer(string address)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    _opcClient = new OpcClient(address);
                    _opcClient.Connect();
                    ServerConnectedEvent?.Invoke(this, EventArgs.Empty);
                    LoadUpDeviceNodes();
                    return null;
                }
                catch (Exception exc)
                {
                    await Console.Out.WriteLineAsync(exc.Message);
                    return exc;
                }
            });
        }

        /// <summary>
        /// Loads up devices found on OPC server and sets it's OPC subscriptions.
        /// Assumes nodes with devices have "Device" in name.
        /// </summary>
        public void LoadUpDeviceNodes()
        {
            DevicesLoadBeginEvent?.Invoke(this, EventArgs.Empty);

            var node = _opcClient.BrowseNode(OpcObjectTypes.ObjectsFolder);

            foreach (var childNode in node.Children())
            {
                if (childNode.DisplayName.Value.Contains("Device"))
                    if (Config.DEVICES_CONNECTION_STRINGS.ContainsKey(childNode.DisplayName.Value))
                        Devices.Add(CreateDeviceFromNode(childNode));
            }

            DevicesLoadedEvent?.Invoke(this, EventArgs.Empty);
            AssociateIoTHubDevices();
        }

        public async Task<Exception?> AddNewDevice(string nodeName, string azureConnectionString)
        {
            try
            {
                var node = _opcClient.BrowseNode(OpcObjectTypes.ObjectsFolder);

                foreach (var childNode in node.Children())
                {
                    if (childNode.DisplayName.Value.Contains(nodeName))
                    {
                        VirtualDevice newDevice = CreateDeviceFromNode(childNode);

                        Devices.Add(newDevice);
                        NewDeviceEvent?.Invoke(this, newDevice);

                        newDevice.DeviceClient = DeviceClient.CreateFromConnectionString(azureConnectionString);
                        newDevice.NotifyOfAzureClientStateChange();

                        _ = newDevice.DeviceClient.SetMethodHandlerAsync("EmergencyStop", newDevice.EmergencyStopMethodHandler, newDevice.DeviceClient);
                        _ = newDevice.DeviceClient.SetMethodHandlerAsync("ResetErrorStatus", newDevice.ResetErrorStatusMethodHandler, newDevice.DeviceClient);

                        Exception? res = Config.AddNewDeviceEntry(nodeName, azureConnectionString);
                        if (res != null) return res;

                        return null;
                    }
                }
                return new Exception($"Could not find node with display name '{nodeName}'");
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                return exc;
            }
        }

        private VirtualDevice CreateDeviceFromNode(OpcNodeInfo nodeInfo)
        {
            VirtualDevice newDevice = new VirtualDevice(nodeInfo.NodeId);
            newDevice.DisplayName = nodeInfo.DisplayName;

            newDevice.ProductionStatusSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/ProductionStatus", newDevice.HandleProductionStatusChanged);
            newDevice.WorkorderIdSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/WorkorderId", newDevice.HandleWorkorderIdChanged);
            newDevice.ProductionRateSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/ProductionRate", newDevice.HandleProductionRateChanged);
            newDevice.GoodCountSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/GoodCount", newDevice.HandleGoodCountChanged);
            newDevice.BadCountSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/BadCount", newDevice.HandleBadCountChanged);
            newDevice.TemperatureSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/Temperature", newDevice.HandleTemperatureChanged);
            newDevice.DeviceErrorsSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/DeviceError", newDevice.HandleDeviceErrorsChanged);

            return newDevice;
        }

        /// <summary>
        /// Checks config file for devices' connection strings
        /// and creates new Azure devices client for them.
        /// </summary>
        public void AssociateIoTHubDevices()
        {
            foreach (VirtualDevice virtualDevice in Devices)
            {
                virtualDevice.DeviceClient = DeviceClient.CreateFromConnectionString(Config.DEVICES_CONNECTION_STRINGS[virtualDevice.DisplayName]);
                virtualDevice.NotifyOfAzureClientStateChange();

                _ = virtualDevice.DeviceClient.SetMethodHandlerAsync("EmergencyStop", virtualDevice.EmergencyStopMethodHandler, virtualDevice.DeviceClient);
                _ = virtualDevice.DeviceClient.SetMethodHandlerAsync("ResetErrorStatus", virtualDevice.ResetErrorStatusMethodHandler, virtualDevice.DeviceClient);
                _ = virtualDevice.DeviceClient.SetDesiredPropertyUpdateCallbackAsync(virtualDevice.TwinPropertyChangedHandler, virtualDevice.DeviceClient);
            }
        }

        /// <summary>
        /// Sets new production rate for given device.
        /// </summary>
        /// <param name="device">Device in which production rate has to be changed.</param>
        /// <param name="newRate">New production rate value.</param>
        /// <returns><c>OpcStatus</c></returns>
        public OpcStatus SetProductionRateInDevice(VirtualDevice device, short newRate)
        {
            return _opcClient.WriteNode(device.NodeId + "/ProductionRate", Convert.ToInt32(newRate));
        }

        /// <summary>
        /// Executes emergency stop method for given device.
        /// </summary>
        /// <param name="device"></param>
        public void PerformEmergencyStop(VirtualDevice device)
        {
            _opcClient.CallMethod(
                device.NodeId.ToString(),
                device.NodeId.ToString() + "/EmergencyStop"
                );
        }

        /// <summary>
        /// Resets error codes for given device.
        /// </summary>
        /// <param name="device"></param>
        public void ResetErrorStatus(VirtualDevice device)
        {
            _opcClient.CallMethod(
                device.NodeId.ToString(),
                device.NodeId.ToString() + "/ResetErrorStatus"
                );
        }
        #endregion
    }
}
