using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class CarController : Controller
    {
        private readonly RepositoryCarImplementation Repository;

        public CarController(RepositoryCarImplementation repository)
        {
            Repository = repository;
        }
        public ActionResult Index()
        {
            return View(Repository.All());
        }

        public ActionResult Details(ObjectId id)
        {
            return View(Repository.Find(id));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Car car)
        {
            try
            {
                Repository.Add(car);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(ObjectId id)
        {
            return View(Repository.Find(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ObjectId id, Car car)
        {
            try
            {
                Repository.Edit(id, car); Repository.Edit()
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(ObjectId id)
        {
            return View(Repository.Find(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ObjectId id, IFormCollection collection)
        {
            try
            {
                Repository.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
