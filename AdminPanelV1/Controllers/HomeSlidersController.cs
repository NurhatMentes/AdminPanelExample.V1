using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using AdminPanelV1.Models;

namespace AdminPanelV1.Controllers
{
    public class HomeSlidersController : Controller
    {
        private AdminV1 db = new AdminV1();

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


        // GET: HomeSliders
        public ActionResult Index()
        {
            var slider = db.Sliders.Where(x=>x.State==true);
            return View(slider.ToList());
        }



        // GET: Sliders/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sliders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Sliders slider, HttpPostedFileBase imgUrl)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName =HttpContext.User.Identity.Name.Split('|')[3];
            
            if (imgUrl != null)
            {
                string imgName = WebpImage(imgUrl, "HomeSlider");
                slider.ImgUrl = "/Uploads/HomeSlider/" + imgName;
                slider.State = true;
                db.Sliders.Add(slider);
                db.SaveChanges();


                TablesLogs logs = new TablesLogs();
                logs.UserId = userId;
                logs.ItemId = slider.SliderId;
                logs.ItemName = slider.Title;
                logs.TableName = "Sliders";
                logs.Process = slider.Title + " " + "Resmi" + " " + userName + " " + "tarafından eklendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(slider);
        }

        // GET: Sliders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sliders slider = db.Sliders.Find(id);
            if (slider == null)
            {
                return HttpNotFound();
            }

            return View(slider);
        }

        // POST: Sliders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Sliders slider, HttpPostedFileBase imgUrl, int id)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName =HttpContext.User.Identity.Name.Split('|')[3];
           

            var sliderId = db.Sliders.Where(x => x.SliderId == id).SingleOrDefault();

            if (ModelState.IsValid)
            {
                if (imgUrl != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(sliderId.ImgUrl)))
                    {
                        System.IO.File.Delete((Server.MapPath((sliderId.ImgUrl))));
                    }

                    string imgName = WebpImage(imgUrl, "HomeSlider");
                    sliderId.ImgUrl = "/Uploads/HomeSlider/" + imgName;
                }

                sliderId.Title = slider.Title;
                sliderId.Description = slider.Description;
                sliderId.EmendatorAdminId= userId;
                db.SaveChanges();


                TablesLogs logs = new TablesLogs();
                logs.UserId = userId;
                logs.ItemId = slider.SliderId;
                logs.ItemName = slider.Title;
                logs.TableName = "Sliders";
                logs.Process = slider.Title + " " + "Resmi" + " " + userName+ " " + "tarafından eklendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(slider);
        }

        // GET: Sliders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sliders slider = db.Sliders.Find(id);
            if (slider == null)
            {
                return HttpNotFound();
            }
            return View(slider);
        }

        // POST: Sliders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sliders slider = db.Sliders.Find(id);

            slider.State = false;
            db.SaveChanges();

            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName =HttpContext.User.Identity.Name.Split('|')[3];
            TablesLogs logs = new TablesLogs();

            logs.UserId = userId;
            logs.ItemId = slider.SliderId;
            logs.ItemName = slider.Title;
            logs.TableName = "Sliders";
            logs.Process = slider.Title + " " + "Ürünü" + " " + userName + " " + "tarafından silindi.";
            logs.LogDate = DateTime.Now;
            db.TablesLogs.Add(logs);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
