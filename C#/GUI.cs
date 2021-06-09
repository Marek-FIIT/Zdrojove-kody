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
using System.Threading;

namespace PSIP_Switch
{
    public partial class GUI : Form
    {
        private Switch oSwitch;

        private List<StatisticsRow> dsStatistics = new List<StatisticsRow>();
        private List<CAMRow>        dsCAM        = new List<CAMRow>();
        private List<FiltersRow>    dsFilters    = new List<FiltersRow>();

        private bool Running = true;


        private class StatisticsRow
        {
            private Switch sourceSwitch;
            private PortStatistic.EProtocols eProtocol;

            public string Protocol
            {
                get
                {
                    return this.eProtocol.ToString();
                }
            }

            public int Ethernet2_IN
            {
                get
                {
                    return this.sourceSwitch.Statistics[this.sourceSwitch.Statistics.Keys.Where(port => port.name == "Ethernet2")
                                                                                         .First()].protocols[this.eProtocol].IN;
                }
            }
            public int Ethernet2_OUT
            {
                get
                {
                    return this.sourceSwitch.Statistics[this.sourceSwitch.Statistics.Keys.Where(port => port.name == "Ethernet2")
                                                                                         .First()].protocols[this.eProtocol].OUT;
                }
            }

            public int Ethernet3_IN
            {
                get
                {
                    return this.sourceSwitch.Statistics[this.sourceSwitch.Statistics.Keys.Where(port => port.name == "Ethernet3")
                                                                                         .First()].protocols[this.eProtocol].IN;
                }
            }
            public int Ethernet3_OUT
            {
                get
                {
                    return this.sourceSwitch.Statistics[this.sourceSwitch.Statistics.Keys.Where(port => port.name == "Ethernet3")
                                                                                         .First()].protocols[this.eProtocol].OUT;
                }
            }

            public StatisticsRow(PortStatistic.EProtocols protocol, Switch sourceSwitch)
            {
                this.eProtocol    = protocol;
                this.sourceSwitch = sourceSwitch;
            }
        }

        private class CAMRow
        {
            public string MAC_address
            {
                get
                {
                    string NiceMAC = this.MAC.Substring(0, 2);
                    for (int i = 1; i < 6; i++)
                        NiceMAC += ":" + this.MAC.Substring(i * 2, 2);

                    return NiceMAC;
                }
                set
                {
                    this.MAC = value;
                }
            }
            public string Port  { get; set; }
            public int    Timer { get; set; }

            private string MAC;
        }

        private class FiltersRow
        {
            public string Permission { get; set; }
            public string Port       { get; set; }
            public string Direction  { get; set; }
            public string srcMAC     { get; set; }
            public string srcIP      { get; set; }
            public string dstMAC     { get; set; }
            public string dstIP      { get; set; }
            public string Protocol   { get; set; }
            public string srcPort    { get; set; }
            public string dstPort    { get; set; }

            public int RuleHash;
        }


        public GUI()
        {
            this.InitializeComponent();

            this.oSwitch = new Switch("MS NDIS 6.0 LoopBack Driver", this);

            this.GUISetup();
        }
        
