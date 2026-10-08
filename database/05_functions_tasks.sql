-- ResearchHub. Лабораторная работа №1
-- 05_functions_tasks.sql
-- CRUD задач, назначение исполнителя, статусы, статистика и процедура изменения приоритета.

CREATE OR REPLACE FUNCTION create_task(
    p_project_id  bigint,
    p_title       varchar,
    p_description text DEFAULT NULL,
    p_assignee_id bigint DEFAULT NULL,
    p_status      varchar DEFAULT 'New',
    p_priority    varchar DEFAULT 'Normal',
    p_due_date    date DEFAULT NULL
)
RETURNS TABLE (
    id bigint,
    project_id bigint,
    title varchar,
    description text,
    assignee_id bigint,
    status varchar,
    priority varchar,
    due_date date,
    created_at timestamptz,
    updated_at timestamptz
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_title varchar(250) := btrim(p_title);
BEGIN
    IF NOT EXISTS (SELECT 1 FROM projects p WHERE p.id = p_project_id) THEN
        RAISE EXCEPTION USING ERRCODE = '23503', MESSAGE = 'Task project does not exist';
    END IF;

    IF v_title IS NULL OR v_title = '' THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Task title must not be empty';
    END IF;

    IF p_status NOT IN ('New', 'InProgress', 'Done', 'Cancelled') THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Invalid task status';
    END IF;

    IF p_priority NOT IN ('Low', 'Normal', 'High', 'Critical') THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Invalid task priority';
    END IF;

    IF p_assignee_id IS NOT NULL AND NOT EXISTS (
        SELECT 1
        FROM project_members pm
        WHERE pm.project_id = p_project_id AND pm.user_id = p_assignee_id
    ) THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Assignee must be a member of the project';
    END IF;

    RETURN QUERY
    INSERT INTO tasks(project_id, title, description, assignee_id, status, priority, due_date)
    VALUES (p_project_id, v_title, p_description, p_assignee_id, p_status, p_priority, p_due_date)
    RETURNING tasks.id, tasks.project_id, tasks.title, tasks.description,
              tasks.assignee_id, tasks.status, tasks.priority, tasks.due_date,
              tasks.created_at, tasks.updated_at;
END;
$$;

CREATE OR REPLACE FUNCTION get_task_by_id(p_task_id bigint)
RETURNS TABLE (
    id bigint,
    project_id bigint,
    title varchar,
    description text,
    assignee_id bigint,
    status varchar,
    priority varchar,
    due_date date,
    created_at timestamptz,
    updated_at timestamptz
)
LANGUAGE sql
STABLE
AS $$
    SELECT t.id, t.project_id, t.title, t.description, t.assignee_id,
           t.status, t.priority, t.due_date, t.created_at, t.updated_at
    FROM tasks t
    WHERE t.id = p_task_id;
$$;

CREATE OR REPLACE FUNCTION get_project_tasks(
    p_project_id  bigint,
    p_status      varchar DEFAULT NULL,
    p_priority    varchar DEFAULT NULL,
    p_assignee_id bigint DEFAULT NULL
)
RETURNS TABLE (
    id bigint,
    project_id bigint,
    title varchar,
    description text,
    assignee_id bigint,
    status varchar,
    priority varchar,
    due_date date,
    created_at timestamptz,
    updated_at timestamptz
)
LANGUAGE plpgsql
STABLE
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM projects p WHERE p.id = p_project_id) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Project not found';
    END IF;

    RETURN QUERY
    SELECT t.id, t.project_id, t.title, t.description, t.assignee_id,
           t.status, t.priority, t.due_date, t.created_at, t.updated_at
    FROM tasks t
    WHERE t.project_id = p_project_id
      AND (p_status IS NULL OR t.status = p_status)
      AND (p_priority IS NULL OR t.priority = p_priority)
      AND (p_assignee_id IS NULL OR t.assignee_id = p_assignee_id)
    ORDER BY t.created_at DESC, t.id DESC;
END;
$$;

