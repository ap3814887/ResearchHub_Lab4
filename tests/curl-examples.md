# curl — примеры ЛР4

Через Nginx reverse proxy (`localhost:8080`):

```bash
curl http://localhost:8080/health/ready
curl http://localhost:8080/metrics
curl "http://localhost:8080/api/projects?page=1&pageSize=5"
```

Получение JWT:

```bash
curl -X POST http://localhost:8080/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"userId":1,"password":"lab4-demo-password"}'
```

Сохраните `accessToken` в `TOKEN`, затем:

```bash
curl -X POST http://localhost:8080/api/projects \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"curl project","description":"Lab4","ownerId":1}'

curl -X PUT http://localhost:8080/api/projects/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"ResearchHub updated","description":"updated by curl"}'

curl -X PATCH http://localhost:8080/api/tasks/4/status \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":"InProgress"}'

curl -X DELETE http://localhost:8080/api/tasks/4 \
  -H "Authorization: Bearer $TOKEN"
```
