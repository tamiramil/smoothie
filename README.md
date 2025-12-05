# Smoothie - Project Management System

Test project for Sibers — intern/junior C# Developer

## Tech Stack

- **.NET 9.0**
- **ASP.NET Core MVC** (not three-layer architecture, my bad)
- **Entity Framework Core**
- **SQLite**

## Run Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Any IDE (optional): Visual Studio, VS Code, Rider

## Quick Start
In terminal (if Windows: press `Win+R`, type `cmd`, press `Enter`)
```bash
git clone https://github.com/tamiramil/smoothie.git
cd smoothie
dotnet restore
dotnet run
```

Database is created automatically at the first run (`<app-root>/app.db`).

**To open the app:**
- HTTPS: `https://localhost:7185`
- HTTP: `http://localhost:5136`

To change the ports edit `<app-root>/Properties/launchSettings.json`:
```json
"applicationUrl": "https://localhost:7185;http://localhost:5136"
```

---

## Project Structure
```
smoothie
├── app.db
├── appsettings.json
├── Controllers
│   ├── CompaniesController.cs
│   ├── EmployeesController.cs
│   ├── HomeController.cs
│   └── ProjectsController.cs
├── Data
│   └── SmoothieContext.cs
├── Models
│   ├── Company.cs
│   ├── Employee.cs
│   ├── Project.cs
│   └── ProjectDocument.cs
├── Properties
│   └── launchSettings.json
├── ViewModel
│   ├── ErrorViewModel.cs
│   └── ProjectWizardViewModel.cs
├── Views
│   ├── Home
│   ├── Projects
│   ├── Shared
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
└── wwwroot
```

---

## Functionality

#### Backend

- [x] **CRUD for Projects and Employees**
- [x] **CRUD for Companies** (through API, not UI)
- [x] **Projects list filtering and sorting**

#### Frontend (Razor)

- [x] **Project creation wizard** (no Drag&Drop)
- [x] **Projects Management:**
  - Projects list view
  - View & edit specific project details
  - Remove project
- [x] AJAX search for employees in wizard

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
dotnet run --urls="http://localhost:xxxx"
```

Or edit `Properties/launchSettings.json`.

---

To reset db just remove `app.db` file in the project root directory.

---

## Dev histoory

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