        private void GUISetup()
        {
            this.grdCAM.DataSource = this.dsCAM;
            this.grdCAM.Columns[0].Width = 100;
            this.grdCAM.Columns[1].Width = 60;
            this.grdCAM.Columns[2].Width = 40;

            foreach (PortStatistic.EProtocols protocol in Enum.GetValues(typeof(PortStatistic.EProtocols)))
                if (protocol != PortStatistic.EProtocols.IP && protocol != PortStatistic.EProtocols.HTTPS)
                    this.dsStatistics.Add(new StatisticsRow(protocol, this.oSwitch));

            this.grdStatistics.DataSource = this.dsStatistics;

            this.grdStatistics.Columns[0].Width = 50;
            for (int i = 1; i < this.grdStatistics.Columns.Count; i++)
                this.grdStatistics.Columns[i].Width = 90;

            this.grdFilters.DataSource = this.dsFilters;

            this.cbPort.Items.AddRange(this.oSwitch.Ports.Select(port => port.name).OrderBy(port => port).ToArray());
            this.cbPort.SelectedIndex = 0;

            this.grdFilters.Columns[0].Width = 66;
            this.grdFilters.Columns[1].Width = 55;
            this.grdFilters.Columns[2].Width = 57;
            this.grdFilters.Columns[3].Width = 108;
            this.grdFilters.Columns[4].Width = 89;
            this.grdFilters.Columns[5].Width = 108;
            this.grdFilters.Columns[6].Width = 89;
            this.grdFilters.Columns[7].Width = 55;
            this.grdFilters.Columns[8].Width = 48;
            this.grdFilters.Columns[9].Width = 48;

            this.txtDstAddress.GotFocus  += new EventHandler(this.HidePlaceholder);
            this.txtDstAddress.LostFocus += new EventHandler(this.ShowPlaceholder);
            this.txtSrcAddress.GotFocus  += new EventHandler(this.HidePlaceholder);
            this.txtSrcAddress.LostFocus += new EventHandler(this.ShowPlaceholder);
            this.txtDstPort   .GotFocus  += new EventHandler(this.HidePlaceholder);
            this.txtDstPort   .LostFocus += new EventHandler(this.ShowPlaceholder);
            this.txtSrcPort   .GotFocus  += new EventHandler(this.HidePlaceholder);
            this.txtSrcPort   .LostFocus += new EventHandler(this.ShowPlaceholder);

            foreach (var control in this.Controls)
                try 
                {
                    ((CheckBox)control).CheckedChanged += new EventHandler(this.UncheckGroup);
                }
                catch { }
        }

        private void UncheckGroup(object sender, EventArgs e)
        {
            var CheckBox = (CheckBox)sender;
            if (CheckBox.Checked)
            {
                if (CheckBox == this.chbPermit)
                    this.chbDeny.Checked = false;
                if (CheckBox == this.chbDeny)
                    this.chbPermit.Checked = false;

                if (CheckBox == this.chbIN)
                    this.chbOUT.Checked = false;
                if (CheckBox == this.chbOUT)
                    this.chbIN.Checked = false;

                if (CheckBox == this.chbMACdst)
                    this.chbIPdst.Checked = false;
                if (CheckBox == this.chbIPdst)
                    this.chbMACdst.Checked = false;

                if (CheckBox == this.chbMACsrc)
                    this.chbIPsrc.Checked = false;
                if (CheckBox == this.chbIPsrc)
                    this.chbMACsrc.Checked = false;

                if (CheckBox == this.chbTCP)
                {
                    this.chbUDP .Checked = false;
                    this.chbICMP.Checked = false;
                }
                if (CheckBox == this.chbUDP)
                {
                    this.chbTCP .Checked = false;
                    this.chbICMP.Checked = false;
                }
                if (CheckBox == this.chbICMP)
                {
                    this.chbUDP.Checked = false;
                    this.chbTCP.Checked = false;
                }
            }
        }

        private void ShowPlaceholder(object sender, EventArgs e)
        {
            var TextBox = (TextBox) sender;
            if (TextBox.Text == "")
            {
                if (TextBox == this.txtDstAddress)
                    TextBox.Text = "Destination Address";
                if (TextBox == this.txtSrcAddress)
                    TextBox.Text = "Source Address";
                if (TextBox == this.txtDstPort)
                    TextBox.Text = "Destination Port";
                if (TextBox == this.txtSrcPort)
                    TextBox.Text = "Source Port";
            }
        }
        
        private void HidePlaceholder(object sender, EventArgs e)
        {
            var TextBox = (TextBox)sender;
            if (TextBox == this.txtDstAddress && TextBox.Text == "Destination Address")
                TextBox.Text = "";
            if (TextBox == this.txtSrcAddress && TextBox.Text == "Source Address")
                TextBox.Text = "";
            if (TextBox == this.txtDstPort    && TextBox.Text == "Destination Port")
                TextBox.Text = "";            
            if (TextBox == this.txtSrcPort    && TextBox.Text == "Source Port")
                TextBox.Text = "";
        }


