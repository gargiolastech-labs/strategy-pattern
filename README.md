# StrategyPattern — Documentazione Tecnica

## Indice

1. [Panoramica del progetto](#1-panoramica-del-progetto)
2. [Architettura generale](#2-architettura-generale)
3. [Il Design Pattern Strategy](#3-il-design-pattern-strategy)
4. [Struttura del progetto](#4-struttura-del-progetto)
5. [Componenti principali](#5-componenti-principali)
   - [Domain Layer](#51-domain-layer)
   - [Application Layer — Astrazioni CQRS](#52-application-layer--astrazioni-cqrs)
   - [Application Layer — Astrazioni Strategy](#53-application-layer--astrazioni-strategy)
   - [Application Layer — Feature: AddUser](#54-application-layer--feature-adduser)
   - [Infrastructure Layer](#55-infrastructure-layer)
   - [API Entry Point](#56-api-entry-point)
6. [Flusso di esecuzione](#6-flusso-di-esecuzione)
7. [Diagramma delle classi](#7-diagramma-delle-classi)
8. [Vantaggi dello Strategy Pattern](#8-vantaggi-dello-strategy-pattern)
9. [Dipendenze NuGet utilizzate](#9-dipendenze-nuget-utilizzate)

---

## 1. Panoramica del progetto

**StrategyPattern** è un'API REST sviluppata in **ASP.NET Core (.NET 9)** che funge da esempio applicativo del **Design Pattern Strategy** integrato con:

- **Clean Architecture** (separazione in layer Domain / Application / Infrastructure / API)
- **CQRS** (Command and Query Responsibility Segregation) tramite **MediatR**
- **Railway-oriented programming** tramite la libreria **CSharpFunctionalExtensions** (gestione esplicita di `Result<T>` senza eccezioni di dominio)

Il caso d'uso implementato è la creazione di un utente (`AddUser`) il cui comportamento varia in base al **profilo dell'utente corrente** (Admin, Guest, Custom). La strategia corretta viene selezionata a runtime senza alcun blocco `if/switch` nel codice applicativo.

---

## 2. Architettura generale

Il progetto segue i principi della **Clean Architecture**:

```
┌──────────────────────────────────────────┐
│               API (Program.cs)           │  ← Entry point HTTP, minimal API
├──────────────────────────────────────────┤
│           Infrastructure Layer           │  ← Implementazioni concrete
│  (StrategySelector, UserProvider, DI)    │
├──────────────────────────────────────────┤
│           Application Layer              │  ← Logica applicativa
│  (CQRS abstractions, Strategy abstractions,
│   Command Handlers, Contexts, Strategies)│
├──────────────────────────────────────────┤
│             Domain Layer                 │  ← Entità e costanti di dominio
│       (User, Entity, Profile enum)       │
└──────────────────────────────────────────┘
```

Le dipendenze puntano sempre verso l'interno: l'Infrastructure dipende dall'Application, che dipende dal Domain. Il Domain non dipende da nessun altro layer.

---

## 3. Il Design Pattern Strategy

### Definizione

Lo **Strategy Pattern** è un pattern comportamentale (GoF) che permette di definire una famiglia di algoritmi, incapsulare ciascuno di essi in una classe separata e renderli intercambiabili. Il client (chiamante) è disaccoppiato dalla logica dell'algoritmo specifico.

### Struttura classica

| Ruolo | Responsabilità |
|-------|---------------|
| **Strategy** (interfaccia) | Definisce il contratto che ogni algoritmo deve rispettare |
| **ConcreteStrategy** | Implementa un algoritmo specifico |
| **Context** | Contiene il riferimento alla strategia attiva e la invoca |
| **Selector/Factory** | Sceglie quale strategia attivare a runtime |

### Come è applicato in questo progetto

| Ruolo | Classe/Interfaccia |
|-------|--------------------|
| Strategy | `IHandlerStrategy<TContext, TResponse>` |
| ConcreteStrategy | `AddUserAdminStrategy`, `AddUserGuestStrategy`, `AddUserCustomStrategy` |
| Context | `AddUserContext` (porta i dati necessari alla strategia) |
| Selector | `StrategySelector<TContext, TResponse>` (infrastruttura) |
| Client | `AddUserCommandHandler` (utilizza il selector senza conoscere la strategia concreta) |

---

## 4. Struttura del progetto

```
src/
└── StrategyPattern.Api/
    ├── Program.cs                          ← Entry point, minimal API endpoint
    ├── Application/
    │   └── Abstractions/
    │       ├── Messages/
    │       │   ├── IBaseCQRS.cs            ← Marker interface comune a Command e Query
    │       │   ├── ICommand.cs             ← Contratto CQRS per i comandi
    │       │   ├── ICommandHandler.cs      ← Contratto per gli handler dei comandi
    │       │   ├── IQuery.cs               ← Contratto CQRS per le query
    │       │   └── IQueryHandler.cs        ← Contratto per gli handler delle query
    │       ├── Providers/
    │       │   └── IUserProvider.cs        ← Astrazione per il profilo utente corrente
    │       └── Strategies/
    │           ├── IContext.cs             ← Contratto base per il contesto di strategia
    │           ├── IHandlerStrategy.cs     ← Contratto della strategia (CanHandle + Execute)
    │           └── IStrategySelector.cs    ← Contratto del selettore di strategia
    │   └── Features/
    │       └── Users/
    │           └── Commands/
    │               └── AddUser/
    │                   ├── AddUserCommand.cs          ← Record CQRS del comando
    │                   ├── AddUserCommandHandler.cs   ← Handler MediatR
    │                   ├── AddUserContext.cs          ← Contesto della strategia
    │                   ├── AddUserRequest.cs          ← DTO HTTP in ingresso
    │                   └── Strategies/
    │                       ├── AddUserAdminStrategy.cs
    │                       ├── AddUserGuestStrategy.cs
    │                       └── AddUserCustomStrategy.cs
    ├── Domain/
    │   ├── Constants/
    │   │   └── Profile.cs                 ← Enum: Admin, Guest, Custom
    │   └── Users/
    │       ├── Entity.cs                  ← Classe base con Id (Guid)
    │       └── User.cs                    ← Entità di dominio User
    └── Infrastructure/
        ├── DependencyInjectionExtensions.cs ← Registrazione DI
        ├── Providers/
        │   └── UserProvider.cs            ← Implementazione: profilo casuale
        └── Strategies/
            └── StrategySelector.cs        ← Implementazione del selettore
```

---

## 5. Componenti principali

### 5.1 Domain Layer

#### `Profile` (enum)

```csharp
public enum Profile
{
    Admin = 1,
    Guest,
    Custom
}
```

Costante di dominio che rappresenta il tipo di profilo utente. Guida la selezione della strategia.

#### `Entity` (classe astratta)

Classe base per le entità di dominio. Espone un `Guid Id` readonly assegnato alla creazione.

#### `User` (entità)

```csharp
public sealed class User : Entity
{
    public string Name { get; private set; }
    public string LastName { get; private set; }
    public Profile ProfileTypeId { get; private set; }

    internal static User Create(string name, string lastName) { ... }
    public void SetProfileTypeId(Profile profile) { ... }
}
```

Il costruttore è privato (factory method `Create`). Il profilo viene impostato dalla strategia dopo la creazione. `Guid.CreateVersion7` garantisce ID ordinabili per data di creazione.

---

### 5.2 Application Layer — Astrazioni CQRS

Le interfacce CQRS sono wrapper tipizzati su **MediatR** che integrano `Result<T>` di CSharpFunctionalExtensions.

| Interfaccia | Firma | Scopo |
|-------------|-------|-------|
| `IBaseCQRS` | marker | Tipo base comune a Command e Query |
| `ICommand<TResponse>` | `: IRequest<Result<TResponse>>, IBaseCQRS` | Contratto dei comandi |
| `ICommandHandler<TRequest, TResponse>` | `: IRequestHandler<TRequest, Result<TResponse>>` | Contratto degli handler |
| `IQuery<TQuery>` | `: IRequest<Result<TQuery>>, IBaseCQRS` | Contratto delle query |
| `IQueryHandler<TQuery, TResponse>` | `: IRequestHandler<TQuery, Result<TResponse>>` | Contratto degli handler di query |

---

### 5.3 Application Layer — Astrazioni Strategy

#### `IContext`

```csharp
public interface IContext
{
    Profile Profile { get; }
}
```

Contratto minimo per qualsiasi contesto di strategia. Ogni contesto deve esporre il `Profile` che permette al selettore di valutare la compatibilità.

#### `IHandlerStrategy<TContext, TResponse>`

```csharp
public interface IHandlerStrategy<in TContext, TResponse>
    where TContext : IContext
{
    Task<bool> CanHandleAsync(TContext context, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}
```

Il cuore del pattern. Ogni strategia:
- **`CanHandleAsync`**: dichiara se è in grado di gestire il contesto dato (tipicamente: confronta `context.Profile` con il profilo di competenza).
- **`ExecuteAsync`**: esegue la logica specifica e restituisce un `Result<TResponse>`.

L'interfaccia è **generica e riusabile**: può essere implementata per qualsiasi comando/query, non solo per `AddUser`.

#### `IStrategySelector<TContext, TResponse>`

```csharp
public interface IStrategySelector<TContext, TResponse>
    where TContext : IContext
{
    Task<IHandlerStrategy<TContext, TResponse>> SelectAsync(
        TContext context,
        CancellationToken cancellationToken = default);
}
```

Astrazione del meccanismo di selezione. L'Application Layer dipende solo da questa interfaccia, non dall'implementazione concreta.

---

### 5.4 Application Layer — Feature: AddUser

#### `AddUserCommand`

```csharp
public sealed record AddUserCommand(string Name, string LastName) : ICommand<Guid?>;
```

Record immutabile. Trasporta i dati del comando verso l'handler tramite MediatR.

#### `AddUserContext`

```csharp
public sealed class AddUserContext : IContext
{
    public Profile Profile { get; private set; }
    public AddUserCommand Command { get; private set; }

    internal static AddUserContext CreateInstance(Profile profile, AddUserCommand command) { ... }
}
```

Oggetto contesto creato dall'handler prima di invocare il selettore. Aggrega il profilo utente e il comando. Il costruttore è privato (factory method `CreateInstance`) per garantire la validità dello stato.

#### `AddUserCommandHandler`

```csharp
public sealed class AddUserCommandHandler : ICommandHandler<AddUserCommand, Guid?>
{
    private readonly IUserProvider _userProvider;
    private readonly IStrategySelector<AddUserContext, User> _strategySelector;

    public async Task<Result<Guid?>> Handle(AddUserCommand request, CancellationToken cancellationToken)
    {
        var profile = _userProvider.GetActiveProfile();
        var context = AddUserContext.CreateInstance(profile, request);

        var activeStrategy = await _strategySelector.SelectAsync(context, cancellationToken);
        var result = await activeStrategy.ExecuteAsync(context, cancellationToken);

        if (result.IsSuccess)
            return Result.Success<Guid?>(result.Value.Id);

        return result.ConvertFailure<Guid?>();
    }
}
```

L'handler **non contiene nessuna logica condizionale** (nessun `if`, nessun `switch`). Si limita a:
1. Recuperare il profilo corrente tramite `IUserProvider`
2. Costruire il contesto
3. Delegare la selezione della strategia a `IStrategySelector`
4. Eseguire la strategia selezionata
5. Mappare il risultato

#### Strategie concrete

Tutte e tre le strategie (`AddUserAdminStrategy`, `AddUserGuestStrategy`, `AddUserCustomStrategy`) seguono lo stesso schema:

```csharp
public class AddUserAdminStrategy : IHandlerStrategy<AddUserContext, User>
{
    private const Profile Profile = Domain.Constants.Profile.Admin;

    public async Task<bool> CanHandleAsync(AddUserContext context, CancellationToken cancellationToken = default)
        => context.Profile == Profile;

    public async Task<Result<User>> ExecuteAsync(AddUserContext context, CancellationToken cancellationToken = default)
    {
        var user = User.Create(context.Command.Name, context.Command.LastName);
        user.SetProfileTypeId(Profile);
        return Result.Success(user);
    }
}
```

Ogni strategia è responsabile esclusivamente della logica associata al proprio profilo. Nella configurazione attuale le tre strategie hanno comportamento identico (creazione dell'utente con il profilo corrispondente), ma ogni classe è il punto di estensione naturale per logiche differenziate (es. validazioni aggiuntive, invio notifiche, integrazione con sistemi esterni specifici per profilo).

---

### 5.5 Infrastructure Layer

#### `UserProvider`

```csharp
internal sealed class UserProvider : IUserProvider
{
    public Profile GetActiveProfile()
    {
        var number = RandomNumberGenerator.GetInt32(1, 3);
        return Enum.Parse<Profile>(number.ToString());
    }
}
```

Implementazione di esempio che simula il profilo utente corrente generando un valore casuale crittograficamente sicuro tra Admin (1) e Guest (2). In un'applicazione reale questa classe leggerebbe il profilo dal token JWT, dalla sessione o da un database.

#### `StrategySelector<TContext, TResponse>`

```csharp
internal sealed class StrategySelector<TContext, TResponse> : IStrategySelector<TContext, TResponse>
    where TContext : IContext
{
    private readonly IEnumerable<IHandlerStrategy<TContext, TResponse>> _strategies;

    public async Task<IHandlerStrategy<TContext, TResponse>> SelectAsync(
        TContext context, CancellationToken cancellationToken = default)
    {
        var activeStrategies = new List<IHandlerStrategy<TContext, TResponse>>();

        foreach (var handlerStrategy in _strategies)
        {
            if (await handlerStrategy.CanHandleAsync(context, cancellationToken))
                activeStrategies.Add(handlerStrategy);
        }

        return activeStrategies.Count is 0 or > 1
            ? throw new InvalidOperationException("Deve essere presente una strategia attiva.")
            : activeStrategies.First();
    }
}
```

Implementazione generica del selettore. Riceve via Dependency Injection **tutte** le strategie registrate per la coppia `<TContext, TResponse>`, le interroga con `CanHandleAsync` e restituisce l'unica abilitata. Lancia `InvalidOperationException` se zero o più di una strategia risultano attive, garantendo la correttezza del contratto.

#### `DependencyInjectionExtensions`

```csharp
internal static void InitializeInfrastructre(this IServiceCollection services, IConfiguration configuration)
{
    InitializeMediatr(services, ...);
    InitializeProviders(services);
    InitializeAddUserCommandStrategy(services);
    services.AddScoped(typeof(IStrategySelector<,>), typeof(StrategySelector<,>));
}

private static void InitializeAddUserCommandStrategy(IServiceCollection services)
{
    services.AddScoped<IHandlerStrategy<AddUserContext, User>, AddUserAdminStrategy>();
    services.AddScoped<IHandlerStrategy<AddUserContext, User>, AddUserCustomStrategy>();
    services.AddScoped<IHandlerStrategy<AddUserContext, User>, AddUserGuestStrategy>();
}
```

Le tre strategie sono registrate tutte con la stessa interfaccia (`IHandlerStrategy<AddUserContext, User>`). Il container .NET le inietta come `IEnumerable<IHandlerStrategy<AddUserContext, User>>` nel costruttore di `StrategySelector`. Il selettore generico `IStrategySelector<,>` è registrato con tipo aperto per supportare automaticamente qualsiasi coppia contesto/risposta futura.

---

### 5.6 API Entry Point

```csharp
app.MapPost("/AddUser", AddUser).WithName("AddUser");

static async Task<IResult> AddUser(
    [FromServices] ISender sender,
    [FromBody] AddUserRequest request)
{
    var command = new AddUserCommand(request.FirstName, request.LastName);
    var result = await sender.Send(command);
    if (result.IsSuccess)
        return Results.Ok(result.Value);
    return Results.BadRequest(result.Error);
}
```

Endpoint minimal API che:
1. Deserializza il body HTTP in `AddUserRequest`
2. Crea un `AddUserCommand` e lo invia tramite MediatR (`ISender`)
3. Restituisce `200 OK` con il `Guid` dell'utente creato, oppure `400 Bad Request` con il messaggio di errore

---

## 6. Flusso di esecuzione

```
HTTP POST /AddUser
      │
      ▼
  Program.cs (Minimal API endpoint)
      │  crea AddUserCommand e invia via MediatR
      ▼
  AddUserCommandHandler.Handle()
      │  1. IUserProvider.GetActiveProfile()  →  Profile (es. Admin)
      │  2. AddUserContext.CreateInstance(profile, command)
      │  3. IStrategySelector.SelectAsync(context)
      ▼
  StrategySelector.SelectAsync()
      │  Itera su [AdminStrategy, CustomStrategy, GuestStrategy]
      │  Chiama CanHandleAsync() su ognuna
      │  → solo AddUserAdminStrategy risponde true
      │  Restituisce AddUserAdminStrategy
      ▼
  AddUserAdminStrategy.ExecuteAsync(context)
      │  User.Create(name, lastName)
      │  user.SetProfileTypeId(Profile.Admin)
      │  return Result.Success(user)
      ▼
  AddUserCommandHandler
      │  result.IsSuccess → restituisce Result.Success<Guid?>(user.Id)
      ▼
  Program.cs
      │  result.IsSuccess → Results.Ok(userId)
      ▼
  HTTP 200 OK  { "guid": "..." }
```

---

## 7. Diagramma delle classi

```
┌─────────────────────────────────────────────────────────────────┐
│                        DOMAIN                                   │
│                                                                 │
│  ┌──────────┐      ┌──────────────────────────┐                 │
│  │  Entity  │◄─────│          User            │                 │
│  │ +Id:Guid │      │ +Name: string            │                 │
│  └──────────┘      │ +LastName: string        │                 │
│                    │ +ProfileTypeId: Profile   │                 │
│                    │ +Create() [static]        │                 │
│                    │ +SetProfileTypeId()       │                 │
│                    └──────────────────────────┘                 │
│                                                                 │
│       ┌──────────────────────┐                                  │
│       │  Profile (enum)      │                                  │
│       │  Admin=1, Guest, Custom                                 │
│       └──────────────────────┘                                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                      APPLICATION                                │
│                                                                 │
│  «interface»              «interface»                           │
│  IContext ◄───────────── AddUserContext                         │
│  +Profile: Profile        +Profile: Profile                     │
│                           +Command: AddUserCommand              │
│                                                                 │
│  «interface»                                                    │
│  IHandlerStrategy<TContext, TResponse>                          │
│  +CanHandleAsync()                                              │
│  +ExecuteAsync()                                                │
│       ▲                                                         │
│       │ implements                                              │
│  ┌────┴─────────────────────────────┐                           │
│  │  AddUserAdminStrategy            │                           │
│  │  AddUserGuestStrategy            │                           │
│  │  AddUserCustomStrategy           │                           │
│  └──────────────────────────────────┘                           │
│                                                                 │
│  «interface»                                                    │
│  IStrategySelector<TContext, TResponse>                         │
│  +SelectAsync()                                                 │
│                                                                 │
│  AddUserCommandHandler                                          │
│  - IUserProvider                                                │
│  - IStrategySelector<AddUserContext, User>                      │
│  +Handle()                                                      │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    INFRASTRUCTURE                               │
│                                                                 │
│  StrategySelector<TContext, TResponse>                          │
│  implements IStrategySelector<TContext, TResponse>              │
│  - IEnumerable<IHandlerStrategy<TContext, TResponse>>           │
│  +SelectAsync()                                                 │
│                                                                 │
│  UserProvider                                                   │
│  implements IUserProvider                                       │
│  +GetActiveProfile() → Profile (random)                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 8. Vantaggi dello Strategy Pattern

### 8.1 Principio Open/Closed (OCP)

Il sistema è **aperto all'estensione ma chiuso alla modifica**. Per aggiungere il supporto a un nuovo profilo (es. `Moderator`) è sufficiente:

1. Aggiungere il valore all'enum `Profile`
2. Creare una nuova classe `AddUserModeratorStrategy : IHandlerStrategy<AddUserContext, User>`
3. Registrarla nel container DI

**Nessun file esistente viene modificato.** L'handler, il selettore e le altre strategie rimangono invariati.

### 8.2 Eliminazione dei blocchi condizionali

Senza Strategy Pattern, l'handler avrebbe contenuto codice come:

```csharp
// ❌ Approccio senza Strategy Pattern
if (profile == Profile.Admin)
{
    // logica Admin
}
else if (profile == Profile.Guest)
{
    // logica Guest
}
else if (profile == Profile.Custom)
{
    // logica Custom
}
```

Questo approccio viola OCP, cresce indefinitamente con i nuovi casi, e rende il testing molto più complesso. Con il pattern Strategy, l'handler è **privo di condizionali** e delega la selezione al selettore.

### 8.3 Singola Responsabilità (SRP)

Ogni strategia ha **una sola ragione per cambiare**: la logica specifica del proprio profilo. L'handler ha una sola ragione per cambiare: l'orchestrazione del flusso CQRS. Il selettore ha una sola ragione per cambiare: il meccanismo di selezione.

### 8.4 Testabilità

Ogni componente può essere testato in isolamento:

| Componente | Come testarlo |
|------------|--------------|
| `AddUserAdminStrategy` | Passare un contesto con `Profile.Admin` → verifica che `CanHandleAsync` sia `true` e che `ExecuteAsync` restituisca un utente con profilo Admin |
| `StrategySelector` | Mockare le strategie e verificare che venga restituita quella corretta; testare i casi di errore (0 o più di 1 strategia attiva) |
| `AddUserCommandHandler` | Mockare `IUserProvider` e `IStrategySelector`; verificare che il flusso venga orchestrato correttamente indipendentemente dalla strategia |

### 8.5 Genericità e riusabilità

Le astrazioni `IContext`, `IHandlerStrategy<TContext, TResponse>`, `IStrategySelector<TContext, TResponse>` e `StrategySelector<TContext, TResponse>` sono **completamente generiche**. Il pattern può essere applicato a qualsiasi altra feature dell'applicazione (es. `UpdateUserCommand`, `ProcessOrderCommand`) senza duplicare infrastruttura:

```csharp
// Per una nuova feature, basta:
// 1. Creare un nuovo contesto
public class UpdateUserContext : IContext { ... }

// 2. Creare le strategie
public class UpdateUserAdminStrategy : IHandlerStrategy<UpdateUserContext, User> { ... }

// 3. Registrarle nel DI
services.AddScoped<IHandlerStrategy<UpdateUserContext, User>, UpdateUserAdminStrategy>();
// IStrategySelector<UpdateUserContext, User> viene risolto automaticamente
// grazie alla registrazione con tipo aperto: typeof(IStrategySelector<,>)
```

### 8.6 Integrazione con la Dependency Injection

Il meccanismo di registrazione multipla di .NET (`IEnumerable<IHandlerStrategy<...>>`) permette al container di **iniettare automaticamente tutte le strategie registrate** nel `StrategySelector`. Non è necessario un factory esplicito o un dizionario manuale di mappatura.

### 8.7 Sicurezza a runtime tramite guard clause

Il `StrategySelector` lancia un'eccezione esplicita se:
- **Nessuna strategia** è in grado di gestire il contesto (mancanza di registrazione o bug nel `CanHandleAsync`)
- **Più di una strategia** risulta attiva contemporaneamente (condizione di ambiguità)

Questo fail-fast garantisce che errori di configurazione emergano immediatamente, evitando comportamenti silenziosi e imprevedibili.

---

## 9. Dipendenze NuGet utilizzate

| Pacchetto | Versione | Scopo |
|-----------|----------|-------|
| `MediatR` | — | Implementazione del pattern CQRS (mediator) |
| `CSharpFunctionalExtensions` | — | Tipo `Result<T>` per Railway-oriented programming |
| `Microsoft.AspNetCore.OpenApi` | — | Generazione OpenAPI/Swagger |
| `Scalar.AspNetCore` | — | UI interattiva per le API (alternativa a Swagger UI) |

---

*Documentazione generata per il progetto **StrategyPattern** — ASP.NET Core (.NET 9)*
