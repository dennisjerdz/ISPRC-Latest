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
    public class ReleasePointsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ReleasePoints
        public ActionResult Index()
        {
            return View(db.ReleasePoints.ToList());
        }

        // GET: ReleasePoints/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReleasePoint releasePoint = db.ReleasePoints.Find(id);
            if (releasePoint == null)
            {
                return HttpNotFound();
            }
            return View(releasePoint);
        }

        // GET: ReleasePoints/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReleasePoints/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReleasePointId,ReleasePointName,ReleasePointCoordinates,RaceLatitudeCoordinate,RaceLongitudeCoordinate")] ReleasePoint releasePoint)
        {
            releasePoint.DateCreated = DateTime.UtcNow.AddHours(8);
            releasePoint.ReleasePointCoordinates = releasePoint.RaceLatitudeCoordinate + "," + releasePoint.RaceLongitudeCoordinate;

            if (ModelState.IsValid)
            {
                db.ReleasePoints.Add(releasePoint);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(releasePoint);
        }

        // GET: ReleasePoints/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReleasePoint releasePoint = db.ReleasePoints.Find(id);
            if (releasePoint == null)
            {
                return HttpNotFound();
            }
            return View(releasePoint);
        }

        // POST: ReleasePoints/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReleasePointId,ReleasePointName,ReleasePointCoordinates,RaceLatitudeCoordinate,RaceLongitudeCoordinate")] ReleasePoint releasePoint)
        {
            if (ModelState.IsValid)
            {
                db.Entry(releasePoint).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(releasePoint);
        }

        // GET: ReleasePoints/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReleasePoint releasePoint = db.ReleasePoints.Find(id);
            if (releasePoint == null)
            {
                return HttpNotFound();
            }
            return View(releasePoint);
        }

        // POST: ReleasePoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ReleasePoint releasePoint = db.ReleasePoints.Find(id);
            db.ReleasePoints.Remove(releasePoint);
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
