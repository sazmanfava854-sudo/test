# IoT Connectivity Recommendation System

ASP.NET Core workflow: **K-Means clustering** → **multi-expert AHP** → questionnaire → **TOPSIS / VIKOR / COPRAS**.

## Repository layout (separate Git projects)

This repository contains **only IoTRecommendation**. These must **not** be mixed in one repo:

| Project | Git repository (intended) |
|---------|---------------------------|
| IoTRecommendation (`IoTRecommendation.*`) | **this repo** |
| HR Performance (`HRPerformance.sln`, `src/`, `frontend/`) | **separate** HR repository |
| Rayvarz resend (`rayvarzresend`) | **separate** Rayvarz repository |

See [docs/REPOSITORY_SEPARATION.md](docs/REPOSITORY_SEPARATION.md) for migration steps.

## Quick start

```bash
dotnet restore IoTRecommendation.sln
dotnet run --project IoTRecommendation.Web
```

Open the URL shown in the console (HTTPS development certificate may be required on first run).

Data files: `IoTRecommendation.Web/Data/` (`Settings.json`, `Technologies.json`, `Experts/`).

## Solution structure

```
IoTRecommendation.sln
├── IoTRecommendation.Core/
├── IoTRecommendation.Infrastructure/
├── IoTRecommendation.Web/
└── IoTRecommendation.Core.Tests/
```

## Releases

IoT packages are published under [GitHub Releases](https://github.com/sazmanfava854-sudo/test/releases) (tags such as `iot-recommendation-kmeans-v3`). Prefer release assets over downloading the full branch ZIP if you only need IoT.
