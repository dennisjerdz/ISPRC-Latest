using ISPRC.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ISPRC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Accounts()
        {
            List<AdminAccountModel> users = db.Users.Select(u=>new AdminAccountModel()
            {
                Email = u.Email,
                Name = u.GivenName+" "+u.LastName,
                Id = u.Id,
                Role = db.Roles.FirstOrDefault(r=>r.Id == u.Roles.FirstOrDefault().RoleId).Name,
                Locked = u.LockoutEnabled
            }).ToList();

            return View(users);
        }
        
    }
}
