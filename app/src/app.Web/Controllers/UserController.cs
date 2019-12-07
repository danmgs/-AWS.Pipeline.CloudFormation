using app.DAL.Managers;
using app.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace app.Web.Controllers
{
    public class UserController : Controller
    {
        IDBManager<User> _dbManager;

        public UserController(IDBManager<User> dbManager)
        {
            _dbManager = dbManager;
        }

        // GET: Default
        public ActionResult<IEnumerable<User>> Index()
        {
            IEnumerable<User> users = _dbManager.Scan();
            return View(users);
        }

        // GET: Default/Details/5
        public ActionResult Details(string id)
        {
            return View();
        }

        // GET: Default/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Default/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                User newUser = new User()
                {
                    Id = Guid.NewGuid().ToString(),
                    LastName = collection["LastName"],
                    FirstName = collection["FirstName"]
                };

                _dbManager.Create(newUser);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Default/Edit/5
        public async Task<ActionResult<User>> Edit(string id)
        {
            User user = await _dbManager.Get(id);
            return View(user);
        }

        // POST: Default/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, IFormCollection collection)
        {
            try
            {
                User updatedUser = new User()
                {
                    Id = id,
                    LastName = collection["LastName"],
                    FirstName = collection["FirstName"]
                };

                _dbManager.Update(updatedUser);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Default/Delete/5
        public async Task<ActionResult<User>> Delete(string id)
        {
            User user = await _dbManager.Get(id);
            return View(user);
        }

        // POST: Default/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<User>> Delete(string id, IFormCollection collection)
        {
            try
            {
                await _dbManager.Delete(id);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}