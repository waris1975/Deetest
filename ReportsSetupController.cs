using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DDSReportingAgent.Models;
using MemberCommunications.Web.Helpers;
using MemberCommunications.Web.ViewModels;


namespace MemberCommunications.Web.Controllers
{
    public class ReportsSetupController : Controller
    {
        //
        // GET: /Reports/
        //lists all the reports which have already been setup
        private MemberComm_DWContext db = new MemberComm_DWContext();
        public ActionResult Index()
        {
            
            List<IndexReportViewModel> lstIndexReportViewModels = new List<IndexReportViewModel>();
            
            foreach (var sr in db.mc_reports.Where(x=>x.deleted != true).Select(x => x))
            {
                lstIndexReportViewModels.Add(new IndexReportViewModel(sr));
            }
            

            return View(lstIndexReportViewModels.AsEnumerable());
        }


        public ActionResult Create()
        {
            return View(new CreateReportViewModel());
        }

        //
        // POST: /Reports/Create
        [HttpPost]
        public ActionResult Create(CreateReportViewModel mcReports)
        {

            try
            {
                var sFile = Request.Form["SelectedReportFile.SelectedFile"];
                var sFolder = Request.Form["SelectedOutputFolder.SelectedFolder"];
                if (sFile == null) sFile = "";
                if (sFolder == null) sFolder = "";
                mcReports.SelectedOutputFolder.SelectedFolder = sFolder;
                mcReports.SelectedReportFile.SelectedFile = sFile;
                if (String.IsNullOrWhiteSpace(mcReports.name)) ModelState.AddModelError("name", "The report must have a name.");
                if (sFile.Length < 1) ModelState.AddModelError("SelectedReportFile.SelectedFile", "No report file was selected");
                if (sFolder.Length < 1) ModelState.AddModelError("SelectedOutputFolder.SelectedFolder", "No report output location was selected or location was not valid.");
                try
                {
                    if (!(new FileInfo(sFile).Exists)) ModelState.AddModelError("SelectedReportFile.SelectedFile", "No report file was selected or report file is not valid.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("SelectedReportFile.SelectedFile", "No report file was selected or report file is not valid.");
                }
                try
                {
                    if (!(new DirectoryInfo(sFolder).Exists)) ModelState.AddModelError("SelectedOutputFolder_SelectedFolder", "No report output location was selected or location was not valid. Exception");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("SelectedOutputFolder_SelectedFolder", "No report output location was selected or location was not valid. Exception");
                }

                if (ModelState.IsValid)
                {
                    //load the crystal report
                    CrRptDef crRpt = new CrRptDef(sFile);

                    //save report to database and retrieve key
                    mc_reports mc = new mc_reports
                    {
                        name = mcReports.name,
                        description = mcReports.description,
                        cr_filepath = sFile,
                        output_path = sFolder,
                        date_created = DateTime.Now,
                        date_changed = DateTime.Now,
                        user_created = User.Identity.Name,
                        user_changed = User.Identity.Name
                    };
                    db.mc_reports.Add(mc);
                    db.SaveChanges();
                    Console.WriteLine(mc.id);
                    var id = mc.id;

                    //insert the child entities
                    foreach (var p in crRpt.parameters)
                    {
                        var mrp = new mc_report_params
                        {
                            report_id = mc.id,
                            name = p.name,
                            type = p.type,
                            default_value = p.default_value
                        };
                        db.mc_report_params.Add(mrp);
                    }
                    db.SaveChanges();

                    foreach (var f in crRpt.forumulafields)
                    {
                        mc_report_formulas mrf = new mc_report_formulas();
                        mrf.report_id = mc.id;
                        mrf.name = f.name;
                        mrf.formula_name = f.formula_name;
                        mrf.code = f.code;
                        mrf.use_formula = f.use_formula;
                        db.mc_report_formulas.Add(mrf);
                    }
                    db.SaveChanges();
                    return RedirectToAction("Edit", new { id = id });
                }
                else
                {
                    return View(mcReports);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("name", "A fatal exception has occured your report was not loaded. Please contact the Application Development team for assistance.");
                return View(mcReports);
            }
        }

        public ActionResult _DeleteReportConfirm(int repId)
        {
            var r = db.mc_reports.Find(repId);
                 var m = new EditParaVm()
                {
                    Id = r.id,
                    Value = r.name
                };
            
           
            return PartialView(m);
            
        }

        [HttpPost]
        public ActionResult PostDeleteReport(EditParaVm p)
        {
            var reportId = p.Id;

            var f = db.mc_reports.Find(reportId);
            if (f != null)
            {
                //mark the schedule as deleted
                f.deleted = true;
                db.SaveChanges();
                return Json(new { Success = "True", Message = "Removed report at Id:" + reportId.ToString(), id = reportId });
            }
            return Json(new { Success = "False", Message = "Could not find report to remove at Id:" + reportId.ToString(), id = reportId });
        }

        //
        // GET: /Reports/Edit/5

        public ActionResult Edit(int id)
        {
            var mcr = db.mc_reports.Find(id);
            ReportViewModel eReport = new ReportViewModel(mcr);
            return View(eReport);
        }

        //
        // POST: /Reports/Edit/5

        [HttpPost]
        public ActionResult Edit(ReportViewModel mcReportViewModel)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            try
            {

                var m = db.mc_reports.Find(Convert.ToInt32(Request["id"]));
                m.export_option1 = Request["export_option1"];
                m.export_option2 = Request["export_option2"];
                if (string.IsNullOrWhiteSpace(m.export_option1)) m.export_option1 = "";
                if (string.IsNullOrWhiteSpace(m.export_option2)) m.export_option2 = "";

                m.description = Request["description"];
                m.is_live_data = Convert.ToBoolean(Request["is_live_data"]);
                m.output_path = Request["FStructure.SelectedFolder"];
                m.create_act_log = Convert.ToBoolean(Request["create_act_log"]);
                m.create_sftp = Convert.ToBoolean(Request["create_sftp"]);
                m.act_subsys_code = Request["act_subsys_code"];
                m.act_act_mast_id = Convert.ToInt32(Request["act_act_mast_id"]);//m.name = mcReportViewModel.name;
                m.act_reason_code = Request["act_reason_code"];
                m.act_log_user = Request["act_log_user"];
                //m.cr_filepath = mcReportViewModel.cr_filepath;
                m.date_changed = DateTime.Now;
                m.user_changed = User.Identity.Name;

                //add output file mask options
                m.output_filemask = Request["OutputFileMask.FileMaskName"];
                m.output_folder_calculated_mask = Request["OutputFileMask.FolderMaskName"];
                m.output_folder_modifier_interval = Request["OutputFileMask.dateModifierIntervalSelected"];
                m.output_use_year_folder = Convert.ToBoolean(Request["OutputFileMask.SplitYear"]);
                m.output_use_date_modifier = Convert.ToBoolean(Request["OutputFileMask.useDateModifier"]);

                //validate stuff
                if (m.output_use_date_modifier == true && String.IsNullOrWhiteSpace(m.output_folder_modifier_interval))
                {
                    errors.Add("OutputFileMask.dateModifierIntervalSelected", "You must selcect a date interval to modify.");
                }
                int o = 0;
                if (!Int32.TryParse(Request["OutputFileMask.dateModifier"], out o))
                {
                    errors.Add("OutputFileMask.dateModifier", "Date Modifier for file mask must be a number.");
                }
                else
                {
                    m.output_folder_date_modifier = Convert.ToInt32(Request["OutputFileMask.dateModifier"]);
                }
                if (string.IsNullOrWhiteSpace(m.description)) errors.Add("description", "description cannot be null.");
                if (!(new DirectoryInfo(Request["FStructure.SelectedFolder"])).Exists) errors.Add("FStructure.SelectedFolder", "Output folder destination is not valid.");
                if (string.IsNullOrWhiteSpace(Request["OutputFileMask.FileMaskName"])) errors.Add("OutputFileMask.FileMaskName", "An output file mask is required.");
                if (errors.Count < 1)
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    //var m = db.mc_reports.Find(Request["id"]);
                    var a = new ReportViewModel(m);
                    a.export_option1 = Request["export_option1"];
                    a.export_option2 = Request["export_option2"];
                    a.description = Request["description"];
                    //a.output_filemask =Request["OutputFileMask_FileMaskName"];
                    a.output_path = Request["FStructure.SelectedFolder"];
                    a.create_act_log = Convert.ToBoolean(Request["create_act_log"]);
                    a.create_sftp = Convert.ToBoolean(Request["create_sftp"]);
                    a.act_subsys_code = Request["act_subsys_code"];
                    a.act_act_mast_id = Convert.ToInt32(Request["act_act_mast_id"]);//m.name = mcReportViewModel.name;
                    a.act_reason_code = Request["act_reason_code"];
                    a.act_log_user = Request["act_log_user"];
                    a.OutputFileMask.FileMaskName = Request["OutputFileMask.FileMaskName"];
                    a.OutputFileMask.FolderMaskName = Request["OutputFileMask.FolderMaskName"];
                    a.OutputFileMask.SplitYear = Convert.ToBoolean(Request["OutputFileMask.SplitYear"]);
                    a.OutputFileMask.useDateModifier = Convert.ToBoolean(Request["OutputFileMask.useDateModifier"]);
                    a.OutputFileMask.dateModifierIntervalSelected = Request["OutputFileMask.dateModifierInterval"];
                    a.OutputFileMask.dateModifier = o;
                    foreach (var kv in errors)
                    {
                        ModelState.AddModelError(kv.Key, kv.Value);
                        ModelState.AddModelError(String.Empty, kv.Value);
                    }
                    //ModelState.AddModelError("description","Description canot be empty.");
                    return View(a);
                }


            }
            catch (Exception ex)
            {
                var m = db.mc_reports.Find(Convert.ToInt32(Request["id"]));
                var a = new ReportViewModel(m);
                a.export_option1 = Request["export_option1"];
                a.export_option2 = Request["export_option2"];
                a.description = Request["description"];
                a.output_path = Request["FStructure.SelectedFolder"];
                a.is_live_data = Convert.ToBoolean(Request["is_live_data"]);
                a.create_act_log = Convert.ToBoolean(Request["create_act_log"]);
                a.act_subsys_code = Request["act_subsys_code"];
                a.act_act_mast_id = Convert.ToInt32(Request["act_act_mast_id"]);//m.name = mcReportViewModel.name;
                a.act_reason_code = Request["act_reason_code"];
                a.act_log_user = Request["act_log_user"];
                a.OutputFileMask.FileMaskName = Request["OutputFileMask.FileMaskName"];
                a.OutputFileMask.FolderMaskName = Request["OutputFileMask.FileMaskName"];
                a.OutputFileMask.SplitYear = Convert.ToBoolean(Request["OutputFileMask.SplitYear"]);
                a.OutputFileMask.useDateModifier = Convert.ToBoolean(Request["OutputFileMask.useDateModifier"]);
                a.OutputFileMask.dateModifierIntervalSelected = Request["OutputFileMask.dateModifierInterval"];
                a.OutputFileMask.dateModifier = 0;
                foreach (var kv in errors)
                {
                    ModelState.AddModelError(kv.Key, kv.Value);
                }
                //ModelState.AddModelError("description","Description canot be empty.");
                return View(a);
            }
        }

