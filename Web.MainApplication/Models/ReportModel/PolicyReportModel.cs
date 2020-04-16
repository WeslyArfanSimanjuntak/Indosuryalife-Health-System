using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.MainApplication.ReportModel
{
    public class PolicyReportModel
    {
        public int NoPolisInduk { get; set; }
        public string NamaPemegangPolis { get; set; }
        public int TransactionAmount { get; set; }
        public string JumlahKaryawan { get; set; }
    }
}