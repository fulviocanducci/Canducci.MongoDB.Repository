using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class PersonController : Controller
    {
        public RepositoryPersonImplementation Repository { get; }

        public PersonController(RepositoryPersonImplementation repository)
        {
            Repository = repository;
        }
                
        public ActionResult Index(int? page)
        {            
            var total = 2;                    
            var data = Repository.PagedList(page ?? 1, total, w => w.Id != null && w.Name != null, x => x.Name);
            return View(data);
        }

        public ActionResult Details(string id)
        {
            var data = Repository.Find(x => x.Id == id);
            return View(data);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Person person)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Repository.Add(person);
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(string id)
        {
            var data = Repository.Find(x => x.Id == id);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, Person person)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Repository.Edit(x => x.Id == id, person);
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(string id)
        {
            var data = Repository.Find(x => x.Id == id);
            return View(data);
        }
                
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id, IFormCollection collection)
        {
            try
            {
                Repository.Delete(x => x.Id == id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
