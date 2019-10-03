using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ISPRC.Models;
using Microsoft.AspNet.Identity;

namespace ISPRC.Controllers
{
    public class BirdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Statistics(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Bird bird = db.Birds.FirstOrDefault(b=>b.BirdId == id);
            if (bird == null)
            {
                return HttpNotFound();
            }

            string totalFlight = "Total Flight: "+bird.Races.Sum(r=>r.Distance);
            string races = "Races: " +bird.Races.Count();
            string content = "Fastest Flight: " + bird.Races.OrderBy(r => r.Speed).FirstOrDefault().Speed;
            return Content(totalFlight + ", " + races + ", " + content);
        }

        // GET: Birds
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();

            var birds = db.Birds.Include(b => b.Owner).Where(b=>b.OwnerId==userId);
            return View(birds.ToList());
        }

        // GET: Birds/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bird bird = db.Birds.Find(id);
            if (bird == null)
            {
                return HttpNotFound();
            }
            return View(bird);
        }

        // GET: Birds/Create
        public ActionResult Create()
        {
            //ViewBag.OwnerId = new SelectList(db.Users, "Id", "GivenName");

            return View();
        }

        // POST: Birds/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BirdId,BirdName")] Bird bird)
        {
            bird.DateCreated = DateTime.UtcNow.AddHours(8);
            bird.OwnerId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.Birds.Add(bird);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // ViewBag.OwnerId = new SelectList(db.Users, "Id", "GivenName", bird.OwnerId);
            return View(bird);
        }

        // GET: Birds/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bird bird = db.Birds.Find(id);
            if (bird == null)
            {
                return HttpNotFound();
            }

            // ViewBag.OwnerId = new SelectList(db.Users, "Id", "GivenName", bird.OwnerId);
            return View(bird);
        }

        // POST: Birds/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BirdId,BirdName,DateCreated,OwnerId")] Bird bird)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bird).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // ViewBag.OwnerId = new SelectList(db.Users, "Id", "GivenName", bird.OwnerId);
            return View(bird);
        }

        // GET: Birds/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bird bird = db.Birds.Find(id);
            if (bird == null)
            {
                return HttpNotFound();
            }
            return View(bird);
        }

        // POST: Birds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Bird bird = db.Birds.Find(id);
            db.Birds.Remove(bird);
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
