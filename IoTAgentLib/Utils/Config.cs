using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IoTAgentLib.Utils
{
    public static class Config
    {
        public static readonly string CONFIG_FILE_PATH = @"config.json";
        public static readonly string BLOB_CONNECTION_STRING;
        public static readonly Dictionary<string, string> DEVICES_CONNECTION_STRINGS;

        public class ConfigHelperClass
        {
            public string BLOB_CONNECTION_STRING { get; set; }
            public Dictionary<string, string> DEVICES_CONNECTION_STRINGS { get; set; }
        }

        static Config()
        {
            string jsonText = File.ReadAllText(CONFIG_FILE_PATH);

            ConfigHelperClass config = JsonConvert.DeserializeObject<ConfigHelperClass>(jsonText);

            BLOB_CONNECTION_STRING = config.BLOB_CONNECTION_STRING;
            DEVICES_CONNECTION_STRINGS = config.DEVICES_CONNECTION_STRINGS;
        }
    }
}
