using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using PayPal.Api;
using RailwayApp.Models;
using static System.Collections.Specialized.BitVector32;
using Payment = RailwayApp.Models.Payment;

namespace RailwayApp.Controllers
{
    public class PaymentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Payments
        public ActionResult Index()
        {
            if (User.IsInRole("Customer"))
            {
                var cust = CustomersController.GetCustomerId(User);
                var payments = db.Payments.Include(p => p.Customer).Include(p => p.Reservation.Route).Where(x=>x.CustomerId == cust).ToList();
                return View(payments);
            }
            else if (User.IsInRole("Admin"))
            {
                var payments = db.Payments.Include(p => p.Customer).Include(p => p.Reservation.Route).ToList();
                return View(payments);
            }
            else
            {
                return View(new List<Payment>());
            }
        }

        // GET: Payments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Include(x=>x.Reservation.Route).Include(x=>x.Customer).FirstOrDefault(x=>x.Id == id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // GET: Payments/Create
        //public ActionResult Create(int? id)
        //{
        //    Reservation reservation = db.Reservations.Include(x => x.Route).FirstOrDefault(x => x.Id == id);

        //    if (reservation != null)
        //    {
        //        string refno = PaymentReferenceGenerator();
        //        string desc = $"{reservation.NoOfReservations} {reservation.Route.RouteName} {(reservation.NoOfReservations > 1 ? "Tickets" : "Tickets")}";
        //        decimal total = reservation.Route.Rate * reservation.NoOfReservations;

        //        //PayController pay = new PayController();
        //        return PaymentWithPaypal(desc, refno, reservation.Route.Rate.ToString(), total.ToString(), reservation.NoOfReservations.ToString());
        //        //return View();

        //    }
        //    return View();
        //}

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(int id)
        {
            Reservation reservation = db.Reservations.Include(x=>x.Route).FirstOrDefault(x => x.Id == id);

            if (reservation != null)
            {
                string refno= PaymentReferenceGenerator();
                string desc = $"{reservation.NoOfReservations} {reservation.Route.RouteName} {(reservation.NoOfReservations > 1 ? "Tickets" : "Tickets")}";
                decimal total = reservation.Route.Rate * reservation.NoOfReservations;

                //PayController pay = new PayController();
                //var payResult = PaymentWithPaypal(desc, refno, reservation.Route.Rate.ToString(), total.ToString(), reservation.NoOfReservations.ToString());

                Payment payment = new Payment();
                payment.CreatedDateTime = DateTime.Now;
                payment.ReservationId = reservation.Id;
                payment.CustomerId = reservation.CustomerId;
                payment.Amount = total;
                payment.RefNo = refno;
                payment.IsActive = true;
                payment.IsPaid = false;
                payment.PaymentMethod = "PayPal";
                payment.InvoiceNumber = InvReferenceGenerator();

                db.Payments.Add(payment);
                db.SaveChanges();
                return PaymentWithPaypal(desc, refno, reservation.Route.Rate.ToString(), total.ToString(), reservation.NoOfReservations.ToString(), payment.InvoiceNumber);
            }
            //ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FirstName", payment.CustomerId);
            //ViewBag.ReservationId = new SelectList(db.Reservations, "Id", "To", payment.ReservationId);
            return View("_Error");
        }
        public ActionResult PaymentSuccess(string guid)
        {
            var payment = db.Payments.Include(x=>x.Reservation.Route).Include(x=>x.Customer).FirstOrDefault(x => x.RefNo == guid);
            if (payment != null && payment?.IsPaid != true)
            {
                payment.IsPaid = true;
                payment.ModifiedDateTime = DateTime.Now;
                payment.Reservation.Booked = true;
                payment.Reservation.ModifiedDateTime = DateTime.Now;
                db.Entry(payment).State = EntityState.Modified;
                db.Entry(payment.Reservation).State = EntityState.Modified;
                db.SaveChanges();
            }
            return View(payment);
        }
        // GET: Payments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FirstName", payment.CustomerId);
            ViewBag.RouteId = new SelectList(db.Reservations, "Id", "To", payment.ReservationId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CustomerId,RouteId,RefNo,IsPaid,Amount,IsActive,CreatedDateTime,ModifiedDateTime")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "FirstName", payment.CustomerId);
            ViewBag.RouteId = new SelectList(db.Reservations, "Id", "To", payment.ReservationId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payment payment = db.Payments.Find(id);
            db.Payments.Remove(payment);
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

        public static string PaymentReferenceGenerator()
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                Random random = new Random();
                int randNumber = random.Next(10000, 99999);
                string RefPrefix = "PR";

                string refnos = string.Format("{0}{1}", RefPrefix, randNumber);
                _ = (context.Payments.Select(x => x.RefNo).Contains(refnos)) ? PaymentReferenceGenerator() : refnos;
                return refnos;
            }
            catch (Exception ex)
            {
                return PaymentReferenceGenerator();
            }
        }

        public ActionResult PaymentWithPaypal(string description, string refNo, string price, string total, string quantity, string invNo, string Cancel = null)
        {
            string guid = String.Empty;
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment PayPal.Api.Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request?.Params["PayerID"] ?? "";
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Payments/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    //var guid = Convert.ToString((new Random()).Next(100000));
                    guid = refNo;
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, description, refNo, price, total, quantity, invNo);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("Failure");
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Failure", new {q=ex.Message});
            }
            //on successful payment, show success page to user.  
            return RedirectToAction("PaymentSuccess", new { guid = guid });
        }
        private PayPal.Api.Payment payment;
        private PayPal.Api.Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new PayPal.Api.Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private PayPal.Api.Payment CreatePayment(APIContext apiContext, string redirectUrl, string description, string reference, string price, string total, string quantity, string invNo)
        {
            double usd= 17.61;
            //price = $"{decimal.Parse(price) * decimal.Parse(usd.ToString())}";
            //total = $"{decimal.Parse(price) * decimal.Parse(usd.ToString())}";
            decimal decTotal = decimal.Parse(price) * int.Parse(quantity);
            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            itemList.items.Add(new Item()
            {
                name = description,
                currency = "USD",
                price = price.Replace(',','.'),
                quantity = quantity,
                sku = reference
            });
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "1",
                shipping = "1",
                subtotal = total.Replace(',', '.'),
            };
            decTotal += decimal.Parse(details.tax);
            decTotal += decimal.Parse(details.shipping);

            string totalAmount = decTotal.ToString("#.##");
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = totalAmount, // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };

            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            transactionList.Add(new Transaction()
            {
                description = description,
                invoice_number = invNo, //Generate an Invoice No  
                amount = amount,
                item_list = itemList
            });
            this.payment = new PayPal.Api.Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }

        public static string InvReferenceGenerator()
        {
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                Random random = new Random();
                int randNumber = random.Next(10000, 99999);
                string RefPrefix = "INV";

                string refnos = string.Format("{0}{1}", RefPrefix, randNumber);
                _ = (context.Payments.Select(x => x.RefNo).Contains(refnos)) ? InvReferenceGenerator() : refnos;
                return refnos;
            }
            catch (Exception ex)
            {
                return InvReferenceGenerator();
            }
        }

        public ActionResult Failure(string q)
        {
            ViewBag.Error = q;
            return View();
        }
    }
}
