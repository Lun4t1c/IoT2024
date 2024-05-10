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
    public class VirtualDevice
    {
        #region Properties
        public DeviceClient DeviceClient { get; set; } = null;
        public string DisplayName { get; set; } = "";

        public OpcNodeId NodeId { get; set; } = "";
        public bool ProductionStatus { get; set; } = false;
        public Guid WorkorderId { get; set; } = Guid.Empty;
        public short ProductionRate { get; set; } = 0;
        public uint GoodCount { get; set; } = 0;
        public uint BadCount { get; set; } = 0;
        public short Temperature { get; set; } = 0;
        public byte DeviceError { get; set; } = 0000;
        #endregion


        #region Subscriptions
        public OpcSubscription ProductionStatusSubscription { get; set; }
        public OpcSubscription WorkorderIdSubscription { get; set; }
        public OpcSubscription ProductionRateSubscription { get; set; }
        public OpcSubscription GoodCountSubscription { get; set; }
        public OpcSubscription BadCountSubscription { get; set; }
        public OpcSubscription TemperatureSubscription { get; set; }
        public OpcSubscription DeviceErrorsSubscription { get; set; }
        #endregion


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
        public async Task DeviceToCloudMessage(string propertyName, object propertyValue)
        {
            Message eventMessage = new Message();
            eventMessage.ContentType = MediaTypeNames.Application.Json;
            eventMessage.ContentEncoding = "utf-8";
            eventMessage.Properties.Add(propertyName, propertyValue.ToString());

            if (DeviceClient != null)
                await DeviceClient.SendEventAsync(eventMessage);
        }

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

            ProductionStateChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void HandleWorkorderIdChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            WorkorderId = Guid.Parse(e.Item.Value.Value.ToString());

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

            GoodCountChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void HandleBadCountChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            BadCount = Convert.ToUInt32(e.Item.Value.Value);

            BadCountChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void HandleTemperatureChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            Temperature = Convert.ToInt16(e.Item.Value.Value);

            TemperatureChangedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void HandleDeviceErrorsChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            DeviceError = Convert.ToByte(e.Item.Value.Value);
            _ = UpdateTwinPropertyAsync(nameof(DeviceError), DeviceError);
            _ = DeviceToCloudMessage(nameof(DeviceError), DeviceError);

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