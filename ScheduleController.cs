using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using DDSReportingAgent.Models;
using MemberCommunications.Web.ViewModels;
using RecurrenceCalculator;


namespace MemberCommunications.Web.Controllers
{
    public class ScheduleController : Controller
    {
        //
        // GET: /Schedule/
        private MemberComm_DWContext db = new MemberComm_DWContext();
        public ActionResult Index()
        {
            List<ScheduleViewModel> ls = new List<ScheduleViewModel>();
            foreach (var s in db.mc_schedule.Select(x => x))
            {
                ls.Add(new ScheduleViewModel(s));

            }

            return View(ls.AsEnumerable());
        }

        public ActionResult ScheduleIndex(int id, string job_type)
        {
            var ls = new List<ScheduleViewModel>();
            foreach (var s in db.mc_schedule.Where(x => x.report_id == id && x.deleted==false).Select(x => x))
            {
                ls.Add(new ScheduleViewModel(s));
            }
            var r = db.mc_reports.Where(x => x.id == id).Select(x => x).First();
            ViewBag.ScheduleDescription = r.schedule_description;
            ViewBag.ReportName = r.name;
            ViewBag.ReportId = id;
            //ViewBag.Title = job_type == "automated" ? "Automated Schedule" : "One Off Schedule";
            return PartialView(ls.AsEnumerable());
        }

        public ActionResult _DeleteScheduleConfirm(int schedId, int repId)
        {
            var j =
                db.mc_scheduled_jobs.Count(x => x.deleted == false && x.complete == false && x.in_progress == false && x.schedule_id == schedId);
            var m = new EditParaVm()
            {
                Id = schedId,
                Value = repId.ToString(),
                ValueModifier = j.ToString()
            };
            return PartialView(m);
        }

        [HttpPost]
        public ActionResult PostDeleteSchedule(EditParaVm p)
        {
            var scheduleId = p.Id;

            var f = db.mc_schedule.Find(scheduleId);
            if (f != null)
            {
                //mark the schedule as deleted
                f.deleted = true;
                //now delete the jobs
                var jobs = db.mc_scheduled_jobs.Where(x => x.deleted == false && x.complete == false && x.in_progress == false && x.schedule_id == scheduleId).Select(x => x);
                foreach (var j in jobs)
                {
                    j.deleted = true;
                }
                db.SaveChanges();
                return Json(new { Success = "True", Message = "Removed schedule at Id:" + scheduleId.ToString(), id = scheduleId });
            }
            return Json(new { Success = "False", Message = "Could not find schedule to remove at Id:" + scheduleId.ToString(), id = scheduleId });
        }

        ////
        //// GET: /Schedule/Details/5

        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        ////
        //// GET: /Schedule/Create

        //public ActionResult Create()
        //{
        //    return View();
        //}

        ////
        //// POST: /Schedule/Create

        //[HttpPost]
        //public ActionResult Create(FormCollection collection)
        //{
        //    try
        //    {
        //        //var s = new ScheduleWidget.ScheduledEvents.Event().MonthlyIntervalOptions = monthlyintervalen
        //        // Add insert logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        ////
        //// GET: /Schedule/Edit/5

        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /Schedule/Edit/5

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
        //// GET: /Schedule/Delete/5

        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /Schedule/Delete/5

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
