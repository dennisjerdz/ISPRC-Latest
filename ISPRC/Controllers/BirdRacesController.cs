using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ISPRC.Models;

namespace ISPRC.Controllers
{
    public class BirdRacesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BirdRaces
        public ActionResult Index()
        {
            var birdsRace = db.BirdsRace.Include(b => b.Bird).Include(b => b.Race);
            return View(birdsRace.ToList());
        }

        // GET: BirdRaces/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BirdRace birdRace = db.BirdsRace.Find(id);
            if (birdRace == null)
            {
                return HttpNotFound();
            }
            return View(birdRace);
        }

        // GET: BirdRaces/Create
        public ActionResult Create()
        {
            ViewBag.BirdId = new SelectList(db.Birds, "BirdId", "BirdName");
            ViewBag.RaceId = new SelectList(db.Races, "RaceId", "RaceName");
            return View();
        }

        // POST: BirdRaces/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BirdRaceId,BirdId,RaceId,EndLatitude,EndLongitude,ReleaseDate,ArrivalDate,Speed,Distance,BirdCode,DateCreated")] BirdRace birdRace)
        {
            if (ModelState.IsValid)
            {
                db.BirdsRace.Add(birdRace);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BirdId = new SelectList(db.Birds, "BirdId", "BirdName", birdRace.BirdId);
            ViewBag.RaceId = new SelectList(db.Races, "RaceId", "RaceName", birdRace.RaceId);
            return View(birdRace);
        }

        // GET: BirdRaces/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BirdRace birdRace = db.BirdsRace.Find(id);
            if (birdRace == null)
            {
                return HttpNotFound();
            }
            ViewBag.BirdId = new SelectList(db.Birds, "BirdId", "BirdName", birdRace.BirdId);
            ViewBag.RaceId = new SelectList(db.Races, "RaceId", "RaceName", birdRace.RaceId);
            return View(birdRace);
        }

        // POST: BirdRaces/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BirdRaceId,BirdId,RaceId,EndLatitude,EndLongitude,ReleaseDate,ArrivalDate,Speed,Distance,BirdCode,DateCreated")] BirdRace birdRace)
        {
            if (ModelState.IsValid)
            {
                db.Entry(birdRace).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BirdId = new SelectList(db.Birds, "BirdId", "BirdName", birdRace.BirdId);
            ViewBag.RaceId = new SelectList(db.Races, "RaceId", "RaceName", birdRace.RaceId);
            return View(birdRace);
        }

        // GET: BirdRaces/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BirdRace birdRace = db.BirdsRace.Find(id);
            if (birdRace == null)
            {
                return HttpNotFound();
            }
            return View(birdRace);
        }

        // POST: BirdRaces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BirdRace birdRace = db.BirdsRace.Find(id);
            db.BirdsRace.Remove(birdRace);
            db.SaveChanges();
            return RedirectToAction("Racers", "Races", new { id = birdRace.RaceId });
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
