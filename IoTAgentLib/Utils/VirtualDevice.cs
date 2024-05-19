using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Opc.Ua;
using Opc.UaFx;
using Opc.UaFx.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace IoTAgentLib.Utils
{
    /// <summary>
    /// Class representing single production line in factory.
    /// </summary>
    public class VirtualDevice
    {
        #region Properties
        /// <summary>
        /// Device client from Microsoft's Azure package, which connects VirtualDevice
        /// with device from Azure's IoT hub.
        /// </summary>
        public DeviceClient DeviceClient { get; set; } = null;

        /// <summary>
        /// Devices name, which is to be displayed as string. Also serves as Azure IoT hub's device ID.
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// OPC server node's ID from node which represents this device
        /// </summary>
        public OpcNodeId NodeId { get; set; } = "";

        /// <summary>
        /// Represents if device line is running
        /// </summary>
        public bool ProductionStatus { get; set; } = false;

        /// <summary>
        /// Device's current workorder ID
        /// </summary>
        public Guid WorkorderId { get; set; } = Guid.Empty;

        /// <summary>
        /// Device's current production rate
        /// </summary>
        public short ProductionRate { get; set; } = 0;

        /// <summary>
        /// Device's current good count
        /// </summary>
        public uint GoodCount { get; set; } = 0;

        /// <summary>
        /// Device's current bad count
        /// </summary>
        public uint BadCount { get; set; } = 0;

        /// <summary>
        /// Device's current temperature
        /// </summary>
        public double Temperature { get; set; } = 0;

        /// <summary>
        /// Device's current errors
        /// </summary>
        public byte DeviceError { get; set; } = 0000;
        #endregion


        /// <summary>
        /// Device's Opc.UaFx.Client's subsriptions for all properties
        /// </summary>
        #region Subscriptions
        public OpcSubscription ProductionStatusSubscription { get; set; }
        public OpcSubscription WorkorderIdSubscription { get; set; }
        public OpcSubscription ProductionRateSubscription { get; set; }
        public OpcSubscription GoodCountSubscription { get; set; }
        public OpcSubscription BadCountSubscription { get; set; }
        public OpcSubscription TemperatureSubscription { get; set; }
        public OpcSubscription DeviceErrorsSubscription { get; set; }
        #endregion


        /// <summary>
        /// Device's event handlers which can be used in external projects to invoke assigned methods
        /// </summary>
        #region Events
        public event EventHandler ProductionStateChangedEvent;
        public event EventHandler WorkorderIdChangedEvent;
        public event EventHandler ProductionRateChangedEvent;
        public event EventHandler GoodCountChangedEvent;
        public event EventHandler BadCountChangedEvent;
        public event EventHandler TemperatureChangedEvent;
        public event EventHandler DeviceErrorsChangedEvent;

        public event EventHandler AzureClientStateChangeEvent;
        #endregion


        #region Constructor
        public VirtualDevice(OpcNodeId nodeId)
        {
            NodeId = nodeId;
        }
        #endregion


        #region Methods
        /// <summary>
        /// Send D2C message with device's specified property and it's value.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        public async Task DeviceToCloudMessageProperty(string propertyName, object propertyValue)
        {
            Message eventMessage = new Message();
            eventMessage.ContentType = MediaTypeNames.Application.Json;
            eventMessage.ContentEncoding = "utf-8";
            eventMessage.Properties.Add("DeviceId", DisplayName.Replace(" ", ""));
            eventMessage.Properties.Add(propertyName, propertyValue.ToString());

            if (DeviceClient != null)
                await DeviceClient.SendEventAsync(eventMessage);
        }

        /// <summary>
        /// Update single device twin's property in cloud.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<string?> UpdateTwinPropertyAsync(string propertyName, object value)
        {
            if (DeviceClient == null) return null;

            try
            {
                var twin = DeviceClient.GetTwinAsync();

                TwinCollection reportedProperties = new TwinCollection();
                reportedProperties[propertyName] = value;

                await DeviceClient.UpdateReportedPropertiesAsync(reportedProperties);

                await Console.Out.WriteLineAsync($"Updated '{propertyName}' for '{DisplayName}' ({value})");
                return null;
            }
            catch (Exception exc)
            {
                await Console.Out.WriteLineAsync(exc.Message);
                return exc.Message;
            }
        }

        public void NotifyOfAzureClientStateChange()
        {
            AzureClientStateChangeEvent?.Invoke(this, EventArgs.Empty);
        }
        #endregion


        #region Event handlers
        public void HandleProductionStatusChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            ProductionStatus = Convert.ToBoolean(e.Item.Value.Value);
            _ = DeviceToCloudMessageProperty(nameof(ProductionStatus), ProductionStatus);

            ProductionStateChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void HandleWorkorderIdChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            WorkorderId = Guid.Parse(e.Item.Value.Value.ToString());
            _ = DeviceToCloudMessageProperty(nameof(WorkorderId), WorkorderId);

            WorkorderIdChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void HandleProductionRateChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            ProductionRate = Convert.ToInt16(e.Item.Value.Value);
            _ = UpdateTwinPropertyAsync(nameof(ProductionRate), ProductionRate);

            ProductionRateChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void HandleGoodCountChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            GoodCount = Convert.ToUInt32(e.Item.Value.Value);
            _ = DeviceToCloudMessageProperty(nameof(GoodCount), GoodCount);

            GoodCountChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void HandleBadCountChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            BadCount = Convert.ToUInt32(e.Item.Value.Value);
            _ = DeviceToCloudMessageProperty(nameof(BadCount), BadCount);

            BadCountChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void HandleTemperatureChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            Temperature = (double)e.Item.Value.Value;
            _ = DeviceToCloudMessageProperty(nameof(Temperature), Temperature);

            TemperatureChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void HandleDeviceErrorsChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            DeviceError = Convert.ToByte(e.Item.Value.Value);
            _ = UpdateTwinPropertyAsync(nameof(DeviceError), DeviceError);
            _ = DeviceToCloudMessageProperty(nameof(DeviceError), DeviceError);

            DeviceErrorsChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public async Task<MethodResponse> EmergencyStopMethodHandler(MethodRequest methodRequest, object userContext)
        {
            IoTAgent.GetInstance().PerformEmergencyStop(this);
            return new MethodResponse(0);
        }

        public async Task<MethodResponse> ResetErrorStatusMethodHandler(MethodRequest methodRequest, object userContext)
        {
            IoTAgent.GetInstance().ResetErrorStatus(this);
            return new MethodResponse(0);
        }
        #endregion
    }
}