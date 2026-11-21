# Canducci MongoDB Repository

[![NuGet](https://img.shields.io/nuget/v/Canducci.MongoDB.Repository.svg?style=plastic&label=version)](https://www.nuget.org/packages/Canducci.MongoDB.Repository/)
[![NuGet](https://img.shields.io/nuget/dt/Canducci.MongoDB.Repository.svg)](https://www.nuget.org/packages/Canducci.MongoDB.Repository/)

#### Install Package (NUGET)
To install Canducci MongoDB Repository run the following command in the Package Manager Console

```csharp
PM> Install-Package Canducci.MongoDB.Repository
```

#### How to use?

Configure MongoDB in your `appsettings.json`:

```json
{
  "MongoDB": {
    "Database": "dbnew",
    "ConnectionStrings": "mongodb://localhost:27017"
  }
}
```

Register the repository using the application configuration:

```csharp
services.AddMongoDBRepository(configuration);
```

Alternatively, provide the database and connection string directly:

```csharp
services.AddMongoDBRepository("dbnew", "mongodb://localhost:27017");
```

Both overloads register a single `IMongoClient` instance and `IConnect` as singletons. The same MongoDB client is reused by the repository.

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

Configure dependency injection in `ConfigureServices`. Repository implementations can remain scoped:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMongoDBRepository(configuration);
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
            return View(person);
        }
    }
}
```
