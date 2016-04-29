using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DDSReportingAgent.Models;
using MVCControlsToolkit.Linq;
using SimpleExamples.Models;

namespace MemberCommunications.Web.Controllers
{
    public class HomeController : Controller
    {
        private MemberComm_DWContext db = new MemberComm_DWContext();
        public ActionResult Index()
        {
            ViewBag.Message = "Allow users to configure, schedule and manage their Crystal Reports with minimal technical intervention.";
            
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult TreeViewExample()
        {
            var model = new TreeViewExampleViewModel();
            model.EmailFolders = new List<EmailElement>();
            EmailElement friends = new EmailFolder()
            {
                Name = "Friends",
                Children =
                    new List<EmailElement>{
                            new EmailDocument{
                                Name="EMail_Friend_1"
                            },
                            new EmailDocument{
                                Name="EMail_Friend_2"
                            },
                            new EmailDocument{
                                Name="EMail_Friend_3"
                            }
                        }

            };
            model.EmailFolders.Add(friends);

            EmailElement relatives = new EmailFolder()
            {
                Name = "Relatives",
                Children =
                    new List<EmailElement>{
                            new EmailDocument{
                                Name="EMail_Relatives_1"
                            },
                            new EmailDocument{
                                Name="EMail_Relatives_2"
                            },
                            new EmailDocument{
                                Name="EMail_Relatives_3"
                            }
                        }

            };
            model.EmailFolders.Add(relatives);
            EmailElement work = new EmailFolder()
            {
                Name = "Work",
                Children =
                    new List<EmailElement>{
                            new EmailDocument{
                                Name="EMail_Work_1"
                            },
                            new EmailDocument{
                                Name="EMail_Work2"
                            },
                            new EmailDocument{
                                Name="EMail_Work_3"
                            }
                        }

            };
            model.EmailFolders.Add(work);
            model.EmailFolders = new List<EmailElement>
            {
                new EmailFolder{
                    Name = "All Folders",
                    Children = model.EmailFolders
                }
            };
            return View(model);
        }
        [HttpPost]
        public ActionResult TreeViewExample(TreeViewExampleViewModel model)
        {
            if (ModelState.IsValid)
            {
                ModelState.Clear();

            }
            return View(model);
        }

        public ActionResult KendoTree()
        {
            return View();
        }

        public ActionResult Index2()
        {
            return View();
        }
        public ActionResult Basic()
        {
            return View();
        }
        public ActionResult BasicPlus()
        {
            return View();
        }
        public ActionResult AngularJS()
        {
            return View();
        }
        public ActionResult JQueryUI()
        {
            return View();
        }
    }
}
