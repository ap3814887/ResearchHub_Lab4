-- ResearchHub. Лабораторная работа №4
-- Автоматизированные проверки функций PostgreSQL и бизнес-правил.
-- Перед запуском БД должна быть инициализирована 01..08.
-- Все изменения выполняются в транзакции и откатываются.

BEGIN;

DO $$
DECLARE
    v_user_id bigint;
    v_project_id bigint;
    v_task_id bigint;
    v_count bigint;
    v_before timestamptz;
    v_after timestamptz;
BEGIN
    RAISE NOTICE 'TEST 1: CRUD пользователя';
    SELECT id INTO v_user_id
    FROM create_user('Lab4 Test User', 'lab4.user@researchhub.local');
    IF v_user_id IS NULL THEN RAISE EXCEPTION 'TEST 1 FAILED: create_user'; END IF;

    PERFORM * FROM update_user(v_user_id, 'Lab4 Test User Updated', 'lab4.user.updated@researchhub.local');
    IF NOT EXISTS (SELECT 1 FROM users WHERE id = v_user_id AND email = 'lab4.user.updated@researchhub.local') THEN
        RAISE EXCEPTION 'TEST 1 FAILED: update_user';
    END IF;

    RAISE NOTICE 'TEST 2: CRUD проекта и автоматическое добавление владельца';
    SELECT id INTO v_project_id
    FROM create_project('Lab4 Test Project', 'Project for database tests', v_user_id);
    IF v_project_id IS NULL THEN RAISE EXCEPTION 'TEST 2 FAILED: create_project'; END IF;
    IF NOT EXISTS (
        SELECT 1 FROM project_members
        WHERE project_id = v_project_id AND user_id = v_user_id AND role = 'Owner'
    ) THEN
        RAISE EXCEPTION 'TEST 2 FAILED: owner was not added automatically';
    END IF;

    RAISE NOTICE 'TEST 3: добавление участника';
    PERFORM * FROM add_project_member(v_project_id, 1, 'Member');
    IF NOT EXISTS (SELECT 1 FROM project_members WHERE project_id = v_project_id AND user_id = 1) THEN
        RAISE EXCEPTION 'TEST 3 FAILED: add_project_member';
    END IF;

    RAISE NOTICE 'TEST 4: повторный участник должен быть отклонён PostgreSQL';
    BEGIN
        PERFORM * FROM add_project_member(v_project_id, 1, 'Member');
        RAISE EXCEPTION 'TEST 4 FAILED: duplicate member was accepted';
    EXCEPTION
        WHEN unique_violation THEN
            RAISE NOTICE 'TEST 4 OK: duplicate member rejected with SQLSTATE 23505';
    END;

    RAISE NOTICE 'TEST 5: CRUD задачи';
    SELECT id INTO v_task_id
    FROM create_task(v_project_id, 'Lab4 Test Task', 'Task for DB tests', 1, 'New', 'Normal', CURRENT_DATE + 5);
    IF v_task_id IS NULL THEN RAISE EXCEPTION 'TEST 5 FAILED: create_task'; END IF;

    PERFORM * FROM update_task(v_task_id, 'Lab4 Test Task Updated', 'Updated', 1, 'High', CURRENT_DATE + 7);
    IF NOT EXISTS (SELECT 1 FROM tasks WHERE id = v_task_id AND title = 'Lab4 Test Task Updated' AND priority = 'High') THEN
        RAISE EXCEPTION 'TEST 5 FAILED: update_task';
    END IF;

    RAISE NOTICE 'TEST 6: updated_at изменяется триггером';
    SELECT updated_at INTO v_before FROM tasks WHERE id = v_task_id;
    PERFORM pg_sleep(0.02);
    UPDATE tasks SET description = description || ' trigger' WHERE id = v_task_id;
    SELECT updated_at INTO v_after FROM tasks WHERE id = v_task_id;
    IF v_after <= v_before THEN RAISE EXCEPTION 'TEST 6 FAILED: updated_at was not changed'; END IF;

    RAISE NOTICE 'TEST 7: смена статуса';
    PERFORM * FROM change_task_status(v_task_id, 'InProgress');
    IF (SELECT status FROM tasks WHERE id = v_task_id) <> 'InProgress' THEN
        RAISE EXCEPTION 'TEST 7 FAILED: status';
    END IF;

    RAISE NOTICE 'TEST 8: процедура смены приоритета';
    CALL change_task_priority(v_task_id, 'Critical');
    IF (SELECT priority FROM tasks WHERE id = v_task_id) <> 'Critical' THEN
        RAISE EXCEPTION 'TEST 8 FAILED: priority procedure';
    END IF;

    RAISE NOTICE 'TEST 9: неучастник не может быть исполнителем';
    BEGIN
        PERFORM * FROM create_task(v_project_id, 'Invalid assignee', NULL, 5, 'New', 'Normal', NULL);
        RAISE EXCEPTION 'TEST 9 FAILED: non-member accepted';
    EXCEPTION
        WHEN check_violation THEN
            RAISE NOTICE 'TEST 9 OK: PostgreSQL rejected non-member with SQLSTATE 23514';
    END;

    RAISE NOTICE 'TEST 10: Cancelled -> Done запрещён';
    PERFORM * FROM change_task_status(v_task_id, 'Cancelled');
    BEGIN
        PERFORM * FROM change_task_status(v_task_id, 'Done');
        RAISE EXCEPTION 'TEST 10 FAILED: forbidden transition accepted';
    EXCEPTION
        WHEN check_violation THEN
            RAISE NOTICE 'TEST 10 OK: forbidden transition rejected';
    END;

    RAISE NOTICE 'TEST 11: каскадное удаление проекта';
    SELECT COUNT(*) INTO v_count FROM tasks WHERE project_id = v_project_id;
    IF v_count = 0 THEN RAISE EXCEPTION 'TEST 11 setup failed: no task'; END IF;
    PERFORM delete_project(v_project_id);
    IF EXISTS (SELECT 1 FROM tasks WHERE project_id = v_project_id) THEN
        RAISE EXCEPTION 'TEST 11 FAILED: tasks were not cascade-deleted';
    END IF;
    IF EXISTS (SELECT 1 FROM project_members WHERE project_id = v_project_id) THEN
        RAISE EXCEPTION 'TEST 11 FAILED: members were not cascade-deleted';
    END IF;

    RAISE NOTICE 'TEST 12: удаление пользователя после снятия ссылок';
    PERFORM delete_user(v_user_id);
    IF EXISTS (SELECT 1 FROM users WHERE id = v_user_id) THEN
        RAISE EXCEPTION 'TEST 12 FAILED: delete_user';
    END IF;

    RAISE NOTICE 'ALL DATABASE TESTS PASSED';
END;
$$;

-- Проверка транзакционности: создаём запись и убеждаемся, что внешний ROLLBACK её уберёт.
SELECT * FROM create_user('Rollback User', 'rollback.user@researchhub.local');

ROLLBACK;

-- После ROLLBACK результат должен быть 0.
SELECT COUNT(*) AS rollback_user_count
FROM users
WHERE email = 'rollback.user@researchhub.local';
