# Canducci MongoDB Repository

[![NuGet](https://img.shields.io/nuget/v/Canducci.MongoDB.Repository.svg?style=plastic&label=version)](https://www.nuget.org/packages/Canducci.MongoDB.Repository/)
[![NuGet](https://img.shields.io/nuget/dt/Canducci.MongoDB.Repository.svg)](https://www.nuget.org/packages/Canducci.MongoDB.Repository/)
[![Build Status](https://travis-ci.org/fulviocanducci/Canducci.MongoDB.Repository.svg?branch=master)](https://travis-ci.org/fulviocanducci/Canducci.MongoDB.Repository)
![Github Workflows](https://github.com/fulviocanducci/Canducci.MongoDB.Repository/workflows/.NET%20Core/badge.svg)

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

Configure `Startup.cs` in method `ConfigureServices`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IConnect, Connect>();
    services.AddScoped<RepositoryPersonImplementation, RepositoryPerson>();

    services.AddControllersWithViews();
}
```

In `controller`:

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