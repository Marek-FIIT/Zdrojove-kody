using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;

using SharpPcap;
using PacketDotNet;


namespace PSIP_Switch
{
    public class Switch
    {
        private GUI form;

        public Port[] Ports;
        public CamTable CAM = new CamTable();

        public Dictionary<Port, PortStatistic> Statistics = new Dictionary<Port, PortStatistic>();
        public Dictionary<Port, DateTime> PortUsage = new Dictionary<Port, DateTime>();

        public Dictionary<Port, FilterRuleSet>  InboundFilterRules = new Dictionary<Port, FilterRuleSet>();
        public Dictionary<Port, FilterRuleSet> OutboundFilterRules = new Dictionary<Port, FilterRuleSet>();


        public class CamTable
        {
            public int Timer;
            public Dictionary<string, CamEntry> Entries = new Dictionary<string, CamEntry>();

            public class CamEntry
            {
                public Port Port;
                public int  Timer;

                public CamEntry(Port port, int timer)
                {
                    this.Port  = port;
                    this.Timer = timer;
                }
            }

            public Port LookUp(string dstMAC)
            {
                lock (this.Entries)
                {
                    return this.Entries.ContainsKey(dstMAC) ? this.Entries[dstMAC].Port : null;
                }
            }

            public void Add(string srcMAC, Port port)
            {
                lock (this.Entries)
                { 
                    if (this.Entries.ContainsKey(srcMAC))
                        this.Entries[srcMAC] = new CamEntry(port, this.Timer);
                    else
                        this.Entries.Add(srcMAC.ToString(), new CamEntry(port, this.Timer));
                }
            }
        }

        public class FilterRuleSet
        {
            public List<FilterRule> Rules = new List<FilterRule>();

            public bool CheckPermission(PacketInfo packetInfo)
            {
                FilterRule[][] PrioritySubSets = new FilterRule[3][]
                {
                    this.Rules.Where(rule => (rule.IPsrc != null || rule.MACsrc != null) && (rule.IPdst != null || rule.MACdst != null))
                              .Where(rule =>  rule.MatchingAddresses(packetInfo)).ToArray(),
                    
                    this.Rules.Where(rule => (rule.IPsrc == null && rule.MACsrc == null) ^  (rule.IPdst == null && rule.MACdst == null))
                              .Where(rule =>  rule.MatchingAddresses(packetInfo)).ToArray(),
                    
                    this.Rules.Where(rule =>  rule.IPsrc == null && rule.MACsrc == null  &&  rule.IPdst == null && rule.MACdst == null)
                              .ToArray()
                };


                foreach (FilterRule[] rules in PrioritySubSets)
                {
                    try
                    {
                        return rules.Where( rule => rule.srcPort != -1 && rule.dstPort != -1)
                                    .Where( rule => rule.MatchingPorts(packetInfo))
                                    .Select(rule => rule.Permission).First();
                    }
                    catch { }

                    try
                    {
                        return rules.Where( rule => rule.srcPort == -1 ^ rule.dstPort == -1)
                                    .Where( rule => rule.MatchingPorts(packetInfo))
                                    .Select(rule => rule.Permission).First();
                    }
                    catch { }

                    try
                    {
                        return rules.Where( rule => rule.srcPort == -1 && rule.dstPort == -1)
                                    .Select(rule => rule.Permission).First();
                    }
                    catch { }
                }

                return true;
            }
        }

        public class FilterRule
        {
            public enum EFilterProtocols { TCP, UDP, ICMP };

            public string MACsrc   = null;
            public string MACdst   = null;

            public string IPsrc    = null;
            public string IPdst    = null;

            public EFilterProtocols Protocol;

            public int    srcPort;
            public int    dstPort;

