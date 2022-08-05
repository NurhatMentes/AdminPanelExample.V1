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
using AdminPanelV1.Models.Dto;

namespace AdminPanelV1.Controllers
{
    public class ProductsController : Controller
    {
        private AdminV1 db = new AdminV1();

        // GET: Products
        public ActionResult Index()
        {
            //...
            var products = db.Products.Include(p => p.Users).Include(p => p.Categories);
            return View(products.Where(x=>x.State).OrderBy(x => x.ProductId).ToList());
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
        public ActionResult Create( Products products, HttpPostedFileBase imgUrl, HttpPostedFileBase uploadFile)
        {
            var userCookie = Request.Cookies["userCookie"];
            TablesLogs logs = new TablesLogs();

           
                if (imgUrl != null)
                {
                    WebImage image = new WebImage(imgUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(imgUrl.FileName);

                    string imgName = Guid.NewGuid() + fileInfo.Extension;
                    image.Resize(1024, 460);
                    image.Save("~/Uploads/Product/" + imgName);

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
                    products.UserId = Convert.ToInt16(userCookie["UserId"]); ;


                    db.Products.Add(products);
                    db.SaveChanges();

                    logs.UserId = Convert.ToInt16(userCookie["UserId"]);
                    logs.ItemId = products.ProductId;
                    logs.ItemName = products.Title;
                    logs.TableName = "Products";
                    logs.Process = products.Title + " " + "Ürünü" + " " + userCookie["FullName"] + " " + "tarafından eklendi.";
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
        public ActionResult Edit(Products product, HttpPostedFileBase ImgUrl, int id, HttpPostedFileBase uploadFile)
        {
            var userCookie = Request.Cookies["userCookie"];
            TablesLogs logs = new TablesLogs();

            if (ModelState.IsValid)
            {
                var productId = db.Products.Where(x => x.ProductId == id).SingleOrDefault();
                if (ImgUrl != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(productId.ImgUrl)))
                    {
                        System.IO.File.Delete(Server.MapPath(productId.ImgUrl));
                    }

                    WebImage image = new WebImage(ImgUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(ImgUrl.FileName);

                    string imgName = Guid.NewGuid() + fileInfo.Extension;
                    image.Resize(1024, 460);
                    image.Save("~/Uploads/Product/" + imgName);

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
                productId.EmendatorAdminId = Convert.ToInt16(userCookie["UserId"]);
                db.SaveChanges();


                logs.UserId = Convert.ToInt16(userCookie["UserId"]);
                logs.ItemId = productId.ProductId;
                logs.ItemName = productId.Title;
                logs.TableName = "Products";
                logs.Process = productId.Title + " " + "Ürünü" + " " + userCookie["FullName"] + " " + "tarafından güncellendi.";
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

                var userCookie = Request.Cookies["userCookie"];
                TablesLogs logs = new TablesLogs();

                logs.UserId = Convert.ToInt16(userCookie["UserId"]);
                logs.ItemId = product.ProductId;
                logs.ItemName = product.Title;
                logs.TableName = "Products";
                logs.Process = product.Title + " " + "Ürünü" + " " + userCookie["FullName"] + " " + "tarafından silindi.";
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
