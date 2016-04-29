using System;
using System.Collections.Generic;

namespace DDSReportingAgent.Models
{
    public partial class WhoIsActive_20150622
    {
        public string dd_hh_mm_ss_mss { get; set; }
        public short session_id { get; set; }
        public string sql_text { get; set; }
        public string login_name { get; set; }
        public string wait_info { get; set; }
        public string tran_log_writes { get; set; }
        public string CPU { get; set; }
        public string tempdb_allocations { get; set; }
        public string tempdb_current { get; set; }
        public Nullable<short> blocking_session_id { get; set; }
        public string reads { get; set; }
        public string writes { get; set; }
        public string physical_reads { get; set; }
        public string query_plan { get; set; }
        public string used_memory { get; set; }
        public string status { get; set; }
        public Nullable<System.DateTime> tran_start_time { get; set; }
        public string open_tran_count { get; set; }
        public string percent_complete { get; set; }
        public string host_name { get; set; }
        public string database_name { get; set; }
        public string program_name { get; set; }
        public System.DateTime start_time { get; set; }
        public Nullable<System.DateTime> login_time { get; set; }
        public Nullable<int> request_id { get; set; }
        public System.DateTime collection_time { get; set; }
    }
}
