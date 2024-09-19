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
    public class RoutesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Routes
        public ActionResult Index()
        {
            return View(db.Routes.Include(x=>x.Train).ToList());
        }

        // GET: Routes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Route route = db.Routes.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        // GET: Routes/Create
        public ActionResult Create()
        {
            ViewBag.Trains = new SelectList(db.Trains, "Id", "Name");
            return View();
        }

        // POST: Routes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Route route)
        {
            try
            {
                var test = Request.Form["Trains"];
                route.TrainId = int.Parse(test ?? "1");
                route.IsActive = true;
                route.CreatedDateTime = DateTime.Now;
                db.Routes.Add(route);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.Trains = new SelectList(db.Trains, "Id", "Name",route.TrainId);
                return View(route);
            }
        }

        // GET: Routes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Route route = db.Routes.Include(x=>x.Train).FirstOrDefault(x=>x.Id == id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        [HttpGet]
        public ActionResult Cancel(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Route route = db.Routes.Include(x => x.Train).FirstOrDefault(x => x.Id == id);
            if (route == null)
            {
                return HttpNotFound();
            }
            route.Cancelled = true;
            db.Entry(route).State = EntityState.Modified;
            db.SaveChanges();
            NotifyCustomers(route.Id, false);

            return RedirectToAction("Index");
        }

        // POST: Routes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Route route)
        {
            if (ModelState.IsValid)
            {
                var findRoute = db.Routes.AsNoTracking().FirstOrDefault(x=>x.Id == route.Id);
                if ((route.Arrival >= findRoute?.Arrival && route.Arrival <= findRoute?.Arrival) && (route.Departure >= findRoute?.Departure && route.Departure <= findRoute?.Departure))
                {
                    route.Rescheduled = true;
                }
                if (route.Rescheduled == true)
                {
                    NotifyCustomers(route.Id, true);
                }
                else if (route.Cancelled == true)
                {
                    NotifyCustomers(route.Id, false);
                }
                db.Entry(route).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(route);
        }

        public static bool? NotifyCustomers(int? routeId, bool IsRescheduled)
        {
            using (ApplicationDbContext _context = new ApplicationDbContext())
            {
                var users = _context.Reservations.Include(x => x.Customer).Include(x => x.Route.Train).Where(x => x.RouteId == routeId).ToList();
                foreach (var user in users)
                {
                    Email.SendEmail(user.Customer.EmailAddress, $"{user.Route.Train.Name} {(IsRescheduled ? "Rescheduled" : "Cancelled")}", $"Dear {user.Customer.FirstName} {user.Customer.LastName}, <br/> Please note that {user.Route.Train.Name} has been {(IsRescheduled ? "Rescheduled" : "Cancelled")}");
                }
                return true;
            }
        }

        // GET: Routes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Route route = db.Routes.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        // POST: Routes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Route route = db.Routes.Find(id);
            db.Routes.Remove(route);
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
