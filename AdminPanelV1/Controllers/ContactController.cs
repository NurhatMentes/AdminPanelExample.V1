using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AdminPanelV1.Models;

namespace AdminPanelV1.Controllers
{
    public class ContactController : Controller
    {
        AdminV1 db = new AdminV1();

        // GET: Contact
        public ActionResult Index()
        {
            return View(db.Contact.ToList());
        }
         
        // GET: Contact/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contact.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // POST: Contact/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ContactId,Adress,Tel,Email,EmailPassword,Whatsapp,Facebook,Twitter,Instagram")] Contact contact)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contact).State = EntityState.Modified;
                var userCookie = Request.Cookies["userCookie"];
                contact.EmendatorAdminId = Convert.ToInt16(userCookie["UserId"]);

                TablesLogs logs = new TablesLogs();
                logs.ItemId = contact.ContactId;
                logs.UserId = Convert.ToInt16(userCookie["UserId"]);
                logs.ItemName = contact.Email;
                logs.TableName = "Contact";
                logs.LogDate = DateTime.Now;
                logs.Process = contact.Email + " " + "iletişim bilgisi" + " " + userCookie["FullName"] + " " + "tarafından güncellendi.";
                db.TablesLogs.Add(logs);
                db.SaveChanges();
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(contact);
        }

    }
}