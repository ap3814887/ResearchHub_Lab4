-- ResearchHub. Лабораторная работа №1
-- 09_tests.sql
-- Воспроизводимая проверка CRUD, бизнес-правил, updated_at и каскадного удаления.
-- Перед запуском выполнить 08_seed.sql.
-- Все временные изменения выполняются в транзакции и в конце откатываются.

BEGIN;

-- 1. Создание пользователя
SELECT * FROM create_user('Тестовый Пользователь', 'test.user@researchhub.local');

-- 2. Изменение пользователя
SELECT * FROM update_user(
    (SELECT id FROM users WHERE email = 'test.user@researchhub.local'),
    'Тестовый Пользователь Обновлён',
    'test.user.updated@researchhub.local'
);

-- 3. Создание проекта
SELECT * FROM create_project(
    'Тестовый проект',
    'Проект для автоматизированной проверки',
    (SELECT id FROM users WHERE email = 'test.user.updated@researchhub.local')
);

-- 4. Проверка автоматического добавления владельца
SELECT * FROM get_project_members((SELECT id FROM projects WHERE name = 'Тестовый проект'));

-- 5. Добавление участника
SELECT * FROM add_project_member((SELECT id FROM projects WHERE name = 'Тестовый проект'), 1, 'Member');

-- 6. Создание задачи без исполнителя
SELECT * FROM create_task(
    (SELECT id FROM projects WHERE name = 'Тестовый проект'),
    'Тестовая задача', 'Проверка CRUD', NULL, 'New', 'Normal', CURRENT_DATE + 14
);

-- 7. Назначение исполнителя
SELECT * FROM assign_task((SELECT id FROM tasks WHERE title = 'Тестовая задача'), 1);

-- 8. Изменение задачи
SELECT * FROM update_task(
    (SELECT id FROM tasks WHERE title = 'Тестовая задача'),
    'Тестовая задача обновлена', 'Описание изменено', 1, 'High', CURRENT_DATE + 21
);

-- Проверка автоматического обновления updated_at триггером
DO $$
DECLARE
    v_before timestamptz;
    v_after  timestamptz;
BEGIN
    SELECT t.updated_at INTO v_before FROM tasks t WHERE t.title = 'Тестовая задача обновлена';
    PERFORM pg_sleep(0.02);
    UPDATE tasks SET description = description || ' [trigger check]' WHERE title = 'Тестовая задача обновлена';
    SELECT t.updated_at INTO v_after FROM tasks t WHERE t.title = 'Тестовая задача обновлена';

    IF v_after <= v_before THEN
        RAISE EXCEPTION 'updated_at trigger check failed';
    END IF;
    RAISE NOTICE 'OK: updated_at changed automatically';
END;
$$;

-- 9. Изменение статуса
SELECT * FROM change_task_status((SELECT id FROM tasks WHERE title = 'Тестовая задача обновлена'), 'InProgress');

-- Дополнительно: вызов процедуры изменения приоритета
DO $$
DECLARE
    v_task_id bigint;
BEGIN
    SELECT id
    INTO v_task_id
    FROM tasks
    WHERE title = 'Тестовая задача обновлена';

    CALL change_task_priority(v_task_id, 'Critical');
END;
$$;

SELECT id, priority, updated_at
FROM tasks
WHERE title = 'Тестовая задача обновлена';

-- 10. Удаление задачи
SELECT delete_task((SELECT id FROM tasks WHERE title = 'Тестовая задача обновлена')) AS task_deleted;
SELECT COUNT(*) AS deleted_test_task_count FROM tasks WHERE title = 'Тестовая задача обновлена';

-- 11. Получение задач проекта
SELECT * FROM get_project_tasks(1);

-- Проверка фильтрации, которая понадобится в следующих лабораторных
SELECT * FROM get_project_tasks(1, 'InProgress', NULL, NULL);

-- 12. Получение задач пользователя
SELECT * FROM get_user_tasks(2);

-- 13. Количество задач по статусам
SELECT * FROM get_task_count_by_status(1);

-- 14. Проверка ошибочных бизнес-сценариев без остановки всего теста
DO $$
BEGIN
    BEGIN
        PERFORM * FROM add_project_member(1, 1, 'Member');
        RAISE EXCEPTION 'Expected duplicate member error was not raised';
    EXCEPTION WHEN unique_violation THEN
        RAISE NOTICE 'OK: duplicate project member rejected';
    END;

    BEGIN
        PERFORM * FROM create_task(1, 'Недопустимое назначение', NULL, 5, 'New', 'Normal', CURRENT_DATE + 1);
        RAISE EXCEPTION 'Expected non-member assignee error was not raised';
    EXCEPTION WHEN check_violation THEN
        RAISE NOTICE 'OK: non-member assignee rejected';
    END;

    BEGIN
        PERFORM * FROM create_project('Проект без владельца', NULL, 999999);
        RAISE EXCEPTION 'Expected missing owner error was not raised';
    EXCEPTION WHEN foreign_key_violation THEN
        RAISE NOTICE 'OK: nonexistent project owner rejected';
    END;

    BEGIN
        PERFORM * FROM change_task_status(15, 'Done');
        RAISE EXCEPTION 'Expected forbidden status transition error was not raised';
    EXCEPTION WHEN check_violation THEN
        RAISE NOTICE 'OK: Cancelled -> Done rejected';
    END;

    BEGIN
        PERFORM * FROM create_task(999999, 'Задача без проекта', NULL, NULL, 'New', 'Normal', NULL);
        RAISE EXCEPTION 'Expected missing project error was not raised';
    EXCEPTION WHEN foreign_key_violation THEN
        RAISE NOTICE 'OK: task without existing project rejected';
    END;
END;
$$;

-- 15. Проверка каскадного удаления проекта.
-- После удаления проекта 4 должны исчезнуть его project_members и tasks.
SELECT * FROM create_task(
    (SELECT id FROM projects WHERE name = 'Тестовый проект'),
    'Задача для проверки CASCADE', NULL, 1, 'New', 'Low', NULL
);

-- Сохраняем идентификатор тестового проекта во временной таблице, чтобы проверить CASCADE после удаления.
CREATE TEMP TABLE _test_project_id(id bigint) ON COMMIT DROP;
INSERT INTO _test_project_id(id) SELECT id FROM projects WHERE name = 'Тестовый проект';

SELECT delete_project((SELECT id FROM _test_project_id)) AS project_deleted;
SELECT COUNT(*) AS test_project_count FROM projects WHERE id = (SELECT id FROM _test_project_id);
SELECT COUNT(*) AS test_project_members_count FROM project_members WHERE project_id = (SELECT id FROM _test_project_id);
SELECT COUNT(*) AS test_project_tasks_count FROM tasks WHERE project_id = (SELECT id FROM _test_project_id);

-- 16. Контроль исходных минимальных объёмов seed-данных внутри транзакции
SELECT
    (SELECT COUNT(*) FROM users) AS users_count,
    (SELECT COUNT(*) FROM projects) AS projects_count,
    (SELECT COUNT(*) FROM project_members) AS members_count,
    (SELECT COUNT(*) FROM tasks) AS tasks_count;

-- Откатываем тестовые изменения: исходные seed-данные остаются без изменений.
ROLLBACK;

-- Контроль после ROLLBACK
SELECT
    (SELECT COUNT(*) FROM users) AS users_after_rollback,
    (SELECT COUNT(*) FROM projects) AS projects_after_rollback,
    (SELECT COUNT(*) FROM project_members) AS members_after_rollback,
    (SELECT COUNT(*) FROM tasks) AS tasks_after_rollback;
