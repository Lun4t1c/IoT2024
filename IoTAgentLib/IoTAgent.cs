using System.Net.Sockets;
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
        public async Task<string?> ConnectWithServer(string address)
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
                    return exc.Message;
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
                {
                    VirtualDevice newDevice = new VirtualDevice(childNode.NodeId);
                    newDevice.DisplayName = childNode.DisplayName;

                    newDevice.ProductionStatusSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/ProductionStatus", newDevice.HandleProductionStatusChanged);
                    newDevice.WorkorderIdSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/WorkorderId", newDevice.HandleWorkorderIdChanged);
                    newDevice.ProductionRateSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/ProductionRate", newDevice.HandleProductionRateChanged);
                    newDevice.GoodCountSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/GoodCount", newDevice.HandleGoodCountChanged);
                    newDevice.BadCountSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/BadCount", newDevice.HandleBadCountChanged);
                    newDevice.TemperatureSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/Temperature", newDevice.HandleTemperatureChanged);
                    newDevice.DeviceErrorsSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/DeviceError", newDevice.HandleDeviceErrorsChanged);

                    Devices.Add(newDevice);
                }
            }

            DevicesLoadedEvent?.Invoke(this, EventArgs.Empty);
            AssociateIoTHubDevices();
        }

        /// <summary>
        /// Checks Azure IoT hub for list of devices
        /// and creates new Azure device client
        /// for every found match with production device.
        /// </summary>
        public async void AssociateIoTHubDevices()
        {
            var registryManager = RegistryManager.CreateFromConnectionString(Utils.Config.IOT_HUB_CONNECTION_STRING);
            var devices = await registryManager.GetDevicesAsync(int.MaxValue);

            foreach (var azureDevice in devices)
            {
                var deviceDetails = await registryManager.GetDeviceAsync(azureDevice.Id);
                string deviceConnectionString = $"HostName=maciek.azure-devices.net;DeviceId={azureDevice.Id};SharedAccessKey={deviceDetails.Authentication.SymmetricKey.PrimaryKey}";

                foreach (VirtualDevice virtualDevice in Devices)
                {
                    if (azureDevice.Id == virtualDevice.DisplayName.Replace(" ", ""))
                    {
                        virtualDevice.DeviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);
                        virtualDevice.NotifyOfAzureClientStateChange();

                        _ = virtualDevice.DeviceClient.SetMethodHandlerAsync("EmergencyStop", virtualDevice.EmergencyStopMethodHandler, virtualDevice.DeviceClient);
                        _ = virtualDevice.DeviceClient.SetMethodHandlerAsync("ResetErrorStatus", virtualDevice.ResetErrorStatusMethodHandler, virtualDevice.DeviceClient);
                    }
                }
            }

            await registryManager.CloseAsync();
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