            public bool Permission;

        
            public bool MatchingAddresses(PacketInfo packetInfo)
            {
                if ((this.MACdst != null && this.MACsrc != null && packetInfo.PacketMACdst == this.MACdst && packetInfo.PacketMACsrc == this.MACsrc) ||
                    (this.MACdst != null && this.IPsrc  != null && packetInfo.PacketMACdst == this.MACdst && packetInfo.PacketIPsrc  == this.IPsrc)  ||
                    (this.IPdst  != null && this.MACsrc != null && packetInfo.PacketIPdst  == this.IPdst  && packetInfo.PacketMACsrc == this.MACsrc) ||
                    (this.IPdst  != null && this.IPsrc  != null && packetInfo.PacketIPdst  == this.IPdst  && packetInfo.PacketIPsrc  == this.IPsrc)  || 
                    
                    (this.MACdst != null && this.IPsrc == null && this.MACsrc == null && packetInfo.PacketMACdst == this.MACdst) ||
                    (this.MACsrc != null && this.IPdst == null && this.MACdst == null && packetInfo.PacketMACsrc == this.MACsrc) ||
                    (this.IPdst  != null && this.IPsrc == null && this.MACsrc == null && packetInfo.PacketIPdst  == this.IPdst)  ||
                    (this.IPsrc  != null && this.IPdst == null && this.MACdst == null && packetInfo.PacketIPsrc  == this.IPsrc)  ||

                    (this.MACdst == null && this.IPdst == null && this.MACsrc == null && this.IPsrc == null))
                    return true;
                    
                return false;
            }

            public bool MatchingPorts(PacketInfo packetInfo)
            {
                if (this.Protocol != packetInfo.FilterProtocol)
                    return false;

                if ((this.dstPort != -1 && this.srcPort != -1 && this.dstPort == packetInfo.PacketPortdst && this.srcPort == packetInfo.PacketPortsrc) ||
                    (this.dstPort != -1 && this.srcPort == -1 && this.dstPort == packetInfo.PacketPortdst)                                             ||
                    (this.srcPort != -1 && this.dstPort == -1 && this.srcPort == packetInfo.PacketPortsrc)                                             ||
                    (this.dstPort == -1 && this.srcPort == -1))
                    return true;


                return false;
            }
        }

        public class PacketInfo
        {
            public Packet Packet;

            public string PacketMACsrc = null;
            public string PacketMACdst = null;
             
            public string PacketIPsrc  = null;
            public string PacketIPdst  = null;
             
            public int    PacketPortsrc = -1;
            public int    PacketPortdst = -1;


            public FilterRule.EFilterProtocols FilterProtocol;

            public PortStatistic.EProtocols StatisticsProtocol;
        }
        

        public Switch(string portType, GUI form) 
        {
            this.form = form;

            this.CAM.Timer = (int)this.form.numTimer.Value;

            this.Ports = CaptureDeviceList.Instance.Where (device => device.Description == portType)
                                                   .Select(device => new Port(device, this)).ToArray();

            foreach (Port port in this.Ports)
            {
                if (port.adapter.Name == "\\Device\\NPF_{C85A8B75-1F60-48B5-ABDC-64E12632A594}")
                    port.name = "Ethernet2";
                if (port.adapter.Name == "\\Device\\NPF_{682E108E-17C4-4AFA-838E-F45BB4B2786E}")
                    port.name = "Ethernet3";

                this.Statistics.Add(port, new PortStatistic(port.name));
                this.PortUsage. Add(port, DateTime.Now);

                this. InboundFilterRules.Add(port, new FilterRuleSet());
                this.OutboundFilterRules.Add(port, new FilterRuleSet());
            }
        }


        public void Start()
        {
            foreach (Port port in this.Ports)
                port.turn_on();
        }

        public void Stop()
        {
            foreach (Port port in this.Ports)
                port.turn_off();
        }


