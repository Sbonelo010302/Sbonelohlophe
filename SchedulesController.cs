using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RailwayApp.Models;

namespace RailwayApp.Controllers
{
    public class SchedulesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Schedules
        public ActionResult Index()
        {
            var schedules = db.Schedules.Include(s => s.Route);
            return View(schedules.ToList());
        }

        // GET: Schedules/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            return View(schedule);
        }

        // GET: Schedules/Create
        public ActionResult Create()
        {
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "To");
            return View();
        }

        // POST: Schedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,RouteId,Departure,Arrival")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                db.Schedules.Add(schedule);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RouteId = new SelectList(db.Routes, "Id", "To", schedule.RouteId);
            return View(schedule);
        }

        // GET: Schedules/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id) ?? db.Schedules.FirstOrDefault(x=>x.RouteId == id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "To", schedule.RouteId);
            return View(schedule);
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,RouteId,Departure,Arrival")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                db.Entry(schedule).State = EntityState.Modified;
                db.SaveChanges();
                //if (schedule.Rescheduled == true)
                //{
                //    NotifyCustomers(schedule.RouteId, true);
                //}
                //else if (schedule.Cancelled == true)
                //{
                //    NotifyCustomers(schedule.RouteId, false);
                //}  
                return RedirectToAction("Index");
            }
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "To", schedule.RouteId);
            return View(schedule);
        }

        public ActionResult EditRes(int? routeId, string Result)
        {
            var find = db.Schedules.FirstOrDefault(x => x.RouteId == routeId);
            //switch (Result)
            //{
            //    case "Rescheduled":
            //        find.Rescheduled = true;
            //        NotifyCustomers(routeId, false);
            //        break;
            //    case "Cancelled":
            //        find.Cancelled = true;
            //        NotifyCustomers(routeId, false);
            //        break;
            //    default:
            //        break;
            //}
            db.Entry(find).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public static bool? NotifyCustomers(int? routeId, bool IsRescheduled)
        {
            using (ApplicationDbContext _context = new ApplicationDbContext())
            {
                var users = _context.Reservations.Include(x=>x.Customer).Include(x=>x.Route.Train).Where(x => x.RouteId == routeId).ToList();
                foreach (var user in users)
                {
                    Email.SendEmail(user.Customer.EmailAddress, $"{user.Route.Train.Name} {(IsRescheduled ? "Rescheduled" : "Cancelled")}", $"Dear {user.Customer.FirstName} {user.Customer.LastName}, <br/> Please note that {user.Route.Train.Name} has been {(IsRescheduled ? "Rescheduled" : "Cancelled")}");
                }
                return true;
            }
        }

        // GET: Schedules/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Schedule schedule = db.Schedules.Find(id);
            db.Schedules.Remove(schedule);
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
