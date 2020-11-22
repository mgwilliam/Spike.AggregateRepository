# Aggregate Repository POC

A POC to demonstrate an entity repository that saves entity deltas (instead of complete entities)

The purpose of the repository is:

* to enable changes to entities to be "patched" *(rather than save the whole entity)*
* to provide a history of changes to an entity *(who changed what and when)*
* to enable getting a previous version of an entity

## Approach

The POC uses a **repository** and **backing store**.

The repository implements a typical CRUD style interface, plus the ability to retrieve all historic changes to a specific entity or retrieve a specific historic version.

Each update to an entity is saved as a discrete change. Each saved change will only contain the fields that were passed to the repository when the update was invoked (ie. a change can be a complete or partial entity).

When an entity is retrieved from the repository, all changes are played back to rebuild the entity upto its most recent state. If a specific version is requested then playback stops after that version is reached.

## Integration

It is envisaged that the repository could be used in place of an existing entity repository class for dealing with entity instances (ie. CRUD in the singular).

To support querying across *multiple* entities (eg. "get many") then projections should be used rather than the repository itself.

***To be clear**: the aggregate repository should only be used for working with a single instance of an entity.*

## Base classes & interfaces

### `IRepository<T>`

Defines a generic repository that supports Create, Retrieve and Update operations.

### `IRepositoryStore<T>`

Defines a generic backing store that Repository instances can use to store each change.

### `BaseEntity`

Base type for entities (exists to support generic code).

### `Change`

Encapsulates the **who** and **when** for a particular change.

Each change is allocated a unique ID which can be subsequently used to retrieve a specific version of the entity.

### `BaseRepository<T>`

Base class containing the code for interacting with the repository store and rehydrating entities from historic saved changes.

There are a couple of pure virtual methods to be implemented:

#### `BaseRepository.InitialiseInstance(Vacancy instance)`

This method is called when a new instance is created and needs to be setup with initial values.

#### `BaseRepository.ApplyChange(Vacancy instance, Vacancy change)`

This method is used to apply a single `change` to an entity `instance`.

Typically this should map any non-null values in the change to the corresponding instance fields, for example.

```csharp
  instance.Title = change.Title ?? instance.Title;
  instance.Description = change.Description ?? instance.Description;
  instance.Status = change.Status;
```

Note that some fields should always be mapped (eg. `Status`) and some never mapped (eg. `Id`)

## Test classes

There are some test classes to test and demonstrate the intent of the repository.

Automated tests cover the majority of the code.

### Entity and repository

A `Vacancy` entity representing a very simple advert for a job.

A repository class `VacancyRepository : BaseRepository<Vacancy>, IRepository`.

A repository backing store `InMemoryRepositoryStore : IRepositoryStore`.

### Application logic

There's a `VacancyService` class which mimics the typical application functionality would interact with the repository (this type of code would typically reside in a series of command handlers).

The service also uses a simple `VacancyValidator` class to show how **validation** can be done in conjunction with the repository. The approach used to validate a given entity update (which is assumed to be a partial entity) is to:

* retrieve the latest *full* version of the entity
* add the changes and then validate the entity as a whole
* if successful, the *partial* entity is added to the repository as a change
* note that the "full" entity is discarded as it was only used for the validation

## Not done

The POC code does not include a **delete** operation - this could be added if a hard delete feature is needed.

The POC doesn't include creating **snapshots** of entities.

> The need for snapshots is a *moot point*... the number of anticipated changes to an entity isn't expected to reach problematic levels so creating snapshots from the outset would be an unjustified optimisation.

## Next steps (TODO)

Implement a persistent backing store, eg. `CosmosDbRepositoryStore`
