using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using iTextSharp.text;
using LinqToDB.Extensions;
using Microsoft.Ajax.Utilities;
using PagedList;
using System.Linq;
using System.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using Path = System.IO.Path;
using DDSReportingAgent.Models;
using System.Web.Services.Description;
using iTextSharp.text.pdf.parser;
using MemberCommunications.Web.ViewModels;
using DDSReportingAgent.Controllers;
using DDSReportingAgent.ViewModels;
using System.Text;


namespace MemberCommunications.Web.Controllers
{
    public class ReviewScheduleController : Controller
    {
        private MemberComm_DWContext db = new MemberComm_DWContext();

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {

            List<IndexReportViewModelSlim> lstIndexReportViewModels = new List<IndexReportViewModelSlim>();

            foreach (var sr in db.mc_reports.Where(x => x.deleted != true).Select(x => x))
            {
                lstIndexReportViewModels.Add(new IndexReportViewModelSlim(sr));
            }
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";


            if (searchString == null)
            {
                searchString = currentFilter;
            }
            if (searchString != null) searchString = searchString.ToLower();

            ViewBag.currentFilter = searchString;


            if (!String.IsNullOrEmpty(searchString))
            {
                lstIndexReportViewModels =
                    lstIndexReportViewModels.Where(j => j.name.ToLower().Contains(searchString)).ToList();

            }


            var orderedJobs = lstIndexReportViewModels.OrderByDescending(j => j.name);
            switch (sortOrder)
            {
                case "name_desc":
                    orderedJobs = lstIndexReportViewModels.OrderByDescending(j => j.name);
                    break;

                default: // Name ascending 
                    orderedJobs = lstIndexReportViewModels.OrderBy(j => j.name);
                    break;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(orderedJobs.ToPagedList(pageNumber, pageSize));

            //return View(lstIndexReportViewModels.AsEnumerable());
        }


        public ActionResult Details(int id)
        {

            return View(new JobViewModel(db.mc_scheduled_jobs.Find(id)));
        }


        public ActionResult _JobIndex(int? reportId, string sortOrder, int? page, string currentFilter,
            string searchString, DateTime? search)
        {
            List<JobViewModel> lstJobs = new List<JobViewModel>();
            //var dFloor = DateTime.Par

            foreach (
                var j in
                    db.mc_scheduled_jobs.Where(
                        x => x.deleted != true && x.cancel != true && x.complete != false && x.report_id == reportId)
                        .Select(x => x))
            {
                lstJobs.Add(new JobViewModel(j));
            }

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            if (sortOrder == null) sortOrder = "Date - Ascending";
            ViewBag.DateSortParm = sortOrder == "Date - Ascending" ? "Date - Descending" : "Date - Ascending";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.currentFilter = searchString;


            var orderedJobs = lstJobs.OrderBy(j => j.run_end);
            switch (sortOrder)
            {
                    //case "name_desc":
                    //    orderedJobs = lstJobs.OrderByDescending(j => j.parentReportName);
                    //    break;
                case "Date - Ascending":
                    orderedJobs = lstJobs.OrderBy(j => j.run_end);
                    break;
                case "Date - Descending":
                    orderedJobs = lstJobs.OrderByDescending(j => j.run_end);
                    break;
                    //default:  // Name ascending 
                    //    orderedJobs = lstJobs.OrderBy(j => j.parentReportName);
                    //    break;
            }


            if (searchString == "search")
            {
                return View((IPagedList<JobViewModel>) db.mc_scheduled_jobs.Where(j => j.run_end == search).ToList());
            }


            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return PartialView(orderedJobs.ToPagedList(pageNumber, pageSize));

        }

        [HttpPost]
        public ActionResult RunSFTP(UpdateSftp p)
        {
            //var u = db.mc_scheduled_jobs.Find(p.id);
            ////List<string> errors = new List<string>();
            var jId = p.id;
            var findId = db.mc_scheduled_jobs.Where(r => jId.Equals(r.id) && r.upd_activity_log == false);
            try
            {
                if (findId != null)
                {
                    var l = new mc_schedule_sftp_log();
                    l.job_id = p.id;
                    l.file_type = p.Value;
                    l.complete = false;
                    db.mc_schedule_sftp_log.Add(l);
                    db.SaveChanges();
                }

                var u = db.mc_scheduled_jobs.Find(p.id);
                u.send_sftp = true;
                db.SaveChanges();


                return Json(new {success = true, Message = "Sent SFTP" + p.Value});
            }
            catch
            {
                //errors.Add(ex.Message);
                return Json(new {success = false, Message = "Failed to send SFTP "});
            }
        }

        [HttpPost]
        public ActionResult RunLog(UpdateLog p)
        {
            var jId = p.id;

            var findId = db.mc_scheduled_jobs.Where(r => jId.Equals(r.id) && r.upd_activity_log != true);
            try
            {
                if (findId != null)
                {
                    var l = new mc_schedule_sftp_log();
                    l.job_id = p.id;
                    l.file_type = p.Value;
                    l.complete = false;
                    db.mc_schedule_sftp_log.Add(l);
                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("This has alredady been updated@!");
                }

                var u = db.mc_scheduled_jobs.Find(p.id);
                u.upd_activity_log = true;
                db.SaveChanges();

                return Json(new {success = true, Message = "Updated Activity log",});


            }
            catch
            {
                //if (findId)
                return Json(new {success = false, Message = "Failed to update Activity log. "});
            }

        }


        //public string RunUpdateDDActlog(int jobId, string typFile)
        //{

        //    string arg = jobId.ToString() + typFile;
        //    var outputText = new StringBuilder();
        //    var errorText = new StringBuilder();
        //    string returnvalue;
        //    string path = Server.MapPath("~/") + "RunConsole\\UpdateDDActivityLog.exe";
        //    using (var process = Process.Start(new ProcessStartInfo(path, arg)
        //    {
        //        CreateNoWindow = true,
        //        ErrorDialog = false,
        //        RedirectStandardError = true,
        //        RedirectStandardOutput = true,
        //        UseShellExecute = false
        //    }))
        //    {
        //        process.OutputDataReceived += (sendingProcess, outLine) =>
        //            outputText.AppendLine(outLine.Data);

        //        process.ErrorDataReceived += (sendingProcess, errorLine) =>
        //            errorText.AppendLine(errorLine.Data);

        //        process.BeginOutputReadLine();
        //        process.BeginErrorReadLine();
        //        process.WaitForExit();
        //        returnvalue = outputText.ToString() + Environment.NewLine + errorText.ToString();
        //    }


        //    return returnvalue.Trim();
        //}


        public ActionResult ReportLookup(string type)
        {
            var reportVars = new List<SearchTypeAheadEntity>();



            //[HttpPost]
            //public ContentResult UploadFiles()
            //{
            //    var r = new List<UploadFilesResult>();
            foreach (var g in db.mc_reports.Where(x => x.deleted == false).Select(x => x))
            {
                reportVars.Add(new SearchTypeAheadEntity() {name = g.name, id = g.id.ToString()});
            }

            return Json(reportVars, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DisplayLog(int id, int? page, string sortOrder, string currentFilter, string pid)
        {

            int inputId = id;


            var lg = (db.mc_act_log.Where(r => r.mc_job == inputId).Select(r => new
            {
                r.mc_log_id
            })).FirstOrDefault();


            var inputligId = lg.mc_log_id;

            var lstIndexJobViewModels = new List<LogViewModel>();


            var viewModel = from l in db.mc_act_log
                join ld in db.mc_act_log_d on l.mc_log_id equals ld.mc_log_id
                where l.mc_log_id == (inputligId)

                select
                    new
                    {
                        logid = l.mc_job,
                        memberid = ld.mc_event_sys_id,
                        Errormessage = l.mc_log_error_msg,
                        DateCompletion = l.mc_log_complete_date
                    };


            foreach (var m in viewModel)
            {
                var gp = new LogViewModel();
                gp.mc_event_sys_id = m.memberid;
                gp.jobid = m.logid;
                gp.dateCompletion = m.DateCompletion;
                gp.errormessage = m.Errormessage;


                lstIndexJobViewModels.Add(gp);
            }

            string searchString = pid;

            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParm = String.IsNullOrEmpty(sortOrder) ? "Id_desc" : "";


            if (searchString == null)
            {
                searchString = currentFilter;
            }

            ViewBag.currentFilter = searchString;
            var det = lstIndexJobViewModels.Where(j => j.mc_event_sys_id.Equals(searchString));
            if (!string.IsNullOrEmpty(searchString))
            {
                det = lstIndexJobViewModels.Where(j => j.mc_event_sys_id.Equals(searchString)).ToList();
            }
            switch (sortOrder)
            {
                case "Id_desc":
                    det = lstIndexJobViewModels.OrderByDescending(j => j.mc_event_sys_id.Equals(searchString));
                    break;

                default:
                    det = lstIndexJobViewModels.OrderBy(j => j.mc_event_sys_id);
                    break;
            }

            int pageSize = 8;
            int pageNumber = (page ?? 1);

            return View(det.ToPagedList(pageNumber, pageSize));

        }


        public ActionResult DataVal(int id)
        {


            var jId = id;
            try
            {
                var findId = db.mc_dataValidation.Where(r => jId.Equals(r.job_id));


                var jobTb = (from j in db.mc_dataValidation
                    where j.job_id == jId
                    select new
                    {

                        xml = j.xmlFile
                        //j.output_xml

                    }).FirstOrDefault();

                DataTable dt = new DataTable();

                String t = null;
                if (jobTb != null)
                {
                    t = jobTb.xml;

             
                   dt = stam("<root>" + t + "</root>");
               }
               else
               {

                   var jID = (from j in db.mc_scheduled_jobs
                              where j.id == jId
                              select new
                              {
                                  j.id,
                                  j.schedule_id,
                                  j.report_id,
                                  j.upd_activity_log,
                                  j.send_sftp,
                                  j.output_xml,
                                  j.output_location_1,
                                  j.output_location_2

                              }).FirstOrDefault();

                   var file = jID.output_xml;


                   var fileName = jID.output_xml;
                   //dt = GetMemebrs(fileName);
                   dt = GetMemebrsAl2(fileName);
               }

               
                return View(dt);
            }

            catch (Exception ex)
            {
                return RedirectToAction("Details");
            }

        }


        public DataTable XElementToDataTable(XElement x)
        {
            DataTable dtable = new DataTable();

            XElement setup = (from p in x.Descendants() select p).First();
            // build your DataTable
            foreach (XElement xe in setup.Descendants())
                dtable.Columns.Add(new DataColumn(xe.Name.ToString(), typeof (string))); // add columns to your dt

            var all = from p in x.Descendants(setup.Name.ToString()) select p;
            foreach (XElement xe in all)
            {
                DataRow dr = dtable.NewRow();
                foreach (XElement xe2 in xe.Descendants())
                    dr[xe2.Name.ToString()] = xe2.Value; //add in the values
                dtable.Rows.Add(dr);
            }
            return dtable;
        }






        public DataTable stam(string xmlData)
        {
            StringReader theReader = new StringReader(xmlData);
            DataSet theDataSet = new DataSet();
            theDataSet.ReadXml(theReader);

            return theDataSet.Tables[0];
        }

        public static DataTable BuildDataTableFromXml(string Name, string XMLString)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(new StringReader(XMLString));
            DataTable Dt = new DataTable(Name);
            try
            {

                XmlNode NodoEstructura = doc.FirstChild.FirstChild;
                //  Table structure (columns definition) 
                foreach (XmlNode columna in NodoEstructura.ChildNodes)
                {
                    Dt.Columns.Add(columna.Name, typeof (String));
                }

                XmlNode Filas = doc.FirstChild;
                //  Data Rows 
                foreach (XmlNode Fila in Filas.ChildNodes)
                {
                    List<string> Valores = new List<string>();
                    foreach (XmlNode Columna in Fila.ChildNodes)
                    {
                        Valores.Add(Columna.InnerText);
                    }
                    Dt.Rows.Add(Valores.ToArray());
                }
            }
            catch (Exception)
            {

            }

            return Dt;
        }

        public static DataTable GetMemebrs(string fileName)
        {

            //var dt = new DataTable();



            //dt.Columns.Add("id", typeof(int));
            var doc = new XmlDocument();
            doc.Load(fileName);
            char[] separatingChars = {Convert.ToChar(";"), Convert.ToChar("|")};
            XmlNodeList xmlnode;
            //if (File.Exists(fileName)) 
            //xmlnode = doc.GetElementsByTagName("ActivityLog1");
            int i;

            xmlnode = doc.GetElementsByTagName("Field");

            Console.WriteLine("Here is the list of members \n\n");
            //file.Close();
            var DT = new DataTable();
            DataTable dt = new DataTable();
            for (i = 0; i < xmlnode.Count; i++)
            {
                XmlAttributeCollection xmlattrc = xmlnode[i].Attributes;


                var ids = new List<int>();

                //XML Attribute Name and Value returned

                //Example: <Feid Name = "memberid1">

                var Title = xmlattrc[0].Value;

                if (Title == "Data1")
                {
                    //DataRow ds = dt.NewRow();
                    Console.Write(xmlattrc[0].Name);
                    Console.WriteLine(":\t" + xmlattrc[0].Value);

                    //First Child of the XML file -  returned
                    //Example: <FormattedValue>1,064</FormattedValue>

                    var MemberIdNf = (xmlnode[i].FirstChild.InnerText);
                    //if (MemberIdNf == "memberid1")
                    //{
                    Console.Write(xmlnode[i].FirstChild.Name);
                    Console.WriteLine(":\t" + xmlnode[i].FirstChild.InnerText);

                    //InnerText = decimal.ToInt32(Convert.ToDecimal(xmlnode[i].LastChild.InnerText));
                    var InnerText = (xmlnode[i].LastChild.InnerText);
                    //ds["id"] = InnerText;

                    Console.WriteLine(":\t" + xmlnode[i].LastChild.InnerText);
                    string newRow = "family_id";
                    //var values = InnerText.Replace("|", ",");
                    var values = InnerText.Trim();
                    //.Remove(InnerText.Length - 1, 1)
                    var cols = values.Split('|');

                    string pattern = Regex.Escape(cols[0].Split(';')[0]);
                    var matches = Regex.Matches(InnerText, "\\b" + pattern + "\\b", RegexOptions.IgnoreCase);
                    int count = matches.Count;




                    for (int f = 0; f < cols.Length; f++)
                    {
                        var p = cols[f].Split(';');
                        if (!dt.Columns.Contains(p[0]))
                            dt.Columns.Add(p[0]);
                        Console.WriteLine(":\t" + p[0]);
                    }
                    DataRow dr = null;
                    int idxCol = 0;
                    for (int k = 0; k < count; k++)
                    {
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            var value = cols[idxCol].Split(';');

                            if (value[0] == newRow)
                            {
                                dr = dt.NewRow();
                                dr[j] = value[1];
                                Console.WriteLine(":\t" + value[1]);
                            }
                            else
                            {
                                dr[j] = value[1];
                                Console.WriteLine(":\t" + value[1]);
                            }
                            idxCol++;
                        }
                        dt.Rows.Add(dr);


                    }

                    DT = dt;
                    DT.AcceptChanges();
                }

            }

            return DT;
        }


        public static DataTable GetMemebrsAl2(string fileName)
        {

            var dt = new DataTable();
            dt.Columns.Add("id", typeof(int));
            var doc = new XmlDocument();
            doc.Load(fileName);



            XmlNodeList xmlnode = doc.GetElementsByTagName("Field");
            Console.WriteLine("Here is the list of members \n\n");
            //file.Close();
            for (int i = 0; i < xmlnode.Count; i++)
            {
                XmlAttributeCollection xmlattrc = xmlnode[i].Attributes;


                var ids = new List<int>();

                //XML Attribute Name and Value returned
                {
                    //Example: <Feid Name = "memberid1">

                  var  Title = xmlattrc[0].Value;

                    if (Title == "familyid1")
                    {




                        DataRow ds = dt.NewRow();
                        Console.Write(xmlattrc[0].Name);
                        Console.WriteLine(":\t" + xmlattrc[0].Value);

                        //First Child of the XML file -  returned
                        //Example: <FormattedValue>1,064</FormattedValue>

                      var  MemberIdNf = (xmlnode[i].FirstChild.InnerText);
                        //if (MemberIdNf == "memberid1")
                        //{
                        Console.Write(xmlnode[i].FirstChild.Name);
                        Console.WriteLine(":\t" + xmlnode[i].FirstChild.InnerText);

                     var   InnerTextD = decimal.ToInt32(Convert.ToDecimal(xmlnode[i].LastChild.InnerText));
                        Convert.ToInt32(xmlnode[i].LastChild.InnerText);
                        ds["id"] = InnerTextD;


                        //}

                        Console.Write(xmlnode[i].LastChild.Name);
                        Console.WriteLine(":\t" + xmlnode[i].LastChild.InnerText);

                        dt.Rows.Add(ds);

                        dt.AcceptChanges();
                    }

                }
            }
            return dt;
        }
        

    


}

}


















