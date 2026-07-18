# HR Performance & Discipline Management System

سیستم جامع مدیریت عملکرد و انضباط کارکنان برای شهرداری‌ها، سازمان‌های دولتی و شرکت‌های بزرگ.

## Architecture (ساده‌شده — یک پروژه)

```
HRPerformance/
├── database/              # SQL Server scripts (01-10)
├── src/HRPerformance.API/ # همه چیز در یک پروژه
│   ├── Controllers/       # API endpoints
│   ├── Services/          # منطق کسب‌وکار + MIS sync
│   ├── Entities/          # مدل‌های دیتابیس
│   ├── Data/              # EF Core DbContext
│   ├── DTOs/              # ورودی/خروجی API
│   └── wwwroot/           # فرانت‌اند بیلد شده
└── frontend/              # سورس React (اختیاری)
```

## Tech Stack

### Backend
- ASP.NET Core 8 Web API (تک‌پروژه‌ای)
- Entity Framework Core 8 + SQL Server
- JWT Authentication + Refresh Token
- Serilog, SignalR, Background Service (MIS sync هر ۵ دقیقه)

### Frontend
- React 18 + TypeScript + Vite
- Material UI (RTL, Dark/Light theme)
- Redux Toolkit, React Router, Axios
- Chart.js, Persian date support, PWA

## Quick Start (یک دستور — فقط .NET)

> **نیاز به Node.js/npm ندارید.** فرانت‌اند از قبل بیلد شده و داخل API سرو می‌شود.

### Windows (با SQL Server — محیط واقعی)

**روش پیشنهادی** (بدون محدودیت PowerShell):

```cmd
cd HRPerformance
start.bat
```

یا دوبار کلیک روی `start.bat`

اگر می‌خواهید `start.ps1` را اجرا کنید و خطای Execution Policy گرفتید:

```powershell
powershell -ExecutionPolicy Bypass -File .\start.ps1
```

یا مستقیم:

```cmd
dotnet run --project src\HRPerformance.API\HRPerformance.API.csproj --launch-profile http
```

### Linux / macOS

```bash
cd HRPerformance
./start.sh
```

سپس مرورگر را باز کنید:
- **Application:** http://localhost:5050
- **Swagger:** http://localhost:5050/swagger

برای توقف: `Ctrl+C`

### پیش‌نیازها

| نرم‌افزار | نسخه | دانلود |
|-----------|------|--------|
| .NET SDK | **8.0** (شما: 8.0.401 ✅) | https://dotnet.microsoft.com/download/dotnet/8.0 |
| SQL Server | 2019+ | برای دیتابیس (یک بار `npm run db:init` یا اسکریپت‌های SQL) |

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

API: `http://localhost:5050` | Swagger: `http://localhost:5050/swagger`

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
