# NewsFlow — .NET Media Automation Platform

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Next.js 14 (App Router) |
| API | ASP.NET Core 8 Web API |
| Workers | .NET 8 BackgroundService |
| Real-time | SignalR |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL 16 |
| Cache / Queue | Redis |
| Media Storage | Cloudflare R2 (S3-compatible) |
| AI | Anthropic Claude API |
| Voice | ElevenLabs |
| Footage | Pexels API |
| Video | FFmpeg |

## Design Patterns Used

- **Factory** — PlatformAdapterFactory, VideoGeneratorFactory, AIProviderFactory
- **Abstract Factory** — IAIProvider (Claude / future providers)
- **Builder** — PostBuilder (platform-aware post construction)
- **Repository + Unit of Work** — all data access
- **Adapter** — TikTokAdapter, TwitterAdapter, InstagramAdapter, YouTubeAdapter
- **Decorator** — logging and retry wrappers on services
- **Facade** — AIFacade hides multi-service complexity
- **Mediator (MediatR)** — thin controllers, clean command/query handlers
- **Pipeline Behaviours** — validation, logging, performance monitoring
- **Observer / Domain Events** — ArticlePublished, PostFlagged, FlagApproved
- **Strategy** — ConflictFilterStrategy, TerrorismFilterStrategy, DefaultFilterStrategy
- **Chain of Responsibility** — IngestPipeline (duplicate → source → category → safety → AI → persist)
- **Template Method** — VideoGeneratorBase → TikTok / YouTube / Reels generators

## Solution Structure

```
NewsFlow.sln
├── src/
│   ├── NewsFlow.Core/           Domain entities, interfaces, enums, events
│   ├── NewsFlow.Infrastructure/ EF Core, repositories, external services, pipeline
│   ├── NewsFlow.API/            ASP.NET Core controllers, hubs, behaviours
│   └── NewsFlow.Workers/        BackgroundService workers
├── apps/
│   └── web/                     Next.js frontend
└── docker-compose.yml
```

## Quick Start

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose
- Node.js 20+ (for frontend)

### 1. Clone and configure
```bash
git clone https://github.com/yourorg/newsflow
cd NewsFlow
cp src/NewsFlow.API/appsettings.json src/NewsFlow.API/appsettings.Development.json
# Edit appsettings.Development.json with your API keys
```

### 2. Start infrastructure
```bash
docker compose up postgres redis -d
```

### 3. Run migrations
```bash
cd src/NewsFlow.API
dotnet ef database update
```

### 4. Start the API
```bash
dotnet run --project src/NewsFlow.API
# API available at http://localhost:5000
# Swagger at http://localhost:5000/swagger
```

### 5. Start workers
```bash
dotnet run --project src/NewsFlow.Workers
```

### 6. Start frontend
```bash
cd apps/web
npm install
npm run dev
# Frontend at http://localhost:3000
```

## Environment Variables

All secrets go in `appsettings.Development.json` (never commit this file).

Required keys:
- `Jwt:Secret` — 256-bit JWT signing key
- `Anthropic:ApiKey` — Claude API key
- `ElevenLabs:ApiKey` — ElevenLabs API key
- `Pexels:ApiKey` — Pexels API key
- `Platforms:TikTok:ClientKey` + `ClientSecret`
- `Platforms:Twitter:ApiKey` + `ApiSecret` + `BearerToken`
- `Platforms:Instagram:AppId` + `AppSecret`
- `Platforms:YouTube:ClientId` + `ClientSecret`
- `Storage:AccountId` + `AccessKey` + `SecretKey` (Cloudflare R2)

## Key API Endpoints

```
POST   /api/auth/register
POST   /api/auth/login

GET    /api/articles
POST   /api/articles
PUT    /api/articles/{id}
POST   /api/articles/{id}/publish

GET    /api/flags                  All pending flagged posts
PATCH  /api/flags/{id}/approve     Approve a flagged post
PATCH  /api/flags/{id}/reject      Reject a flagged post
PATCH  /api/flags/{id}/escalate    Escalate to senior editor
GET    /api/flags/rules            Get flag rule config
PUT    /api/flags/rules            Update flag rules

GET    /api/accounts
POST   /api/accounts/{id}/connect  OAuth platform connection

GET    /api/analytics/overview
GET    /api/feed

WS     /hubs/collaboration         SignalR real-time hub
```

## Flag Severity System

| Score | Decision | Trigger |
|---|---|---|
| 1–3 | AutoPost (trusted source) | Low keyword density, whitelisted source |
| 4–7 | FlagForReview | Conflict/terrorism keywords detected |
| 8–10 | Block | Extremist content, hard violations |

Rules are configurable per user per category via `PUT /api/flags/rules`.
# newsflow
