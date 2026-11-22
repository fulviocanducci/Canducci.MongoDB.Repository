using Microsoft.AspNetCore.Mvc;
using Web11.Models;
using Web11.Repositories;

namespace Web11.Controllers
{
    public class PeopleController(IRepositoryPeople repository) : Controller
    {
        private readonly IRepositoryPeople repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async Task<IActionResult> Index(int? current, CancellationToken cancellationToken = default)
        {
            current ??= 1;
            var data = await repository.PagedListAsync(current.Value, 5, o => o.Name, cancellationToken);

            return View(data);
        }

        public ActionResult Details(string id)
        {
            var data = repository.Find(id);
            return View(data);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(People model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    repository.Add(model);
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
            var data = repository.Find(x => x.Id == id);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, People person)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    repository.Edit(x => x.Id == id, person);
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
            var data = repository.Find(x => x.Id == id);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id, IFormCollection collection)
        {
            try
            {
                repository.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