        private void PeriodicAction()
        {
            this.grdStatistics.Refresh();

            lock (this.oSwitch.CAM)
            {
                foreach (Port PortKey in this.oSwitch.PortUsage.Keys)
                    if (DateTime.Now.Subtract(this.oSwitch.PortUsage[PortKey]).TotalSeconds >= 50)
                        foreach (string CAMKey in this.oSwitch.CAM.Entries.Keys.ToArray())
                            if (this.oSwitch.CAM.Entries[CAMKey].Port == PortKey)
                                this.oSwitch.CAM.Entries.Remove(CAMKey);

                this.dsCAM = new List<CAMRow>();
                foreach (string Key in this.oSwitch.CAM.Entries.Keys.ToArray())
                    if (--this.oSwitch.CAM.Entries[Key].Timer == 0)
                        this.oSwitch.CAM.Entries.Remove(Key);
                    else
                        this.dsCAM.Add(new CAMRow()
                        {
                            MAC_address = Key,
                            Port  = this.oSwitch.CAM.Entries[Key].Port.name,
                            Timer = this.oSwitch.CAM.Entries[Key].Timer
                        });
            }

            this.grdCAM.DataSource = this.dsCAM;
        }
        

        protected override void OnClosed(EventArgs e)
        {
            this.Running = false;

            this.oSwitch.Stop();
            base.OnClosed(e);
        }

        private void NumTimer_ValueChanged(object sender, EventArgs e)
        {
            int value = (int)((NumericUpDown)sender).Value;
            lock (this.oSwitch.CAM)
            {
                foreach (string Key in this.oSwitch.CAM.Entries.Keys.ToArray())
                    if ((this.oSwitch.CAM.Entries[Key].Timer -= (this.oSwitch.CAM.Timer - value)) <= 0)
                        this.oSwitch.CAM.Entries.Remove(Key);

                this.oSwitch.CAM.Timer = value;
            }
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            this.btnStart.Enabled = false;
            this.oSwitch.Start();

            Task.Run(() => {    
                                while (this.Running)
                                {
                                    Thread.Sleep(1000);
                                    try { this.Invoke((Action)this.PeriodicAction); }
                                    catch { return; }
                                }
                            });
        }


        private void BtnClearStatistics_Click(object sender, EventArgs e)
        {
            lock (this.oSwitch.Statistics)
            {
                foreach (Port port in this.oSwitch.Statistics.Keys)
                    foreach (PortStatistic.EProtocols protocol in Enum.GetValues(typeof(PortStatistic.EProtocols)))
                    {
                        this.oSwitch.Statistics[port].protocols[protocol].IN  = 0;
                        this.oSwitch.Statistics[port].protocols[protocol].OUT = 0;
                    }
            }
        }

        private void BtnClearCAM_Click(object sender, EventArgs e)
        {
            lock (this.oSwitch.CAM)
            {
                this.oSwitch.CAM.Entries = new Dictionary<string, Switch.CamTable.CamEntry>();
            }
        }


