using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using AdminPanelV1.Models;
using AdminPanelV1.Models.Dto;

namespace AdminPanelV1.Controllers
{
    public class BlogController : Controller
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


        // GET: Blog
        public ActionResult Index()
        {
            db.Configuration.LazyLoadingEnabled = false;
            var records = db.Blogs.Include("Categories").Include("SubCategories").ToList();
            return View(records);
        }
        //dropdown category cascading 
        public ActionResult CategoriesPartial()
        {
            CategoriesDto categoriesDto = new CategoriesDto();
            categoriesDto.Categories = new SelectList(db.Categories, "CategoryId", "CategoryName");
            categoriesDto.SubCategories = new SelectList(db.SubCategories, "SubCategoryId", "SubCategoryName");

            return View(categoriesDto);
        }

        public JsonResult SubCategory(int categoryId)
        {
            var subCategories = (from x in db.SubCategories
                                 join y in db.Categories on x.Categories.CategoryId equals y.CategoryId
                where x.Categories.CategoryId == categoryId
                select new
                {
                    Text = x.SubCategoryName,
                    Value = x.SubCategoryId.ToString()
                }).ToList();
            return Json(subCategories, JsonRequestBehavior.AllowGet);
        }



        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Blogs blog, HttpPostedFileBase imgUrl)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];


            if (ModelState.IsValid)
            {
                if (imgUrl != null)
                {
                    string imgName = WebpImage(imgUrl, "Blog");

                    blog.ImgUrl = "/Uploads/Blog/" + imgName;
                    blog.UserId = userId;
                    blog.State = true;

                    db.Blogs.Add(blog);
                    db.SaveChanges();

                    //logs
                    TablesLogs logs = new TablesLogs();
                    logs.UserId = userId;
                    logs.ItemId = blog.BlogId;
                    logs.ItemName = blog.Title;
                    logs.TableName = "Blog";
                    logs.Process = blog.Title + " " + "Bloğu" + " " + userName + " " + "tarafından eklendi.";
                    logs.LogDate = DateTime.Now;
                    db.TablesLogs.Add(logs);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                
            }

            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", blog.UserId);
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", blog.CategoryId);
            return View(blog);
        }

        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var blog = db.Blogs.Where(x => x.BlogId == id).SingleOrDefault();

            if (blog == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", blog.CategoryId);
            return View(blog);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Blogs blog, int id, HttpPostedFileBase imgUrl)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];
  

            if (ModelState.IsValid)
            {
                var blogId = db.Blogs.Where(x => x.BlogId == id).SingleOrDefault();

                if (imgUrl != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(blogId.ImgUrl)))
                    {
                        System.IO.File.Delete((Server.MapPath((blogId.ImgUrl))));
                    }

                    string imgName = WebpImage(imgUrl, "Blog");

                    blogId.ImgUrl = "/Uploads/Blog/" + imgName;
                }

                if (blogId.SubCategoryId != null && blogId.SubCategoryId == null)
                {
                    blogId.SubCategoryId = blogId.SubCategoryId;
                }
                else if (blogId.SubCategoryId != null)
                {
                    blogId.SubCategoryId = blogId.SubCategoryId;
                }
                else
                {
                    ViewBag.SubCategory = "Lütfen kategori seçiniz";
                }


                blogId.Content = blog.Content;
                blogId.Title = blog.Title;
                blogId.CategoryId = blog.CategoryId;
                blogId.UserId = userId;
                blogId.EmendatorAdminId = userId;

                db.SaveChanges();

                TablesLogs logs = new TablesLogs();
                logs.UserId = userId;
                logs.ItemId = blog.BlogId;
                logs.ItemName = blog.Title;
                logs.TableName = "Blog";
                logs.Process = blog.Title + " " + "Bloğu" + " " + userName + " " + "tarafından güncellendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(blog);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Blogs blog = db.Blogs.Find(id);

            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var blog = db.Blogs.Find(id);

            if (blog == null)
            {
                return HttpNotFound();
            }


            blog.State = false;
            db.SaveChanges();

            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];
            TablesLogs logs = new TablesLogs();

            logs.UserId = userId;
            logs.ItemId = blog.BlogId;
            logs.ItemName = blog.Title;
            logs.TableName = "Blogs";
            logs.Process = blog.Title + " " + "Bloğu" + " " + userName + " " + "tarafından silindi.";
            logs.LogDate = DateTime.Now;
            db.TablesLogs.Add(logs);
            db.SaveChanges();

        
            return RedirectToAction("Index");
        }
    }
}