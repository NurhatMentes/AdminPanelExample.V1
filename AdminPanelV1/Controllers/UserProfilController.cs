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
    public class UserProfilController : Controller
    {
        // GET: AdminProfil
        AdminV1 db = new AdminV1();
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: AdminProfil/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
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
        public ActionResult Create([Bind(Include = "AdminId,FullName,Job,Phone,Email,Password,Auth")] Users users)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(users);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(users);
        }

        // GET: AdminProfil/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Users users = db.Users.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // POST: AdminProfil/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int id, Users users, string password)
        {
            if (ModelState.IsValid)
            {
                var usr = db.Users.Where(x => x.UserId == id).SingleOrDefault();
                if (password != usr.Password)
                {
                    usr.Password = Crypto.Hash(password, "MD5");
                    usr.RePassword = Crypto.Hash(password, "MD5");
                }
                usr.Phone = users.Phone;
                usr.Job = users.Job;
                usr.FullName = users.FullName;
                usr.Email = users.Email;
                usr.Auth = usr.Auth;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(users);
        }

        public ActionResult PasswordPartial(Users users, int id)
        {
            var user = db.Users.Where(x => x.UserId == id).SingleOrDefault();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult PasswordPartial(Users users, int id, string password, string newPassword)
        {
            if (ModelState.IsValid)
            {
                var usr = db.Users.Where(x => x.UserId == id).SingleOrDefault();

                if (usr.RePassword == Crypto.Hash(users.RePassword, "MD5"))
                {
                    if (newPassword == password)
                    {
                        usr.Password = Crypto.Hash(password, "MD5");
                        usr.RePassword = Crypto.Hash(password, "MD5");
                        usr.Phone = usr.Phone;
                        usr.Job = usr.Job;
                        usr.FullName = usr.FullName;
                        usr.Email = usr.Email;
                        usr.Auth = usr.Auth;

                        db.SaveChanges();
                        return RedirectToAction("Index", "UserProfil");
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

            return View(users);
        }

        // GET: AdminProfil/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // POST: AdminProfil/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Users admin = db.Users.Find(id);
            db.Users.Remove(admin);
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