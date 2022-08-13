using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdminPanelV1.Models;

namespace AdminPanelV1.Controllers
{
    public class AboutUsController : Controller
    {
        AdminV1 db = new AdminV1();

        // GET: AboutUs
        public ActionResult Index()
        {
            var aboutUs = db.AboutUs.ToList();
            return View(aboutUs);
        }

        public ActionResult Edit(int id)
        {
            var aboutUs = db.AboutUs.Where(x => x.AboutUsId == id).FirstOrDefault();
            return View(aboutUs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int id, AboutUs aboutUs)
        {
            
            TablesLogs logs = new TablesLogs();
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];
            if (ModelState.IsValid)
            {
                var about = db.AboutUs.Where(x => x.AboutUsId == id).FirstOrDefault();
                about.Description = aboutUs.Description;
                about.EmendatorAdminId = userId;
                db.SaveChanges();

                logs.UserId = userId;
                logs.ItemId = about.AboutUsId;
                logs.ItemName = "Hakkımızda";
                logs.TableName = "AboutUs";
                logs.Process = "Hakkımızda" + " " + userName + " " + "tarafından güncellendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(aboutUs);
        }
    }
}