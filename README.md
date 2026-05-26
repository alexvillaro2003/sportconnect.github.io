@'
# SportConnect

WPF desktop application to find sports activities and players in your area. Built as the Inspiration Lab project for Thomas More Mechelen, AY 2025-2026.

## Tech stack

- .NET 8 + WPF (Windows desktop)
- Entity Framework Core 8 with SQLite
- MVVM with CommunityToolkit.Mvvm
- BCrypt.Net for password hashing
- xUnit + Moq for testing

## Requirements

- Windows 10 or 11
- .NET 8 SDK ([download](https://dotnet.microsoft.com/download/dotnet/8.0))
- Optional: Visual Studio 2022 Community with the ".NET desktop development" workload

## Run

```bash
dotnet restore
dotnet build SportConnect.sln
dotnet run --project SportConnect.UI
```

On first run the SQLite database is created at `%LocalAppData%\SportConnect\sportconnect.db` and seeded with sports, languages, achievements and a default admin (`admin` / `Admin123!`).

## Run tests

```bash
dotnet test
```

## Authors

Inspiration Lab 2025-2026 — Thomas More Mechelen.
'@ | Out-File -FilePath README.md -Encoding utf8