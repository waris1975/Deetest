using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using DDSReportingAgent.Models;
using MemberCommunications.Web.ViewModels;

namespace MemberCommunications.Web.Controllers
{
    

    public class JobsController : Controller
    {
        private MemberComm_DWContext db = new MemberComm_DWContext();
        //
        // GET: /Jobs/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Jobs/Details/5

        public ActionResult Details(int id)
        {

            return View(new JobViewModel(db.mc_scheduled_jobs.Find(id)));
        }

        //
        // GET: /Jobs/Create

        public ActionResult JobIndex(int id, bool allJobs)
        {
            List<JobViewModel> lstJobs = new List<JobViewModel>();
            foreach (var j in db.mc_scheduled_jobs.Where(x => x.report_id == id && x.deleted != true && x.cancel != true && x.complete != true).Select(x => x))
            {
                lstJobs.Add(new JobViewModel(j));
            }
            return PartialView(lstJobs);
        }

        public ActionResult CreateJob(int id)
        {

            return View(new mc_scheduled_jobs { report_id = id });
        }

        [HttpPost]
        public ActionResult CreateJob(mc_scheduled_jobs j)
        {
            var u = new mc_scheduled_jobs();
            try
            {
                if (j.start_datetime == null)
                    ModelState.AddModelError(String.Empty, "You must choose a time for this job to start after.");
                if (!ModelState.IsValid) throw new Exception("excption");
                
                //add job
                u.report_id = j.report_id;
                u.date_created = DateTime.Now;

                u.start_datetime = j.start_datetime;
                u.job_type = "ondemand";
                u.complete = false;
                u.deleted = false;
                u.cancel = false;

                db.mc_scheduled_jobs.Add(u);
                db.SaveChanges();

                //add parameters for the job
                var ps = db.mc_report_params.Where(x => x.report_id == u.report_id).Select(x => x);
                foreach (var p in ps)
                {
                    var i = new mc_scheduled_job_params();
                    i.report_id = p.report_id;
                    i.job_id = u.id;
                    if (p.mc_global_variables == null)
                    {
                        i.name = p.name;
                        i.type = p.type;
                        i.value = p.current_value;
                        i.value_modifier = p.value_modifier;
                        i.modifier_interval = p.modifier_interval;
                    }
                    else
                    {
                        var g = p.mc_global_variables;
                        i.name = p.name;
                        i.type = p.type;
                        i.value = g.current_value;
                        i.value_modifier = g.value_modifier;
                        i.modifier_interval = g.modifier_interval;
                    }
                    
                    db.mc_scheduled_job_params.Add(i);

                }
                db.SaveChanges();
                return RedirectToAction("Details", "Jobs", new { id = u.id });
            }
            catch (Exception ex)
            {

                return View(j);
            }


        }


        public ActionResult EditParamDate(int pid)
        {
            List<SelectListItem> lstInterval = new List<SelectListItem>();
            lstInterval.Add(new SelectListItem() { Value = "", Text = "None" });
            lstInterval.Add(new SelectListItem() { Value = "day", Text = "Daily" });
            lstInterval.Add(new SelectListItem() { Value = "month", Text = "Monthly" });
            lstInterval.Add(new SelectListItem() { Value = "year", Text = "Yearly" });
            //    new { Value = "week", Text = "Weekly" },
            //    new { Value = "month", Text = "Monthly" }
            //}, "Value", "Text");
            //List<SelectListItem>  lstJobTypes = new List<SelectListItem>();
            //lstJobTypes.Add(new SelectListItem() { Value = "automated", Text = "Automated Job" }); 
            ViewBag.lstInterval = lstInterval;
            return PartialView(db.mc_scheduled_job_params.Find(pid));
        }
        public ActionResult EditParamNumber(int pid)
        {


            return PartialView(db.mc_scheduled_job_params.Find(pid));
        }
        public ActionResult EditParamString(int pid)
        {

            return PartialView(db.mc_scheduled_job_params.Find(pid));
        }

        [HttpPost]
        public ActionResult EditParam(EditParaVm p)
        {
            var u = db.mc_scheduled_job_params.Find(p.Id);
            try
            {
                u.modifier_interval = p.ModifierInterval;
                u.value_modifier = p.ValueModifier;
                u.value = p.Value;
                db.SaveChanges();
                return Json(new { Success = "True", Message = "Updated parameter value.", id = u.id });
            }
            catch (Exception)
            {

                return Json(new { Success = "False", Message = "Failed to update parameter value.", id = p.Id });
            }


           
        }

        public ActionResult _DeleteJobConfirm(int jobId, int repId, bool allJobs)
        {
            var m = new EditParaVm()
            {
                Id = jobId,
                Value = repId.ToString(),
                ValueModifier = allJobs.ToString()
            };
            return PartialView(m);
        }

        [HttpPost]
        public ActionResult PostDeleteJob(EditParaVm p)
        {
            var jobId = p.Id;
            var f = db.mc_scheduled_jobs.Find(jobId);
            
            if (f != null)
            {
                //db.mc_scheduled_jobs.Remove(f);
                f.deleted = true;
                
                db.SaveChanges();
                return Json(new { Success = "True", Message = "Removed Job at Id:" + jobId, id = jobId });
            }
            return Json(new { Success = "False", Message = "Could not find job to remove at Id:" + jobId, id = jobId });
        }
        ////
        //// GET: /Jobs/Edit/5

        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /Jobs/Edit/5

        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        ////
        //// GET: /Jobs/Delete/5

        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /Jobs/Delete/5

        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
