-- ResearchHub. Лабораторная работа №1
-- 03_functions_users.sql
-- CRUD-функции пользователей.

CREATE OR REPLACE FUNCTION create_user(
    p_name  varchar,
    p_email varchar
)
RETURNS TABLE (
    id bigint,
    name varchar,
    email varchar,
    created_at timestamptz
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_name  varchar(150) := btrim(p_name);
    v_email varchar(255) := lower(btrim(p_email));
BEGIN
    IF v_name IS NULL OR v_name = '' THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'User name must not be empty';
    END IF;

    IF v_email IS NULL OR v_email = '' THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'User email must not be empty';
    END IF;

    IF EXISTS (SELECT 1 FROM users u WHERE u.email = v_email) THEN
        RAISE EXCEPTION USING ERRCODE = '23505', MESSAGE = 'User with this email already exists';
    END IF;

    RETURN QUERY
    INSERT INTO users(name, email)
    VALUES (v_name, v_email)
    RETURNING users.id, users.name, users.email, users.created_at;
END;
$$;

CREATE OR REPLACE FUNCTION get_user_by_id(p_user_id bigint)
RETURNS TABLE (
    id bigint,
    name varchar,
    email varchar,
    created_at timestamptz
)
LANGUAGE sql
STABLE
AS $$
    SELECT u.id, u.name, u.email, u.created_at
    FROM users u
    WHERE u.id = p_user_id;
$$;

CREATE OR REPLACE FUNCTION get_users()
RETURNS TABLE (
    id bigint,
    name varchar,
    email varchar,
    created_at timestamptz
)
LANGUAGE sql
STABLE
AS $$
    SELECT u.id, u.name, u.email, u.created_at
    FROM users u
    ORDER BY u.id;
$$;

CREATE OR REPLACE FUNCTION update_user(
    p_user_id bigint,
    p_name    varchar,
    p_email   varchar
)
RETURNS TABLE (
    id bigint,
    name varchar,
    email varchar,
    created_at timestamptz
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_name  varchar(150) := btrim(p_name);
    v_email varchar(255) := lower(btrim(p_email));
BEGIN
    IF NOT EXISTS (SELECT 1 FROM users u WHERE u.id = p_user_id) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'User not found';
    END IF;

    IF v_name IS NULL OR v_name = '' OR v_email IS NULL OR v_email = '' THEN
        RAISE EXCEPTION USING ERRCODE = '23514', MESSAGE = 'User name and email must not be empty';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM users u
        WHERE u.email = v_email AND u.id <> p_user_id
    ) THEN
        RAISE EXCEPTION USING ERRCODE = '23505', MESSAGE = 'User with this email already exists';
    END IF;

    RETURN QUERY
    UPDATE users u
    SET name = v_name,
        email = v_email
    WHERE u.id = p_user_id
    RETURNING u.id, u.name, u.email, u.created_at;
END;
$$;

CREATE OR REPLACE FUNCTION delete_user(p_user_id bigint)
RETURNS boolean
LANGUAGE plpgsql
AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM users u WHERE u.id = p_user_id) THEN
        RAISE EXCEPTION USING ERRCODE = 'P0002', MESSAGE = 'User not found';
    END IF;

    DELETE FROM users u WHERE u.id = p_user_id;
    RETURN true;
EXCEPTION
    WHEN foreign_key_violation THEN
        RAISE EXCEPTION USING
            ERRCODE = '23503',
            MESSAGE = 'User cannot be deleted while referenced by a project or project membership';
END;
$$;
