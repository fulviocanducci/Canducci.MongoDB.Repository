# Canducci MongoDB Repository

[![Build and Publish](https://github.com/fulviocanducci/Canducci.MongoDB.Repository/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/fulviocanducci/Canducci.MongoDB.Repository/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Canducci.MongoDB.Repository.svg?style=plastic&label=version)](https://www.nuget.org/packages/Canducci.MongoDB.Repository/)
[![NuGet](https://img.shields.io/nuget/dt/Canducci.MongoDB.Repository.svg)](https://www.nuget.org/packages/Canducci.MongoDB.Repository/)

## Installation

Install the package from NuGet:

```powershell
dotnet add package Canducci.MongoDB.Repository
```

Or use the Package Manager Console:

```powershell
Install-Package Canducci.MongoDB.Repository
```

## Target frameworks and MongoDB driver

| Target framework | MongoDB.Driver |
| --- | --- |
| `netstandard2.0` | 2.30.0 |
| `netstandard2.1` | 3.14.1 |
| `net10.0` | 3.14.1 |

## Configuration

Add the MongoDB settings to your `appsettings.json`:

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
builder.Services.AddMongoDBRepository(builder.Configuration);
```

Alternatively, provide the database name and connection string directly:

```csharp
builder.Services.AddMongoDBRepository("dbnew", "mongodb://localhost:27017");
```

Both overloads register a singleton `IMongoClient` and a singleton `IConnect`. The same MongoDB client instance is reused by the repository.

## Model

Create a class that represents a MongoDB collection:

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
        [BsonRequired]
        [Required]
        public string Name { get; set; }
    }
}
```

The `BsonCollectionName` attribute defines the MongoDB collection name. If it is not provided, the repository uses the class name.

## Repository

Create an abstract repository and its concrete implementation:

```csharp
using Canducci.MongoDB.Repository;

namespace WebApp.Models
{
    public abstract class RepositoryPersonImplementation : Repository<Person>
    {
        protected RepositoryPersonImplementation(IConnect connect) : base(connect)
        {
        }
    }

    public sealed class RepositoryPerson : RepositoryPersonImplementation
    {
        public RepositoryPerson(IConnect connect) : base(connect)
        {
        }
    }
}
```

Register the repository implementation in dependency injection:

```csharp
builder.Services.AddScoped<RepositoryPersonImplementation, RepositoryPerson>();
```

Inject it into your controller:

```csharp
public class PersonController : Controller
{
    private readonly RepositoryPersonImplementation _repository;

    public PersonController(RepositoryPersonImplementation repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        IReadOnlyList<Person> people = _repository.All();
        return View(people);
    }

    public IActionResult Details(string id)
    {
        Person person = _repository.Find(x => x.Id == id);
        return View(person);
    }
}
```

## Pagination

The pagination methods return `Canducci.Pagination.Interfaces.IPaginated<T>`:

```csharp
using Canducci.Pagination.Interfaces;

public async Task<IActionResult> Index(int? page)
{
    const int pageSize = 5;
    IPaginated<Person> people = await _repository.PagedListAsync(page ?? 1, pageSize);
    return View(people);
}
```

In a Razor view:

```cshtml
@using Canducci.Pagination.Interfaces
@model IPaginated<WebApp.Models.Person>

<table class="table">
    <thead>
        <tr>
            <th>Id</th>
            <th>Name</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var person in Model)
        {
            <tr>
                <td>@person.Id</td>
                <td>@person.Name</td>
            </tr>
        }
    </tbody>
</table>
```

### Canducci.Pagination.Mvc integration

The repository pagination result implements `IPaginated<T>`, so it can be passed directly to the HTML helper from `Canducci.Pagination.Mvc`.

Install the MVC integration package:

```powershell
dotnet add package Canducci.Pagination.Mvc
```

Use the helper in your Razor view:

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

The MVC package provides pagination styles and HTML rendering. The MongoDB repository remains independent of ASP.NET Core MVC and executes paged queries through the MongoDB driver.

## Repository operation contracts (4.0.0)

Collection-returning methods return materialized `IReadOnlyList<T>` results. This applies to `Add(IEnumerable<T>)`, `AddAsync(IEnumerable<T>)`, `All`, `AllAsync`, `List` and `ListAsync`. The returned collection is a stable snapshot and does not execute a deferred MongoDB query when enumerated.

Update methods return the MongoDB driver's `UpdateResult` instead of a boolean:

- `Update` / `UpdateAsync`: update one matching document.
- `UpdateAll` / `UpdateAllAsync`: update all matching documents.

Use `MatchedCount` to inspect how many documents matched, `ModifiedCount` to inspect how many documents changed, `IsAcknowledged` to inspect write acknowledgement, and `UpsertedId` when an upsert created a document.

`Edit` and `EditAsync` return `true` when a document matched the replacement filter, even if the replacement content was identical and MongoDB reports no modified documents.

Asynchronous operations accept an optional `CancellationToken`.

## Build and release

The GitHub Actions workflow is defined in `.github/workflows/build.yml`. It restores dependencies, builds the library for all target frameworks, runs the test project, and generates a NuGet package artifact.

The workflow runs on pushes and pull requests targeting `master`, and can also be started manually. These executions **do not publish** to NuGet.

Publishing is restricted to version tags matching `v*`. For example, creating and pushing the tag `v4.0.1` runs the build and tests, creates package version `4.0.1`, and publishes it through NuGet Trusted Publishing (GitHub OIDC). No long-lived NuGet API key is stored in GitHub Secrets.

To create a release tag after reviewing and merging the changes:

```bash
git tag v4.0.1
git push origin v4.0.1
```

Do not create a version tag until you are ready to publish that version.
