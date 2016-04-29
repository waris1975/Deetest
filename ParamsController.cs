using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DDSReportingAgent.Models;
using MemberCommunications.Web.ViewModels;

namespace MemberCommunications.Web.Controllers
{
    public class ParamsController : Controller
    {
        //
        // GET: /Params/
        private MemberComm_DWContext db = new MemberComm_DWContext();

        public ActionResult ParamsIndex(IEnumerable<mc_report_params> mcp)
        {
            //var mc_params = db.mc_report_params.Where(x => x.report_id == reportId).Select(x => x);
            return View(mcp);
        }

        public ActionResult ParamsUsedIndex(IEnumerable<mc_report_params> mcp)
        {
            var mc_params = db.mc_report_params.Where(x => x.id == 84).Select(x => x);
            return View();
        }

        public ActionResult EditString(int id)
        {
            var e = db.mc_report_params.Find(id);
            return View(e);
        }

        //
        // POST: /Params/Edit/5

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditString(string editor, string content)
        {
            try
            {
                int id = 0;
                Int32.TryParse(editor, out id);
                if (id > 0)
                {
                    var u = db.mc_report_params.Find(id);
                    u.current_value = content;
                    db.SaveChanges();
                }
                else
                {
                    new Exception();
                }
                return Json("success");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult EditDate(int id)
        {
            var e = db.mc_report_params.Find(id);
            return View(e);
        }

        //
        // POST: /Params/Edit/5

        [HttpPost]
        public ActionResult EditDate(mc_report_params mr)
        {
            try
            {
                var u = db.mc_report_params.Find(mr.id);
                //try parse date
                bool dateOk = false;
                if (!(mr.current_value.Contains("NOW")))
                {
                   DateTime tryDate;
                    dateOk = DateTime.TryParse(mr.current_value, out tryDate); 
                } else if (mr.current_value.Contains("NOW"))
                {
                    dateOk = true;
                }
                
                //check modifier
                bool modOk = false;
                int tryInt = 0;
                if (String.IsNullOrEmpty(mr.value_modifier) || Int32.TryParse(mr.value_modifier, out tryInt)) modOk = true;

                if (dateOk) u.current_value = mr.current_value;
                if (modOk) u.value_modifier = mr.value_modifier;
                u.modifier_interval = mr.modifier_interval;
                db.SaveChanges();
                
                return RedirectToAction("Details","Reports", new {id=u.report_id});
            }
            catch
            {
                return View();
            }
        }

        public ActionResult EditNumber(int id)
        {
            var e = db.mc_report_params.Find(id);
            return View(e);
        }

        //
        // POST: /Params/Edit/5

        [HttpPost]
        public ActionResult EditNumber(mc_report_params mr)
        {
            try
            {
                var u = db.mc_report_params.Find(mr.id);
                int tryInt = 0;

                if (Int32.TryParse(mr.current_value, out tryInt))
                {
                    u.current_value = mr.current_value;
                    db.SaveChanges();
                }
                return RedirectToAction("Details", "Reports", new { id = u.report_id });
            }
            catch
            {
                return View();
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
            return PartialView(db.mc_report_params.Find(pid));
        }
        public ActionResult EditParamNumber(int pid)
        {


            return PartialView(db.mc_report_params.Find(pid));
        }
        public ActionResult EditParamString(int pid)
        {

            return PartialView(db.mc_report_params.Find(pid));
        }

        [HttpPost]
        public ActionResult EditParam(EditParaVm p)
        {
            var u = db.mc_report_params.Find(p.Id);
            //List<string> errors = new List<string>();
            try
            {
                
                
                var d = new DateTime();
                if (DateTime.TryParse(p.Value, out d) || p.Value == "NOW")
                {
                    if (!String.IsNullOrWhiteSpace(p.ValueModifier))
                    {
                        u.value_modifier = Convert.ToInt32(p.ValueModifier).ToString();
                        if (string.IsNullOrWhiteSpace(p.ModifierInterval))
                        {
                           throw new Exception("You must select a modifier interval if you are selecting a date modifier.");
                        }
                        u.modifier_interval = p.ModifierInterval;
                    }
                    
                }
                
                u.value_modifier = p.ValueModifier;
                u.current_value = p.Value;
                db.SaveChanges();
                return Json(new { success = true, Message = "Updated parameter value."+p.ModifierInterval, id = u.id });
            }
            catch (Exception ex)
            {
                //errors.Add(ex.Message);
                return Json(new { success = false, Message = "Failed to update parameter value. " + ex.Message, id = p.Id });
            }


            //return PartialView(db.mc_scheduled_job_params.Find(p.id));
        }

        ////
        //// GET: /Params/Details/5

        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        ////
        //// GET: /Params/Create

        //public ActionResult Create()
        //{
        //    return View();
        //}

        ////
        //// POST: /Params/Create

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
        // GET: /Params/Edit/5

        

        ////
        //// GET: /Params/Delete/5

        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /Params/Delete/5

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
