using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using AdminPanelV1.Models;

namespace AdminPanelV1.Controllers
{
    public class SiteIdentityController : Controller
    {
        AdminV1 db = new AdminV1();

        // GET: SiteIdentity
        public ActionResult Index()
        {
            return View(db.SiteIdentity.ToList());
        }


        // GET: SiteIdentity/Edit/5
        public ActionResult Edit(int id)
        {
            var identity = db.SiteIdentity.Where(x => x.IdentityId == id).SingleOrDefault();
            return View(identity);
        }

        // POST: SiteIdentity/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int id, SiteIdentity siteIdentity, HttpPostedFileBase LogoUrl)
        {
            if (ModelState.IsValid)
            {
                var identity = db.SiteIdentity.Where(x => x.IdentityId == id).SingleOrDefault();
                string logoName;


                if (LogoUrl != null)
                {

                    if (System.IO.File.Exists(Server.MapPath(identity.LogoUrl)))
                    {
                        System.IO.File.Delete(Server.MapPath(identity.LogoUrl));
                    }

                    WebImage image = new WebImage(LogoUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(LogoUrl.FileName);

                    logoName = LogoUrl.FileName + fileInfo.Extension;

                    image.Resize(300, 200);
                    image.Save("~/Uploads/SiteIdentity/" + logoName);

                    identity.LogoUrl = "/Uploads/SiteIdentity/" + logoName;
                }


                identity.Title = siteIdentity.Title;
                identity.Description = siteIdentity.Description;
                identity.Keywords = siteIdentity.Keywords;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(siteIdentity);
        }
    }
}