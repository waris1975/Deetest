using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DDSReportingAgent.Models;
using MemberCommunications.Web.ViewModels;

namespace MemberCommunications.Web.Controllers
{
    public class FormulasController : Controller
    {
        private MemberComm_DWContext db = new MemberComm_DWContext();
        //
        // GET: /Formulas/

        public ActionResult FormulasIndex(IEnumerable<mc_report_formulas> mcf)
        {
            return View(mcf);
        }

        public ActionResult FormulasUsedIndex(IEnumerable<mc_report_formulas> mcf)
        {
            List<FormulaViewModel> uf = (from m in mcf where m.use_formula == true select new FormulaViewModel(m)).ToList();
            return View(uf.AsEnumerable());
        }
        ////
        //// GET: /Formulas/Details/5

        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        ////
        //// GET: /Formulas/Create

        //public ActionResult Create()
        //{
        //    return View();
        //}

        ////
        //// POST: /Formulas/Create

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

        ////
        //// GET: /Formulas/Edit/5

        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //
        // POST: /Formulas/Edit/5

        [HttpPost]
        public ActionResult Edit(FormulaUsageViewModel f)
        {
            try
            {
                // TODO: Add update logic here

                var u = db.mc_report_formulas.Find(f.id);
                    u.use_formula = Convert.ToBoolean(f.use_formula);
                    db.SaveChanges();

                return Json(new {Success = "True", Message = "Updated formula usage."});
            }
            catch
            {
                return Json(new { Success = "False", Message = "Error updating formula usage." });
            }
        }

        //
        // GET: /Formulas/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Formulas/Delete/5

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
}
