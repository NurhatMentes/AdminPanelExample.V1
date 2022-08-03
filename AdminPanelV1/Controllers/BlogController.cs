﻿using System;
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
        // GET: Blog
        public ActionResult Index()
        {
            db.Configuration.LazyLoadingEnabled = false;
            var records = db.Blog.Include("Category").Include("SubCategory").ToList();
            return View(records);
        }

        public ActionResult CategoriesPartial()
        {
            CategoriesDto categoriesDto = new CategoriesDto();
            categoriesDto.Categories = new SelectList(db.Category, "CategoryId", "CategoryName");
            categoriesDto.SubCategories = new SelectList(db.SubCategory, "SubCategoryId", "SubCategoryName");

            return View(categoriesDto);
        }

        public JsonResult SubCategory(int categoryId)
        {
            var subCategories = (from x in db.SubCategory
                join y in db.Category on x.Category.CategoryId equals y.CategoryId
                where x.Category.CategoryId == categoryId
                select new
                {
                    Text = x.SubCategoryName,
                    Value = x.SubCategoryId.ToString()
                }).ToList();
            return Json(subCategories, JsonRequestBehavior.AllowGet);
        }



        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Category, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Blog blog, HttpPostedFileBase imgUrl)
        {
            if (imgUrl != null)
            {
                WebImage image = new WebImage(imgUrl.InputStream);
                FileInfo fileInfo = new FileInfo(imgUrl.FileName);

                string imgName = Guid.NewGuid() + fileInfo.Extension;
                image.Resize(600, 400);
                image.Save("~/Uploads/Blog/" + imgName);

                blog.ImgUrl = "/Uploads/Blog/" + imgName;
            }

            db.Blog.Add(blog);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var blog = db.Blog.Where(x => x.BlogId == id).SingleOrDefault();

            if (blog == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryId = new SelectList(db.Category, "CategoryId", "CategoryName", blog.CategoryId);
            return View(blog);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Blog blog, int id, HttpPostedFileBase ImgUrl)
        {
            if (ModelState.IsValid)
            {
                var blogId = db.Blog.Where(x => x.BlogId == id).SingleOrDefault();
                if (ImgUrl != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(blogId.ImgUrl)))
                    {
                        System.IO.File.Delete((Server.MapPath((blogId.ImgUrl))));
                    }

                    WebImage image = new WebImage(ImgUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(ImgUrl.FileName);

                    string imgName = Guid.NewGuid() + fileInfo.Extension;
                    image.Resize(600, 400);
                    image.Save("~/Uploads/Blog/" + imgName);

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
            Blog blog = db.Blog.Find(id);
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
            var blog = db.Blog.Find(id);

            if (blog == null)
            {
                return HttpNotFound();
            }

            if (System.IO.File.Exists(Server.MapPath(blog.ImgUrl)))
            {
                System.IO.File.Delete(Server.MapPath(blog.ImgUrl));
            }

            db.Blog.Remove(blog);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}