        public void process_packet(Packet rawPacket, Port port)
        {
            int TypeLength = rawPacket.Bytes[12] * 256 + rawPacket.Bytes[13];

            bool isEthernet = TypeLength > 1536 ? true : false;

            PacketInfo PacketInfo;
                                          //LLDP                   LOOP                   IPv6                   ????
            if (isEthernet && TypeLength != 35020 && TypeLength != 36864 && TypeLength != 34525 && TypeLength != 24578)
            {
                ArpPacket arpPacket;
                IPPacket   ipPacket;
                if ((ipPacket = rawPacket.Extract<IPPacket>()) != null)
                {
                    UdpPacket udpTmp;
                    if ((udpTmp = ipPacket.Extract<UdpPacket>()) != null)
                                                  //DHCP                          DHCP                            SSDP                              Browser                          NBSN                             LLMNR                             MDNS                              ????
                        if (udpTmp.DestinationPort == 68 || udpTmp.DestinationPort == 67 || udpTmp.DestinationPort == 1900 || udpTmp.DestinationPort == 138 || udpTmp.DestinationPort == 137 || udpTmp.DestinationPort == 5355 || udpTmp.DestinationPort == 5353 || udpTmp.DestinationPort == 3702)
                            return;
                }


                this.PortUsage[port] = DateTime.Now;
                PacketInfo = new PacketInfo() { Packet = rawPacket };

                var EthernetPacket = rawPacket.Extract<EthernetPacket>();
                PacketInfo.StatisticsProtocol = PortStatistic.EProtocols.Ethernet;

                PacketInfo.PacketMACsrc = EthernetPacket.SourceHardwareAddress.ToString();
                PacketInfo.PacketMACdst = EthernetPacket.DestinationHardwareAddress.ToString();


                if (ipPacket != null)
                    this.process_packet_ip(ipPacket, PacketInfo);
                else if ((arpPacket = rawPacket.Extract<ArpPacket>()) != null)
                    this.process_packet_arp(arpPacket, PacketInfo);


                if (this.InboundFilterRules[port].CheckPermission(PacketInfo))
                {
                    this.CAM.Add(PacketInfo.PacketMACsrc, port);

                    switch (PacketInfo.StatisticsProtocol)
                    {
                        case PortStatistic.EProtocols.Ethernet:
                            this.Statistics[port].protocols[PortStatistic.EProtocols.Ethernet].IN++;
                            break;
                        case PortStatistic.EProtocols.ARP:
                            this.Statistics[port].protocols[PortStatistic.EProtocols.Ethernet].IN++;
                            this.Statistics[port].protocols[PortStatistic.EProtocols.ARP].IN++;
                            break;
                        case PortStatistic.EProtocols.ICMP:
                            this.Statistics[port].protocols[PortStatistic.EProtocols.Ethernet].IN++;
                            this.Statistics[port].protocols[PortStatistic.EProtocols.ICMP].IN++;
                            break;
                        case PortStatistic.EProtocols.UDP:
                            this.Statistics[port].protocols[PortStatistic.EProtocols.Ethernet].IN++;
                            this.Statistics[port].protocols[PortStatistic.EProtocols.UDP].IN++;
                            break;
                        case PortStatistic.EProtocols.TCP:
                            this.Statistics[port].protocols[PortStatistic.EProtocols.Ethernet].IN++;
                            this.Statistics[port].protocols[PortStatistic.EProtocols.TCP].IN++;
                            break;
                        case PortStatistic.EProtocols.HTTP:
                            this.Statistics[port].protocols[PortStatistic.EProtocols.Ethernet].IN++;
                            this.Statistics[port].protocols[PortStatistic.EProtocols.TCP].IN++;
                            this.Statistics[port].protocols[PortStatistic.EProtocols.HTTP].IN++;
                            break;
                    }

                    this.send_packet(PacketInfo, port);
                }
            }
        }

        #region Layer 3
        private void process_packet_arp(ArpPacket packet, PacketInfo packetInfo)
        {
            packetInfo.StatisticsProtocol    = PortStatistic.EProtocols.ARP;

            packetInfo.PacketIPsrc = packet.SenderProtocolAddress.ToString();
            packetInfo.PacketIPdst = packet.TargetProtocolAddress.ToString();
        }

