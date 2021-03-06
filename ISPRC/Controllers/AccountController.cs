﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ISPRC.Models;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ISPRC.Controllers
{
    
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            
            // Subscription subscription = db.Subscriptions.Where(s => s.User.Email == model.Email).OrderByDescending(s => s.EndOfSubscriptionDate).FirstOrDefault();
            // bool subscriptionExpired = false;
            switch (result)
            {
                case SignInStatus.Success:
                    bool locked = db.Users.FirstOrDefault(u => u.Email == model.Email).LockoutEnabled;

                    if (locked)
                    {
                        goto case SignInStatus.LockedOut;
                    }

                    /*
                    if (subscription != null)
                    {
                        if (DateTime.UtcNow.AddHours(8) > subscription.EndOfSubscriptionDate)
                        {
                            subscriptionExpired = true;

                            if (!User.IsInRole("Member"))
                            {
                                return RedirectToAction("Index","Home",new { id="SubscriptionExpired" });
                            }

                            goto default;
                        }
                    } */

                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    /*if (subscriptionExpired)
                    {
                        ModelState.AddModelError("", "Your subscription expired at " + subscription.EndOfSubscriptionDate + ". Please contact your Club Owner to renew.");
                    }*/
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ClubName");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, GivenName = model.GivenName, MiddleName = model.MiddleName, LastName = model.LastName };
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var roleStore = new RoleStore<IdentityRole>(db);
                    var roleManager = new RoleManager<IdentityRole>(roleStore);

                    var userStore = new UserStore<ApplicationUser>(db);
                    var userManager = new UserManager<ApplicationUser>(userStore);
                    userManager.AddToRole(user.Id, "Member");

                    var newUser = db.Users.FirstOrDefault(u => u.Email == user.Email);
                    newUser.GivenName = model.GivenName;
                    newUser.MiddleName = model.MiddleName;
                    newUser.LastName = model.LastName;
                    newUser.Address = model.Address;
                    newUser.ClubId = model.ClubId;
                    newUser.MobileNumber = model.MobileNumber;
                    // lock initially
                    newUser.LockoutEnabled = true;
                    db.SaveChanges();

                    // await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ClubName");
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        public ActionResult Accounts()
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (User.IsInRole("Admin"))
            {
                List<AdminAccountModel> users = db.Users.Select(u => new AdminAccountModel()
                {
                    Email = u.Email,
                    Name = u.GivenName + " " + u.LastName,
                    ClubName = (u.Club.ClubName == null) ? "No Club" : u.Club.ClubName,
                    Id = u.Id,
                    Role = db.Roles.FirstOrDefault(r => r.Id == u.Roles.FirstOrDefault().RoleId).Name,
                    Locked = u.LockoutEnabled
                }).ToList();

                return View(users);
            }
            else
            {
                List<AdminAccountModel> users = db.Users.Where(u=>u.ClubId == user.ClubId).Select(u => new AdminAccountModel()
                {
                    Email = u.Email,
                    Name = u.GivenName + " " + u.LastName,
                    ClubName = (u.Club.ClubName == null) ? "No Club" : u.Club.ClubName,
                    Id = u.Id,
                    Role = db.Roles.FirstOrDefault(r => r.Id == u.Roles.FirstOrDefault().RoleId).Name,
                    Locked = u.LockoutEnabled
                }).ToList();

                return View(users);
            }
        }

        public ActionResult Unlock(string id)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == id);
            user.LockoutEnabled = false;
            db.SaveChanges();

            return RedirectToAction("Accounts");
        }

        public ActionResult Lock(string id)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == id);
            user.LockoutEnabled = true;
            db.SaveChanges();

            return RedirectToAction("Accounts");
        }

        public ActionResult EditLoftCoordinates(string id)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == id);

            EditLoftCoordinateModel e = new EditLoftCoordinateModel();

            e.Name = user.GivenName;
            e.UserId = user.Id;
            e.LoftLatitudeCoordinate = user.LoftLatitudeCoordinate;
            e.LoftLongitudeCoordinate = user.LoftLongitudeCoordinate;

            return View(e);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditLoftCoordinates(EditLoftCoordinateModel e)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == e.UserId);
            user.LoftLatitudeCoordinate = e.LoftLatitudeCoordinate;
            user.LoftLongitudeCoordinate = e.LoftLongitudeCoordinate;
            db.SaveChanges();

            return RedirectToAction("Accounts", "Account");
        }

        public ActionResult AddMember()
        {
            if (User.IsInRole("Club Owner"))
            {
                string userId = User.Identity.GetUserId();
                ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);
                int clubId = user.ClubId.Value;

                ViewBag.ClubId = new SelectList(db.Clubs.Where(x=>x.ClubId == clubId), "ClubId", "ClubName");
                return View();
            }
            else
            {
                ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ClubName");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddMember(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, GivenName = model.GivenName, MiddleName = model.MiddleName, LastName = model.LastName };
                var result = await UserManager.CreateAsync(user, model.Password);

                var roleStore = new RoleStore<IdentityRole>(db);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);
                userManager.AddToRole(user.Id, "Member");

                if (result.Succeeded)
                {
                    var newUser = db.Users.FirstOrDefault(u => u.Email == user.Email);
                    newUser.GivenName = model.GivenName;
                    newUser.MiddleName = model.MiddleName;
                    newUser.LastName = model.LastName;
                    newUser.ClubId = model.ClubId;
                    // lock initially
                    newUser.LockoutEnabled = true;

                    db.SaveChanges();

                    // await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Accounts", "Account");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            if (User.IsInRole("Club Owner"))
            {
                string userId = User.Identity.GetUserId();
                ApplicationUser user = db.Users.FirstOrDefault(u => u.Id == userId);
                int clubId = user.ClubId.Value;

                ViewBag.ClubId = new SelectList(db.Clubs.Where(x => x.ClubId == clubId), "ClubId", "ClubName");
                return View(model);
            }
            else
            {
                ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ClubName");
                return View(model);
            }
        }

        public ActionResult AddClubOwner()
        {
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ClubName");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddClubOwner(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, GivenName = model.GivenName, MiddleName = model.MiddleName, LastName = model.LastName };
                var result = await UserManager.CreateAsync(user, model.Password);

                var roleStore = new RoleStore<IdentityRole>(db);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);
                userManager.AddToRole(user.Id, "Club Owner");

                if (result.Succeeded)
                {
                    var newUser = db.Users.FirstOrDefault(u => u.Email == user.Email);
                    newUser.GivenName = model.GivenName;
                    newUser.MiddleName = model.MiddleName;
                    newUser.LastName = model.LastName;
                    newUser.ClubId = model.ClubId;
                    // lock initially
                    newUser.LockoutEnabled = true;

                    db.SaveChanges();

                    // await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Accounts", "Account");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            ViewBag.ClubId = new SelectList(db.Clubs, "ClubId", "ClubName");
            return View(model);
        }

        public ActionResult Subscriptions(string id)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                return View(user);
            }
            else
            {
                return new HttpStatusCodeResult(404);
            }
        }

        public ActionResult AddSubscription(string id)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                Subscription s = new Subscription();
                s.User = user;
                s.UserId = user.Id;
                s.SubscriptionDescription = DateTime.UtcNow.AddHours(8).Month.ToString()+"-"+ DateTime.UtcNow.AddHours(8).Year.ToString()+"-Subscription";

                return View(s);
            }
            else
            {
                return new HttpStatusCodeResult(404);
            }
        }

        [HttpPost]
        public ActionResult AddSubscription(Subscription s)
        {
            s.DateCreated = DateTime.UtcNow.AddHours(8);

            if (ModelState.IsValid)
            {
                db.Subscriptions.Add(s);
                db.SaveChanges();
                return RedirectToAction("Subscriptions", new { id = s.UserId });
            }
            else
            {
                return View(s);
            }
        }

        public ActionResult DeleteSubscription(int id)
        {
            var subscription = db.Subscriptions.FirstOrDefault(s=>s.SubscriptionId == id);
            if (subscription != null)
            {
                string userId = subscription.UserId;
                db.Subscriptions.Remove(subscription);
                db.SaveChanges();
                return RedirectToAction("Subscriptions", new { id = userId });
            }

            return RedirectToAction("Accounts");
        }
    }
}