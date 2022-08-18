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
using System.Web.Optimization;
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
            var login = db.Users.Where(x => x.Email == admin.Email && x.UserPasswords.CurrentPassword == md5pass).FirstOrDefault();

            HttpCookie userCookie = new HttpCookie("userCookie");
            userCookie.Expires = DateTime.Now.AddMinutes(30);
            UserLogs adminLog = new UserLogs();

            try
            {

                if (login != null)
                {
                    FormsAuthentication.SetAuthCookie(login.Email + "|" + login.UserId + "|" +
                                                      login.Auth + "|" + login.FullName + "|" + login.Job + "|" +
                                                      login.Phone, true);


                    adminLog.UserId = login.UserId;
                    adminLog.State = "Giriş Yapıldı";
                    adminLog.LogDate = DateTime.Now; ;
                    db.UserLogs.Add(adminLog);
                    db.SaveChanges();


                    return RedirectToAction("Index", "Users");

                }
                else
                {
                    ViewBag.Danger = "E-posta veya Şifre hatalı";
                    return View(admin);
                }

            }
            catch (Exception)
            {

                return View(admin);
            }
            ViewBag.Danger = "Hesabınız aktif değil. Giriş yapmak için yönetici ile irtibata geçiniz.";

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
                user.UserPasswords.CurrentPassword = Crypto.Hash(newPassword, "MD5");
                user.UserPasswords.Password = Crypto.Hash(newPassword, "MD5");
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
            return View(db.Users.Where(x => x.Auth != "0").ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Users user, string password)
        {
            if (ModelState.IsValid)
            {
                if (password.Length > 7)
                {
                    user.UserPasswords.Password = Crypto.Hash(password, "MD5");
                    user.UserPasswords.CurrentPassword = Crypto.Hash(password, "MD5");
                    db.Users.Add(user);
                    db.SaveChanges();

                    TablesLogs logs = new TablesLogs();
                    var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
                    var userName = HttpContext.User.Identity.Name.Split('|')[3];

                    logs.UserId = userId;
                    logs.ItemId = user.UserId;
                    logs.ItemName = user.FullName;
                    logs.TableName = "Users";
                    logs.Process = user.FullName + " " + "kişisi" + " " + userName + " " + "tarafından eklendi.";
                    logs.LogDate = DateTime.Now;
                    db.TablesLogs.Add(logs);
                    db.SaveChanges();
                    return RedirectToAction("Admins");
                }
                else
                {
                    ViewBag.Warning = "Şifreniz En Az 8 Karakter Olmalıdır";
                }

            }

            return View(user);
        }

        public ActionResult Edit(int id)
        {
            var admin = db.Users.Where(x => x.UserId == id).SingleOrDefault();
            ViewBag.AuthCheck = db.Users.FirstOrDefault(x => x.UserId == id).Auth;
            ViewBag.StateCheck = db.Users.FirstOrDefault(x => x.UserId == id).State;

            return View(admin);
        }

        [HttpPost]
        public ActionResult Edit(int id, Users user, string password)
        {
            ViewBag.AuthCheck = db.Users.FirstOrDefault(x => x.UserId == id).Auth;
            ViewBag.StateCheck = db.Users.FirstOrDefault(x => x.UserId == id).State;

            var auth = System.Web.HttpContext.Current.User.Identity.Name.Split('|')[2];
            var userId = System.Web.HttpContext.Current.User.Identity.Name.Split('|')[1];



            var adm = db.Users.Where(x => x.UserId == id).SingleOrDefault();
            if (password != null)
            {
                if (password != adm.UserPasswords.CurrentPassword)
                {
                    adm.UserPasswords.CurrentPassword = Crypto.Hash(password, "MD5");
                    adm.UserPasswords.Password = Crypto.Hash(password, "MD5");
                    db.SaveChanges();
                    ViewBag.State = "1";
                    ViewBag.AuthWarning = "Şifre Değiştirildi";
                }
            }

            if (user.UserId == int.Parse(userId) || user.UserId != 0 || user.UserId != 1)
            {
                if (adm.State == user.State && adm.Auth == user.Auth)
                {
                    if (adm.Phone != user.Phone || adm.Job != user.Job || adm.FullName != user.FullName || adm.Email != user.Email)
                    {
                        adm.Phone = user.Phone;
                        adm.Job = user.Job;
                        adm.FullName = user.FullName;
                        adm.Email = user.Email;
                        ViewBag.State = "0";
                        ViewBag.Success = "Kişisel bilgiler güncellendi.";
                        db.SaveChanges();
                        if (auth == "0")
                        {
                            return RedirectToAction("Admins");
                        }
                    }
                }
                else
                {
                    if (Convert.ToInt16(auth) == 1 || Convert.ToInt16(auth) == 0)
                    {
                        if (adm.UserId == Convert.ToInt16(userId))
                        {
                            if (adm.Auth != user.Auth)
                            {
                                ViewBag.State = "1";
                                ViewBag.AuthWarning = "Kendi Yetki seviyeni değiştiremezsin!";
                            }
                        }
                        else
                        {
                            adm.Auth = user.Auth;
                            ViewBag.State = "2";
                            ViewBag.AuthSuccess = "Yetki seviyesi değiştirildi";
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        ViewBag.Warning2 = "Yetki seviyesini değiştiremezsin!";
                    }

                    if (auth == "0" || auth == "1")
                    {
                        if (auth == "0")
                        {
                            adm.State = user.State;
                        }

                        if (auth == "1")
                        {
                            if (adm.Auth != "0" && adm.Auth != "1")
                            {
                                adm.State = user.State;
                                ViewBag.State = "3";
                                ViewBag.StateSuccess = "Hesap durumu değiştirildi.";
                                db.SaveChanges();
                            }
                            else
                            {
                                if (adm.State != user.State)
                                {
                                    ViewBag.State = "4";
                                    ViewBag.StateWarning = "Kişisel bilgiler güncellendi. Fakat durumu değiştirme yetkisine sahip değilsin.";
                                    db.SaveChanges();
                                }

                            }
                        }

                        db.SaveChanges();
                        ViewBag.AuthCheck = db.Users.FirstOrDefault(x => x.UserId == id).Auth;
                        ViewBag.StateCheck = db.Users.FirstOrDefault(x => x.UserId == id).State;
                        return View(user);
                    }
                }
            }


            return View(user);
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

            try
            {
                var userId = Convert.ToInt16(HttpContext.User.Identity.Name.Split('|')[1]);
                var userName = HttpContext.User.Identity.Name.Split('|')[3];
                TablesLogs logs = new TablesLogs();

                logs.UserId = userId;
                logs.ItemId = users.UserId;
                logs.ItemName = users.FullName;
                logs.TableName = "Users";
                logs.Process = users.FullName + " " + "kullanıcısı" + " " + userName + " " + "tarafından silindi.";
                logs.LogDate = DateTime.Now;
                db.TablesLogs.Add(logs);
                db.SaveChanges();
                return RedirectToAction("Users");
            }
            catch (Exception)
            {
                return View(users);
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