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
        public OpcNodeId NodeId { get; set; }
        public bool ProductionStatus { get; set; }
        public Guid WorkorderId { get; set; }
        public short ProductionRate { get; set; }
        public uint GoodCount { get; set; }
        public uint BadCount { get; set; }
        public short Temperature { get; set; }
        public byte DeviceErrors { get; set; } = 0000;
        #endregion


        #region Subscriptions
        public OpcSubscription ProductionStatusSubscription { get; set; }
        #endregion


        #region Events
        public event EventHandler ProductionStateChangedEvent;
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
        #endregion
    }
}
