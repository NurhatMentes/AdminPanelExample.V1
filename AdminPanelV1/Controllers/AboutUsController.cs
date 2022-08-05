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
            var userCookie = Request.Cookies["userCookie"];
            TablesLogs logs = new TablesLogs();

            if (ModelState.IsValid)
            {
                var about = db.AboutUs.Where(x => x.AboutUsId == id).FirstOrDefault();
                about.Description = aboutUs.Description;
                about.EmendatorAdminId = Convert.ToInt16(userCookie["UserId"]);
                db.SaveChanges();

                logs.UserId = Convert.ToInt16(userCookie["UserId"]);
                logs.ItemId = about.AboutUsId;
                logs.ItemName = "Hakkımızda";
                logs.TableName = "AboutUs";
                logs.Process = "Hakkımızda" + " " + userCookie["FullName"] + " " + "tarafından güncellendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(aboutUs);
        }
    }
}