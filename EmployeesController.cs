using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RailwayApp.Migrations;
using RailwayApp.Models;

namespace RailwayApp.Controllers
{
    public class EmployeesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Employees
        public ActionResult Index()
        {
            var employees = db.Employees.Include(e => e.EmployeeType);
            return View(employees.ToList());
        }

        // GET: Employees/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            ViewBag.EmployeeTypeId = new SelectList(db.EmployeeTypes, "Id", "Name");
            ViewBag.RailwayUserId = new SelectList(db.RailwayUsers, "Id", "FirstName");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee employee)
        {
            ViewBag.EmployeeTypeId = new SelectList(db.EmployeeTypes, "Id", "Name", employee.EmployeeTypeId);
            if (ModelState.IsValid)
            {
                try
                {
                    if (db.Employees.Any(x=>x.EmailAddress == employee.EmailAddress))
                    {
                        ModelState.AddModelError("", "Employee already exists!");
                        return View(employee);
                    }
                    employee.CreatedDateTime = DateTime.Now;
                    employee.IsActive = true;
                    employee.StatusId = "Active";
                    db.Employees.Add(employee);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                }
            }
            //ViewBag.EmployeeTypeId = new SelectList(db.EmployeeTypes, "Id", "Name", employee.EmployeeTypeId);
            //ViewBag.RailwayUserId = new SelectList(db.RailwayUsers, "Id", "FirstName", employee.RailwayUserId);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            ViewBag.EmployeeId = new SelectList(db.EmployeeTypes, "Id", "Name", employee.EmployeeTypeId);
            //ViewBag.RailwayUserId = new SelectList(db.RailwayUsers, "Id", "FirstName", employee.RailwayUserId);
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,EmailAddress,EmployeeId,StatusId,MobileNumber,RailwayUserId,IsActive,CreatedDateTime,ModifiedDateTime")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EmployeeId = new SelectList(db.EmployeeTypes, "Id", "Name", employee.EmployeeTypeId);
            //ViewBag.RailwayUserId = new SelectList(db.RailwayUsers, "Id", "FirstName", employee.RailwayUserId);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Employee employee = db.Employees.Find(id);
            db.Employees.Remove(employee);
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
