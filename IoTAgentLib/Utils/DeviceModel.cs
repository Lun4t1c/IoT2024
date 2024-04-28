using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTAgentLib.Utils
{
    public class DeviceModel
    {
        #region Properties
        public bool ProductionStatus { get; set; }
        public Guid WorkorderId { get; set; }
        public short ProductionRate { get; set; }
        public uint GoodCount { get; set; }
        public uint BadCount { get; set; }
        public short Temperature { get; set; }
        public byte DeviceErrors { get; set; } = 0000;
        #endregion

        public DeviceModel()
        {
            
        }

        public void PerformEmergencyStop()
        {
            throw new NotImplementedException();
        }

        public void ResetErrorStatus()
        {
            throw new NotImplementedException();
        }
    }
}
