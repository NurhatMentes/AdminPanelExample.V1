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
    public class CategoryController : Controller
    {
        AdminV1 db = new AdminV1();

        // GET: Category
        public ActionResult Index()
        {
            var category = db.Categories.Where(x => x.State == true);
            return View(category.ToList());
        }

        // GET: Category/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Categories category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryId,ParentId,CategoryName,Description,ImgUrl")] Categories category, HttpPostedFileBase imgUrl)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];
            if (ModelState.IsValid)
            {
                if (imgUrl != null)
                {
                    //var userCookie = Request.Cookies["userCookie"];

                    WebImage image = new WebImage(imgUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(imgUrl.FileName);

                    string imgName = Guid.NewGuid() + fileInfo.Extension;
                    image.Resize(600, 400);
                    image.Save("~/Uploads/Category/" + imgName);
                    category.ImgUrl = "/Uploads/Category/" + imgName;
                    category.State = true;
                    category.UserId = userId;
                    db.Categories.Add(category);
                    db.SaveChanges();

                    TablesLogs logs = new TablesLogs();
                    var cat = db.Categories.OrderByDescending(x => x.CategoryId).FirstOrDefault();
                    logs.ItemId = cat.CategoryId;
                    logs.UserId = userId;
                    logs.ItemName = cat.CategoryName;
                    logs.TableName = "Categories";
                    logs.LogDate = DateTime.Now;
                    logs.Process = cat.CategoryName + " " + "kategorisi" + " " + userName + " " + "tarafından eklendi.";
                    db.TablesLogs.Add(logs);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Warning = "Lütfen resim seçiniz";
                }
            }

            return View(category);
        }
        // GET: Category/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Categories category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryId,ParentId,CategoryName,Description,ImgUrl")] Categories category, HttpPostedFileBase imgUrl, int id)
        {
            var categoryId = db.Categories.Where(x => x.CategoryId == id).SingleOrDefault();
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];

            if (ModelState.IsValid)
            {
                if (imgUrl != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(categoryId.ImgUrl)))
                    {
                        System.IO.File.Delete(Server.MapPath(categoryId.ImgUrl));
                    }

                    WebImage image = new WebImage(imgUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(imgUrl.FileName);

                    string imgName = Guid.NewGuid() + fileInfo.Extension;
                    image.Resize(600, 400);
                    image.Save("~/Uploads/Category/" + imgName);

                    categoryId.ImgUrl = "/Uploads/Category/" + imgName;

                    category.EmendatorAdminId = userId;



                }
                categoryId.CategoryName = category.CategoryName;
                categoryId.Description = category.Description;



                TablesLogs logs = new TablesLogs();
                logs.ItemId = category.CategoryId;
                logs.UserId = userId;
                logs.ItemName = category.CategoryName;
                logs.TableName = "Categories";
                logs.LogDate = DateTime.Now;
                logs.Process = category.CategoryName + " " + "kategorisi" + " " + userName + " " + "tarafından güncellendi.";
                db.TablesLogs.Add(logs);
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            return View(category);
        }

        // GET: Category/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Categories category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Categories category = db.Categories.Find(id);
            try
            {
                if (category == null)
                {
                    return HttpNotFound();
                }

                var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
                var userName = HttpContext.User.Identity.Name.Split('|')[3];

                category.State = false;
                db.SaveChanges();

                TablesLogs logs = new TablesLogs();
                logs.ItemId = category.CategoryId;
                logs.UserId = userId;
                logs.ItemName = category.CategoryName;
                logs.TableName = "Categories";
                logs.LogDate = DateTime.Now;
                logs.Process = category.CategoryName + " " + "Kategorisi" + " " + userName + " " + "tarafından silindi.";
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Danger = "Kategoriyi silmek için öncelikle kategoriye ait ürünleri silmeniz gerekir";
                return View(category);
            }
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