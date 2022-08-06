using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using AdminPanelV1.Models;

namespace AdminPanelV1.Controllers
{
    public class UsersController : Controller
    {
        AdminV1 db = new AdminV1();

        // GET: Admins
        public ActionResult Index()
        {

            ViewBag.CommentConf = db.Comments.Where(x => x.Confirmation == false).Count();
            ViewBag.Comment = db.Comments.Where(x => x.Confirmation == false).ToList();
            ViewBag.Blog = db.Blogs.Count();
            ViewBag.Product = db.Products.Count();
            ViewBag.Service = db.Services.Count();
            ViewBag.Category = db.Categories.Count();
            ViewBag.CommentNumber = db.Comments.Count();
            ViewBag.AdminName = db.Users.ToList();
            ViewBag.LoginDate = db.UserLogs.OrderByDescending(x => x.UserLogId);

            var categoryList = db.Categories.ToList();
            return View(categoryList);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(Users admin, string password)
        {
            var md5pass = Crypto.Hash(password, "MD5");
            var login = db.Users.Where(x => x.Email == admin.Email && x.Password == md5pass).FirstOrDefault();

            HttpCookie userCookie = new HttpCookie("userCookie");
            userCookie.Expires = DateTime.Now.AddMinutes(30);
            UserLogs adminLog = new UserLogs();

            if (login != null)
            {
                FormsAuthentication.SetAuthCookie(login.Email + "|" + login.UserId + "|" +
                                                  login.Auth + "|" + login.FullName + "|" + login.Job + "|" +
                                                  login.Phone + "|" + login.RePassword, true);


                adminLog.UserId = login.UserId;
                adminLog.State = "Giriş Yapıldı";
                adminLog.LogDate = DateTime.Now; ;
                db.UserLogs.Add(adminLog);
                db.SaveChanges();


                return RedirectToAction("Index", "Users");

            }
            ViewBag.Danger = "E-posta veya Şifre hatalı";
            return View(admin);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            UserLogs adminLog = new UserLogs();
            adminLog.UserId = Convert.ToInt32(System.Web.HttpContext.Current.User.Identity.Name.Split('|')[1]); ;
            adminLog.State = "Güvenli Çıkış Yapıldı";
            adminLog.LogDate = DateTime.Now;
            db.UserLogs.Add(adminLog);
            db.SaveChanges();


            return RedirectToAction("Login", "Users");
        }

        [AllowAnonymous]
        public ActionResult ForgotMyPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ForgotMyPassword(string email = null)
        {
            var user = db.Users.Where(x => x.Email == email).SingleOrDefault();
            var ePosta = db.SystemAdmin.Select(x => x.Email).SingleOrDefault();
            var password = db.SystemAdmin.Select(x => x.Password).SingleOrDefault();

            if (user != null)
            {
                Random random = new Random();
                int rndPass = random.Next(10035, 999654);
                int rndPass2 = random.Next(10035, 999654);

                string newPassword = rndPass.ToString() + rndPass2.ToString();

                Users admin = new Users();
                user.Password = Crypto.Hash(newPassword, "MD5");
                user.RePassword = Crypto.Hash(newPassword, "MD5");
                db.SaveChanges();

                try
                {
                    WebMail.SmtpServer = "smtp.gmail.com";
                    WebMail.EnableSsl = true;
                    WebMail.UserName = ePosta;
                    WebMail.Password = password;
                    WebMail.SmtpPort = 587;
                    WebMail.Send(email, "Yönetim paneline yeni giriş şifreniz", "Şifrenizi değiştirmeyi unutmayınız!" + "<br/>" + "<strong/>" + "Şifreniz: " + newPassword);
                    ViewBag.Danger = "Yeni Şifreniz gönderilmiştir.";
                }
                catch (Exception ex)
                {

                    ViewBag.Danger = ex.Message;
                }
            }
            else
            {
                ViewBag.Danger = "Kayıtlı kullanıcı bulunamadı!";
            }

            return View();
        }

        public ActionResult CommentPartial()
        {
            ViewBag.CommentConf = db.Comments.Where(x => x.Confirmation == false).Count();
            return View(db.Comments.Where(x => x.Confirmation == false).ToList());
        }

        public ActionResult Admins()
        {
            return View(db.Users.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Users admin, string password)
        {
            if (ModelState.IsValid)
            {
                admin.Password = Crypto.Hash(password, "MD5");
                admin.RePassword = Crypto.Hash(password, "MD5");
                db.Users.Add(admin);
                db.SaveChanges();

                return RedirectToAction("Admins");
            }

            return View(admin);
        }

        public ActionResult Edit(int id)
        {
            var admin = db.Users.Where(x => x.UserId == id).SingleOrDefault();
            return View(admin);
        }

        [HttpPost]
        public ActionResult Edit(int id, Users admin, string password)
        {
            if (ModelState.IsValid)
            {
                var adm = db.Users.Where(x => x.UserId == id).SingleOrDefault();
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
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Users users = db.Users.Find(id);
            users.State = false;
            db.SaveChanges();
            return RedirectToAction("Admins");
        }
    }
}