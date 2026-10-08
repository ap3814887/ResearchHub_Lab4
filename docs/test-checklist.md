# checklist ручного тестирования

Отметка результата: `PASS / FAIL`. Для чистого старта: `docker compose down -v`, затем `docker compose up --build`.

| № | Уровень | Проверка | Действие | Ожидаемый результат |
|---:|---|---|---|---|
| 1 | Docker | Сборка полного стека | `docker compose up --build` | postgres, backend и frontend запускаются без ошибок |
| 2 | Health | PostgreSQL | `docker compose ps` | postgres имеет статус healthy |
| 3 | Health | Backend ready | открыть `/health/ready` | HTTP 200, PostgreSQL Healthy |
| 4 | Health | Frontend | открыть `/healthz` | `ok` |
| 5 | Frontend | Стартовая страница | открыть `http://localhost:8080` | видна форма входа/регистрации |
| 6 | Auth | Регистрация | зарегистрировать нового пользователя | получен JWT, выполнен вход |
| 7 | Auth | Неверный пароль | попытка входа с неверным demo password | операция отклонена |
| 8 | Users | Создание пользователя | POST `/api/users` с JWT | 201 Created |
| 9 | Users | Дубликат email | создать пользователя с тем же email | 409 Conflict, сообщение PostgreSQL |
| 10 | Projects | Получение проектов | открыть `/projects` | список загружен, HTTP 200 |
| 11 | Projects | Создание проекта | создать проект | 201 Created, карточка появилась |
| 12 | DB rule | Владелец = участник | открыть участников нового проекта | Owner добавлен автоматически |
| 13 | Members | Добавление участника | добавить пользователя | 201 Created, участник отображается |
| 14 | DB rule | Дубликат участника | добавить того же пользователя повторно | 409 Conflict, понятное сообщение |
| 15 | Tasks | Создание задачи | создать задачу участнику | 201 Created |
| 16 | DB rule | Неучастник как исполнитель | назначить user, не входящего в проект | 400, `Assignee must be a member...` |
| 17 | Tasks | Изменение задачи | изменить название/срок | 200 OK, данные обновлены |
| 18 | Tasks | Приоритет | PATCH priority | 204 No Content, приоритет изменён |
| 19 | Tasks | Статус | New → InProgress | 200 OK |
| 20 | DB rule | Запрещённый статус | Cancelled → Done | 400, бизнес-ошибка PostgreSQL |
| 21 | Filter | Фильтр status | выбрать InProgress | backend получает query, список фильтруется |
| 22 | Filter | Фильтр priority | выбрать High/Critical | список фильтруется |
| 23 | Filter | Фильтр assignee | выбрать участника | список фильтруется |
| 24 | Frontend | Поиск и сортировка | применить поиск/сортировку | текущая страница перестраивается |
| 25 | Frontend | Kanban | переключить доску | 4 колонки отображаются |
| 26 | Frontend | Drag-and-drop | перетащить задачу | PATCH status, карточка перемещается |
| 27 | Persistence | Сохранение данных | обновить браузер | созданные данные остаются в PostgreSQL |
| 28 | API | Все HTTP-методы | запустить `scripts/api-smoke.ps1` | GET/POST/PUT/PATCH/DELETE успешны |
| 29 | Metrics | Метрики | открыть `/metrics` | Prometheus-текст с request/error/uptime counters |
| 30 | Logging | Логи backend | `docker compose logs backend` | видны method/path/status/elapsed |
| 31 | Reverse proxy | Nginx → backend | открыть `/api/projects` через порт 8080 | ответ API приходит через Nginx |
| 32 | Test DB | Отдельная БД | `docker compose --profile test up -d postgres-test` | отдельный `researchhub_test` healthy |
| 33 | DB tests | SQL-тесты | `scripts/run-db-tests.ps1` | `ALL DATABASE TESTS PASSED`, rollback count = 0 |
| 34 | Backend tests | xUnit + Testcontainers | `dotnet test ResearchHub.sln` | минимум 7 integration tests passed |
| 35 | Frontend tests | Vitest | `npm test` | component tests passed |
| 36 | CI | GitHub Actions | push/PR | backend/frontend tests и Docker build green |
