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
    public class DayToDayTrainRoutesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DayToDayTrainRoutes
        public ActionResult Index()
        {
            var dayToDayTrainRoutes = db.DayToDayTrainRoutes.Include(d => d.Route).Include(d => d.Train);
            return View(dayToDayTrainRoutes.ToList());
        }

        // GET: DayToDayTrainRoutes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DayToDayTrainRoute dayToDayTrainRoute = db.DayToDayTrainRoutes.Find(id);
            if (dayToDayTrainRoute == null)
            {
                return HttpNotFound();
            }
            return View(dayToDayTrainRoute);
        }

        // GET: DayToDayTrainRoutes/Create
        public ActionResult Create()
        {
            string[] times = { "08:00 am", "08:30 am", "09:00 am", "09:30 am", "10:00 am", "10:30 am", "11:00 am", "11:30 am",
                "12:00 pm", "12:30 pm", "13:00 pm", "13:30 pm", "14:00 pm", "14:30 pm", "15:00 pm", "15:30 pm", "16:00 pm" };
            ViewBag.TrainTimes = new SelectList(times);
            ViewBag.RouteId = new SelectList(db.Routes.OrderBy(a=>a.From).ToList(), "Id", "RouteName");
            ViewBag.TrainId = new SelectList(db.Trains.OrderBy(a=>a.Name).ToList(), "Id", "Name");
            ViewBag.Error = null;
            return View();
        }

        // POST: DayToDayTrainRoutes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Day,DepatureTime,ArrivalTime,TrainId,RouteId")] DayToDayTrainRoute dayToDayTrainRoute)
        {
            ViewBag.Error = null;
            dayToDayTrainRoute = GetUpdatedDayToDayTrainRoute(dayToDayTrainRoute);
            //dayToDayTrainRoute.ArrivalTime = hours;
            //var train = db.Trains.FirstOrDefault(a=>a.Id== dayToDayTrainRoute.TrainId);
            var day2dayExists = db.DayToDayTrainRoutes.Where(a=>a.RouteId==dayToDayTrainRoute.RouteId && a.TrainId==dayToDayTrainRoute.TrainId
                                && a.Day == dayToDayTrainRoute.Day && a.DepatureTime == dayToDayTrainRoute.DepatureTime).ToList();


            if (ModelState.IsValid && day2dayExists.Count == 0)
            {
                db.DayToDayTrainRoutes.Add(dayToDayTrainRoute);
                db.SaveChanges();
                return RedirectToAction("AvailableTrains", "Trains");
            }
            if(day2dayExists.Count > 0)
            {
                ViewBag.Error = "Route already exists for the day.";
            }
            string[] times = { "08:00 am", "08:30 am", "09:00 am", "09:30 am", "10:00 am", "10:30 am", "11:00 am", "11:30 am",
                "12:00 pm", "12:30 pm", "13:00 pm", "13:30 pm", "14:00 pm", "14:30 pm", "15:00 pm", "15:30 pm", "16:00 pm" };
            ViewBag.TrainTimes = new SelectList(times);
            ViewBag.RouteId = new SelectList(db.Routes.OrderBy(a => a.From).ToList(), "Id", "RouteName");
            ViewBag.TrainId = new SelectList(db.Trains.OrderBy(a => a.Name).ToList(), "Id", "Name");
            return View(dayToDayTrainRoute);
        }

        private DayToDayTrainRoute GetUpdatedDayToDayTrainRoute(DayToDayTrainRoute dayToDayTrainRoute)
        {
            var hours = Convert.ToInt32(dayToDayTrainRoute.DepatureTime.Substring(0, 2));
            var mins = Convert.ToInt32(dayToDayTrainRoute.DepatureTime.Substring(3, 2));
            var route = db.Routes.FirstOrDefault(a => a.Id == dayToDayTrainRoute.RouteId);
            if (route.Minutes > 0)
            {
                mins += route.Minutes;
                while(mins > 60)
                {
                    mins -= 60;
                    hours++;
                }
            }
            if(route.Hours > 0)
            {
                hours += route.Hours;
                while(hours > 24)
                {
                    //dayToDayTrainRoute.Day = dayToDayTrainRoute.Day.AddDays(1);
                    hours -= 24;
                    if(hours == 24)
                    {
                        //dayToDayTrainRoute.Day = dayToDayTrainRoute.Day.AddDays(1);
                        hours -= 24;
                    }
                }
            }
            var sfx = (hours > 11) ? "pm" : "am";
            dayToDayTrainRoute.ArrivalTime =  $"{hours:00}:{mins:00} {sfx}";
            return dayToDayTrainRoute;
        }
        public string GetArrival(DayToDayTrainRoute dayToDayTrainRoute)
        {
            var hours = Convert.ToInt32(dayToDayTrainRoute.DepatureTime.Substring(0, 2));
            var mins = Convert.ToInt32(dayToDayTrainRoute.DepatureTime.Substring(3, 2));
            var route = db.Routes.FirstOrDefault(a => a.Id == dayToDayTrainRoute.RouteId);
            var arrivalDay = dayToDayTrainRoute.Day;
            if (route.Minutes > 0)
            {
                mins += route.Minutes;
                while (mins > 60)
                {
                    mins -= 60;
                    hours++;
                }
            }
            if (route.Hours > 0)
            {
                hours += route.Hours;
                while (hours > 24)
                {
                    arrivalDay = dayToDayTrainRoute.Day.AddDays(1);
                    hours -= 24;
                    if (hours == 24)
                    {
                        arrivalDay = dayToDayTrainRoute.Day.AddDays(1);
                        hours -= 24;
                    }
                }
            }
            return arrivalDay.ToShortDateString();
        }

        // GET: DayToDayTrainRoutes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DayToDayTrainRoute dayToDayTrainRoute = db.DayToDayTrainRoutes.Find(id);
            if (dayToDayTrainRoute == null)
            {
                return HttpNotFound();
            }
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "RouteName", dayToDayTrainRoute.RouteId);
            ViewBag.TrainId = new SelectList(db.Trains, "Id", "Name", dayToDayTrainRoute.TrainId);
            return View(dayToDayTrainRoute);
        }

        // POST: DayToDayTrainRoutes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Day,DepatureTime,ArrivalTime,TrainId,RouteId")] DayToDayTrainRoute dayToDayTrainRoute)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dayToDayTrainRoute).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RouteId = new SelectList(db.Routes, "Id", "RouteName", dayToDayTrainRoute.RouteId);
            ViewBag.TrainId = new SelectList(db.Trains, "Id", "Name", dayToDayTrainRoute.TrainId);
            return View(dayToDayTrainRoute);
        }

        // GET: DayToDayTrainRoutes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DayToDayTrainRoute dayToDayTrainRoute = db.DayToDayTrainRoutes.Find(id);
            if (dayToDayTrainRoute == null)
            {
                return HttpNotFound();
            }
            return View(dayToDayTrainRoute);
        }

        // POST: DayToDayTrainRoutes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DayToDayTrainRoute dayToDayTrainRoute = db.DayToDayTrainRoutes.Find(id);
            db.DayToDayTrainRoutes.Remove(dayToDayTrainRoute);
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
