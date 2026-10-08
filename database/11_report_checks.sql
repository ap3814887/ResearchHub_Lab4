-- ResearchHub. Лабораторная работа №1
-- 11_report_checks.sql
-- Запросы для проверки

-- A. Версия PostgreSQL и активная база
SELECT current_database() AS database_name, version() AS postgresql_version;

-- B. Таблицы и их колонки
SELECT
    c.table_name,
    c.ordinal_position,
    c.column_name,
    c.data_type,
    c.is_nullable,
    c.column_default
FROM information_schema.columns c
WHERE c.table_schema = 'public'
  AND c.table_name IN ('users', 'projects', 'project_members', 'tasks')
ORDER BY c.table_name, c.ordinal_position;

-- C. Ограничения
SELECT
    tc.table_name,
    tc.constraint_name,
    tc.constraint_type
FROM information_schema.table_constraints tc
WHERE tc.table_schema = 'public'
  AND tc.table_name IN ('users', 'projects', 'project_members', 'tasks')
ORDER BY tc.table_name, tc.constraint_type, tc.constraint_name;

-- D. Функции и процедуры ResearchHub
SELECT
    n.nspname AS schema_name,
    p.proname AS routine_name,
    CASE p.prokind WHEN 'f' THEN 'FUNCTION' WHEN 'p' THEN 'PROCEDURE' ELSE p.prokind::text END AS routine_type,
    pg_get_function_identity_arguments(p.oid) AS arguments
FROM pg_proc p
JOIN pg_namespace n ON n.oid = p.pronamespace
WHERE n.nspname = 'public'
  AND (
      p.proname LIKE '%user%'
      OR p.proname LIKE '%project%'
      OR p.proname LIKE '%task%'
      OR p.proname = 'trg_set_task_updated_at'
  )
ORDER BY routine_type, routine_name;

-- E. Индексы
SELECT tablename, indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public'
  AND tablename IN ('users', 'projects', 'project_members', 'tasks')
ORDER BY tablename, indexname;

-- F. Представление task_details
SELECT * FROM task_details ORDER BY task_id LIMIT 10;

-- G. Минимальные объёмы тестовых данных
SELECT
    (SELECT COUNT(*) FROM users) AS users_count,
    (SELECT COUNT(*) FROM projects) AS projects_count,
    (SELECT COUNT(*) FROM project_members) AS project_members_count,
    (SELECT COUNT(*) FROM tasks) AS tasks_count;

-- H. Список пользователей
SELECT * FROM get_users();

-- I. Список проектов
SELECT * FROM get_projects();

-- J. Участники проекта 1
SELECT * FROM get_project_members(1);

-- K. Задачи проекта 1
SELECT * FROM get_project_tasks(1);

-- L. Фильтрация задач проекта 1
SELECT * FROM get_project_tasks(1, 'InProgress', NULL, NULL);

-- M. Задачи пользователя 2
SELECT * FROM get_user_tasks(2);

-- N. Статистика по статусам проекта 1
SELECT * FROM get_task_count_by_status(1);
