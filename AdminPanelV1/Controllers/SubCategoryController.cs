using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Services.Description;
using AdminPanelV1.Models;

namespace AdminPanelV1.Controllers
{
    public class SubCategoryController : Controller
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


        // GET: SubCategory
        public ActionResult Index()
        {
            var subCategory = db.SubCategories.Include("Categories");
            return View(subCategory.Where(x => x.State == true).ToList());
        }


        // GET: SubCategory/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: SubCategory/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SubCategories subCategory, HttpPostedFileBase imgUrl)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];
           

            if (ModelState.IsValid)
            {

                if (imgUrl != null)
                {
                    string imgName = WebpImage(imgUrl, "SubCategory");

                    subCategory.ImgUrl = "/Uploads/SubCategory/" + imgName;
                }

                subCategory.State = true;
                subCategory.UserId= userId;
                db.SubCategories.Add(subCategory);
                db.SaveChanges();


                TablesLogs logs = new TablesLogs();
                logs.UserId = userId;
                logs.ItemId = subCategory.SubCategoryId;
                logs.ItemName = subCategory.SubCategoryName;
                logs.TableName = "SubCategories";
                logs.Process = subCategory.SubCategoryName + " " + "Kategorisi" + " " + userName + " " + "tarafından eklendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", subCategory.CategoryId);
            return View(subCategory);
        }

        // GET: SubCategory/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubCategories subCategory = db.SubCategories.Find(id);
            if (subCategory == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", subCategory.CategoryId);
            return View(subCategory);
        }

        // POST: SubCategory/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SubCategoryId,CategoryId,SubCategoryName,ImgUrl")] SubCategories subCategory, HttpPostedFileBase imgUrl, int id)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];


            var categoryId = db.SubCategories.Where(x => x.SubCategoryId == id).SingleOrDefault();
            if (ModelState.IsValid)
            {
                if (imgUrl != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(categoryId.ImgUrl)))
                    {
                        System.IO.File.Delete(Server.MapPath(categoryId.ImgUrl));
                    }

                    string imgName = WebpImage(imgUrl, "SubCategory");

                    categoryId.ImgUrl = "/Uploads/SubCategory/" + imgName;
                }

                categoryId.SubCategoryName = subCategory.SubCategoryName;
                categoryId.CategoryId = subCategory.CategoryId;
                categoryId.EmendatorAdminId = userId;
                db.SaveChanges();


                TablesLogs logs = new TablesLogs();
                logs.UserId = userId;
                logs.ItemId = subCategory.SubCategoryId;
                logs.ItemName = subCategory.SubCategoryName;
                logs.TableName = "SubCategories";
                logs.Process = subCategory.SubCategoryName + " " + "Kategorisi" + " " + userName + " " + "tarafından güncellendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", subCategory.CategoryId);
            return View(subCategory);
        }

        // GET: SubCategory/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubCategories subCategory = db.SubCategories.Find(id);
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
            SubCategories subCategory = db.SubCategories.Find(id);
            try
            {
                if (subCategory == null)
                {
                    return HttpNotFound();
                }


                subCategory.State = false;
                db.SaveChanges();

                var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
                var userName =HttpContext.User.Identity.Name.Split('|')[3];

                TablesLogs logs = new TablesLogs();

                logs.UserId = userId;
                logs.ItemId = subCategory.SubCategoryId;
                logs.ItemName = subCategory.SubCategoryName;
                logs.TableName = "SubCategories";
                logs.Process = subCategory.SubCategoryName + " " + "Kategorisi" + " " + userName + " " + "tarafından silindi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
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