# Contributing to .NET Enterprise API

Thank you for your interest in contributing! This guide will help you get started.

## How to Contribute

### Reporting Bugs

1. Check [existing issues](https://github.com/iamchittaranjandas/dotnet-enterprise-api/issues) to avoid duplicates
2. Open a new issue using the **Bug Report** template
3. Include steps to reproduce, expected behavior, and actual behavior
4. Mention your environment (.NET version, OS, DataProvider setting)

### Suggesting Features

1. Open a new issue using the **Feature Request** template
2. Describe the problem your feature would solve
3. Propose your solution and any alternatives you've considered

### Submitting Code

1. **Fork** the repository
2. **Create a branch** from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes** following the guidelines below
4. **Test** your changes with all three data providers (EntityFramework, Dapper, Ado)
5. **Commit** with a clear message:
   ```bash
   git commit -m "Add: brief description of what you added"
   ```
6. **Push** and open a **Pull Request** against `main`

## Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server LocalDB
- Git

### Getting Started

```bash
git clone https://github.com/iamchittaranjandas/dotnet-enterprise-api.git
cd dotnet-enterprise-api
dotnet restore
dotnet ef database update --project DotnetEnterpriseApi.Infrastructure --startup-project DotnetEnterpriseApi.Api
dotnet run --project DotnetEnterpriseApi.Api
```

### Testing with Different Providers

Update `DataProvider` in `appsettings.json` to test with each provider:

```json
"DataProvider": "EntityFramework"   // or "Dapper" or "Ado"
```

Run the API and verify your changes work correctly with all three providers.

## Code Guidelines

### Architecture Rules

- **Domain layer** has zero dependencies on other project layers
- **Application layer** depends only on Domain
- **Infrastructure layer** implements Application interfaces
- **API layer** delegates all logic to MediatR handlers

### When Adding a New Feature

| Layer | What to Add |
|-------|-------------|
| Domain | Entity, Domain Event (if needed) |
| Application | Command/Query, Handler, Validator, DTO, Repository Interface |
| Infrastructure | Repository implementation for **all three providers** (EF, Dapper, ADO) |
| API | Controller endpoint |

### When Adding a New Data Provider

1. Create a folder under `Infrastructure/Repositories/YourProvider/`
2. Implement `ITaskRepository` and `IUserRepository`
3. Add a DI registration method in `DependencyInjection.cs`
4. Add the provider option to the `AddInfrastructure` switch

### Code Style

- Follow existing naming conventions in the codebase
- Use `async/await` for all database operations
- Keep controllers thin - business logic belongs in handlers
- Each handler handles exactly one command or query
- Use FluentValidation for request validation

### Commit Message Format

```
Add: new feature description
Fix: bug fix description
Update: enhancement to existing feature
Refactor: code restructure without behavior change
Docs: documentation changes
```

## Pull Request Checklist

- [ ] Code builds without errors or warnings (`dotnet build`)
- [ ] Changes work with EntityFramework provider
- [ ] Changes work with Dapper provider
- [ ] Changes work with ADO.NET provider
- [ ] New repository interfaces have implementations for all three providers
- [ ] No secrets or connection strings are committed
- [ ] README updated if adding new features or providers

## Questions?

- Open a [Discussion](https://github.com/iamchittaranjandas/dotnet-enterprise-api/issues) using the Question template
- Reach out via [LinkedIn](https://www.linkedin.com/in/iamchittaranjandas)

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
