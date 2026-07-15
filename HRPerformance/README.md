# HR Performance & Discipline Management System

سیستم جامع مدیریت عملکرد و انضباط کارکنان برای شهرداری‌ها، سازمان‌های دولتی و شرکت‌های بزرگ.

## Architecture

```
HRPerformance/
├── database/           # SQL Server scripts (01-08)
├── src/
│   ├── HRPerformance.Domain/          # Entities, Enums, Interfaces
│   ├── HRPerformance.Application/     # CQRS (MediatR), DTOs, Validators
│   ├── HRPerformance.Infrastructure/  # EF Core, Repositories, Services
│   └── HRPerformance.API/             # REST API, SignalR, Background Services
└── frontend/
    └── hr-performance-web/            # React + TypeScript + MUI (RTL)
```

## Tech Stack

### Backend
- ASP.NET Core 9 Web API
- Entity Framework Core 9 + SQL Server
- JWT Authentication + Refresh Token
- Clean Architecture + CQRS (MediatR)
- FluentValidation, AutoMapper, Serilog
- SignalR (real-time notifications)
- Background Service (attendance sync every 5 min)

### Frontend
- React 18 + TypeScript + Vite
- Material UI (RTL, Dark/Light theme)
- Redux Toolkit, React Router, Axios
- Chart.js, Persian date support, PWA

## Database Setup

```bash
# Run scripts in order against SQL Server:
sqlcmd -S localhost -i database/01_CreateDatabase.sql
sqlcmd -S localhost -d HRPerformanceDB -i database/02_Tables.sql
sqlcmd -S localhost -d HRPerformanceDB -i database/03_ForeignKeys.sql
sqlcmd -S localhost -d HRPerformanceDB -i database/04_Indexes.sql
sqlcmd -S localhost -d HRPerformanceDB -i database/05_Views.sql
sqlcmd -S localhost -d HRPerformanceDB -i database/06_StoredProcedures.sql
sqlcmd -S localhost -d HRPerformanceDB -i database/07_Triggers.sql
sqlcmd -S localhost -d HRPerformanceDB -i database/08_SeedData.sql
```

## Backend Setup

```bash
cd HRPerformance
# Update connection string in src/HRPerformance.API/appsettings.json
dotnet restore
dotnet build
cd src/HRPerformance.API
dotnet run
```

API: `https://localhost:5001` | Swagger: `https://localhost:5001/swagger`

## Frontend Setup

```bash
cd HRPerformance/frontend/hr-performance-web
npm install
npm run dev
```

Frontend: `http://localhost:5173` (proxies API to backend)

## User Roles

| Role | Access |
|------|--------|
| SuperAdministrator | Full system access |
| OrganizationAdministrator | Org structure, policies, managers |
| Manager | Subordinate employees only |
| Employee | Own profile and scores |

## Key Features

- Dynamic organization hierarchy (unlimited levels)
- Dynamic evaluation categories, items, and rule engine
- Attendance integration (REST/SOAP/SQL View) with auto sync
- Manual evaluations with attachments and workflow
- Employee/Manager/Admin dashboards with charts
- Ranking engine, appeals system, audit log
- Smart alerts via SignalR
- Reports (employee, department, attendance)
- Excel/PDF export ready architecture

## API Endpoints

| Controller | Endpoints |
|------------|-----------|
| Auth | POST /api/auth/login, /refresh |
| Employees | CRUD + search |
| Dashboard | /employee, /manager, /admin |
| Evaluations | Categories, rules, manual evaluations |
| Appeals | Create, review, list |
| Settings | Key-value settings, holidays |
| Notifications | List, mark read |
| Health | GET /api/health |

## Security

- JWT + Refresh Token rotation
- Role-based authorization
- Password hashing (ASP.NET Identity)
- Rate limiting (AspNetCoreRateLimit)
- Input validation (FluentValidation)
- Full audit logging

## License

Proprietary - Enterprise HR Management System
