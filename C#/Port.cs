using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpPcap;
using PacketDotNet;


namespace PSIP_Switch
{
    public class Port
    {
        public ICaptureDevice adapter { get; }

        private List<string> packetsToIgnore = new List<string>();
        public Switch parentSwitch;
        public string name;
        public bool ON = false;


        public Port(ICaptureDevice adapter, Switch parentSwitch, string name = "")
        {
            this.adapter = adapter;
            this.parentSwitch = parentSwitch;
            this.name = name;

            adapter.OnPacketArrival += new PacketArrivalEventHandler(this.device_OnPacketArrival);
        }

        private void device_OnPacketArrival(object sender, CaptureEventArgs packet)
        {
            Packet rawPacket = Packet.ParsePacket(packet.Packet.LinkLayerType, packet.Packet.Data);

            if (!this.packetsToIgnore.Remove(Encoding.UTF8.GetString(rawPacket.Bytes)))
                Task.Run(() => this.parentSwitch.process_packet(rawPacket, this));
        }

        public void send_packet(Packet packet)
        {
            this.packetsToIgnore.Add(Encoding.UTF8.GetString(packet.Bytes));

            this.adapter.SendPacket(packet);
        }

        public void turn_on()
        {
            this.ON = true;

            this.adapter.Open(DeviceMode.Promiscuous, 100);
            this.adapter.StartCapture();
        }

        public void turn_off()
        {
            this.ON = false;

            this.adapter.StopCapture();
            this.adapter.Close();
        }
    }
}
