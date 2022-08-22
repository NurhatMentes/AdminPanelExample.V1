using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.HtmlControls;
using AdminPanelV1.Models;

namespace AdminPanelV1.Controllers
{
    public class HomeVideoController : Controller
    {
        AdminV1 db = new AdminV1();


        // GET: HomeVideo
        public ActionResult Index()
        {
            var homeVideo = db.HomeVideo;

            return View(homeVideo.ToList());
        }

       
        // GET: HomeVideo/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeVideo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "HomeVideoId,Title,Description,VideoUrl")] HomeVideo homeVideo, HttpPostedFileBase VideoUrl)
        {
            if (VideoUrl != null)
            {
                string folderPath = Server.MapPath("~/Uploads/HomeVideo/");
                HtmlVideo video = new HtmlVideo();

                FileInfo fileInfo = new FileInfo(VideoUrl.FileName);
                video.Src = "~/Uploads/HomeVideo/" + VideoUrl.FileName + fileInfo.Extension;

                db.HomeVideo.Add(new HomeVideo
                {
                    Title = homeVideo.Title,
                    Description = homeVideo.Description,
                    VideoUrl = "/Uploads/HomeVideo/" + fileInfo.Name
                });
                var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);

                homeVideo.EmendatorAdminId = userId;
                VideoUrl.SaveAs(folderPath + fileInfo.Name);

             
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Warning = "Please select a video";
            }

           
            return View(homeVideo);
        }

        // GET: HomeVideo/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HomeVideo homeVideo = db.HomeVideo.Find(id);
            if (homeVideo == null)
            {
                return HttpNotFound();
            }
            return View(homeVideo);
        }

        // POST: HomeVideo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "HomeVideoId,Title,Description,VideoUrl")] HomeVideo homeVideo, HttpPostedFileBase VideoUrl, int id)
        {
            if (ModelState.IsValid)
            {
                var videoUpdate = db.HomeVideo.Where(x => x.HomeVideoId == id).SingleOrDefault();
                string folderPath = Server.MapPath("~/Uploads/HomeVideo/");
                HtmlVideo video = new HtmlVideo();
                FileInfo fileInfo = new FileInfo(VideoUrl.FileName);
                video.Src = "~/Uploads/HomeVideo/" + VideoUrl.FileName + fileInfo.Extension;

                if (System.IO.File.Exists(Server.MapPath(homeVideo.VideoUrl)))
                {
                    System.IO.File.Delete(Server.MapPath(homeVideo.VideoUrl));
                }

                var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
                var userName =HttpContext.User.Identity.Name.Split('|')[3];

                videoUpdate.EmendatorAdminId = userId;
                videoUpdate.Title = homeVideo.Title;
                videoUpdate.Description = homeVideo.Description;
                videoUpdate.VideoUrl = "/Uploads/HomeVideo/" + fileInfo.Name;
                VideoUrl.SaveAs(folderPath + fileInfo.Name);
                db.SaveChanges();

                TablesLogs logs = new TablesLogs();
                logs.ItemId = videoUpdate.HomeVideoId;
                logs.UserId = userId;
                logs.ItemName = videoUpdate.Title;
                logs.TableName = "HomeVideo";
                logs.LogDate = DateTime.Now;
                logs.Process = videoUpdate.Title + " " + "Ana sayfa video" + " " + userName + " " + "tarafından güncellendi.";
                db.TablesLogs.Add(logs);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(homeVideo);
        }

        // GET: HomeVideo/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HomeVideo homeVideo = db.HomeVideo.Find(id);
            if (homeVideo == null)
            {
                return HttpNotFound();
            }
            return View(homeVideo);
        }

        // POST: HomeVideo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            HomeVideo homeVideo = db.HomeVideo.Find(id);


            db.HomeVideo.Remove(homeVideo);
            db.SaveChanges();

            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName =HttpContext.User.Identity.Name.Split('|')[3];
            TablesLogs logs = new TablesLogs();

            logs.UserId = userId;
            logs.ItemId = homeVideo.HomeVideoId;
            logs.ItemName = homeVideo.Title;
            logs.TableName = "HomeVideo";
            logs.Process = homeVideo.Title + " " + "Videosu" + " " + userName + " " + "tarafından silindi.";
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