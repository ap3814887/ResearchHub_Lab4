-- ResearchHub. Лабораторная работа №1
-- 04_functions_projects.sql
-- CRUD проектов и работа с участниками.

CREATE OR REPLACE FUNCTION create_project(
    p_name        varchar,
    p_description text,
    p_owner_id    bigint
)
RETURNS TABLE (
    id bigint,
    name varchar,
    description text,
    owner_id bigint,
    created_at timestamptz
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_project_id bigint;
    v_name varchar(200) := btrim(p_name);
BEGIN
    IF v_name IS NULL OR v_name = '' THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Project name must not be empty';
    END IF;

    IF p_owner_id IS NULL OR NOT EXISTS (SELECT 1 FROM users u WHERE u.id = p_owner_id) THEN
        RAISE EXCEPTION USING ERRCODE = '23503', MESSAGE = 'Project owner does not exist';
    END IF;

    -- Обе операции выполняются в рамках одной транзакции вызова функции:
    -- 1) создаётся проект;
    -- 2) владелец автоматически добавляется в project_members.
    INSERT INTO projects(name, description, owner_id)
    VALUES (v_name, p_description, p_owner_id)
    RETURNING projects.id INTO v_project_id;

    INSERT INTO project_members(project_id, user_id, role)
    VALUES (v_project_id, p_owner_id, 'Owner');

    RETURN QUERY
    SELECT p.id, p.name, p.description, p.owner_id, p.created_at
    FROM projects p
    WHERE p.id = v_project_id;
END;
$$;

CREATE OR REPLACE FUNCTION get_project_by_id(p_project_id bigint)
RETURNS TABLE (
    id bigint,
    name varchar,
    description text,
    owner_id bigint,
    created_at timestamptz
)
LANGUAGE sql
STABLE
AS $$
    SELECT p.id, p.name, p.description, p.owner_id, p.created_at
    FROM projects p
    WHERE p.id = p_project_id;
$$;

CREATE OR REPLACE FUNCTION get_projects()
RETURNS TABLE (
    id bigint,
    name varchar,
    description text,
    owner_id bigint,
    created_at timestamptz
)
LANGUAGE sql
STABLE
AS $$
    SELECT p.id, p.name, p.description, p.owner_id, p.created_at
    FROM projects p
    ORDER BY p.id;
$$;

CREATE OR REPLACE FUNCTION update_project(
    p_project_id bigint,
    p_name       varchar,
    p_description text
)
RETURNS TABLE (
    id bigint,
    name varchar,
    description text,
    owner_id bigint,
    created_at timestamptz
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_name varchar(200) := btrim(p_name);
BEGIN
    IF NOT EXISTS (SELECT 1 FROM projects p WHERE p.id = p_project_id) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Project not found';
    END IF;

    IF v_name IS NULL OR v_name = '' THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Project name must not be empty';
    END IF;

    RETURN QUERY
    UPDATE projects p
    SET name = v_name,
        description = p_description
    WHERE p.id = p_project_id
    RETURNING p.id, p.name, p.description, p.owner_id, p.created_at;
END;
$$;

CREATE OR REPLACE FUNCTION delete_project(p_project_id bigint)
RETURNS boolean
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM projects p WHERE p.id = p_project_id) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Project not found';
    END IF;

    -- Связанные строки project_members и tasks удаляются каскадно внешними ключами.
    DELETE FROM projects p WHERE p.id = p_project_id;
    RETURN true;
END;
$$;

CREATE OR REPLACE FUNCTION add_project_member(
    p_project_id bigint,
    p_user_id    bigint,
    p_role       varchar DEFAULT 'Member'
)
RETURNS TABLE (
    project_id bigint,
    user_id bigint,
    role varchar,
    joined_at timestamptz
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_owner_id bigint;
BEGIN
    SELECT p.owner_id INTO v_owner_id
    FROM projects p
    WHERE p.id = p_project_id;

    IF v_owner_id IS NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Project not found';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM users u WHERE u.id = p_user_id) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'User not found';
    END IF;

    IF EXISTS (
        SELECT 1 FROM project_members pm
        WHERE pm.project_id = p_project_id AND pm.user_id = p_user_id
    ) THEN
        RAISE EXCEPTION USING ERRCODE = '23505', MESSAGE = 'User is already a member of the project';
    END IF;

    IF p_role NOT IN ('Owner', 'Member') THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Invalid project role';
    END IF;

    IF p_role = 'Owner' AND p_user_id <> v_owner_id THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Only the project owner can have role Owner';
    END IF;

    RETURN QUERY
    INSERT INTO project_members(project_id, user_id, role)
    VALUES (p_project_id, p_user_id, p_role)
    RETURNING project_members.project_id,
              project_members.user_id,
              project_members.role,
              project_members.joined_at;
END;
$$;

CREATE OR REPLACE FUNCTION get_project_members(p_project_id bigint)
RETURNS TABLE (
    project_id bigint,
    user_id bigint,
    user_name varchar,
    email varchar,
    role varchar,
    joined_at timestamptz
)
LANGUAGE plpgsql
STABLE
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM projects p WHERE p.id = p_project_id) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Project not found';
    END IF;

    RETURN QUERY
    SELECT pm.project_id,
           pm.user_id,
           u.name,
           u.email,
           pm.role,
           pm.joined_at
    FROM project_members pm
    JOIN users u ON u.id = pm.user_id
    WHERE pm.project_id = p_project_id
    ORDER BY CASE WHEN pm.role = 'Owner' THEN 0 ELSE 1 END, u.name;
END;
$$;

CREATE OR REPLACE FUNCTION remove_project_member(
    p_project_id bigint,
    p_user_id    bigint
)
RETURNS boolean
LANGUAGE plpgsql
AS $$
DECLARE
    v_owner_id bigint;
BEGIN
    SELECT p.owner_id INTO v_owner_id
    FROM projects p
    WHERE p.id = p_project_id;

    IF v_owner_id IS NULL THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Project not found';
    END IF;

    IF p_user_id = v_owner_id THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'Project owner cannot be removed from project members';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM project_members pm
        WHERE pm.project_id = p_project_id AND pm.user_id = p_user_id
    ) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'Project member not found';
    END IF;

    -- Если пользователь был исполнителем задач этого проекта, назначение снимается.
    UPDATE tasks t
    SET assignee_id = NULL
    WHERE t.project_id = p_project_id AND t.assignee_id = p_user_id;

    DELETE FROM project_members pm
    WHERE pm.project_id = p_project_id AND pm.user_id = p_user_id;

    RETURN true;
END;
$$;
