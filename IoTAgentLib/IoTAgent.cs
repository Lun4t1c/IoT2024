using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using IoTAgentLib.Utils;
using Opc.Ua;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace IoTAgentLib
{
    public class IoTAgent
    {
        public OpcClient _opcClient { get; set; } = null;
        public List<VirtualDevice> Devices { get; set; } = new List<VirtualDevice>();

        public bool IsConnected { get { return _opcClient != null; } }

        public event EventHandler ServerConnectedEvent;
        public event EventHandler DevicesLoadedEvent;

        public IoTAgent()
        {
        }

        public async Task<string?> ConnectWithServer(string address)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    _opcClient = new OpcClient(address);
                    _opcClient.Connect(); // Run Connect asynchronously
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

        public void LoadUpDeviceNodes()
        {
            var node = _opcClient.BrowseNode(OpcObjectTypes.ObjectsFolder);

            foreach (var childNode in node.Children())
            {
                if (childNode.DisplayName.Value.Contains("Device"))
                {
                    VirtualDevice newDevice = new VirtualDevice(childNode.NodeId);
                    
                    newDevice.ProductionStatusSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/ProductionStatus", newDevice.HandleProductionStatusChanged);
                    newDevice.WorkorderIdSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/WorkorderId", newDevice.HandleProductionStatusChanged);
                    newDevice.ProductionRateSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/ProductionRate", newDevice.HandleProductionRateChanged);
                    newDevice.GoodCountSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/GoodCount", newDevice.HandleGoodCountChanged);
                    newDevice.BadCountSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/BadCount", newDevice.HandleBadCountChanged);
                    newDevice.BadCountSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/Temperature", newDevice.HandleTemperatureChanged);
                    newDevice.DeviceErrorsSubscription = _opcClient.SubscribeDataChange(newDevice.NodeId + "/DeviceErrors", newDevice.HandleDeviceErrorsChanged);

                    Devices.Add(newDevice);
                }
            }

            DevicesLoadedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void AddDevice()
        {
            try
            {
                OpcReadNode[] commands = new OpcReadNode[] 
                {
                    new OpcReadNode("ns=2;s=Device 1/ProductionStatus", OpcAttribute.DisplayName),
                    new OpcReadNode("ns=2;s=Device 1/ProductionStatus"),
                    new OpcReadNode("ns=2;s=Device 1/ProductionRate", OpcAttribute.DisplayName),
                    new OpcReadNode("ns=2;s=Device 1/ProductionRate"),
                    new OpcReadNode("ns=2;s=Device 1/WorkorderId", OpcAttribute.DisplayName),
                    new OpcReadNode("ns=2;s=Device 1/WorkorderId"),
                    new OpcReadNode("ns=2;s=Device 1/Temperature", OpcAttribute.DisplayName),
                    new OpcReadNode("ns=2;s=Device 1/Temperature"),
                    new OpcReadNode("ns=2;s=Device 1/GoodCount", OpcAttribute.DisplayName),
                    new OpcReadNode("ns=2;s=Device 1/GoodCount"),
                    new OpcReadNode("ns=2;s=Device 1/BadCount", OpcAttribute.DisplayName),
                    new OpcReadNode("ns=2;s=Device 1/BadCount"),
                    new OpcReadNode("ns=2;s=Device 1/DeviceError", OpcAttribute.DisplayName),
                    new OpcReadNode("ns=2;s=Device 1/DeviceError"),
                };
                
                IEnumerable<OpcValue> job = _opcClient.ReadNodes(commands);
                
                foreach (var item in job)
                {
                    Console.WriteLine(item.Value);
                }
                
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        public void PerformEmergencyStop(VirtualDevice device)
        {
            _opcClient.CallMethod(
                device.NodeId.ToString(),
                device.NodeId.ToString() + "/EmergencyStop"
                );
        }

        public void ResetErrorStatus(VirtualDevice device)
        {
            _opcClient.CallMethod(
                device.NodeId.ToString(),
                device.NodeId.ToString() + "/ResetErrorStatus"
                );
        }
    }
}
