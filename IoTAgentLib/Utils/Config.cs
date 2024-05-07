using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTAgentLib.Utils
{
    public static class Config
    {
        public static string BLOB_CONNECTION_STRING = "DefaultEndpointsProtocol=https;AccountName=zajeciaiot;AccountKey=CU1oHBri/il5DB6XsngYdiyoUfDZBVVVjzP9AbfDNrz74CQVFIUQ7xSzzB5uhhSDCvzSmC/FAGiz+AStl4xfzQ==;EndpointSuffix=core.windows.net";
        public static string IOT_HUB_CONNECTION_STRING { get; set; } = "HostName=maciek.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=3nBZVKGKMDN9EeZH3hKdHCxERRyIAMXBRAIoTGZkDuI=";
    }
}