CREATE OR REPLACE FUNCTION update_task(
    p_task_id      bigint,
    p_title        varchar,
    p_description  text,
    p_assignee_id  bigint,
    p_priority     varchar,
    p_due_date     date
)
RETURNS TABLE (
    id bigint,
    project_id bigint,
    title varchar,
    description text,
    assignee_id bigint,
    status varchar,
    priority varchar,
    due_date date,
    created_at timestamptz,
    updated_at timestamptz
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_project_id bigint;
    v_title varchar(250) := btrim(p_title);
BEGIN
    SELECT t.project_id INTO v_project_id
    FROM tasks t
    WHERE t.id = p_task_id;

    IF v_project_id IS NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Task not found';
    END IF;

    IF v_title IS NULL OR v_title = '' THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Task title must not be empty';
    END IF;

    IF p_priority NOT IN ('Low', 'Normal', 'High', 'Critical') THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Invalid task priority';
    END IF;

    IF p_assignee_id IS NOT NULL AND NOT EXISTS (
        SELECT 1
        FROM project_members pm
        WHERE pm.project_id = v_project_id AND pm.user_id = p_assignee_id
    ) THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Assignee must be a member of the project';
    END IF;

    RETURN QUERY
    UPDATE tasks t
    SET title = v_title,
        description = p_description,
        assignee_id = p_assignee_id,
        priority = p_priority,
        due_date = p_due_date
    WHERE t.id = p_task_id
    RETURNING t.id, t.project_id, t.title, t.description, t.assignee_id,
              t.status, t.priority, t.due_date, t.created_at, t.updated_at;
END;
$$;

CREATE OR REPLACE FUNCTION assign_task(
    p_task_id bigint,
    p_assignee_id bigint
)
RETURNS TABLE (
    id bigint,
    project_id bigint,
    assignee_id bigint,
    updated_at timestamptz
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_project_id bigint;
BEGIN
    SELECT t.project_id INTO v_project_id
    FROM tasks t
    WHERE t.id = p_task_id;

    IF v_project_id IS NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Task not found';
    END IF;

    IF p_assignee_id IS NOT NULL AND NOT EXISTS (
        SELECT 1
        FROM project_members pm
        WHERE pm.project_id = v_project_id AND pm.user_id = p_assignee_id
    ) THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Assignee must be a member of the project';
    END IF;

    RETURN QUERY
    UPDATE tasks t
    SET assignee_id = p_assignee_id
    WHERE t.id = p_task_id
    RETURNING t.id, t.project_id, t.assignee_id, t.updated_at;
END;
$$;

CREATE OR REPLACE FUNCTION change_task_status(
    p_task_id bigint,
    p_new_status varchar
)
RETURNS TABLE (
    id bigint,
    old_status varchar,
    new_status varchar,
    updated_at timestamptz
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_old_status varchar(30);
BEGIN
    IF p_new_status NOT IN ('New', 'InProgress', 'Done', 'Cancelled') THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Invalid task status';
    END IF;

    SELECT t.status INTO v_old_status
    FROM tasks t
    WHERE t.id = p_task_id
    FOR UPDATE;

    IF v_old_status IS NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Task not found';
    END IF;

    IF v_old_status = 'Cancelled' AND p_new_status = 'Done' THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Cancelled task cannot be moved to Done';
    END IF;

    UPDATE tasks t
    SET status = p_new_status
    WHERE t.id = p_task_id;

    RETURN QUERY
    SELECT t.id, v_old_status, t.status, t.updated_at
    FROM tasks t
    WHERE t.id = p_task_id;
END;
$$;

CREATE OR REPLACE PROCEDURE change_task_priority(
    p_task_id bigint,
    p_new_priority varchar
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_new_priority NOT IN ('Low', 'Normal', 'High', 'Critical') THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Invalid task priority';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM tasks t WHERE t.id = p_task_id) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Task not found';
    END IF;

    UPDATE tasks t
    SET priority = p_new_priority
    WHERE t.id = p_task_id;
END;
$$;

CREATE OR REPLACE FUNCTION delete_task(p_task_id bigint)
RETURNS boolean
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tasks t WHERE t.id = p_task_id) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Task not found';
    END IF;

    DELETE FROM tasks t WHERE t.id = p_task_id;
    RETURN true;
END;
$$;

CREATE OR REPLACE FUNCTION get_user_tasks(p_user_id bigint)
RETURNS TABLE (
    id bigint,
    project_id bigint,
    project_name varchar,
    title varchar,
    status varchar,
    priority varchar,
    due_date date
)
LANGUAGE plpgsql
STABLE
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM users u WHERE u.id = p_user_id) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'User not found';
    END IF;

    RETURN QUERY
    SELECT t.id, t.project_id, p.name, t.title, t.status, t.priority, t.due_date
    FROM tasks t
    JOIN projects p ON p.id = t.project_id
    WHERE t.assignee_id = p_user_id
    ORDER BY t.due_date NULLS LAST, t.id;
END;
$$;

CREATE OR REPLACE FUNCTION get_task_count_by_status(p_project_id bigint DEFAULT NULL)
RETURNS TABLE (
    status varchar,
    task_count bigint
)
LANGUAGE sql
STABLE
AS $$
    SELECT t.status, COUNT(*)::bigint AS task_count
    FROM tasks t
    WHERE p_project_id IS NULL OR t.project_id = p_project_id
    GROUP BY t.status
    ORDER BY t.status;
$$;