        [HttpGet]
        public ActionResult GetEmails(string reportId)
        {
            var lookupId = int.Parse(reportId);
            var model = db.mc_reports.Find(lookupId).mc_email_notifications;
            return PartialView("GetEmails", model);
        }

        [HttpPost]
        public ActionResult AddEmail(EditParaVmAlt p)
        {
            var u = db.mc_reports.Find(p.Id);
            var f = p.Value.ToLower();
            //List<string> errors = new List<string>();
            try
            {
                var r = new RegexUtilities();
                if(!r.IsValidEmail(f)) throw new Exception("The email address "+f+" is not valid!");
                if(string.IsNullOrWhiteSpace(p.Value)) throw new Exception("There is no email to update!");
                //make sure it is not already there
                if (u.mc_email_notifications.Any(x => x.email == f && x.deleted == false))
                {
                    throw new Exception("Email has already been added!");
                }
                else if (u.mc_email_notifications.Any(x => x.email == f))
                {
                    //email was previously deleted just reinstate
                    var uEmail = db.mc_email_notifications.First(x => x.email == f && x.report_id == u.id);
                    uEmail.deleted = false;
                }
                else
                {
                    //no email found create a new one
                    var e = new mc_email_notifications()
                    {
                        date_changed = DateTime.Now,
                        date_created = DateTime.Now,
                        deleted = false,
                        email = f,
                        isInternal = false
                    };
                    u.mc_email_notifications.Add(e);
                }
                
                db.SaveChanges();
               
                return Json(new { success = true, Message = "Updated email values:" + p.Value , id = u.id });
            }
            catch (Exception ex)
            {
                //errors.Add(ex.Message);
                return Json(new { success = false, Message = "Failed to update email. " + ex.Message, id = p.Id });
            }
            //return PartialView(db.mc_scheduled_job_params.Find(p.id));
        }

        [HttpPost]
        public ActionResult RemoveEmail(EditParaVmAlt p)
        {
            var id = p.Id;
            var emailId = int.Parse(p.Value);
            var u = db.mc_email_notifications.Find(emailId);
            
            //List<string> errors = new List<string>();
            try
            {
                if(u.report_id != id) throw new Exception("Cannot delete an email notification of another report!");
                u.deleted = true;
                db.SaveChanges();
                return Json(new { success = true, Message = "Removed email: " + u.email , id = u.report_id });
            }
            catch (Exception ex)
            {
                //errors.Add(ex.Message);
                return Json(new { success = false, Message = "Failed to delete email." + ex.Message, id = p.Id });
            }
            //return PartialView(db.mc_scheduled_job_params.Find(p.id));
        }
    }
}
