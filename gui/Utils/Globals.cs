using IoTAgentLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gui.Utils
{
    public static class Globals
    {
        public static IoTAgent IoTAgent { get; set; } = IoTAgent.GetInstance();
    }
}
