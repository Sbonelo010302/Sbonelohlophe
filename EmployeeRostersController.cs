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
    public class EmployeeRostersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EmployeeRosters
        public ActionResult Index()
        {
            var employeeRosters = db.EmployeeRosters.Include(e => e.Employee).Include(e => e.Route);
            return View(employeeRosters.ToList());
        }

        // GET: EmployeeRosters/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployeeRoster employeeRoster = db.EmployeeRosters.Find(id);
            if (employeeRoster == null)
            {
                return HttpNotFound();
            }
            return View(employeeRoster);
        }

        // GET: EmployeeRosters/Create
        public ActionResult Create()
        {
            ViewBag.EmployeeId = new SelectList(db.Employees, "Id", "FirstName");
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "To");
            return View();
        }

        // POST: EmployeeRosters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,EmployeeId,RouteId,IsActive,CreatedDateTime,ModifiedDateTime")] EmployeeRoster employeeRoster)
        {
            if (ModelState.IsValid)
            {
                db.EmployeeRosters.Add(employeeRoster);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.EmployeeId = new SelectList(db.Employees, "Id", "FirstName", employeeRoster.EmployeeId);
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "To", employeeRoster.RouteId);
            return View(employeeRoster);
        }

        // GET: EmployeeRosters/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployeeRoster employeeRoster = db.EmployeeRosters.Find(id);
            if (employeeRoster == null)
            {
                return HttpNotFound();
            }
            ViewBag.EmployeeId = new SelectList(db.Employees, "Id", "FirstName", employeeRoster.EmployeeId);
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "To", employeeRoster.RouteId);
            return View(employeeRoster);
        }

        // POST: EmployeeRosters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,EmployeeId,RouteId,IsActive,CreatedDateTime,ModifiedDateTime")] EmployeeRoster employeeRoster)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employeeRoster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EmployeeId = new SelectList(db.Employees, "Id", "FirstName", employeeRoster.EmployeeId);
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "To", employeeRoster.RouteId);
            return View(employeeRoster);
        }

        // GET: EmployeeRosters/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployeeRoster employeeRoster = db.EmployeeRosters.Find(id);
            if (employeeRoster == null)
            {
                return HttpNotFound();
            }
            return View(employeeRoster);
        }

        // POST: EmployeeRosters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EmployeeRoster employeeRoster = db.EmployeeRosters.Find(id);
            db.EmployeeRosters.Remove(employeeRoster);
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
