using System;
using System.Collections.Generic;

namespace DDSReportingAgent.Models
{
    public partial class address
    {
        public int address_id { get; set; }
        public string subsys_code { get; set; }
        public Nullable<int> sys_rec_id { get; set; }
        public string addr_type { get; set; }
        public string addr1 { get; set; }
        public string addr2 { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string county { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string mail { get; set; }
        public Nullable<int> history_rec { get; set; }
        public Nullable<int> geo_x { get; set; }
        public Nullable<int> geo_y { get; set; }
    }
}
