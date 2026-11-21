# Canducci MongoDB Repository

[![NuGet](https://img.shields.io/nuget/v/Canducci.MongoDB.Repository.svg?style=plastic&label=version)](https://www.nuget.org/packages/Canducci.MongoDB.Repository/)
[![NuGet](https://img.shields.io/nuget/dt/Canducci.MongoDB.Repository.svg)](https://www.nuget.org/packages/Canducci.MongoDB.Repository/)

#### Install Package (NUGET)

To install Canducci MongoDB Repository run the following command in the Package Manager Console

```csharp
PM> Install-Package Canducci.MongoDB.Repository
```

#### How to use?

Create in your `appsettings.json` key ***MongoDB***:

```json
{
  ...
  "MongoDB": {
    "Database": "dbnew",
    "ConnectionStrings": "mongodb://localhost:27017"
  }
}
```

Make a class that represents your Collection in MongoDB

```csharp
using Canducci.MongoDB.Repository.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    [BsonCollectionName("person")]
    public class Person
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }

        [BsonElement("name")]
        [BsonRequired()]
        [Required()]
        public string Name { get; set; }
    }
}
```

**Note:** It has a BsonCollectionName attribute that has the configuration of the name of your collection in mongo , if by chance not pass he takes the class name.

Next step will be the creation of Repository.

Create a class and declare these two `namespace`:

#### Codification:

```csharp
using Canducci.MongoDB.Repository;
namespace WebApp.Models
{
    //class abstract
    public abstract class RepositoryPersonImplementation : Repository<Person>
    {
        protected RepositoryPersonImplementation(IConnect connect) : base(connect)
        {
        }
    }
}

namespace WebApp.Models
{
    //class Concret
    public sealed class RepositoryPerson : RepositoryPersonImplementation
    {
        public RepositoryPerson(IConnect connect) : base(connect)
        {
        }
    }
}
```

Configure dependency injection in `ConfigureServices`. The `AddMongoDBRepository()` extension registers `IConnect` as a singleton, so the application reuses the same `MongoClient` and its connection pools across requests. Repository implementations can remain scoped:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMongoDBRepository();
    services.AddScoped<RepositoryPersonImplementation, RepositoryPerson>();

    services.AddControllersWithViews();
}
```

In `controller`, import `Canducci.Pagination.Interfaces` to use the `IPaginated<T>` contract:

```csharp
public class PersonController : Controller
{
    public RepositoryPersonImplementation Repository { get; }

    public PersonController(RepositoryPersonImplementation repository)
    {
        Repository = repository;
    }

    public ActionResult Index()
    {
        var data = Repository.All();
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
```

#### Paginated:

In `controller`:

```csharp
public async Task<IActionResult> Index(int? page)
{
    const int pageSize = 5;
    IPaginated<Person> data = await Repository
        .PagedListAsync(page ?? 1, pageSize);
    return View(data);
}
```

In `View`:

```cshtml
@using Canducci.Pagination.Interfaces
@model IPaginated<WebApp.Models.Person>

@{
    ViewData["Title"] = "Index";
}

<table class="table">
    <thead>
        <tr>
            <th>Id</th>
            <th>Name</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
        {
            <tr>
                <td>@item.Id</td>
                <td>@item.Name</td>
            </tr>
        }
    </tbody>
</table>

<div class="row">
    <div class="col-md-5 text-right">
        @if (Model.HasPreviousPage)
        {
            <a href="/Person?page=@(Model.PageNumber - 1)">Previous</a>
        }
    </div>
    <div class="col-md-5 text-left">
        @if (Model.HasNextPage)
        {
            <a href="/Person?page=@(Model.PageNumber + 1)">Next</a>
        }
    </div>
</div>
```

#### Integration with Canducci.Pagination.Mvc

The repository pagination result implements `Canducci.Pagination.Interfaces.IPaginated<T>`. You can therefore pass the same `IPaginated<T>` directly to the HTML helper provided by `Canducci.Pagination.Mvc`, without converting the result or loading the collection into memory again.

Install `Canducci.Pagination.Mvc` in your ASP.NET Core MVC application:

```powershell
PM> Install-Package Canducci.Pagination.Mvc
```

In your Razor View:

```cshtml
@using Canducci.Pagination.Mvc
@using Canducci.Pagination.Interfaces
@model IPaginated<WebApp.Models.Person>

@Html.Pagination(
    Model,
    page => Url.Action("Index", new { page }),
    PaginatedStyle.NumbersWithFirstPreviousNextLast
)
```

The MVC package provides the pagination styles and HTML rendering. The MongoDB repository remains independent of ASP.NET Core MVC and continues to execute paged queries through the MongoDB driver.
