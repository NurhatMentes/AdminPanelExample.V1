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