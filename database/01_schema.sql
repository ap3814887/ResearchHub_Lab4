-- ResearchHub. Лабораторная работа №1
-- 01_schema.sql
-- Создание основных таблиц.
-- Повторный запуск удаляет существующие таблицы ResearchHub вместе с зависимыми объектами.

DROP TABLE IF EXISTS tasks, project_members, projects, users CASCADE;

CREATE TABLE users (
    id          bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name        varchar(150) NOT NULL,
    email       varchar(255) NOT NULL,
    created_at  timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE projects (
    id          bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name        varchar(200) NOT NULL,
    description text,
    owner_id    bigint NOT NULL,
    created_at  timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE project_members (
    project_id  bigint NOT NULL,
    user_id     bigint NOT NULL,
    role        varchar(30) NOT NULL DEFAULT 'Member',
    joined_at   timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_project_members PRIMARY KEY (project_id, user_id)
);

CREATE TABLE tasks (
    id          bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    project_id  bigint NOT NULL,
    title       varchar(250) NOT NULL,
    description text,
    assignee_id bigint,
    status      varchar(30) NOT NULL DEFAULT 'New',
    priority    varchar(30) NOT NULL DEFAULT 'Normal',
    due_date    date,
    created_at  timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);
