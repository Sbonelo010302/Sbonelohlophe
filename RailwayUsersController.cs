using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RailwayApp.Models;

namespace RailwayApp.Controllers
{
    public class RailwayUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: RailwayUsers
        public ActionResult Index()
        {
            return View(db.RailwayUsers.ToList());
        }

        // GET: RailwayUsers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RailwayUser railwayUser = db.RailwayUsers.Find(id);
            if (railwayUser == null)
            {
                return HttpNotFound();
            }
            return View(railwayUser);
        }

        // GET: RailwayUsers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RailwayUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,UserName,CompanyName,Designation,EmailAddress,SystemUserTypeId,StatusId,IsPasswordReset,IsTemporaryPassword,IdentificationNumber,MobileNumber,Code,IsActive,CreatedDateTime,ModifiedDateTime")] RailwayUser railwayUser)
        {
            if (ModelState.IsValid)
            {
                db.RailwayUsers.Add(railwayUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(railwayUser);
        }

        // GET: RailwayUsers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RailwayUser railwayUser = db.RailwayUsers.Find(id);
            if (railwayUser == null)
            {
                return HttpNotFound();
            }
            return View(railwayUser);
        }

        // POST: RailwayUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,UserName,CompanyName,Designation,EmailAddress,SystemUserTypeId,StatusId,IsPasswordReset,IsTemporaryPassword,IdentificationNumber,MobileNumber,Code,IsActive,CreatedDateTime,ModifiedDateTime")] RailwayUser railwayUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(railwayUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(railwayUser);
        }

        // GET: RailwayUsers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RailwayUser railwayUser = db.RailwayUsers.Find(id);
            if (railwayUser == null)
            {
                return HttpNotFound();
            }
            return View(railwayUser);
        }

        // POST: RailwayUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RailwayUser railwayUser = db.RailwayUsers.Find(id);
            db.RailwayUsers.Remove(railwayUser);
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
