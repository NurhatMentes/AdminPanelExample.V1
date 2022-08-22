using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using AdminPanelV1.Models;

namespace AdminPanelV1.Controllers
{
    public class ServiceController : Controller
    {
        AdminV1 db = new AdminV1();

        private string WebpImage(HttpPostedFileBase imgUrl, string fileName)
        {
            string[] allowedImageTypes = new string[] { "image/jpeg", "image/jpg", "image/png" };
            if (allowedImageTypes.Contains(imgUrl.ContentType.ToLower()))
            {
                string normalImagePath = Path.Combine(Server.MapPath("~/Uploads/" + fileName), imgUrl.FileName);
                string webPFileName = Path.GetFileNameWithoutExtension(imgUrl.FileName) + ".webp";
                string webPImagePath = Path.Combine(Server.MapPath("~/Uploads/" + fileName), webPFileName);
                imgUrl.SaveAs(normalImagePath);
                var document = Aspose.Imaging.Image.Load(normalImagePath);
                Aspose.Imaging.ImageOptions.WebPOptions options = new Aspose.Imaging.ImageOptions.WebPOptions();
                document.Save(webPImagePath, options);
                return webPFileName;
            }
            return null;
        }

        // GET: Service
        public ActionResult Index()
        {
            return View(db.Services.Where(x => x.State == true).ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Services service, HttpPostedFileBase imgUrl)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName =HttpContext.User.Identity.Name.Split('|')[3];

        

            if (ModelState.IsValid)
            {
                if (imgUrl != null)
                {
                    string imgName = WebpImage(imgUrl, "Service");
                    service.ImgUrl = "/Uploads/Service/" + imgName;
                    service.UserId= userId;
                    service.State = true;

                    db.Services.Add(service);
                    db.SaveChanges();


                    TablesLogs logs = new TablesLogs();
                    logs.UserId = userId;
                    logs.ItemId = service.ServiceId;
                    logs.ItemName = service.Title;
                    logs.TableName = "Services";
                    logs.Process = service.Title + " " + "Hizmeti" + " " + userName + " " + "tarafından eklendi.";
                    logs.LogDate = DateTime.Now;
                    db.TablesLogs.Add(logs);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            return View(service);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                ViewBag.Warning = "Güncellenecek hizmet bulunamadı.";
            }

            var service = db.Services.Find(id);

            if (service == null)
            {
                return HttpNotFound();
            }

            return View(service);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int? id, Services service, HttpPostedFileBase imgUrl)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];
          

            if (ModelState.IsValid)
            {
                var serviceId = db.Services.Where(x => x.ServiceId == id).SingleOrDefault();
                if (imgUrl != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(serviceId.ImgUrl)))
                    {
                        System.IO.File.Delete((Server.MapPath((serviceId.ImgUrl))));
                    }


                    string imgName = WebpImage(imgUrl, "Service");
                    serviceId.ImgUrl = "/Uploads/Service/" + imgName;
                }

                

                serviceId.Description = service.Description;
                serviceId.Title = service.Title;
                serviceId.Tag = service.Tag;
                serviceId.EmendatorAdminId = userId;
                db.SaveChanges();

                TablesLogs logs = new TablesLogs();
                logs.UserId = userId;
                logs.ItemId = serviceId.ServiceId;
                logs.ItemName = service.Title;
                logs.TableName = "Services";
                logs.Process = service.Title + " " + "Hizmeti" + " " + userName + " " + "tarafından güncellendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Services service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var service = db.Services.Find(id);

            if (service == null)
            {
                return HttpNotFound();
            }


            service.State = false;
            db.SaveChanges();

            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName =HttpContext.User.Identity.Name.Split('|')[3];

            TablesLogs logs = new TablesLogs();

            logs.UserId = userId;
            logs.ItemId = service.ServiceId;
            logs.ItemName = service.Title;
            logs.TableName = "Services";
            logs.Process = service.Title + " " + "Hizmeti" + " " + userName + " " + "tarafından silindi.";
            logs.LogDate = DateTime.Now;
            db.TablesLogs.Add(logs);
            db.SaveChanges();

            return RedirectToAction("Index");

        }
    }
}