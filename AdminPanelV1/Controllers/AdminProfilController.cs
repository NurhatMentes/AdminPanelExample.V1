using AdminPanelV1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace AdminPanelV1.Controllers
{
    public class AdminProfilController : Controller
    {
        // GET: AdminProfil
        AdminV1 db = new AdminV1();
        public ActionResult Index()
        {
            return View(db.Admin.ToList());
        }

        // GET: AdminProfil/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admin.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // GET: AdminProfil/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminProfil/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AdminId,FullName,Job,Phone,Email,Password,Auth")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                db.Admin.Add(admin);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(admin);
        }

        // GET: AdminProfil/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Admin admin = db.Admin.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // POST: AdminProfil/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int id, Admin admin, string password)
        {
            if (ModelState.IsValid)
            {
                var adm = db.Admin.Where(x => x.AdminId == id).SingleOrDefault();
                if (password != adm.Password)
                {
                    adm.Password = Crypto.Hash(password, "MD5");
                    adm.RePassword = Crypto.Hash(password, "MD5");
                }
                adm.Phone = admin.Phone;
                adm.Job = admin.Job;
                adm.FullName = admin.FullName;
                adm.Email = admin.Email;
                adm.Auth = adm.Auth;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(admin);
        }

        public ActionResult PasswordPartial(Admin admin, int id)
        {
            var user = db.Admin.Where(x => x.AdminId == id).SingleOrDefault();

            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult PasswordPartial(Admin admin, int id, string password, string newPassword)
        {
            if (ModelState.IsValid)
            {
                var adm = db.Admin.Where(x => x.AdminId == id).SingleOrDefault();

                if (adm.RePassword == Crypto.Hash(admin.RePassword, "MD5"))
                {
                    if (newPassword == password)
                    {
                        adm.Password = Crypto.Hash(password, "MD5");
                        adm.RePassword = Crypto.Hash(password, "MD5");
                        adm.Phone = adm.Phone;
                        adm.Job = adm.Job;
                        adm.FullName = adm.FullName;
                        adm.Email = adm.Email;
                        adm.Auth = adm.Auth;

                        db.SaveChanges();
                        return RedirectToAction("Index", "AdminProfil");
                    }
                    else
                    {
                        ViewBag.Warning = "Şifreler Uyuşmuyor";
                    }
                }
                else
                {
                    ViewBag.Danger = "Güncel Şifrenizi Yanlış Girdiniz";
                }
            }

            return View(admin);
        }

        // GET: AdminProfil/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admin.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // POST: AdminProfil/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Admin admin = db.Admin.Find(id);
            db.Admin.Remove(admin);
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