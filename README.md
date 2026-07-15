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

## Quick Start (یک دستور — فقط .NET)

> **نیاز به Node.js/npm ندارید.** فرانت‌اند از قبل بیلد شده و داخل API سرو می‌شود.

### Windows

```powershell
cd HRPerformance
.\start.ps1
```

یا دوبار کلیک روی `start.bat`

### Linux / macOS

```bash
cd HRPerformance
./start.sh
```

سپس مرورگر را باز کنید:
- **Application:** http://localhost:5000
- **Swagger:** http://localhost:5000/swagger

برای توقف: `Ctrl+C`

### پیش‌نیازها

| نرم‌افزار | نسخه | دانلود |
|-----------|------|--------|
| .NET SDK | **8.0** (شما: 8.0.401 ✅) | https://dotnet.microsoft.com/download/dotnet/8.0 |
| SQL Server | 2019+ | برای دیتابیس (یک بار `npm run db:init` یا اسکریپت‌های SQL) |

### رفع کندی Cursor / خطای NuGet (SSL)

اگر در IDE پیام `unresolved dependencies` یا خطای SSL هنگام دانلود پکیج می‌بینید:

```powershell
# Windows — از ریشه پروژه
.\scripts\restore-packages.ps1
```

```cmd
scripts\restore-packages.bat
```

سپس Cursor را ببندید و دوباره پوشه پروژه را باز کنید. فقط `HRPerformance.sln` را باز کنید (نه چند solution همزمان).

**علت رایج:** فایروال/آنتی‌ویروس/VPN اتصال به `api.nuget.org` را قطع می‌کند. در صورت نیاز:
- `dotnet nuget locals all --clear` و restore مجدد
- تنظیم پروکسی: `$env:HTTPS_PROXY='http://proxy:port'`

**Node.js فقط برای توسعه‌دهندگان** که می‌خواهند UI را تغییر دهند — برای اجرای عادی لازم نیست.

---

## توسعه UI (اختیاری — نیاز به Node.js)

اگر می‌خواهید فرانت‌اند را ویرایش کنید:

```bash
cd HRPerformance/frontend/hr-performance-web
npm install
npm run dev
```

بعد از تغییرات UI:
```bash
npm run build
# فایل‌های dist را به src/HRPerformance.API/wwwroot کپی کنید
```

---

## Database Setup (دستی)

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

## Backend Setup (جداگانه - اختیاری)

```bash
cd HRPerformance
dotnet run --project src/HRPerformance.API --launch-profile http
```

API: `http://localhost:5000` | Swagger: `http://localhost:5000/swagger`

## Frontend Setup (جداگانه - اختیاری)

```bash
cd HRPerformance/frontend/hr-performance-web
npm run dev
```

Frontend: `http://localhost:3000` (proxies API to backend)

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
