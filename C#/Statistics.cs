using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpPcap;
using PacketDotNet;


namespace PSIP_Switch
{
    public class PortStatistic
    {
        public string portName;
        public ProtocolInOut total;

        public enum EProtocols { Ethernet, IP, ARP, TCP, UDP, ICMP, HTTP, HTTPS };
        public Dictionary<EProtocols, ProtocolInOut> protocols = new Dictionary<EProtocols, ProtocolInOut>();


        public PortStatistic(string portName)
        {
            this.portName = portName;
            this.total = new ProtocolInOut();

            foreach (EProtocols protocol in Enum.GetValues(typeof(EProtocols)))
                this.protocols.Add(protocol, new ProtocolInOut());
        }

        public class ProtocolInOut
        {
            public int IN;
            public int OUT;

            public ProtocolInOut()
            {
                int IN  = 0;
                int OUT = 0;
            }
        }
    }
}
