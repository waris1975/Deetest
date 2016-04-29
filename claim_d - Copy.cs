using System;
using System.Collections.Generic;

namespace DDSReportingAgent.Models
{
    public partial class claim_d
    {
        public int claim_d_id { get; set; }
        public int claim_id { get; set; }
        public Nullable<System.DateTime> svc_beg { get; set; }
        public string d_proc_code { get; set; }
        public string tooth_no { get; set; }
        public string surface { get; set; }
        public string quad { get; set; }
        public decimal submitted_amt { get; set; }
        public decimal lab_fee { get; set; }
        public decimal allowed { get; set; }
        public decimal patient_resp { get; set; }
        public decimal plan_resp { get; set; }
        public System.DateTime h_datetime { get; set; }
        public int h_msi { get; set; }
        public string h_action { get; set; }
        public string h_user { get; set; }
        public Nullable<System.DateTime> date_to_pay { get; set; }
        public Nullable<System.DateTime> date_processed { get; set; }
        public string status { get; set; }
        public int reversal_switch { get; set; }
        public int statement_id { get; set; }
        public decimal cob_amt { get; set; }
        public decimal max_allowed { get; set; }
        public decimal perc_covered { get; set; }
        public decimal ded_applied { get; set; }
        public decimal co_ins { get; set; }
        public decimal provider_resp { get; set; }
        public int tpa_inv_number { get; set; }
        public Nullable<int> relates_to { get; set; }
        public Nullable<int> relates_to_order { get; set; }
        public string hist_proc_code { get; set; }
        public int substitute_sw { get; set; }
        public decimal rollover_applied { get; set; }
        public int proc_subst_id { get; set; }
    }
}
