﻿using System;
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
using AdminPanelV1.Models.Dto;

namespace AdminPanelV1.Controllers
{
    public class ProductsController : Controller
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

        // GET: Products
        public ActionResult Index()
        {
            //...
            var products = db.Products.Include(p => p.Users).Include(p => p.Categories);
            return View(products.Where(x => x.State).OrderBy(x => x.ProductId).ToList());
        }
        //Methods
        private void Viewbags()
        {
            ViewBag.AdminId = new SelectList(db.Users, "UserId", "FullName");
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName");
        }
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

        // GET: Product/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // GET: Product/Create
        public ActionResult Create()
        {
            ViewBag.Categories = db.Categories.ToList();
            Viewbags();
            return View();
        }



        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Products products, HttpPostedFileBase imgUrl, HttpPostedFileBase uploadFile)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];
          


            if (imgUrl != null)
            {
                string imgName = WebpImage(imgUrl, "Product");
                products.ImgUrl = "/Uploads/Product/" + imgName;

                if (uploadFile != null)
                {
                    string fileName = Path.GetFileName(uploadFile.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/Uploads/Catalog/"), fileName);
                    uploadFile.SaveAs(filePath);
                    products.File = "/Uploads/Catalog/" + fileName;
                }

                products.OldPrice = 0;
                products.Date = DateTime.Now;
                products.UserId = userId;


                db.Products.Add(products);
                db.SaveChanges();


                TablesLogs logs = new TablesLogs();
                logs.UserId = userId;
                logs.ItemId = products.ProductId;
                logs.ItemName = products.Title;
                logs.TableName = "Products";
                logs.Process = products.Title + " " + "Ürünü" + " " + userName + " " + "tarafından eklendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Warning = "Lütfen Resim Seçiniz";
            }



            Viewbags();
            return View(products);
        }

        // GET: Product/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            Viewbags();
            return View(products);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Products product, HttpPostedFileBase imgUrl, int id, HttpPostedFileBase uploadFile)
        {
            var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
            var userName = HttpContext.User.Identity.Name.Split('|')[3];


            if (ModelState.IsValid)
            {
                var productId = db.Products.Where(x => x.ProductId == id).SingleOrDefault();

                if (imgUrl != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(productId.ImgUrl)))
                    {
                        System.IO.File.Delete(Server.MapPath(productId.ImgUrl));
                    }
                    string imgName = WebpImage(imgUrl, "Product");
                    productId.ImgUrl = "/Uploads/Product/" + imgName;
                }


                if (uploadFile != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(productId.File)))
                    {
                        System.IO.File.Delete(Server.MapPath(productId.File));
                    }

                    string fileName = Path.GetFileName(uploadFile.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/Uploads/Catalog/"), fileName);
                    uploadFile.SaveAs(filePath);
                    productId.File = "/Uploads/Catalog/" + fileName;
                }


                productId.Content = product.Content;
                productId.Title = product.Title;


                productId.CategoryId = product.CategoryId;

                if (productId.SubCategoryId != null && product.SubCategoryId == null)
                {
                    productId.SubCategoryId = productId.SubCategoryId;
                }
                else if (product.SubCategoryId != null)
                {
                    productId.SubCategoryId = product.SubCategoryId;
                }
                else
                {
                    ViewBag.SubCategory = "Lütfen kategori seçiniz";
                }

              

                productId.OldPrice = productId.Price;
                productId.Price = product.Price;
                productId.Stock = product.Stock;
                productId.Color = product.Color;
                productId.UpdateDate = DateTime.Now;
                productId.Tag = product.Tag;
                productId.EmendatorAdminId = userId;
                db.SaveChanges();


                TablesLogs logs = new TablesLogs();
                logs.UserId = userId;
                logs.ItemId = productId.ProductId;
                logs.ItemName = productId.Title;
                logs.TableName = "Products";
                logs.Process = productId.Title + " " + "Ürünü" + " " + userName + " " + "tarafından güncellendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            Viewbags();
            return View(product);
        }

        // GET: Product/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            var product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            try
            {
                product.State = false;
                db.SaveChanges();


                var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
                var userName = HttpContext.User.Identity.Name.Split('|')[3];
                TablesLogs logs = new TablesLogs();

                logs.UserId = userId;
                logs.ItemId = product.ProductId;
                logs.ItemName = product.Title;
                logs.TableName = "Products";
                logs.Process = product.Title + " " + "Ürünü" + " " + userName + " " + "tarafından silindi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception m)
            {
                ViewBag.Danger = "Ürünü silmek için öncelikle slider içerisinde bulunun resimleri silmeniz gerekir";
                ViewBag.Danger2 = "İşlemler > Slider Resim";
                return View(product);
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