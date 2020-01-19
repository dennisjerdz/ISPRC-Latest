using ISPRC.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ISPRC.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            DateTime now = DateTime.UtcNow.AddHours(8);

            if (User.IsInRole("Admin"))
            {
                ViewBag.Accounts = db.Users.ToList().Count();
                ViewBag.Races = db.Races.ToList().Count();
                ViewBag.Birds = db.Birds.Count();
                ViewBag.ActiveRaces = db.Races.Where(x => x.RaceStartDate < now && x.RaceCutOffDate > now).ToList().Count();
                ViewBag.ActiveRacers = db.BirdsRace.Where(x => x.ArrivalDate != null).ToList().Count();
                ViewBag.LastRace = db.Races.OrderByDescending(x => x.RaceCutOffDate).FirstOrDefault()?.RaceName;
            }

            if (User.IsInRole("Club Owner"))
            {
                string userId = User.Identity.GetUserId();
                ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);
                int clubId = user.ClubId.Value;

                ViewBag.Members = db.Users.Where(x=>x.ClubId == clubId).ToList().Count();
                ViewBag.Races = db.Races.Where(x=>x.ClubId == clubId).ToList().Count();
                ViewBag.Birds = db.Birds.Include("Owner").Where(x=>x.Owner.ClubId == clubId).Count();
                ViewBag.ActiveRaces = db.Races.Where(x=>x.ClubId == clubId && x.RaceStartDate < now && x.RaceCutOffDate > now).ToList().Count();
                ViewBag.ActiveRacers = db.BirdsRace.Include("Race").Where(x=>x.Race.ClubId == clubId && x.ArrivalDate != null).ToList().Count();
                ViewBag.LastRace = db.Races.OrderByDescending(x => x.RaceCutOffDate).FirstOrDefault(x=>x.ClubId == clubId)?.RaceName;
            }

            if (User.IsInRole("Member"))
            {
                string userId = User.Identity.GetUserId();
                ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);

                ViewBag.Birds = db.Birds.Where(x => x.OwnerId == userId).Count();
                ViewBag.RacesParticipated = db.BirdsRace.Where(x=>x.Bird.OwnerId == userId).ToList().Count();
                ViewBag.ActiveRacers = db.BirdsRace.Include("Race").Where(x => x.Bird.OwnerId == userId && x.ArrivalDate != null).ToList().Count();
                
                int won = 0;
                var racesJoined = db.Races.Where(x => x.Birds.Any(y => y.Bird.OwnerId == userId)).ToList();
                foreach (var r in racesJoined)
                {
                    var firstPlace = r.Birds.OrderByDescending(b => b.Speed).FirstOrDefault();
                    if (firstPlace != null)
                    {
                        if (firstPlace.Bird.OwnerId == userId)
                        {
                            won++;
                        }
                    }
                }

                ViewBag.RaceWon = won;
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}