﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using AdminPanelV1.Models;

namespace AdminPanelV1.Controllers
{
    public class AdminController : Controller
    {
        AdminV1 db = new AdminV1();

        // GET: Admins
        public ActionResult Index()
        {
            var adminId = (int)Session["adminid"];
            var adminName = Session["FullName"];

            ViewBag.CommentConf = db.Comment.Where(x => x.Confirmation == false).Count();
            ViewBag.Comment = db.Comment.Where(x => x.Confirmation == false).ToList();
            ViewBag.Blog = db.Blog.Count();
            ViewBag.Service = db.Service.Count();
            ViewBag.Category = db.Category.Count();
            ViewBag.CommentNumber = db.Comment.Count();
            ViewBag.RecentlyAdminName=db.Admin.Where(x=>x.FullName == adminName).ToList();
            ViewBag.RecentlyLoginDate = db.AdminLog.Where(x=>x.AdminId == adminId).ToList();

            var categoryList = db.Category.ToList();
            return View(categoryList);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Admin admin, string password)
        {
            var md5pass = Crypto.Hash(password, "MD5");
            var login = db.Admin.Where(x => x.Email == admin.Email || x.Password == admin.Password).FirstOrDefault();

            AdminLog adminLog = new AdminLog();

            if (login != null)
            {
                if (login.Password == md5pass && login.Email == admin.Email)
                {
                    Session["adminid"] = login.AdminId;
                    Session["email"] = login.Email;
                    Session["Auth"] = login.Auth;
                    Session["FullName"] = login.FullName;
                    Session["Job"] = login.Job;
                    Session["Phone"] = login.Phone;
                    Session["OldPassword"] = login.RePassword;

                    adminLog.AdminId = login.AdminId;
                    adminLog.State = "Giriş Yapıldı";    
                    adminLog.LogDate=DateTime.Now;;
                    db.AdminLog.Add(adminLog);
                    db.SaveChanges();

                    return RedirectToAction("Index", "Admin");
                }
            }

            ViewBag.Danger = "E-posta veya Şifre hatalı";
            return View(admin);
        }

        public ActionResult Logout()
        {
            AdminLog adminLog = new AdminLog();
            adminLog.AdminId = (int)Session["adminid"];
            adminLog.State = "Çıkış Yapıldı";
            adminLog.LogDate = DateTime.Now; ;
            db.AdminLog.Add(adminLog);
            db.SaveChanges();

            Session["adminid"] = null;
            Session["email"] = null;
            Session["Auth"] = null;
            Session.Abandon();

           
            return RedirectToAction("Login", "Admin");
        }

        public ActionResult ForgotMyPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotMyPassword(string email = null)
        {
            var user = db.Admin.Where(x => x.Email == email).SingleOrDefault();
            var ePosta = db.SystemAdmin.Select(x => x.Email).SingleOrDefault();
            var password = db.SystemAdmin.Select(x => x.Password).SingleOrDefault();

            if (user != null)
            {
                Random random = new Random();
                int rndPass = random.Next(10035, 999654);
                int rndPass2 = random.Next(10035, 999654);

                string newPassword = rndPass.ToString() + rndPass2.ToString();

                Admin admin = new Admin();
                user.Password = Crypto.Hash(newPassword, "MD5");
                user.RePassword = Crypto.Hash(newPassword, "MD5");
                db.SaveChanges();

                WebMail.SmtpServer = "smtp.gmail.com";
                WebMail.EnableSsl = true;
                WebMail.UserName = ePosta;
                WebMail.Password = password;
                WebMail.SmtpPort = 587;
                WebMail.Send(email, "Yönetim paneline yeni giriş şifreniz", "Şifrenizi değiştirmeyi unutmayınız!" + "<br/>" + "<strong/>" + "Şifreniz: " + newPassword);
                ViewBag.Danger = "Yeni Şifreniz gönderilmiştir.";
            }
            else
            {
                ViewBag.Danger = "Kayıtlı kullanıcı bulunamadı!";
            }

            return View();
        }

        public ActionResult CommentPartial()
        {
            ViewBag.CommentConf = db.Comment.Where(x => x.Confirmation == false).Count();
            return View(db.Comment.Where(x => x.Confirmation == false).ToList());
        }

        public ActionResult Admins()
        {
            return View(db.Admin.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Admin admin, string password)
        {
            if (ModelState.IsValid)
            {
                admin.Password = Crypto.Hash(password, "MD5");
                admin.RePassword = Crypto.Hash(password, "MD5");
                db.Admin.Add(admin);
                db.SaveChanges();

                return RedirectToAction("Admins");
            }

            return View(admin);
        }

        public ActionResult Edit(int id)
        {
            var admin = db.Admin.Where(x => x.AdminId == id).SingleOrDefault();
            return View(admin);
        }

        [HttpPost]
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
                adm.Auth = admin.Auth;

                db.SaveChanges();
                return RedirectToAction("Admins");
            }

            return View(admin);
        }

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Admin admin = db.Admin.Find(id);
            db.Admin.Remove(admin);
            db.SaveChanges();
            return RedirectToAction("Admins");
        }
    }
}