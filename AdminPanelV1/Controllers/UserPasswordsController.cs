using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using AdminPanelV1.Models;

namespace AdminPanelV1.Controllers
{
    public class UserPasswordsController : Controller
    {
        private AdminV1 db = new AdminV1();

        // GET: UserPasswords
        public ActionResult Index()
        {
            var userPasswords = db.UserPasswords.Include(u => u.Users);
            return View(userPasswords.ToList());
        }

        // GET: UserPasswords/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserPasswords userPasswords = db.UserPasswords.Find(id);
            if (userPasswords == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", userPasswords.UserId);
            return View(userPasswords);
        }

        // POST: UserPasswords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserPasswords userPasswords, int id, string password, string newPassword, string currentPassword)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Where(x => x.UserId == id).SingleOrDefault();
                try
                {
                    if (user.UserPasswords.CurrentPassword == Crypto.Hash(currentPassword, "MD5"))
                    {
                        if (newPassword.Length > 7)
                        {
                            if (newPassword == password)
                            {
                                user.UserPasswords.Password = Crypto.Hash(password, "MD5");
                                user.UserPasswords.CurrentPassword = Crypto.Hash(password, "MD5");

                                db.SaveChanges();
                                ViewBag.Success = "Şifreniz Değiştirildi. Şifrenizi Kaydetmeyi Unutmayınız";

                                TablesLogs logs = new TablesLogs();
                                var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
                                var userName = HttpContext.User.Identity.Name.Split('|')[3];

                                logs.UserId = userId;
                                logs.ItemId = user.UserId;
                                logs.ItemName = user.FullName;
                                logs.TableName = "UserPasswords";
                                logs.Process = user.FullName + " " + "kişisi" + " " + "şifresini değiştirdi.";
                                logs.LogDate = DateTime.Now;
                                db.TablesLogs.Add(logs);
                                db.SaveChanges();
                            }
                            else
                            {
                                ViewBag.Warning = "Şifreler Uyuşmuyor";
                            }
                        }
                        else
                        {
                            ViewBag.Warning = "Şifreniz En Az 8 Karakter Olmalıdır";
                        }
                    }
                    else
                    {
                        ViewBag.Danger = "Güncel Şifrenizi Yanlış Girdiniz";
                    }
                }
                catch (Exception)
                {

                    return View(userPasswords);
                }
            }

            return View(userPasswords);
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
