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
    public class ProductSlidersController : Controller
    {
        AdminV1 db = new AdminV1();

        // GET: ProductSliders
        public ActionResult Index(int id)
        {
            var productSlider = db.ProductSliders.Include("Products").Where(p => p.Products.ProductId == id).Where(x => x.State == true);


            return View(productSlider.ToList());
        }

        // GET: ProductSliders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductSliders productSlider = db.ProductSliders.Find(id);

            if (productSlider == null)
            {
                return HttpNotFound();
            }
            return View(productSlider);
        }

        // GET: ProductSliders/Create
        public ActionResult Create()
        {
            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "Title");

            return View();
        }

        // POST: ProductSliders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductSliders productSlider, HttpPostedFileBase imgUrl)
        {
          
            TablesLogs logs = new TablesLogs();

            int productId = productSlider.ProductId;
            if (ModelState.IsValid)
            {

                WebImage image = new WebImage(imgUrl.InputStream);
                FileInfo fileInfo = new FileInfo(imgUrl.FileName);

                string imgName = Guid.NewGuid() + fileInfo.Extension;
                image.Resize(1024, 768);
                image.Save("~/Uploads/ProductSlider/" + imgName);

                productSlider.ImgUrl = "/Uploads/ProductSlider/" + imgName;

                db.ProductSliders.Add(productSlider);
                db.SaveChanges();

                var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
                var userName =HttpContext.User.Identity.Name.Split('|')[3];

                logs.UserId = userId;
                logs.ItemId = productId;
                logs.ItemName = db.Products.FirstOrDefault(x=>x.ProductId==productId)?.Title;
                logs.TableName = "ProductSliders";
                logs.Process = db.Products.FirstOrDefault(x => x.ProductId == productId)?.Title + " " + "Ürünün resmi" + " " + userName + " " + "tarafından eklendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();
                return RedirectToAction("Index/" + productId);
            }

            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "Title", productSlider.ProductId);
            return View(productSlider);
        }


        // GET: ProductSliders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductSliders productSlider = db.ProductSliders.Find(id);
            if (productSlider == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "Title", productSlider.ProductId);
            return View(productSlider);
        }

        // POST: ProductSliders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //değişken id olarak productId girildi böylece dropdown list içerisinden ürünün id'sini değişken olarak aldı.
        public ActionResult Edit([Bind(Include = "SliderId,ProductId,ImgUrl")] ProductSliders productSlider, HttpPostedFileBase imgUrl, int ProductId)
        {
            
            TablesLogs logs = new TablesLogs();

            if (ModelState.IsValid)
            {
                ProductSliders pSlider = db.ProductSliders.Where(x => x.ProductId == ProductId).FirstOrDefault();

                if (imgUrl != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(pSlider.ImgUrl)))
                    {
                        System.IO.File.Delete(Server.MapPath(pSlider.ImgUrl));
                    }

                    WebImage image = new WebImage(imgUrl.InputStream);
                    FileInfo fileInfo = new FileInfo(imgUrl.FileName);

                    string imgName = Guid.NewGuid() + fileInfo.Extension;
                    image.Resize(1024, 768);
                    image.Save("~/Uploads/ProductSlider/" + imgName);

                    pSlider.ImgUrl = "/Uploads/ProductSlider/" + imgName;
                }

                db.SaveChanges();

                var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
                var userName =HttpContext.User.Identity.Name.Split('|')[3];

                logs.UserId = userId;
                logs.ItemId = pSlider.SliderId;
                logs.ItemName = db.Products.FirstOrDefault(x => x.ProductId == pSlider.ProductId)?.Title;
                logs.TableName = "ProductSliders";
                logs.Process = db.Products.FirstOrDefault(x => x.ProductId == pSlider.ProductId)?.Title + " " + "Ürünün resmi" + " " + userName + " " + "tarafından güncellendi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index/" + ProductId);
            }
            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "Title", productSlider.ProductId);
            return View(productSlider);
        }

        // GET: ProductSliders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductSliders productSlider = db.ProductSliders.Find(id);
            if (productSlider == null)
            {
                return HttpNotFound();
            }
            return View(productSlider);
        }

        // POST: ProductSliders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductSliders productSlider = db.ProductSliders.Find(id);
            int productId = productSlider.ProductId;

            try
            {
                if (productSlider == null)
                {
                    return HttpNotFound();
                }


                productSlider.State = false;
                db.SaveChanges();

                var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
                var userName =HttpContext.User.Identity.Name.Split('|')[3];

                TablesLogs logs = new TablesLogs();

                logs.UserId = userId;
                logs.ItemId = productSlider.SliderId;
                logs.ItemName = db.Products.FirstOrDefault(x => x.ProductId == productSlider.ProductId)?.Title;
                logs.TableName = "ProductSliders";
                logs.Process = db.Products.FirstOrDefault(x => x.ProductId == productSlider.ProductId)?.Title + " " + "Resmi" + " " + userName + " " + "tarafından silindi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();

                return RedirectToAction("Index/" + productId);
            }
            catch (Exception e)
            {
                return View(productSlider);
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