        private void btnAddFilter_Click(object sender, EventArgs e)
        {
            if (!this.chbDeny.Checked && !this.chbPermit.Checked || !this.chbIN.Checked && !this.chbOUT.Checked || !this.chbTCP.Checked && !this.chbUDP.Checked && !this.chbICMP.Checked)
                return;

            var SelectedPort = this.oSwitch.Ports.Where(port => port.name == (string)this.cbPort.SelectedItem).First();
            Switch.FilterRuleSet RuleSet = this.chbIN.Checked ? this.oSwitch.InboundFilterRules[SelectedPort]
                                                              : this.oSwitch.OutboundFilterRules[SelectedPort];

            var FilterRule = new Switch.FilterRule()
            {
                MACsrc = this.chbMACsrc.Checked ? (this.txtSrcAddress.Text != "Source Address"      ? string.Join("", this.txtSrcAddress.Text.Split(':')).ToUpper()
                                                                                                    : null)
                                                : null,
                MACdst = this.chbMACdst.Checked ? (this.txtDstAddress.Text != "Destination Address" ? string.Join("", this.txtDstAddress.Text.Split(':')).ToUpper()
                                                                                                    : null)
                                                : null,

                IPsrc = this.chbIPsrc.Checked ? (this.txtSrcAddress.Text != "Source Address"      ? this.txtSrcAddress.Text
                                                                                                  : null)
                                              : null,

                IPdst = this.chbIPdst.Checked ? (this.txtDstAddress.Text != "Destination Address" ? this.txtDstAddress.Text
                                                                                                  : null)
                                              : null,

                Protocol =   this.chbTCP.Checked ? Switch.FilterRule.EFilterProtocols.TCP
                           : this.chbUDP.Checked ? Switch.FilterRule.EFilterProtocols.UDP
                           :                       Switch.FilterRule.EFilterProtocols.ICMP,

                srcPort = this.chbICMP.Checked ? -1
                                               : this.txtSrcPort.Text != "Source Port" ? int.Parse(this.txtSrcPort.Text) : -1,

                dstPort = this.txtDstPort.Text != "Destination Port" ? int.Parse(this.txtDstPort.Text) : -1,

                Permission = this.chbPermit.Checked ? true : false
            };

            lock (RuleSet)
            {
                RuleSet.Rules.Add(FilterRule);
            }

            this.dsFilters.Add(new FiltersRow()
            {
                Permission = this.chbPermit.Checked ? "Permit" : "Deny",
                Port       = SelectedPort.name,
                Direction  = this.chbIN.Checked ? "IN" : "OUT",

                srcMAC     = FilterRule.MACsrc != null ? this.txtSrcAddress.Text : "Any",
                srcIP      = FilterRule.IPsrc  != null ? this.txtSrcAddress.Text : "Any",
                dstMAC     = FilterRule.MACdst != null ? this.txtDstAddress.Text : "Any",
                dstIP      = FilterRule.IPdst  != null ? this.txtDstAddress.Text : "Any",

                Protocol   = FilterRule.Protocol.ToString(),
                srcPort    = FilterRule.srcPort != -1 ? FilterRule.srcPort.ToString() : FilterRule.Protocol != Switch.FilterRule.EFilterProtocols.ICMP ? "Any" : "-",
                dstPort    = FilterRule.dstPort != -1 ? FilterRule.dstPort.ToString() : "Any",

                RuleHash   = FilterRule.GetHashCode()
            });

            var NewDataSource = new List<FiltersRow>();
            foreach (FiltersRow row in this.dsFilters)
                NewDataSource.Add(row);
            this.grdFilters.DataSource = NewDataSource;
        }

        private void btnRemoveFilter_Click(object sender, EventArgs e)
        {
            int RowIndex = this.grdFilters.SelectedCells[0].RowIndex;

            lock (this.oSwitch.InboundFilterRules)
            {
                foreach (var ruleSet in this.oSwitch.InboundFilterRules.Select(item => item.Value.Rules).ToArray())
                    try { ruleSet.RemoveAt(ruleSet.FindIndex(rule => rule.GetHashCode() == this.dsFilters[RowIndex].RuleHash)); }
                    catch { }
            }

            lock (this.oSwitch.OutboundFilterRules)
            {
                foreach (var ruleSet in this.oSwitch.OutboundFilterRules.Select(item => item.Value.Rules).ToArray())
                    try { ruleSet.RemoveAt(ruleSet.FindIndex(rule => rule.GetHashCode() == this.dsFilters[RowIndex].RuleHash)); }
                    catch { }
            }

            this.dsFilters.RemoveAt(RowIndex);

            var NewDataSource = new List<FiltersRow>();
            foreach (FiltersRow row in this.dsFilters)
                NewDataSource.Add(row);
            this.grdFilters.DataSource = NewDataSource;
        }

        private void btnRemoveAllFilters_Click(object sender, EventArgs e)
        {
            lock (this.oSwitch.InboundFilterRules)
            {
                foreach (var ruleSet in this.oSwitch.InboundFilterRules.Select(item => item.Value.Rules).ToArray())
                    ruleSet.RemoveRange(0, ruleSet.Count);
            }

            lock (this.oSwitch.OutboundFilterRules)
            {
                foreach (var ruleSet in this.oSwitch.OutboundFilterRules.Select(item => item.Value.Rules).ToArray())
                    ruleSet.RemoveRange(0, ruleSet.Count);
            }

            this.dsFilters.RemoveRange(0, this.dsFilters.Count);
            this.grdFilters.DataSource = new List<FiltersRow>(); ;
        }
    }
}
