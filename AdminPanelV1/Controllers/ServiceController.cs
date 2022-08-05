﻿using System;
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
        public ActionResult Create(Services service, HttpPostedFileBase ImgUrl)
        {
            var userCookie = Request.Cookies["userCookie"];
            TablesLogs logs = new TablesLogs();

            if (ModelState.IsValid)
            {
                if (ImgUrl != null)
                {
                    WebImage image = new WebImage(ImgUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(ImgUrl.FileName);

                    string imgName = Guid.NewGuid() + fileInfo.Extension;
                    image.Resize(750, 600);
                    image.Save("~/Uploads/Service/" + imgName);

                    service.ImgUrl = "/Uploads/Service/" + imgName;
                    service.UserId= Convert.ToInt16(userCookie["UserId"]);


                    db.Services.Add(service);
                    db.SaveChanges();

                    logs.UserId = Convert.ToInt16(userCookie["UserId"]);
                    logs.ItemId = service.ServiceId;
                    logs.ItemName = service.Title;
                    logs.TableName = "Services";
                    logs.Process = service.Title + " " + "Hizmeti" + " " + userCookie["FullName"] + " " + "tarafından eklendi.";
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
        public ActionResult Edit(int? id, Services service, HttpPostedFileBase ImgUrl)
        {
            var userCookie = Request.Cookies["userCookie"];
            TablesLogs logs = new TablesLogs();

            if (ModelState.IsValid)
            {
                var serviceId = db.Services.Where(x => x.ServiceId == id).SingleOrDefault();
                if (ImgUrl != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(serviceId.ImgUrl)))
                    {
                        System.IO.File.Delete((Server.MapPath((serviceId.ImgUrl))));
                    }

                    WebImage image = new WebImage(ImgUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(ImgUrl.FileName);

                    string imgName = Guid.NewGuid() + fileInfo.Extension;
                    image.Resize(750, 600);
                    image.Save("~/Uploads/Service/" + imgName);

                    serviceId.ImgUrl = "/Uploads/Service/" + imgName;
                }

                serviceId.Description = service.Description;
                serviceId.Title = service.Title;
                serviceId.Tag = service.Tag;
                serviceId.EmendatorAdminId = Convert.ToInt16(userCookie["UserId"]);

                db.SaveChanges();
                logs.UserId = Convert.ToInt16(userCookie["UserId"]);
                logs.ItemId = serviceId.ServiceId;
                logs.ItemName = service.Title;
                logs.TableName = "Services";
                logs.Process = service.Title + " " + "Hizmeti" + " " + userCookie["FullName"] + " " + "tarafından güncellendi.";
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

            var userCookie = Request.Cookies["userCookie"];
            TablesLogs logs = new TablesLogs();

            logs.UserId = Convert.ToInt16(userCookie["UserId"]);
            logs.ItemId = service.ServiceId;
            logs.ItemName = service.Title;
            logs.TableName = "Services";
            logs.Process = service.Title + " " + "Hizmeti" + " " + userCookie["FullName"] + " " + "tarafından silindi.";
            logs.LogDate = DateTime.Now;
            db.TablesLogs.Add(logs);
            db.SaveChanges();

            return RedirectToAction("Index");

        }
    }
}