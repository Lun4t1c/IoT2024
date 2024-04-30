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
        public OpcSubscription ProductionRateSubscription { get; set; }
        #endregion


        #region Events
        public event EventHandler ProductionStateChangedEvent;
        public event EventHandler ProductionRateChangedEvent;
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

        public void HandleProductionRateChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            OpcMonitoredItem item = (OpcMonitoredItem)sender;
            ProductionRate = Convert.ToInt16(e.Item.Value.Value);
            ProductionRateChangedEvent?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
