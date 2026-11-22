# Canducci MongoDB Repository

[![Build and Publish](https://github.com/fulviocanducci/Canducci.MongoDB.Repository/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/fulviocanducci/Canducci.MongoDB.Repository/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Canducci.MongoDB.Repository.svg?style=plastic&label=version)](https://www.nuget.org/packages/Canducci.MongoDB.Repository/)
[![NuGet downloads](https://img.shields.io/nuget/dt/Canducci.MongoDB.Repository.svg)](https://www.nuget.org/packages/Canducci.MongoDB.Repository/)

**A repository abstraction for MongoDB with dependency injection, dynamic proxies, synchronous and asynchronous operations, and pagination.**

**Uma abstração de repositório para MongoDB com injeção de dependência, proxies dinâmicos, operações síncronas e assíncronas e paginação.**

- [English](#english)
- [Português](#português)

---

# English

## 1. Version guide

### Versions 3.x.x — previous approach

Version **3.x.x and earlier** use the previous repository workflow, based on creating repository implementation classes and registering those implementations in dependency injection.

If your application uses version 3.x.x, keep its existing implementation while you plan the migration. Version 4 introduces a different, interface-first registration model and changes some public contracts.

### Versions 4.x.x — interface-first approach

Version **4.x.x** introduces a simpler way to consume repositories:

1. Create your entity model.
2. Declare an empty interface inheriting from `IRepository<TEntity>`.
3. Configure the library with `RepositoryRegistrationMode.Proxy`.
4. Inject your interface wherever it is needed.

The library discovers eligible interfaces and creates their implementations dynamically. You do not need to write a repository class for standard CRUD and query operations.

| Version | Repository style | Registration |
| --- | --- | --- |
| 3.x.x and earlier | Concrete repository implementation | Register the implementation manually |
| 4.x.x | Interface-first, with dynamic proxy support | Automatic in Proxy mode, or manual implementation |

**Important:** the 4.x API is not a drop-in replacement for every 3.x implementation. Review the contracts and return types when upgrading.

## 2. Installation

Install the package from NuGet:

```powershell
dotnet add package Canducci.MongoDB.Repository
```

Or use the Package Manager Console:

```powershell
Install-Package Canducci.MongoDB.Repository
```

## 3. Target frameworks and MongoDB driver

| Target framework | MongoDB.Driver |
| --- | --- |
| `netstandard2.0` | 2.30.0 |
| `netstandard2.1` | 3.14.1 |
| `net10.0` | 3.14.1 |

## 4. Configure the connection

### Option A — configure in Program.cs

This is the most direct configuration and supports both registration modes.

```csharp
using Canducci.MongoDB.Repository;
using Canducci.MongoDB.Repository.DependencyInjection;

builder.Services.AddMongoDBRepository(config =>
{
    config.ConnectionString = "mongodb://localhost:27017";
    config.Database = "my_database";
    config.RegistrationMode = RepositoryRegistrationMode.Proxy;
});
```

- `ConnectionString`: MongoDB connection string.
- `Database`: database name.
- `RegistrationMode.Proxy`: discovers and registers eligible repository interfaces automatically.
- `RegistrationMode.Manual`: registers the MongoDB client and connection, leaving repository registration to your application.

### Option B — appsettings.json

```json
{
  "MongoDB": {
    "Database": "my_database",
    "ConnectionStrings": "mongodb://localhost:27017",
    "Proxy": 1
  }
}
```

Register the configuration:

```csharp
using Canducci.MongoDB.Repository.DependencyInjection;

builder.Services.AddMongoDBRepository(builder.Configuration);
```

The `MongoDB:Proxy` setting accepts:

| Value | Mode | Behavior |
| --- | --- | --- |
| `0` | Manual | No repository interfaces are registered automatically |
| `1` | Proxy | Eligible empty interfaces inheriting from `IRepository<TEntity>` are registered automatically |

### Option C — manual mode with explicit values

```csharp
using Canducci.MongoDB.Repository.DependencyInjection;

builder.Services.AddMongoDBRepository(
    "my_database",
    "mongodb://localhost:27017");
```

This overload uses `Manual` mode.

**Security note:** do not commit production credentials to source control. Use environment variables, user secrets, or a secret manager to provide connection strings.

All configuration overloads register a singleton `IMongoClient` and a singleton `IConnect`. Repository proxies use the scoped lifetime.

## 5. Create the entity model

Create a class representing a MongoDB document:

```csharp
using Canducci.MongoDB.Repository.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel.DataAnnotations;

namespace MyApplication.Models;

[BsonCollectionName("people")]
public class People
{
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    public string Id { get; set; }

    [BsonElement("name")]
    [BsonRequired]
    [Required]
    public string Name { get; set; }

    [BsonElement("active")]
    public bool Active { get; set; }
}
```

- `BsonCollectionName` defines the MongoDB collection name. If omitted, the class name is used.
- `BsonId` maps the property to MongoDB's document identifier.
- `BsonElement` defines the stored field name.

## 6. Create the repository interface — Proxy mode

Create an empty interface that inherits from `IRepository<TEntity>`:

```csharp
using Canducci.MongoDB.Repository;
using MyApplication.Models;

namespace MyApplication.Repositories;

public interface IRepositoryPeople : IRepository<People>
{
}
```

That's all. No concrete `RepositoryPeople` class is required for the standard repository operations.

With Proxy mode enabled, the library:

1. Scans the assemblies already loaded by the application.
2. Finds public, non-generic, empty interfaces that inherit from a closed `IRepository<TEntity>`.
3. Creates a dynamic implementation using `DispatchProxy`.
4. Forwards repository calls to the internal MongoDB repository implementation.
5. Registers the interface as scoped in dependency injection.

Inject the interface into a controller or service:

```csharp
using Microsoft.AspNetCore.Mvc;
using MyApplication.Models;
using MyApplication.Repositories;

public class PeopleController : Controller
{
    private readonly IRepositoryPeople _repository;

    public PeopleController(IRepositoryPeople repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        IReadOnlyList<People> people = _repository.All();
        return View(people);
    }
}
```

**Proxy requirements:** the interface must be public, non-generic, empty (no methods of its own), and inherit from a closed `IRepository<TEntity>` contract. Interfaces with additional methods are not automatically registered by this proxy mechanism.

## 7. Manual repository mode

If you need a custom repository class, or prefer explicit registration, keep using the traditional inheritance model:

```csharp
using Canducci.MongoDB.Repository;
using MyApplication.Models;

public class PeopleRepository : Repository<People>
{
    public PeopleRepository(IConnect connect)
        : base(connect)
    {
    }
}
```

Register it:

```csharp
builder.Services.AddScoped<PeopleRepository>();
```

Then inject `PeopleRepository` into your service or controller. You can also implement your own interface and register it explicitly.

## 8. Repository methods

All methods below are available through `IRepository<TEntity>`. Unless noted otherwise, asynchronous methods accept an optional `CancellationToken`.

### 8.1 Add — insert documents

| Method | Description |
| --- | --- |
| `Add(T model)` | Inserts one document and returns the inserted model. |
| `Add(IEnumerable<T> models)` | Inserts multiple documents and returns the materialized inserted models. |
| `AddAsync(T model)` | Asynchronously inserts one document. |
| `AddAsync(IEnumerable<T> models)` | Asynchronously inserts multiple documents. |

### 8.2 Find — retrieve one document

| Method | Description |
| --- | --- |
| `Find<TKey>(TKey key)` | Finds one document by its identifier. |
| `Find(Expression<Func<T, bool>> filter)` | Finds the first document matching the expression. |
| `FindAsync<TKey>(TKey key)` | Asynchronously finds one document by identifier. |
| `FindAsync(Expression<Func<T, bool>> filter)` | Asynchronously finds the first matching document. |

### 8.3 All — retrieve collections using expressions

| Method | Description |
| --- | --- |
| `All()` | Returns all documents. |
| `All(filter)` | Returns documents matching an expression. |
| `All(filter, orderBy)` | Filters and orders documents ascending by the selected expression. |
| `AllAsync()` | Asynchronously returns all documents. |
| `AllAsync(filter)` | Asynchronously returns documents matching an expression. |
| `AllAsync(filter, orderBy)` | Asynchronously filters and orders documents ascending. |

### 8.4 FindBy and FindAll — use the official MongoDB builders

These methods accept MongoDB's own `FilterDefinition<T>` and `SortDefinition<T>`. Use them when you need the full expressive power of the MongoDB driver, including compound filters and ascending or descending sorts.

**FindBy** — filter and sort:

```csharp
using MongoDB.Driver;

FilterDefinition<People> filter =
    Builders<People>.Filter.Eq(person => person.Active, true);

SortDefinition<People> sort =
    Builders<People>.Sort.Descending(person => person.Name);

IReadOnlyList<People> people = repository.FindBy(filter, sort);
IReadOnlyList<People> asyncPeople = await repository.FindByAsync(filter, sort);
```

| Method | Description |
| --- | --- |
| `FindBy(filter, sort)` | Returns matching documents using the supplied filter and sort. |
| `FindByAsync(filter, sort)` | Asynchronous version of `FindBy`. |

**FindAll** — sort all documents:

```csharp
SortDefinition<People> sort =
    Builders<People>.Sort.Ascending(person => person.Name);

IReadOnlyList<People> people = repository.FindAll(sort);
IReadOnlyList<People> asyncPeople = await repository.FindAllAsync(sort);
```

| Method | Description |
| --- | --- |
| `FindAll(sort)` | Returns all documents using the supplied sort. |
| `FindAllAsync(sort)` | Asynchronous version of `FindAll`. |

You can compose filters with the MongoDB builder:

```csharp
FilterDefinition<People> filter = Builders<People>.Filter.And(
    Builders<People>.Filter.Eq(person => person.Active, true),
    Builders<People>.Filter.Regex(person => person.Name, new MongoDB.Bson.BsonRegularExpression("^A", "i"))
);

SortDefinition<People> sort = Builders<People>.Sort
    .Ascending(person => person.Name)
    .Descending(person => person.Id);

IReadOnlyList<People> result = await repository.FindByAsync(filter, sort);
```

### 8.5 List — order and optionally filter

| Method | Description |
| --- | --- |
| `List(orderBy, filter = null)` | Returns documents ordered ascending, optionally applying an expression filter. |
| `ListAsync(orderBy, filter = null)` | Asynchronous version of `List`. |

### 8.6 Edit — replace one document

| Method | Description |
| --- | --- |
| `Edit(key, model)` | Replaces a document identified by its key. |
| `Edit(filter, model)` | Replaces the first document matching the expression. |
| `EditAsync(key, model)` | Asynchronously replaces a document by key. |
| `EditAsync(filter, model)` | Asynchronously replaces the first matching document. |

These methods return `bool`: `true` means a document matched the replacement filter, even when the replacement content is identical.

### 8.7 Update — update document fields

| Method | Description |
| --- | --- |
| `Update(filter, update)` | Updates one matching document. |
| `UpdateAll(filter, update)` | Updates all matching documents. |
| `UpdateAsync(filter, update)` | Asynchronously updates one matching document. |
| `UpdateAllAsync(filter, update)` | Asynchronously updates all matching documents. |

Update methods return MongoDB's `UpdateResult`, not a boolean. Inspect `MatchedCount`, `ModifiedCount`, `IsAcknowledged`, and `UpsertedId` as needed.

```csharp
UpdateDefinition<People> update =
    Builders<People>.Update.Set(person => person.Active, false);

UpdateResult result = repository.Update(
    person => person.Name == "John",
    update);

long matched = result.MatchedCount;
long modified = result.ModifiedCount;
```

### 8.8 Delete — remove documents

| Method | Description |
| --- | --- |
| `Delete(filter)` | Deletes one document matching the expression. |
| `Delete(key)` | Deletes one document by identifier. |
| `DeleteAsync(filter)` | Asynchronously deletes one matching document. |
| `DeleteAsync(key)` | Asynchronously deletes one document by identifier. |

Returns `bool`, indicating whether a document was deleted.

### 8.9 Count — count documents

| Method | Description |
| --- | --- |
| `Count()` | Counts all documents. |
| `Count(filter, options = null)` | Counts documents matching an expression. |
| `CountAsync()` | Asynchronously counts all documents. |
| `CountAsync(filter, options = null)` | Asynchronously counts matching documents. |

### 8.10 Query — access IQueryable

`Query()` exposes the collection as `IQueryable<T>`, allowing LINQ query composition supported by the MongoDB LINQ provider.

```csharp
IReadOnlyList<People> activePeople = repository.Query()
    .Where(person => person.Active)
    .OrderBy(person => person.Name)
    .ToList();
```

### 8.11 Collection — access IMongoCollection

The `Collection` property exposes the underlying `IMongoCollection<T>` for scenarios that require direct access to MongoDB driver functionality.

### 8.12 CreateObjectId — create an ObjectId

`CreateObjectId(string value)` converts a string representation into a MongoDB `ObjectId`.

## 9. Pagination

Pagination returns `Canducci.Pagination.Interfaces.IPaginated<T>`. You can request a page with or without ordering, and with an optional expression filter.

### Available pagination methods

| Method | Description |
| --- | --- |
| `PagedList(pageNumber, pageSize)` | Gets a page without an explicit sort. |
| `PagedListAsync(pageNumber, pageSize)` | Asynchronous version. |
| `PagedList(pageNumber, pageSize, orderBy)` | Gets an ordered page. |
| `PagedListAsync(pageNumber, pageSize, orderBy)` | Asynchronous ordered page. |
| `PagedList(pageNumber, pageSize, filter, orderBy)` | Gets a filtered and ordered page. |
| `PagedListAsync(pageNumber, pageSize, filter, orderBy)` | Asynchronous filtered and ordered page. |

### Example — ASP.NET Core controller

```csharp
using Canducci.Pagination.Interfaces;
using Microsoft.AspNetCore.Mvc;

public async Task<IActionResult> Index(int? page)
{
    const int pageSize = 10;

    IPaginated<People> people = await _repository.PagedListAsync(
        pageNumber: page ?? 1,
        pageSize: pageSize,
        filter: person => person.Active,
        orderBy: person => person.Name);

    return View(people);
}
```

### Example — Razor view

```cshtml
@using Canducci.Pagination.Interfaces
@model IPaginated<MyApplication.Models.People>

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

You can render pagination links using the companion MVC package:

```powershell
dotnet add package Canducci.Pagination.Mvc
```

```cshtml
@using Canducci.Pagination.Interfaces
@using Canducci.Pagination.Mvc
@model IPaginated<MyApplication.Models.People>

@Html.Pagination(
    Model,
    page => Url.Action("Index", new { page }),
    PaginatedStyle.NumbersWithFirstPreviousNextLast
)
```

The MVC package renders the navigation UI. The repository handles the paged MongoDB query.

## 10. Cancellation

Operations that perform database I/O accept an optional `CancellationToken`. Pass the request cancellation token from ASP.NET Core when appropriate:

```csharp
public async Task<IActionResult> Index(CancellationToken cancellationToken)
{
    IReadOnlyList<People> people = await _repository.FindAllAsync(
        Builders<People>.Sort.Ascending(person => person.Name),
        cancellationToken);

    return View(people);
}
```

## 11. Build and release

The GitHub Actions workflow restores dependencies, builds the library, runs the test project, and creates a NuGet package artifact.

Pull requests and pushes to `master` run build and test validation. These executions do not publish to NuGet.

Publishing is restricted to version tags matching `v*`. For example:

```bash
git tag v4.0.1
git push origin v4.0.1
```

Only create and push a version tag when you are ready to publish that release. The workflow uses NuGet Trusted Publishing (GitHub OIDC).

---

# Português

## 1. Guia de versões

### Versões 3.x.x — abordagem anterior

As versões **3.x.x e anteriores** utilizam o fluxo anterior de repositório, baseado na criação de classes concretas que implementam o repositório e no registro manual dessas classes na injeção de dependência.

Se sua aplicação utiliza a versão 3.x.x, mantenha a implementação existente enquanto planeja a migração. A versão 4 introduz um modelo diferente, orientado a interfaces, e altera alguns contratos públicos.

### Versões 4.x.x — abordagem orientada a interfaces

A versão **4.x.x** simplifica o consumo dos repositórios:

1. Crie o modelo da entidade.
2. Declare uma interface vazia herdando de `IRepository<TEntity>`.
3. Configure a biblioteca com `RepositoryRegistrationMode.Proxy`.
4. Injete sua interface onde precisar utilizá-la.

A biblioteca localiza as interfaces elegíveis e cria suas implementações dinamicamente. Você não precisa escrever uma classe de repositório para as operações padrão de CRUD e consulta.

| Versão | Estilo de repositório | Registro |
| --- | --- | --- |
| 3.x.x e anteriores | Implementação concreta do repositório | Registro manual da implementação |
| 4.x.x | Interface primeiro, com suporte a proxy dinâmico | Automático no modo Proxy ou implementação manual |

**Importante:** a API 4.x não é uma substituição automática para toda implementação 3.x. Ao atualizar, revise os contratos e os tipos de retorno.

## 2. Instalação

Instale o pacote pelo NuGet:

```powershell
dotnet add package Canducci.MongoDB.Repository
```

Ou utilize o Package Manager Console:

```powershell
Install-Package Canducci.MongoDB.Repository
```

## 3. Frameworks e driver MongoDB

| Target framework | MongoDB.Driver |
| --- | --- |
| `netstandard2.0` | 2.30.0 |
| `netstandard2.1` | 3.14.1 |
| `net10.0` | 3.14.1 |

## 4. Configurando a conexão

### Opção A — configuração no Program.cs

Esta é a configuração mais direta e permite utilizar os dois modos de registro.

```csharp
using Canducci.MongoDB.Repository;
using Canducci.MongoDB.Repository.DependencyInjection;

builder.Services.AddMongoDBRepository(config =>
{
    config.ConnectionString = "mongodb://localhost:27017";
    config.Database = "meu_banco";
    config.RegistrationMode = RepositoryRegistrationMode.Proxy;
});
```

- `ConnectionString`: string de conexão do MongoDB.
- `Database`: nome do banco de dados.
- `RegistrationMode.Proxy`: localiza e registra automaticamente as interfaces de repositório elegíveis.
- `RegistrationMode.Manual`: registra o cliente e a conexão do MongoDB, deixando o registro dos repositórios por conta da aplicação.

### Opção B — appsettings.json

```json
{
  "MongoDB": {
    "Database": "meu_banco",
    "ConnectionStrings": "mongodb://localhost:27017",
    "Proxy": 1
  }
}
```

Registre a configuração:

```csharp
using Canducci.MongoDB.Repository.DependencyInjection;

builder.Services.AddMongoDBRepository(builder.Configuration);
```

A configuração `MongoDB:Proxy` aceita:

| Valor | Modo | Comportamento |
| --- | --- | --- |
| `0` | Manual | As interfaces de repositório não são registradas automaticamente |
| `1` | Proxy | Interfaces vazias elegíveis que herdam de `IRepository<TEntity>` são registradas automaticamente |

### Opção C — modo manual com valores explícitos

```csharp
using Canducci.MongoDB.Repository.DependencyInjection;

builder.Services.AddMongoDBRepository(
    "meu_banco",
    "mongodb://localhost:27017");
```

Essa sobrecarga utiliza o modo `Manual`.

**Segurança:** não grave credenciais de produção diretamente no código-fonte. Utilize variáveis de ambiente, user secrets ou um gerenciador de segredos para fornecer a string de conexão.

Todas as sobrecargas registram um `IMongoClient` singleton e um `IConnect` singleton. Os proxies dos repositórios utilizam o ciclo de vida scoped.

## 5. Criando o modelo da entidade

Crie uma classe que represente um documento do MongoDB:

```csharp
using Canducci.MongoDB.Repository.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel.DataAnnotations;

namespace MinhaAplicacao.Models;

[BsonCollectionName("people")]
public class People
{
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    public string Id { get; set; }

    [BsonElement("name")]
    [BsonRequired]
    [Required]
    public string Name { get; set; }

    [BsonElement("active")]
    public bool Active { get; set; }
}
```

- `BsonCollectionName` define o nome da coleção no MongoDB. Se não for informado, será utilizado o nome da classe.
- `BsonId` mapeia a propriedade para o identificador do documento no MongoDB.
- `BsonElement` define o nome do campo armazenado.

## 6. Criando a interface do repositório — modo Proxy

Crie uma interface vazia que herde de `IRepository<TEntity>`:

```csharp
using Canducci.MongoDB.Repository;
using MinhaAplicacao.Models;

namespace MinhaAplicacao.Repositories;

public interface IRepositoryPeople : IRepository<People>
{
}
```

É só isso. Não é necessário criar uma classe concreta `RepositoryPeople` para utilizar as operações padrão do repositório.

Com o modo Proxy habilitado, a biblioteca:

1. Examina os assemblies que já estão carregados pela aplicação.
2. Localiza interfaces públicas, não genéricas e vazias que herdam de um contrato fechado `IRepository<TEntity>`.
3. Cria uma implementação dinâmica utilizando `DispatchProxy`.
4. Encaminha as chamadas para a implementação interna do repositório MongoDB.
5. Registra a interface com ciclo de vida scoped na injeção de dependência.

Injete a interface em um controller ou serviço:

```csharp
using Microsoft.AspNetCore.Mvc;
using MinhaAplicacao.Models;
using MinhaAplicacao.Repositories;

public class PeopleController : Controller
{
    private readonly IRepositoryPeople _repository;

    public PeopleController(IRepositoryPeople repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        IReadOnlyList<People> people = _repository.All();
        return View(people);
    }
}
```

**Requisitos do Proxy:** a interface deve ser pública, não genérica, vazia (sem métodos próprios) e herdar de um contrato fechado `IRepository<TEntity>`. Interfaces que declaram métodos adicionais não são registradas automaticamente por esse mecanismo.

## 7. Modo de repositório manual

Se precisar de uma classe de repositório personalizada ou preferir o registro explícito, continue utilizando o modelo tradicional de herança:

```csharp
using Canducci.MongoDB.Repository;
using MinhaAplicacao.Models;

public class PeopleRepository : Repository<People>
{
    public PeopleRepository(IConnect connect)
        : base(connect)
    {
    }
}
```

Registre a classe:

```csharp
builder.Services.AddScoped<PeopleRepository>();
```

Depois, injete `PeopleRepository` no seu serviço ou controller. Também é possível implementar uma interface própria e registrá-la explicitamente.

## 8. Métodos do repositório

Todos os métodos abaixo estão disponíveis por meio de `IRepository<TEntity>`. Salvo indicação contrária, os métodos assíncronos aceitam um `CancellationToken` opcional.

### 8.1 Add — inserir documentos

| Método | Descrição |
| --- | --- |
| `Add(T model)` | Insere um documento e retorna o modelo inserido. |
| `Add(IEnumerable<T> models)` | Insere vários documentos e retorna os modelos materializados. |
| `AddAsync(T model)` | Insere um documento de forma assíncrona. |
| `AddAsync(IEnumerable<T> models)` | Insere vários documentos de forma assíncrona. |

### 8.2 Find — localizar um documento

| Método | Descrição |
| --- | --- |
| `Find<TKey>(TKey key)` | Localiza um documento pelo identificador. |
| `Find(Expression<Func<T, bool>> filter)` | Localiza o primeiro documento que atende à expressão. |
| `FindAsync<TKey>(TKey key)` | Localiza um documento pelo identificador de forma assíncrona. |
| `FindAsync(Expression<Func<T, bool>> filter)` | Localiza o primeiro documento correspondente de forma assíncrona. |

### 8.3 All — consultar coleções utilizando expressões

| Método | Descrição |
| --- | --- |
| `All()` | Retorna todos os documentos. |
| `All(filter)` | Retorna documentos que atendem à expressão. |
| `All(filter, orderBy)` | Filtra e ordena os documentos de forma ascendente pela expressão informada. |
| `AllAsync()` | Retorna todos os documentos de forma assíncrona. |
| `AllAsync(filter)` | Retorna documentos que atendem à expressão de forma assíncrona. |
| `AllAsync(filter, orderBy)` | Filtra e ordena os documentos de forma assíncrona e ascendente. |

### 8.4 FindBy e FindAll — utilizando os builders oficiais do MongoDB

Esses métodos recebem os próprios tipos `FilterDefinition<T>` e `SortDefinition<T>` do MongoDB. Utilize-os quando precisar de todos os recursos do driver, incluindo filtros compostos e ordenações ascendentes ou descendentes.

**FindBy** — filtro e ordenação:

```csharp
using MongoDB.Driver;

FilterDefinition<People> filter =
    Builders<People>.Filter.Eq(person => person.Active, true);

SortDefinition<People> sort =
    Builders<People>.Sort.Descending(person => person.Name);

IReadOnlyList<People> people = repository.FindBy(filter, sort);
IReadOnlyList<People> asyncPeople = await repository.FindByAsync(filter, sort);
```

| Método | Descrição |
| --- | --- |
| `FindBy(filter, sort)` | Retorna documentos correspondentes ao filtro e à ordenação informados. |
| `FindByAsync(filter, sort)` | Versão assíncrona de `FindBy`. |

**FindAll** — ordenar todos os documentos:

```csharp
SortDefinition<People> sort =
    Builders<People>.Sort.Ascending(person => person.Name);

IReadOnlyList<People> people = repository.FindAll(sort);
IReadOnlyList<People> asyncPeople = await repository.FindAllAsync(sort);
```

| Método | Descrição |
| --- | --- |
| `FindAll(sort)` | Retorna todos os documentos utilizando a ordenação informada. |
| `FindAllAsync(sort)` | Versão assíncrona de `FindAll`. |

É possível combinar filtros utilizando o builder do MongoDB:

```csharp
FilterDefinition<People> filter = Builders<People>.Filter.And(
    Builders<People>.Filter.Eq(person => person.Active, true),
    Builders<People>.Filter.Regex(person => person.Name, new MongoDB.Bson.BsonRegularExpression("^A", "i"))
);

SortDefinition<People> sort = Builders<People>.Sort
    .Ascending(person => person.Name)
    .Descending(person => person.Id);

IReadOnlyList<People> result = await repository.FindByAsync(filter, sort);
```

### 8.5 List — ordenar e filtrar opcionalmente

| Método | Descrição |
| --- | --- |
| `List(orderBy, filter = null)` | Retorna documentos ordenados de forma ascendente, aplicando opcionalmente um filtro por expressão. |
| `ListAsync(orderBy, filter = null)` | Versão assíncrona de `List`. |

### 8.6 Edit — substituir um documento

| Método | Descrição |
| --- | --- |
| `Edit(key, model)` | Substitui um documento identificado pela chave. |
| `Edit(filter, model)` | Substitui o primeiro documento que corresponde à expressão. |
| `EditAsync(key, model)` | Substitui um documento pela chave de forma assíncrona. |
| `EditAsync(filter, model)` | Substitui o primeiro documento correspondente de forma assíncrona. |

Esses métodos retornam `bool`: `true` significa que um documento correspondeu ao filtro de substituição, mesmo quando o conteúdo substituído é idêntico.

### 8.7 Update — atualizar campos dos documentos

| Método | Descrição |
| --- | --- |
| `Update(filter, update)` | Atualiza um documento correspondente. |
| `UpdateAll(filter, update)` | Atualiza todos os documentos correspondentes. |
| `UpdateAsync(filter, update)` | Atualiza um documento correspondente de forma assíncrona. |
| `UpdateAllAsync(filter, update)` | Atualiza todos os documentos correspondentes de forma assíncrona. |

Os métodos de atualização retornam `UpdateResult` do MongoDB, e não um booleano. Consulte `MatchedCount`, `ModifiedCount`, `IsAcknowledged` e `UpsertedId` conforme necessário.

```csharp
UpdateDefinition<People> update =
    Builders<People>.Update.Set(person => person.Active, false);

UpdateResult result = repository.Update(
    person => person.Name == "João",
    update);

long matched = result.MatchedCount;
long modified = result.ModifiedCount;
```

### 8.8 Delete — excluir documentos

| Método | Descrição |
| --- | --- |
| `Delete(filter)` | Exclui um documento que corresponde à expressão. |
| `Delete(key)` | Exclui um documento pelo identificador. |
| `DeleteAsync(filter)` | Exclui um documento correspondente de forma assíncrona. |
| `DeleteAsync(key)` | Exclui um documento pelo identificador de forma assíncrona. |

Retorna `bool`, indicando se um documento foi excluído.

### 8.9 Count — contar documentos

| Método | Descrição |
| --- | --- |
| `Count()` | Conta todos os documentos. |
| `Count(filter, options = null)` | Conta os documentos que atendem à expressão. |
| `CountAsync()` | Conta todos os documentos de forma assíncrona. |
| `CountAsync(filter, options = null)` | Conta os documentos correspondentes de forma assíncrona. |

### 8.10 Query — acessar IQueryable

`Query()` disponibiliza a coleção como `IQueryable<T>`, permitindo compor consultas LINQ compatíveis com o provedor LINQ do MongoDB.

```csharp
IReadOnlyList<People> activePeople = repository.Query()
    .Where(person => person.Active)
    .OrderBy(person => person.Name)
    .ToList();
```

### 8.11 Collection — acessar IMongoCollection

A propriedade `Collection` disponibiliza a `IMongoCollection<T>` subjacente para cenários que exigem acesso direto aos recursos do driver MongoDB.

### 8.12 CreateObjectId — criar um ObjectId

`CreateObjectId(string value)` converte uma representação textual em um `ObjectId` do MongoDB.

## 9. Paginação

A paginação retorna `Canducci.Pagination.Interfaces.IPaginated<T>`. É possível solicitar uma página sem ordenação explícita, com ordenação ou com filtro e ordenação por expressão.

### Métodos de paginação disponíveis

| Método | Descrição |
| --- | --- |
| `PagedList(pageNumber, pageSize)` | Obtém uma página sem ordenação explícita. |
| `PagedListAsync(pageNumber, pageSize)` | Versão assíncrona. |
| `PagedList(pageNumber, pageSize, orderBy)` | Obtém uma página ordenada. |
| `PagedListAsync(pageNumber, pageSize, orderBy)` | Página ordenada de forma assíncrona. |
| `PagedList(pageNumber, pageSize, filter, orderBy)` | Obtém uma página filtrada e ordenada. |
| `PagedListAsync(pageNumber, pageSize, filter, orderBy)` | Página filtrada e ordenada de forma assíncrona. |

### Exemplo — controller ASP.NET Core

```csharp
using Canducci.Pagination.Interfaces;
using Microsoft.AspNetCore.Mvc;

public async Task<IActionResult> Index(int? page)
{
    const int pageSize = 10;

    IPaginated<People> people = await _repository.PagedListAsync(
        pageNumber: page ?? 1,
        pageSize: pageSize,
        filter: person => person.Active,
        orderBy: person => person.Name);

    return View(people);
}
```

### Exemplo — view Razor

```cshtml
@using Canducci.Pagination.Interfaces
@model IPaginated<MinhaAplicacao.Models.People>

<table class="table">
    <thead>
        <tr>
            <th>Id</th>
            <th>Nome</th>
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

### Integração com Canducci.Pagination.Mvc

Você pode renderizar os links de navegação utilizando o pacote complementar para MVC:

```powershell
dotnet add package Canducci.Pagination.Mvc
```

```cshtml
@using Canducci.Pagination.Interfaces
@using Canducci.Pagination.Mvc
@model IPaginated<MinhaAplicacao.Models.People>

@Html.Pagination(
    Model,
    page => Url.Action("Index", new { page }),
    PaginatedStyle.NumbersWithFirstPreviousNextLast
)
```

O pacote MVC renderiza a interface de navegação. O repositório é responsável por executar a consulta paginada no MongoDB.

## 10. Cancelamento de operações

As operações que acessam o banco aceitam um `CancellationToken` opcional. Quando apropriado, encaminhe o token de cancelamento da requisição ASP.NET Core:

```csharp
public async Task<IActionResult> Index(CancellationToken cancellationToken)
{
    IReadOnlyList<People> people = await _repository.FindAllAsync(
        Builders<People>.Sort.Ascending(person => person.Name),
        cancellationToken);

    return View(people);
}
```

## 11. Build e publicação

O workflow do GitHub Actions restaura as dependências, compila a biblioteca, executa o projeto de testes e gera um artefato do pacote NuGet.

Pull requests e pushes para `master` executam a validação de build e testes. Essas execuções não publicam o pacote no NuGet.

A publicação é restrita a tags de versão que correspondam a `v*`. Por exemplo:

```bash
git tag v4.0.1
git push origin v4.0.1
```

Crie e envie uma tag de versão somente quando estiver pronto para publicar aquela versão. O workflow utiliza NuGet Trusted Publishing (GitHub OIDC).
