using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using DDSReportingAgent.Models;
using DDSReportingAgent.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;
using MemberCommunications.Web.ViewModels;
using PagedList;
using Path = System.IO.Path;

namespace DDSReportingAgent.Controllers
{
    public class TestController : Controller
    {


        private MemberComm_DWContext db = new MemberComm_DWContext();
        //
        // GET: /Reports/
        //private UploadFilesResult fr = new UploadFilesResult();

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
                lstIndexReportViewModels = lstIndexReportViewModels.Where(j => j.name.ToLower().Contains(searchString)).ToList();

            }


            var orderedJobs = lstIndexReportViewModels.OrderByDescending(j => j.name);
            switch (sortOrder)
            {
                case "name_desc":
                    orderedJobs = lstIndexReportViewModels.OrderByDescending(j => j.name);
                    break;

                default:  // Name ascending 
                    orderedJobs = lstIndexReportViewModels.OrderBy(j => j.name);
                    break;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(orderedJobs.ToPagedList(pageNumber, pageSize));

            //return View(lstIndexReportViewModels.AsEnumerable());
        }

        //
        // GET: /Test/Details/5

        public ActionResult Details(int id)
        {

            return View(new JobViewModel(db.mc_scheduled_jobs.Find(id)));
        }



        public ActionResult _JobIndex(int? reportId, string sortOrder, int? page, string currentFilter, string searchString)
        {
            List<JobViewModel> lstJobs = new List<JobViewModel>();
            //var dFloor = DateTime.Par

            foreach (var j in db.mc_scheduled_jobs.Where(x => x.deleted != true && x.cancel != true && x.complete != false && x.report_id == reportId).Select(x => x))
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

            //var JobViewModel = from j in db.mc_scheduled_jobs select j;


            if (!String.IsNullOrEmpty(searchString))
            {
                lstJobs = lstJobs.Where(j => j.parentReportName.Contains(searchString)).ToList();

            }


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
                    l.file_type = "SFTP";
                        //p.Value;
                    l.complete = false;
                    db.mc_schedule_sftp_log.Add(l);
                    db.SaveChanges();
                }
                
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
                    l.file_type = "log";
                        //p.Value.TrimEnd();
                    l.complete = false;
                    l.in_progress = false;
                    db.mc_schedule_sftp_log.Add(l);
                    db.SaveChanges();
                }
           

                var u = db.mc_scheduled_jobs.Find(p.id);
                u.upd_activity_log = true;
                db.SaveChanges();

                return Json(new {success = true, Message = "Updated Activity log",});

              

            }
            catch
            {
                //if (findId)
                return Json(new {success = false, Message = "Failed to update Activity log. ",});
            }

        }


       // public string RunUpdateDDActlog(int jobId, string typFile)
       // {

       //     string arg = jobId.ToString() + typFile;
       //     var outputText = new StringBuilder();
       //     var errorText = new StringBuilder();
       //     string returnvalue;
       //     string path = Server.MapPath("~/") + "RunConsole\\UpdateDDActivityLog.exe";
       //     using (var process = Process.Start(new ProcessStartInfo(path, arg)
       //     {
       //         CreateNoWindow = true,
       //         ErrorDialog = false,
       //         RedirectStandardError = true,
       //         RedirectStandardOutput = true,
       //         UseShellExecute = false
       //     }))
       //     {
       //         process.OutputDataReceived += (sendingProcess, outLine) =>
       //             outputText.AppendLine(outLine.Data);

       //         process.ErrorDataReceived += (sendingProcess, errorLine) =>
       //             errorText.AppendLine(errorLine.Data);

       //         process.BeginOutputReadLine();
       //         process.BeginErrorReadLine();
       //         process.WaitForExit();
       //         returnvalue = outputText.ToString() + Environment.NewLine + errorText.ToString();
       //     }


       //return returnvalue.Trim();
       // }
      
        public ActionResult _DisplayLog(int id)
        {
            int inputId = id;


            var lg = (db.mc_act_log.Where(r => r.mc_job == inputId).Select(r => new
            {
                r.mc_log_id
            })).FirstOrDefault(); 

            var inputligId = lg.mc_log_id;
  
       var lstIndexJobViewModels = new List<MemberViewModel>();
            //foreach (
            //    var sr in
            //        db.mc_act_log_d.Where(x => x.mc_log_id == inputligId)
            //            .Select(x => x))

            //{
            //    lstIndexJobViewModels.Add(new MemberViewModel(sr));
            //}
            //return View(lstIndexJobViewModels);
      var viewModel = from l in db.mc_act_log
        join ld in db.mc_act_log_d on l.mc_log_id equals ld.mc_log_id
        where l.mc_log_id == ( inputligId)

        select new { logid = l.mc_job, memberid = ld.mc_event_sys_id, Errormessage = l.mc_log_error_msg, DateCompletion = l.mc_log_complete_date  };


      foreach (var m in viewModel)
      {
          var gp = new MemberViewModel();
          gp.mc_event_sys_id = m.memberid;
          gp.jobid = m.logid;
          gp.dateCompletion = m.DateCompletion;
          gp.errormessage = m.Errormessage;

          lstIndexJobViewModels.Add(gp);
      }
      return View(lstIndexJobViewModels);

        }
    }




}



//[HttpPost]
            //public ActionResult Save(FormCollection formCollection)
            //{
            //    if (Request != null)
            //    {
            //        HttpPostedFileBase file = Request.Files["UploadedFile"];

            //        //if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            //        //{
            //        //    string fileName = file.FileName;
            //        //    string fileContentType = file.ContentType;
            //        //    byte[] fileBytes = new byte[file.ContentLength];
            //        //    file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

            //        //    //fr.sentSFTP();

            //        //}
            //    }


            //    return Content(" File Uploaded");



            //}


            ////[HttpPost]
//public ContentResult UploadFiles()
//{
//    var r = new List<UploadFilesResult>();

//    foreach (string file in Request.Files)
//    {
//        var hpf = Request.Files[file] as HttpPostedFileBase;
//        if (hpf.ContentLength == 0)
//            continue;

//        string savedFileName = Path.Combine(Server.MapPath("~/App_Data"), Path.GetFileName(hpf.FileName));
//        hpf.SaveAs(savedFileName); // Save the file

//        r.Add(new UploadFilesResult()
//        {
//            Name = hpf.FileName,
//            Length = hpf.ContentLength,
//            Type = hpf.ContentType
//        });
//    }
//    // Returns json
//    return Content("{\"name\":\"" + r[0].Name + "\",\"type\":\"" + r[0].Type + "\",\"size\":\"" + string.Format("{0} bytes", r[0].Length) + "\"}", "application/json");

//}






//[HttpPost]
//public JsonResult UploadFiles()
//{
//    IList<string> filesSaved = new List<string>();

//    foreach (string file in Request.Files)
//    {
//    HttpPostedFileBase hpf = Request.Files[file];
//    if (hpf.ContentLength > 0)
//    {
//        var fileName = hpf.FileName;
//        filesSaved.Add(fileName);
//    }
//    }

//    return Json(new {files = filesSaved}, JsonRequestBehavior.DenyGet);
//}

            //}


        
 