        private void process_packet_ip(IPPacket packet, PacketInfo packetInfo)
        {
            TcpPacket     tcpPacket;
            UdpPacket     udpPacket;
            IcmpV4Packet icmpPacket;

            packetInfo.PacketIPsrc = packet.SourceAddress.ToString();
            packetInfo.PacketIPdst = packet.DestinationAddress.ToString();

            if      ((tcpPacket  = packet.Extract<TcpPacket>())    != null)
                this.process_packet_tcp (tcpPacket, packetInfo);
            else if ((udpPacket  = packet.Extract<UdpPacket>())    != null)
                this.process_packet_udp (udpPacket, packetInfo);
            else if ((icmpPacket = packet.Extract<IcmpV4Packet>()) != null)
                this.process_packet_icmp(icmpPacket, packetInfo);
        }

        private void process_packet_icmp(IcmpV4Packet packet, PacketInfo packetInfo)
        {
            packetInfo.StatisticsProtocol = PortStatistic.EProtocols.ICMP;
            packetInfo.FilterProtocol     = FilterRule.EFilterProtocols.ICMP;

            packetInfo.PacketPortdst      = (int)packet.TypeCode >> 8;
            packetInfo.PacketPortsrc      = -1;
        }
        #endregion

        #region Layer 4
        private void process_packet_tcp(TcpPacket packet, PacketInfo packetInfo)
        {
            packetInfo.StatisticsProtocol = PortStatistic.EProtocols.TCP;
            packetInfo.FilterProtocol     = FilterRule.EFilterProtocols.TCP;
            
            packetInfo.PacketPortdst = packet.DestinationPort;
            packetInfo.PacketPortsrc = packet.SourcePort;


            if (packetInfo.PacketPortdst == 80 || packetInfo.PacketPortsrc == 80)
                this.process_packet_http(packet, packetInfo);
            else if (packetInfo.PacketPortdst == 443 || packetInfo.PacketPortsrc == 443)
                this.process_packet_https(packet, packetInfo);
        }

        private void process_packet_udp(UdpPacket packet, PacketInfo packetInfo)
        {
            packetInfo.StatisticsProtocol = PortStatistic.EProtocols.UDP;
            packetInfo.FilterProtocol     = FilterRule.EFilterProtocols.UDP;

            packetInfo.PacketPortdst = packet.DestinationPort;
            packetInfo.PacketPortsrc = packet.SourcePort;
        }
        #endregion

        #region Layer > 4
        private void process_packet_http(TcpPacket packet, PacketInfo packetInfo)
        {
            packetInfo.StatisticsProtocol = PortStatistic.EProtocols.HTTP;
        }

        private void process_packet_https(TcpPacket packet, PacketInfo packetInfo)
        {
            packetInfo.StatisticsProtocol = PortStatistic.EProtocols.HTTPS;
        }
        #endregion


        public void send_packet(PacketInfo PacketInfo, Port receivingPort)
        {
            Port CAMPort = this.CAM.LookUp(PacketInfo.PacketMACdst);

            if (CAMPort == receivingPort)
                return;

            Port[] sendPorts = CAMPort != null ? new Port[] { CAMPort } : this.Ports.Where(port => port != receivingPort)
                                                                                    .Where(port => this.OutboundFilterRules[port].CheckPermission(PacketInfo))
                                                                                    .ToArray();

            foreach (Port sendPort in sendPorts)
            { 
                switch (PacketInfo.StatisticsProtocol)
                {
                    case PortStatistic.EProtocols.Ethernet:
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.Ethernet].OUT++;
                        break;
                    case PortStatistic.EProtocols.ARP:
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.Ethernet].OUT++;
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.ARP]     .OUT++;
                        break;
                    case PortStatistic.EProtocols.ICMP:
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.Ethernet].OUT++;
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.ICMP]    .OUT++;
                        break;
                    case PortStatistic.EProtocols.UDP:
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.Ethernet].OUT++;
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.UDP]     .OUT++;
                        break;
                    case PortStatistic.EProtocols.TCP:
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.Ethernet].OUT++;
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.TCP]     .OUT++;
                        break;
                    case PortStatistic.EProtocols.HTTP:
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.Ethernet].OUT++;
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.TCP]     .OUT++;
                        this.Statistics[sendPort].protocols[PortStatistic.EProtocols.HTTP]    .OUT++;
                        break;
                }

                sendPort.send_packet(PacketInfo.Packet);
            }
        }
    }
}
