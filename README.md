# ResearchHub — лабораторная работа №4

## Тестирование и воспроизводимый запуск через Docker Compose

ЛР4 объединяет результаты ЛР1–ЛР3 в единый fullstack-проект:

```text
Browser
  |
  v
Nginx + Vue 3 + TypeScript
  |
  | HTTP / JSON (reverse proxy)
  v
ASP.NET Core Web API
  |
  | Npgsql
  v
PostgreSQL
  |
  +-- CRUD-функции/процедуры
  +-- ограничения и транзакции
  +-- бизнес-логика
```

ORM не используется. Backend остаётся транспортным слоем: принимает HTTP, вызывает PostgreSQL через Npgsql и переводит результат/ошибки в HTTP.

## Что реализовано по ЛР4

- checklist ручного тестирования: `docs/test-checklist.md` — 36 проверок;
- автоматизированный SQL-тест БД: `database/09_lab4_tests.sql`;
- xUnit integration tests: `backend/ResearchHub.Tests/` — 9 тестов;
- Testcontainers PostgreSQL в xUnit;
- API smoke test GET/POST/PUT/PATCH/DELETE: `scripts/api-smoke.ps1`;
- Dockerfile backend: `backend/ResearchHub.Api/Dockerfile`;
- Dockerfile frontend: `frontend/Dockerfile`;
- PostgreSQL `postgres:16-alpine` с автоматической инициализацией 01..08;
- `docker-compose.yml` с сервисами `postgres`, `backend`, `frontend`;
- переменные окружения через `.env`/`.env.example`;
- Vue frontend через Nginx;
- persistence в Docker volume;
- business error PostgreSQL → HTTP ProblemDetails → frontend.

## Все дополнительные задания

1. **Health checks**
   - API: `/health/live`, `/health/ready`, `/health/postgres`;
   - Docker healthcheck у postgres/backend/frontend;
   - frontend Nginx: `/healthz`.
2. **Testcontainers**
   - `ResearchHubWebApplicationFactory.cs` поднимает изолированный `postgres:16-alpine` для xUnit.
3. **CI**
   - `.github/workflows/ci.yml`: backend tests, frontend tests/build, Docker Compose validation/build.
4. **Frontend-тесты**
   - Vitest + Vue Test Utils + jsdom;
   - тесты ErrorMessage, LoadingIndicator, PaginationControls, TaskFilters.
5. **Nginx reverse proxy**
   - `frontend/nginx.conf`;
   - `/api/*`, `/health/*`, `/metrics` проксируются в backend.
6. **Логирование**
   - `RequestLoggingMiddleware`: method/path/status/elapsed;
   - ошибки PostgreSQL логируются централизованно.
7. **Метрики**
   - `/metrics` в Prometheus text format;
   - uptime, HTTP requests, errors, accumulated duration.
8. **Отдельная test database**
   - Compose service `postgres-test` под профилем `test`;
   - отдельные имя/пользователь/порт, tmpfs без production volume.

## Быстрый запуск

Требуется Docker Desktop с Docker Compose v2.

В архиве уже есть локальный учебный `.env`. Для собственных значений отредактируйте его или скопируйте `.env.example`.

Из корня проекта:

```powershell
docker compose up --build
```

После запуска:

- frontend: `http://localhost:8080`;
- Swagger: `http://localhost:5082/swagger`;
- backend ready: `http://localhost:5082/health/ready`;
- frontend health: `http://localhost:8080/healthz`;
- metrics: `http://localhost:8080/metrics`;
- PostgreSQL: `localhost:5432`.

Остановка:

```powershell
docker compose down
```

Полный сброс данных и повторная инициализация:

```powershell
docker compose down -v
docker compose up --build
```

или `scripts/reset-local.ps1`.

## Автоматическая инициализация PostgreSQL

Compose монтирует в `/docker-entrypoint-initdb.d`:

```text
01_schema.sql
02_constraints.sql
03_functions_users.sql
04_functions_projects.sql
05_functions_tasks.sql
06_views.sql
07_indexes.sql
08_seed.sql
```

Они автоматически выполняются при создании нового volume PostgreSQL. Тестовые скрипты 09+ специально не подключаются к init, чтобы не менять production-like данные при старте.

## SQL-тесты и отдельная test database

```powershell
.\scripts\run-db-tests.ps1
```

Скрипт запускает профиль `test`, сервис `postgres-test`, а затем выполняет `database/09_lab4_tests.sql`. Изменения теста обёрнуты в транзакцию и откатываются.

Ручной запуск test DB:

```powershell
docker compose --profile test up -d postgres-test
```

## Backend xUnit + Testcontainers

Требуются .NET 8 SDK и запущенный Docker Desktop:

```powershell
dotnet test ResearchHub.sln
```

Тесты сами создают отдельный PostgreSQL container через Testcontainers, применяют SQL 01..08 и проверяют API вместе с реальной бизнес-логикой БД.

Проверяются, в частности:

- GET projects → 200;
- missing project → 404;
- create project → 201;
- duplicate member → PostgreSQL SQLSTATE 23505 → HTTP 409;
- non-member assignee → SQLSTATE 23514 → HTTP 400;
- successful status change;
- forbidden Cancelled → Done;
- health checks;
- metrics.

## Frontend-тесты

```powershell
cd frontend
npm install
npm test
npm run build
```

Тесты находятся рядом с Vue-компонентами в `src/components/*.spec.ts`.

## Полная тестовая последовательность

При установленном Docker Desktop, .NET 8 SDK и Node.js 22:

```powershell
.\scripts\run-all-tests.ps1
```

Она выполняет SQL tests → xUnit/Testcontainers → Vitest → frontend build.

## API smoke test

После `docker compose up --build`:

```powershell
.\scripts\api-smoke.ps1
```

Скрипт демонстрирует GET, POST, PUT, PATCH, DELETE и ожидаемую бизнес-ошибку duplicate member = 409.

## Swagger / Postman / Bruno / curl

- Swagger: `http://localhost:5082/swagger`.
- Postman: `tests/postman/ResearchHub_Lab4.postman_collection.json`.
- Bruno: `tests/bruno/`.
- curl: примеры в `tests/curl-examples.md`.

## Логи

```powershell
docker compose logs -f backend
```

Пример структуры:

```text
HTTP GET /api/projects -> 200 in 12 ms
PostgreSQL business error 23505: User is already a member of the project
```

## Метрики

```text
GET http://localhost:8080/metrics
```

Пример:

```text
researchhub_up 1
researchhub_uptime_seconds 120
researchhub_http_requests_total 42
researchhub_http_errors_total 2
researchhub_http_request_duration_ms_total 550
```

## CI

Workflow `.github/workflows/ci.yml` выполняется на push и pull request:

1. .NET restore/build;
2. xUnit + Testcontainers;
3. frontend `npm install`, Vitest, build/type-check;
4. `docker compose config`;
5. build backend/frontend images.

## Структура

```text
ResearchHub_Lab4/
├── backend/
│   ├── ResearchHub.Api/
│   └── ResearchHub.Tests/
├── frontend/
├── database/
├── docs/
├── scripts/
├── tests/
├── .github/workflows/ci.yml
├── docker-compose.yml
├── .env.example
├── ResearchHub.sln
└── README.md
```
