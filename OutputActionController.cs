using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Backload.Contracts.Context.Config;
using MemberCommunications.Web.ViewModels;
using MemberCommunications.Web.Helpers;
using DDSReportingAgent.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MemberCommunications.Web.Controllers
{
    public class OutputActionController : Controller
    {
        //
        // GET: /OutputAction/
        MemberComm_DWContext db = new MemberComm_DWContext();

        public ActionResult _AllOutputActions(int repId)
        {
            
            return PartialView(db.mc_output_actions.Where(x=>x.report_id==repId && x.active == true && x.delted==false).Select(x=>x).ToList());
        }
        public ActionResult _GetImagesFromFileForm(int repId)
        {
            var m = new EditParaVm()
            {
                Id = repId
            };
            return PartialView(m);
        }

        [HttpPost]
        public ActionResult GetImagesFromPDF(EditParaVmAlt p)
        {
            var id = p.Value;
            var h = new pdfHelper();
            var images = h.GetImages(id);
            h = null;
            List<PDFImageVM> vmImages = new List<PDFImageVM>();
            foreach (var i in images)
            {
                vmImages.Add(
                    new PDFImageVM()
                    {
                        CalcSize = i.CalcSize.ToString(),
                        width = i.width,
                        height = i.height,
                        imagePath = i.imageRelativePath()
                    });
            }
            
            JObject json = new JObject();
            json["jsonList"] = JToken.FromObject(vmImages);
            
            return new JSONNetResult(json);
        }

        [HttpPost]
        public ActionResult PostImagesFromFile(EditParaVmAlt2 p)
        {
            
            if (p.Value == null || string.IsNullOrWhiteSpace(p.Value)) ModelState.AddModelError("sourceImage", "You must upload a source image.");

            //copy the sourceImage from the temp location to the permanent location
            var path = CustomHelper.Pathing.GetUNCPath(HostingEnvironment.ApplicationPhysicalPath);
            var path2 = @"\\FPS1\Reports\Member Communications\mc_resources\UsedImages\";
            var sourceLocation = path + "Files\\" + p.Value;
            var targetLocation = path2 +  p.Value;
            if (!System.IO.File.Exists(sourceLocation)) ModelState.AddModelError("sourceImage", "You must upload a source image.");
            
            //if (System.IO.File.Exists(targetLocation)) ModelState.AddModelError("sourceImage", "There is already a file of this name. Please, rename your file and re-upload.");
            if (ModelState.IsValid)
            {
                if (!System.IO.File.Exists(targetLocation)) System.IO.File.Copy(sourceLocation, targetLocation);

                var action = new mc_output_actions()
                {
                    active = true,
                    date_changed = DateTime.Now,
                    date_created = DateTime.Now,
                    delted = false,
                    params_xml = p.Value,
                    description = "Switch images in PDF",
                    report_id = p.Id,
                    output_type = "pdf",
                    output_action = "PDFImageSubstitution",
                    value1 = targetLocation,
                    value2 = p.Value2,
                    value3 = p.Value3,
                    value4 = p.Value4
                };

                db.mc_output_actions.Add(action);
                db.SaveChanges();
                return
                    Json(
                        new
                        {
                            success = true,
                            Message = "Created new image substitution.",
                            id = p.Id,
                            image = p.Value,
                            w = p.Value2,
                            h = p.Value3,
                            s = p.Value4
                        });

            }
            else
            {
                var l = new List<string>();
                foreach (var v in ModelState["imageSource"].Errors)
                {
                    
                    l.Add(v.ErrorMessage.ToString());
                }
                JObject json = new JObject();
                json["success"] = false;
                json["jsonList"] = JToken.FromObject(l);
                return new JSONNetResult(json);
            }


        }

        [HttpPost]
        public ActionResult PostDeleteOutputAction(EditParaVmAlt p)
        {
            var a = db.mc_output_actions.Find(p.Id);
            if (a != null)
            {
                db.mc_output_actions.Remove(a);
                db.SaveChanges();
            }
            else
            {
                return
                    Json(
                        new
                        {
                            success = true,
                            Message = "Could not delete output action for: "+p.Id,
                            id = p.Id
                        });
            }
            return
                    Json(
                        new
                        {
                            success = true,
                            Message = "Deleted Output Action: " + p.Id,
                            id = p.Id
                        });
        }

        public ActionResult GetImageSubstitutionForm()
        {
            return View();
        }

        public ActionResult PostImageSubstitution(PDFSubstituteImage psImage)
        {
            return View();
        }

        //
        // GET: /OutputAction/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /OutputAction/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /OutputAction/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /OutputAction/Edit/5

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

        //
        // GET: /OutputAction/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /OutputAction/Delete/5

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

        public class JSONNetResult : ActionResult
        {
            private readonly JObject _data;
            public JSONNetResult(JObject data)
            {
                _data = data;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                var response = context.HttpContext.Response;
                response.ContentType = "application/json";
                response.Write(_data.ToString(Newtonsoft.Json.Formatting.None));
            }
        }
    }
}
