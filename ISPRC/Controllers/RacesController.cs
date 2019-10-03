﻿using System;
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
    [Authorize(Roles = "Admin, Club Owner")]
    public class RacesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [AllowAnonymous]
        public ActionResult Arrive(string code)
        {
            BirdRace br = db.BirdsRace.Include(r => r.Bird).Include(r => r.Race).FirstOrDefault(r => r.BirdCode == code);

            if (br == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (br.ReleaseDate == null)
            {
                return Content("Release Date not yet set");
            }

            br.ArrivalDate = DateTime.UtcNow.AddHours(8);

            double time = br.ArrivalDate.Value.Subtract(br.ReleaseDate.Value).TotalSeconds;

            br.Speed = br.Distance / time; // meters per second
            db.SaveChanges();

            return Content(code +" "+time.ToString()+" "+br.Speed.ToString());
        }

        public ActionResult ManageRacer(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BirdRace br = db.BirdsRace.Include(r=>r.Bird).Include(r=>r.Race).FirstOrDefault(r=>r.BirdRaceId == id.Value);
            if (br == null)
            {
                return HttpNotFound();
            }

            return View(br);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageRacer([Bind(Include = "BirdRaceId,BirdId,RaceId,EndLatitude,EndLongitude,ReleaseDate,ArrivalDate,Speed,Distance,BirdCode,DateCreated")] BirdRace birdRace)
        {
            if (ModelState.IsValid)
            {
                db.Entry(birdRace).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(birdRace);
        }

        public ActionResult Racers(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Race race = db.Races.Find(id);
            if (race == null)
            {
                return HttpNotFound();
            }

            return View(db.BirdsRace.Include(r=>r.Bird).Where(r=>r.RaceId==race.RaceId).ToList());
        }

        [AllowAnonymous]
        public ActionResult Join(int? id)
        {
            Race race = db.Races.FirstOrDefault(r => r.RaceId == id);

            if (race == null)
            {
                return HttpNotFound();
            }
            else
            {
                JoinViewModel j = new JoinViewModel();
                j.RaceId = race.RaceId;
                j.RaceName = race.RaceName;
                string ownerId = User.Identity.GetUserId();
                ViewBag.BirdId = new SelectList(db.Birds.Where(b=>b.OwnerId == ownerId && !b.Races.Any(br=>br.RaceId == race.RaceId)), "BirdId", "BirdName");

                return View(j);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Join(JoinViewModel j)
        {
            if (ModelState.IsValid)
            {
                BirdRace br = new BirdRace();
                br.BirdId = j.BirdId;
                br.RaceId = j.RaceId;
                br.BirdCode = Guid.NewGuid().ToString().Substring(0,16);
                br.DateCreated = DateTime.UtcNow.AddHours(8);

                db.BirdsRace.Add(br);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            string ownerId = User.Identity.GetUserId();
            ViewBag.BirdId = new SelectList(db.Birds.Where(b => b.OwnerId == ownerId && !b.Races.Any(br => br.RaceId == j.RaceId)), "BirdId", "BirdName");
            return View(j);
        }

        // GET: Races
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View(db.Races.ToList());
        }

        // GET: Races/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Race race = db.Races.Find(id);
            if (race == null)
            {
                return HttpNotFound();
            }
            return View(race);
        }

        // GET: Races/Create
        public ActionResult Create()
        {
            // https://schmich.github.io/instascan/
            // https://davidshimjs.github.io/qrcodejs/

            return View();
        }

        // POST: Races/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RaceId,RaceName,RaceStartDate,RaceCutOffDate,RaceEndedDate,RaceLatitudeCoordinate,RaceLongitudeCoordinate,ForceRaceDone")] Race race)
        {
            race.DateCreated = DateTime.UtcNow.AddHours(8);

            if (ModelState.IsValid)
            {
                db.Races.Add(race);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(race);
        }

        // GET: Races/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Race race = db.Races.Find(id);
            if (race == null)
            {
                return HttpNotFound();
            }
            return View(race);
        }

        // POST: Races/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RaceId,RaceName,RaceStartDate,RaceCutOffDate,RaceEndedDate,DateCreated,RaceLatitudeCoordinate,RaceLongitudeCoordinate,ForceRaceDone")] Race race)
        {
            if (ModelState.IsValid)
            {
                db.Entry(race).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(race);
        }

        // GET: Races/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Race race = db.Races.Find(id);
            if (race == null)
            {
                return HttpNotFound();
            }
            return View(race);
        }

        // POST: Races/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Race race = db.Races.Find(id);
            db.Races.Remove(race);
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