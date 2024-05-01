using Opc.Ua;
using Opc.UaFx;
using Opc.UaFx.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTAgentLib.Utils
{
    public class VirtualDevice
    {
        #region Properties
        public OpcNodeId NodeId { get; set; } = "";
        public bool ProductionStatus { get; set; } = false;
        public Guid WorkorderId { get; set; } = Guid.Empty;
        public short ProductionRate { get; set; } = 0;
        public uint GoodCount { get; set; } = 0;
        public uint BadCount { get; set; } = 0;
        public short Temperature { get; set; } = 0;
        public byte DeviceErrors { get; set; } = 0000;
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
        #endregion


        #region Constructor
        public VirtualDevice(OpcNodeId nodeId)
        {
            NodeId = nodeId;
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
            DeviceErrors = Convert.ToByte(e.Item.Value.Value);
            DeviceErrorsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}