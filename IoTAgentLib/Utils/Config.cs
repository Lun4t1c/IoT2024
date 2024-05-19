using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTAgentLib.Utils
{
    public static class Config
    {
        public static readonly string BLOB_CONNECTION_STRING;
        public static readonly string IOT_HUB_CONNECTION_STRING;

        static Config()
        {
            var lines = File.ReadLines("config.ini");
            foreach (var line in lines)
            {
                string[] keyValue = line.Split(" = ");
                switch (keyValue[0])
                {
                    case "BLOB_CONNECTION_STRING":
                        BLOB_CONNECTION_STRING = keyValue[1];
                        break;

                    case "IOT_HUB_CONNECTION_STRING":
                        IOT_HUB_CONNECTION_STRING = keyValue[1];
                        break;
                }
            }
        }
    }
}
