using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DDSReportingAgent.Models;
using MemberCommunications.Web.ViewModels;
using Microsoft.Ajax.Utilities;

namespace MemberCommunications.Web.Controllers
{
    public class GlobalVariablesController : Controller
    {
        private MemberComm_DWContext db = new MemberComm_DWContext();

        //
        // GET: /GlobalVariables/

        public ActionResult Index()
        {
            return View(db.mc_global_variables.Select(x => x));
        }

        ////
        //// GET: /GlobalVariables/Details/5

        //public ActionResult Details(int id)
        //{
        //    return View();
        //}
        
        //
        
        public ActionResult GlobalVariableLookup(string type)
        {
            var globalVars = new List<SearchTypeAheadEntity>();

            foreach (var g in db.mc_global_variables.Select(x => x))
            {
                globalVars.Add(new SearchTypeAheadEntity(){name = g.name,id = g.id.ToString()});
            }

            return Json(globalVars, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditGlobal(EditGlobalVM g)
        {
            try
            {
                var u = db.mc_report_params.Find(Convert.ToInt32(g.paramId));
                if (string.IsNullOrWhiteSpace(g.globalId))
                {
                    u.global_variable_id = null;
                }
                else
                {
                    u.global_variable_id = Convert.ToInt32(g.globalId);
                }
                
                db.SaveChanges();
                return Json(new { Success = "True", Message = "Global variable successfully linked.", paramId = g.paramId, globalId = g.globalId });
            
            }
            catch (Exception ex)
            {

                return Json(new { Success = "False", Message = "There was an error linking the global parameter to the report parameter. " + ex.Message, paramId = g.paramId, globalId = g.globalId });
            }
            return Json(new { Success = "False", Message = "There was an error linking the global parameter to the report parameter.", paramId = g.paramId, globalId = g.globalId });
            
        }

        // GET: /GlobalVariables/Create

        public ActionResult Create()
        {
            return View(new CreateGlobalVarViewModel());
        }

        //
        // POST: /GlobalVariables/Create

        [HttpPost]
        public ActionResult Create(CreateGlobalVarViewModel c)
        {
            try
            {
                //check name

                if (String.IsNullOrWhiteSpace(c.name))
                {

                    ModelState.AddModelError("name", "Name cannot be empty");
                }
                else
                {
                    if (c.name.Contains(" ")) ModelState.AddModelError("name", "Name cannot contain spaces");
                    if (db.mc_global_variables.Any(x => x.name == c.name)) ModelState.AddModelError("name", "This variable name is already in use");
                }


                //check type
                if (String.IsNullOrWhiteSpace(c.type)) ModelState.AddModelError("type", "You must choose a value type");

                if (ModelState.IsValid)
                {
                    var n = new mc_global_variables()
                    {
                        name = c.name,
                        current_value = "",
                        type = c.type
                    };
                    db.mc_global_variables.Add(n);
                    db.SaveChanges();

                    return RedirectToAction("Edit", new { id = n.id });
                }
                else
                {
                    return View(c);
                }

            }
            catch
            {
                return View(c);
            }
        }

        //
        // GET: /GlobalVariables/Edit/5

        public ActionResult Edit(int id)
        {
            return View(new EditGlobalVariableViewModel(db.mc_global_variables.Find(id)));
        }

        //
        // POST: /GlobalVariables/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult EditParamNumber(int pid)
        {


            return PartialView(new EditGlobalVariableViewModel(db.mc_global_variables.Find(pid)));
        }
        public ActionResult EditParamString(int pid)
        {

            return PartialView(new EditGlobalVariableViewModel(db.mc_global_variables.Find(pid)));
        }

        [HttpPost]
        public ActionResult EditParam(EditParaVm p)
        {
            var u = db.mc_global_variables.Find(p.Id);
            try
            {
                u.current_value = p.Value;

                db.SaveChanges();
                return Json(new { Success = "True", Message = "Updated parameter value.", id = u.id });
            }
            catch (Exception)
            {

                return Json(new { Success = "False", Message = "Failed to update parameter value.", id = p.Id });
            }


            //return PartialView(db.mc_scheduled_job_params.Find(p.id));
        }

        //
        // GET: /GlobalVariables/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /GlobalVariables/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }

    public class SearchTypeAheadEntity
    {
        
        public string name;
        public string id;
    }
}
