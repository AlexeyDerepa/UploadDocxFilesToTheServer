using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UploadFilesToServer.Models;

namespace UploadFilesToServer.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("UploadFiles", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Password or login is wrong");
                }
            }
            return View(model);
        }

        //
        // GET: /Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        //
        // POST: /Account/RegisterPost
        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (UploadContext db = new UploadContext())
                {
                    User existUser = db.Users.FirstOrDefault(u => u.Login == model.UserName);
                    if (existUser != null)
                    {
                        ModelState.AddModelError("", "This login already exist. Enter another login");
                        return View();
                    }

                    Role role = db.Roles.FirstOrDefault(r => r.Name == "User");

                    if (role != null)
                    {
                        var user = new User { Login = model.UserName, Password = model.Password, Role = role, RoleId = role.Id };
                        db.Users.Add(user);
                        db.SaveChanges();

                        return RedirectToAction("Login", "Account");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        private bool ValidateUser(string login, string password)
        {
            bool isValid = false;

            using (UploadContext _db = new UploadContext())
            {
                try
                {
                    User user = (from u in _db.Users
                                 where u.Login == login && u.Password == password
                                 select u).FirstOrDefault();

                    if (user != null)
                    {
                        isValid = true;
                    }
                }
                catch
                {
                    isValid = false;
                }
            }
            return isValid;
        }
    }
}