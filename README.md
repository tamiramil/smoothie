# Smoothie - Project Management System

Test project for Sibers — intern/junior C# Developer

## Tech Stack

- **.NET 9.0**
- **ASP.NET Core MVC**
- **N-Layer Architecture** (PL/BLL/DAL)
- **Entity Framework Core**
- **SQLite**

## Architecture

The project follows **N-Layer Architecture** pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────┐
│  Presentation Layer (smoothie.Web)          │
│  Controllers, Views, ViewModels             │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  Business Logic Layer (smoothie.BLL)        │
│  Services, DTOs                             │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  Data Access Layer (smoothie.DAL)           │
│  DbContext, Models                          │
└─────────────────────────────────────────────┘
```

### Layer Dependencies
- **PLL** → depends on → **BLL**
- **BLL** → depends on → **DAL**
- **DAL** → independent (only EF Core dependency)

## Run Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Any IDE (optional): Visual Studio, VS Code, Rider

## Quick Start

In terminal (if Windows: press `Win+R`, type `cmd`, press `Enter`)

```bash
git clone https://github.com/tamiramil/smoothie.git
cd smoothie
dotnet restore
dotnet build
dotnet run --project smoothie.Web
```

Database is created automatically at the first run (`smoothie.Web/app.db`).

**To open the app:**
- HTTPS: `https://localhost:7185`
- HTTP: `http://localhost:5136`

To change the ports edit `smoothie.Web/Properties/launchSettings.json`:
```json
"applicationUrl": "https://localhost:7185;http://localhost:5136"
```

---

## Features

### Backend

- [x] **CRUD for Projects, Employees and Companies**
- [x] **Projects list filtering and sorting**

### Frontend

- [x] **5-Step Project Creation Wizard**
  - Step A: Basic information
  - Step B: Companies selection
  - Step C: Project manager assignment (AJAX search)
  - Step D: Team members selection (AJAX search)
  - Step E: Files uploading
- [x] **Projects Management:**
  - Projects list view with filtering
  - View & edit specific project details
  - Remove project with confirmation
- [x] **AJAX search** for employees in wizard
- [x] **File uploads** for project documents

---

## Troubleshooting

### App doesn't run

**Error:** `The specified framework 'Microsoft.NETCore.App', version '9.0.0' was not found`

**Solution:** Install [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

---

### Port is busy

**Error:** `Address already in use`

**Solution:** Change port:
```bash
dotnet run --project smoothie.Web --urls="http://localhost:xxxx"
```

Or edit `smoothie.Web/Properties/launchSettings.json`.

---

To reset db just remove `app.db` file in the project root directory.

---

## Development History

Full development history is accessible here in commits:
```bash
git log --oneline
```

[Or here in GitHub](https://github.com/tamiramil/smoothie/commits/master/)

---

## Author

**Temirlan Emilbekov**

📧 Email: temirlan.emilekov@proton.me  
💻 GitHub: [github.com/tamiramil](https://github.com/tamiramil)  
💬 Telegram: @emilbektemir

Thanks :heart: