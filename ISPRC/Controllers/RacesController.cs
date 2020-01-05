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
using System.Text;

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

            if (br.ArrivalDate != null) {
                StringBuilder contentE = new StringBuilder();
                var durationE = (br.ArrivalDate - br.Race.RaceStartDate).Value;
                contentE.Append("<html>"+
                                    "<center style='margin-top:50px;'>"+
                                        "<h1>Racer already arrived at " + br.ArrivalDate + ".</h1>"+
                                        "<h3>Release Date: " + br.ReleaseDate + ".</h3>" +
                                        "<h3>Distance: " + Math.Round(br.Distance.Value,4) + " meters.</h3>" +
                                        "<h3>Flight Duration: " + durationE.TotalMinutes + " minutes.</h3>" +
                                        "<h3>Speed: " + br.Speed + " m/s.</h3>" +
                                        "<h5><a href='" + Url.Action("Racers", "Races", new { id = br.RaceId }) + "'>View Results</a></h5>" +
                                    "</center>" +
                "</html>");
                return Content(contentE.ToString());
            }

            br.ArrivalDate = DateTime.UtcNow.AddHours(8);

            double time = br.ArrivalDate.Value.Subtract(br.ReleaseDate.Value).TotalSeconds;

            br.Speed = br.Distance / time; // meters per second
            db.SaveChanges();

            StringBuilder content = new StringBuilder();
            var duration = (br.ArrivalDate - br.Race.RaceStartDate).Value;
            content.Append("<html>" +
                                "<center style='margin-top:50px;'>" +
                                    "<h1>We've received your code. The results are as follows;</h1>" +
                                    "<h3>Arrival Date " + br.ArrivalDate + ".</h3>" +
                                    "<h3>Release Date: " + br.ReleaseDate + ".</h3>" +
                                    "<h3>Distance: " + Math.Round(br.Distance.Value, 4) + " meters.</h3>" +
                                    "<h3>Flight Duration: " + duration.TotalMinutes + " minutes.</h3>" +
                                    "<h3>Speed: " + br.Speed + " m/s.</h3>" +
                                    "<h5><a href='"+Url.Action("Racers","Races", new { id=br.RaceId })+"'>View Results</a></h5>" +
                                "</center>" +
            "</html>");
            return Content(content.ToString());
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

            ViewBag.RaceName = race.RaceName;
            ViewBag.ReleasePointName = race.ReleasePoint.ReleasePointName;
            ViewBag.RaceCoordinates = race.ReleasePoint.ReleasePointCoordinates;
            ViewBag.RaceStartDate = race.RaceStartDate;
            ViewBag.RaceCutOffDate = race.RaceCutOffDate;
            ViewBag.RaceDescription = race.RaceDescription;

            return View(db.BirdsRace.Include(r=>r.Bird).Where(r=>r.RaceId==race.RaceId).ToList());
        }

        [AllowAnonymous]
        public ActionResult Join(int? id)
        {
            Race race = db.Races.FirstOrDefault(r => r.RaceId == id);

            string userId = User.Identity.GetUserId();
            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (race == null)
            {
                return HttpNotFound();
            }
            else
            {
                JoinViewModel j = new JoinViewModel();
                j.RaceId = race.RaceId;
                j.RaceName = race.RaceName;
                j.RaceLatitudeCoordinate = race.ReleasePoint.RaceLatitudeCoordinate;
                j.RaceLongitudeCoordinate = race.ReleasePoint.RaceLongitudeCoordinate;
                j.LoftLatitudeCoordinate = user.LoftLatitudeCoordinate;
                j.LoftLongitudeCoordinate = user.LoftLongitudeCoordinate;
                j.RaceDescription = race.RaceDescription;

                j.ReleasePointName = race.ReleasePoint.ReleasePointName;
                j.RaceLoadingDate = race.RaceLoadingDate;
                j.RaceStartDate = race.RaceStartDate;
                j.RaceCutOffDate = race.RaceCutOffDate;

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
                br.Distance = j.Distance;
                br.EndLatitude = j.LoftLatitudeCoordinate;
                br.EndLongitude = j.LoftLongitudeCoordinate;
                br.BirdCode = Guid.NewGuid().ToString().Substring(0,16);
                br.DateCreated = DateTime.UtcNow.AddHours(8);
                br.ReleaseDate = db.Races.FirstOrDefault(r => r.RaceId == j.RaceId).RaceStartDate;

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
            string userId = User.Identity.GetUserId();

            ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);

            if(user.ClubId != null)
            {
                return View(db.Races.Include(r => r.ReleasePoint).Where(r=>r.ClubId == user.ClubId).ToList());
            }
            else
            {
                return View(db.Races.Include(r => r.ReleasePoint).ToList());
            }
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

            // ViewBag.ReleasePointId = new SelectList(db.ReleasePoints, "ReleasePointId", "ReleasePointName");

            string userId = User.Identity.GetUserId();
            if (userId != null)
            {
                ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);
                ViewBag.ReleasePoints = db.ReleasePoints.Where(r=>r.ClubId == user.ClubId && r.IsActive == true).OrderBy(r => r.ReleasePointName).ToList();
            }else
            {
                ViewBag.ReleasePoints = db.ReleasePoints.Where(r=>r.IsActive == true).OrderBy(r=>r.ReleasePointName).ToList();
            }

            return View();
        }

        // POST: Races/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RaceId,RaceName,RaceStartDate,ReleasePointId,RaceCutOffDate,RaceEndedDate,ForceRaceDone,RaceLoadingDate,RaceDescription")] Race race)
        {
            race.DateCreated = DateTime.UtcNow.AddHours(8);

            string userId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (user.ClubId != null)
            {
                race.ClubId = user.ClubId;
            }

            if (ModelState.IsValid)
            {
                db.Races.Add(race);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // ViewBag.ReleasePointId = new SelectList(db.ReleasePoints, "ReleasePointId", "ReleasePointName");

            if (userId != null)
            {
                ViewBag.ReleasePoints = db.ReleasePoints.Where(r => r.ClubId == user.ClubId && r.IsActive == true).OrderBy(r => r.ReleasePointName).ToList();
            }
            else
            {
                ViewBag.ReleasePoints = db.ReleasePoints.Where(r => r.IsActive == true).OrderBy(r => r.ReleasePointName).ToList();
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
            Race race = db.Races.Include(r=>r.ReleasePoint).FirstOrDefault(r=>r.RaceId == id);
            if (race == null)
            {
                return HttpNotFound();
            }

            string userId = User.Identity.GetUserId();
            if (userId != null)
            {
                ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);
                ViewBag.ReleasePoints = db.ReleasePoints.Where(r => r.ClubId == user.ClubId && r.IsActive == true).OrderBy(r => r.ReleasePointName).ToList();
            }
            else
            {
                ViewBag.ReleasePoints = db.ReleasePoints.Where(r => r.IsActive == true).OrderBy(r => r.ReleasePointName).ToList();
            }

            return View(race);
        }

        // POST: Races/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RaceId,ClubId,RaceName,RaceStartDate,RaceCutOffDate,ReleasePointId,RaceEndedDate,DateCreated,ForceRaceDone,RaceLoadingDate,RaceDescription")] Race race)
        {
            if (ModelState.IsValid)
            {
                db.Entry(race).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            string userId = User.Identity.GetUserId();
            if (userId != null)
            {
                ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);
                ViewBag.ReleasePoints = db.ReleasePoints.Where(r => r.ClubId == user.ClubId && r.IsActive == true).OrderBy(r => r.ReleasePointName).ToList();
            }
            else
            {
                ViewBag.ReleasePoints = db.ReleasePoints.Where(r => r.IsActive == true).OrderBy(r => r.ReleasePointName).ToList();
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

        public ActionResult EndRace(int? id)
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

            race.ForceRaceDone = true;
            race.RaceEndedDate = DateTime.UtcNow.AddHours(8);
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

        public ActionResult PrintQR(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BirdRace br = db.BirdsRace.Include(r=>r.Bird).FirstOrDefault(r=>r.BirdRaceId == id);

            if (br == null)
            {
                return HttpNotFound();
            }

            return View(br);
        }

        public ActionResult PrintQRAll(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Race br = db.Races.Include(r => r.Birds).FirstOrDefault(r => r.RaceId == id);

            if (br == null)
            {
                return HttpNotFound();
            }

            return View(br);
        }
    }
}
