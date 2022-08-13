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

            var category = db.Category;
            return View(category.ToList());
        }

        // GET: Category/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Category.Find(id);
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
        public ActionResult Create([Bind(Include = "CategoryId,ParentId,CategoryName,Description,ImgUrl")] Category category, HttpPostedFileBase imgUrl)
        {
            if (ModelState.IsValid)
            {
                if (imgUrl != null)
                {
                    WebImage image = new WebImage(imgUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(imgUrl.FileName);

                    string imgName = Guid.NewGuid() + fileInfo.Extension;
                    image.Resize(600, 400);
                    image.Save("~/Uploads/Category/" + imgName);

                    category.ImgUrl = "/Uploads/Category/" + imgName;


                    db.Category.Add(category);
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
            Category category = db.Category.Find(id);
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
        public ActionResult Edit([Bind(Include = "CategoryId,ParentId,CategoryName,Description,ImgUrl")] Category category, HttpPostedFileBase imgUrl, int id)
        {
            var categoryId = db.Category.Where(x => x.CategoryId == id).SingleOrDefault();

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


                }
                categoryId.CategoryName = category.CategoryName;
                categoryId.Description = category.Description;

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
            Category category = db.Category.Find(id);
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
            Category category = db.Category.Find(id);
            try
            {
                if (category == null)
                {
                    return HttpNotFound();
                }

                if (System.IO.File.Exists(Server.MapPath(category.ImgUrl)))
                {
                    System.IO.File.Delete(Server.MapPath(category.ImgUrl));
                }


                db.Category.Remove(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
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