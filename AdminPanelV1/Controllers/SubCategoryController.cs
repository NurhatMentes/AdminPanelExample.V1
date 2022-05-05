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
    public class SubCategoryController : Controller
    {
        AdminV1 db = new AdminV1();

        // GET: SubCategory
        public ActionResult Index()
        {
            var subCategory = db.SubCategory.Include("Category");
            return View(subCategory.ToList());
        }


        // GET: SubCategory/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Category, "CategoryId", "CategoryName");
            return View();
        }

        // POST: SubCategory/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SubCategoryId,CategoryId,SubCategoryName,ImgUrl")] SubCategory subCategory, HttpPostedFileBase imgUrl)
        {
            if (ModelState.IsValid)
            {

                if (imgUrl != null)
                {
                    WebImage image = new WebImage(imgUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(imgUrl.FileName);

                    string imgName = Guid.NewGuid() + fileInfo.Extension;
                    image.Resize(600, 400);
                    image.Save("~/Uploads/SubCategory/" + imgName);

                    subCategory.ImgUrl = "/Uploads/SubCategory/" + imgName;
                }

                db.SubCategory.Add(subCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Category, "CategoryId", "CategoryName", subCategory.CategoryId);
            return View(subCategory);
        }

        // GET: SubCategory/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubCategory subCategory = db.SubCategory.Find(id);
            if (subCategory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Category, "CategoryId", "CategoryName", subCategory.CategoryId);
            return View(subCategory);
        }

        // POST: SubCategory/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SubCategoryId,CategoryId,SubCategoryName,ImgUrl")] SubCategory subCategory, HttpPostedFileBase imgUrl, int id)
        {
            var categoryId = db.SubCategory.Where(x => x.SubCategoryId == id).SingleOrDefault();
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
                    image.Save("~/Uploads/SubCategory/" + imgName);

                    categoryId.ImgUrl = "/Uploads/SubCategory/" + imgName;
                }


                categoryId.SubCategoryName = subCategory.SubCategoryName;
                categoryId.CategoryId = subCategory.CategoryId;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Category, "CategoryId", "CategoryName", subCategory.CategoryId);
            return View(subCategory);
        }

        // GET: SubCategory/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubCategory subCategory = db.SubCategory.Find(id);
            if (subCategory == null)
            {
                return HttpNotFound();
            }
            return View(subCategory);
        }

        // POST: SubCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SubCategory subCategory = db.SubCategory.Find(id);
            try
            {
                if (subCategory == null)
                {
                    return HttpNotFound();
                }

                if (System.IO.File.Exists(Server.MapPath(subCategory.ImgUrl)))
                {
                    System.IO.File.Delete(Server.MapPath(subCategory.ImgUrl));
                }


                db.SubCategory.Remove(subCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ViewBag.Danger = "Kategoriyi silmek için öncelikle kategoriye ait ürünleri silmeniz gerekir";
                return View(subCategory);
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