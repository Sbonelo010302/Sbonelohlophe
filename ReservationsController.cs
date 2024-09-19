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
    public class ReservationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Reservations
        public ActionResult Index()
        {
            if (User.IsInRole("Customer"))
            {
                //var countRes = db.Reservations.Where(x => x.Booked != true && x.Expiry > DateTime.Now).Count();
                var cust = CustomersController.GetCustomerId(User);
                var reservations = db.Reservations.Include(x => x.Route.Train).Include(x=>x.Customer).Where(x => x.Expiry > DateTime.Now && x.CustomerId == cust).ToList();
                return View(reservations);
            }
            else if (User.IsInRole("Admin"))
            {
                var reservations = db.Reservations.Include(x => x.Route.Train).Include(x => x.Customer);
                return View(reservations.ToList());
            }
            return View(new List<Reservation>());
        }

        // GET: Reservations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Include(x=>x.Route).Include(x=>x.Customer).FirstOrDefault(x=>x.Id == id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // GET: Reservations/Create
        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FirstName");
            //ViewBag.RouteId = new SelectList(db.Routes.Include(x => x.Train), "Id", "RouteNameDrp");
            //var t = new SelectList(db.Routes.Include(x => x.Train), "Id", "RouteName").ToList();
            ViewBag.RouteId = (from r in db.Routes
                               join t in db.Trains on r.TrainId equals t.Id
                               select new SelectListItem
                               {
                                   Text = r.To + " , " + r.From + " -- " + t.Name,
                                   Value = r.Id.ToString()
                               });
            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                if (!User.IsInRole("Customer"))
                {
                    ModelState.AddModelError("", "Only customers can make reservations");
                }
                else
                {
                    reservation.Booked = false;
                    reservation.Expiry = DateTime.Now.AddMinutes(10);
                    reservation.CreatedDateTime = DateTime.Now;
                    reservation.IsActive = true;
                    reservation.CustomerId = CustomersController.GetCustomerId(User);
                    reservation.SingleReferenceNumber = RefGen.ResReferenceGenerator(true);
                    reservation.GroupReferenceNumber = RefGen.ResReferenceGenerator(false);

                    db.Reservations.Add(reservation);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FirstName", reservation.CustomerId);
            ViewBag.RouteId = new SelectList(db.Routes.Include(x => x.Train), "Id", "RouteName"+ " "+"Train.Name", reservation.RouteId);
            ViewBag.RouteId = (from r in db.Routes
                               join t in db.Trains on r.TrainId equals t.Id
                               select new SelectListItem
                               {
                                   Text = r.To + " , " + r.From + " -- " + t.Name,
                                   Value = r.Id.ToString(),
                                   Selected = (r.Id == reservation.RouteId ? true : false),

                               });
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FirstName", reservation.CustomerId);
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "To", reservation.RouteId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CustomerId,RouteId,IsActive,CreatedDateTime,ModifiedDateTime")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FirstName", reservation.CustomerId);
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "To", reservation.RouteId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reservation reservation = db.Reservations.Find(id);
            db.Reservations.Remove(reservation);
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
