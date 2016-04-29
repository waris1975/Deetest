using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DDSReportingAgent.Models;
using MemberCommunications.Web.ViewModels;
using RecurrenceCalculator;

namespace MemberCommunications.Web.Controllers
{
    public class ReportsController : Controller
    {
        private MemberComm_DWContext db = new MemberComm_DWContext();
        //
        // GET: /Reports/

        public ActionResult Index()
        {
            List<IndexReportViewModel> lstIndexReportViewModels = new List<IndexReportViewModel>();
            foreach (var sr in db.mc_reports.Select(x => x))
            {
                lstIndexReportViewModels.Add(new IndexReportViewModel(sr));
            }
            return View(lstIndexReportViewModels);
        }

        //
        // GET: /Reports/Details/5

        public ActionResult Details(int id)
        {
            var mcr = db.mc_reports.Find(id);
            return View(new IndexReportViewModel(mcr));
        }

        public ActionResult CreateSchedule(int id)
        {

            return View(new AppointmentRecurrence(){ReportId = id});
        }

        [HttpPost]
        public ActionResult CreateSchedule(AppointmentRecurrence appointment)
        {
            var _calendarUtility = new Calculator();
            var r = Request["RecType"];
            var errors = new List<string>();
            //set daily by default
            var rType = RecurrenceCalculator.RecurrenceType.Daily;
            if (appointment.RecType == "weekly")
            {
                rType = RecurrenceType.Weekly;
            }
            else if (appointment.RecType == "monthly")
            {
                rType = RecurrenceType.Monthly;
            }
            else if (appointment.RecType == "yearly")
            {
                rType = RecurrenceType.Yearly;
            } else if (appointment.RecType == "daily")
            {
                rType = RecurrenceCalculator.RecurrenceType.Daily;
            }
            else
            {
                errors.Add("You must select a schedule type from the tabs above!");
            }
            if(appointment.Interval < 1) errors.Add("There must be at least one interval!");

            Appointment a = null;
            if (rType == RecurrenceType.Yearly)
            {
                var dayOfMonth = appointment.StartDate.Day;
                var monthOfYear = appointment.StartDate.Month;
                a = new Appointment()
                {
                    RecurrenceType = rType,
                    Interval = appointment.Interval,
                    StartDate = appointment.StartDate,
                    Occurrences = appointment.Occurrences,
                    DayOfMonth = dayOfMonth,
                    MonthOfYear = monthOfYear
                };
            }
            else if (rType == RecurrenceType.Monthly)
            {
                var dayOfMonth = appointment.StartDate.Day;
                a = new Appointment()
                {
                    RecurrenceType = rType,
                    Interval = appointment.Interval,
                    StartDate = appointment.StartDate,
                    Occurrences = appointment.Occurrences,
                    //Sunday = false,
                    //Monday = true,
                    //Tuesday = true,
                    //Wednesday = true,
                    //Thursday = true,
                    //Friday = true,
                    //Saturday = false,
                    DayOfMonth = dayOfMonth
                    //Day = 5
                };
                
            }
            else if (rType == RecurrenceType.Weekly)
            {
                a = new Appointment()
                {
                    RecurrenceType = rType,
                    Interval = appointment.Interval,
                    StartDate = appointment.StartDate,
                    Occurrences = appointment.Occurrences,
                    Sunday = appointment.Sunday,
                    Monday = appointment.Monday,
                    Tuesday = appointment.Tuesday,
                    Wednesday = appointment.Wednesday,
                    Thursday = appointment.Thursday,
                    Friday = appointment.Friday,
                    Saturday = appointment.Saturday
                };
            }
            else //daily with acceptable days
            {
               a = new Appointment()
                {
                    RecurrenceType = rType,
                    Interval = appointment.Interval,
                    StartDate = appointment.StartDate,
                    Occurrences = appointment.Occurrences,
                    Sunday = appointment.Sunday,
                    Monday = appointment.Monday,
                    Tuesday = appointment.Tuesday,
                    Wednesday = appointment.Wednesday,
                    Thursday = appointment.Thursday,
                    Friday = appointment.Friday,
                    Saturday = appointment.Saturday
                };
            }

           

            try
            {
                var type = appointment.RecType;
                if (type == "monthly" || type == "yearly")
                    type = type + " on " + appointment.StartDate.Day + " of every month";
                var occurences = _calendarUtility.CalculateOccurrences(a);
                //create schedule record
                var s = new mc_schedule
                {
                    date_created = DateTime.Now,
                    date_modified = DateTime.Now,
                    deleted = false,
                    interval = appointment.Interval.ToString(),
                    paused = false,
                    report_id = appointment.ReportId,
                    start_datetime = appointment.StartDate,
                    user_created = "",
                    user_modified = "",
                    sunday = appointment.Sunday,
                    monday = appointment.Monday,
                    tuesday = appointment.Tuesday,
                    wednesday = appointment.Wednesday,
                    thursday = appointment.Thursday,
                    friday = appointment.Friday,
                    saturday = appointment.Saturday,
                    occurences = appointment.Occurrences,
                    schedule_expire_datetime = occurences.Max(x => x),
                    job_type_code = appointment.RecType,
                    job_type = type 
                };

                db.mc_schedule.Add(s);
                db.SaveChanges();

                //create jobs
                foreach (var o in occurences)
                {
                    var j = new mc_scheduled_jobs
                    {
                        cancel = false,
                        complete = false,
                        date_created = DateTime.Now,
                        deleted = false,
                        in_progress = false,
                        job_type = "scheduled",
                        report_id = appointment.ReportId,
                        schedule_id = s.id,
                        start_datetime = o
                    };
                    db.mc_scheduled_jobs.Add(j);
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                errors.Add("You must choose at least one day of the week when you occurence can run!");
                foreach (var e in errors)
                {
                    ModelState.AddModelError(string.Empty, e);
                }
                
                return View(appointment);
            }

            return RedirectToAction("Details", "Reports", new {id = appointment.ReportId});
        }


        public ActionResult EditDescription(int pid)
        {

            return PartialView(db.mc_reports.Find(pid));
        }

        [HttpPost]
        public ActionResult EditDescription(EditParaVm p)
        {
            var u = db.mc_reports.Find(p.Id);
            //List<string> errors = new List<string>();
            try
            {


                u.schedule_description = p.Value;
                db.SaveChanges();
                return Json(new { success = true, Message = "Updated schedule description value." + p.Value, id = u.id });
            }
            catch (Exception ex)
            {
                //errors.Add(ex.Message);
                return Json(new { success = false, Message = "Failed to update schedule description value. " + ex.Message, id = p.Id });
            }
            //return PartialView(db.mc_scheduled_job_params.Find(p.id));
        }

        [HttpPost]
        public ActionResult EditExportType(EditParaVmAlt p)
        {
            var u = db.mc_reports.Find(p.Id);
            //List<string> errors = new List<string>();
            try
            {
                u.export_option1 = p.Value;
                u.export_option2 = p.Value2;
                if (string.IsNullOrWhiteSpace(u.export_option1)) u.export_option1 = "";
                if (string.IsNullOrWhiteSpace(u.export_option2)) u.export_option2 = "";
                if (string.IsNullOrWhiteSpace(u.export_option1) && string.IsNullOrWhiteSpace(u.export_option2)) throw new Exception("At least one export option must be selected!");
                db.SaveChanges();
                return Json(new { success = true, Message = "Updated export options values." + p.Value +" "+p.Value2, id = u.id });
            }
            catch (Exception ex)
            {
                //errors.Add(ex.Message);
                return Json(new { success = false, Message = "Failed to update export options values. " + ex.Message, id = p.Id });
            }
            //return PartialView(db.mc_scheduled_job_params.Find(p.id));
        }
        ///// <summary>
        ///// create schedule for report
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public ActionResult CreateSchedule(int id)
        //{
        //    var s = new ScheduleViewModel(db.mc_reports.Find(id));
        //    //set viewbag with the select list for interval


        //    //use a combobox and button to add multiple days to the selection with also a clear button
        //    ViewBag.week_modifier = new SelectList(new[]
        //                        {
        //                            new { Value = "1", Text = "Monday" },
        //                            new { Value = "2", Text = "Tuesday" },
        //                            new { Value = "3", Text = "Wednesday" },
        //                            new { Value = "4", Text = "Thursday" },
        //                            new { Value = "5", Text = "Friday" },
        //                            new { Value = "6", Text = "Saturday" },
        //                            new { Value = "7", Text = "Sunday" }
        //                        }, "Value", "Text");
        //    //use datepicker to add multiple days from a month to a textbox with a add and a clear button
        //    return View(s);
        //}

        //[HttpPost]
        //public ActionResult CreateSchedule(ScheduleViewModel s)
        //{
        //    var u = new mc_schedule();
        //    try
        //    {
        //        u.report_id = s.report_id;
        //        u.date_created = DateTime.Now;
        //        u.date_modified = DateTime.Now;
        //        u.interval = s.interval;
        //        u.start_datetime = s.start_datetime;
        //        u.job_type = s.job_type;
        //       // u.complete = false;
        //        u.deleted = false;
        //        u.paused = false;
        //        u.user_created = User.Identity.Name;
        //        u.user_modified = User.Identity.Name;
        //        db.mc_schedule.Add(u);
        //        db.SaveChanges();
        //        return RedirectToAction("Details", "Reports", new { id = s.report_id });
        //    }
        //    catch (Exception)
        //    {

        //        return View();
        //    }


        //}


       

        ////
        //// GET: /Reports/Create

        //public ActionResult Create()
        //{
        //    return View();
        //}

        ////
        //// POST: /Reports/Create

        //[HttpPost]
        //public ActionResult Create(FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //
        // GET: /Reports/Edit/5

        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /Reports/Edit/5

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

        //
        // GET: /Reports/Delete/5

        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /Reports/Delete/5

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
