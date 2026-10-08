-- ResearchHub. Лабораторная работа №1
-- 07_indexes.sql
-- Индексы для типовых условий поиска и соединений.

CREATE INDEX IF NOT EXISTS idx_projects_owner_id
    ON projects(owner_id);

-- project_members(project_id, user_id) уже индексируется первичным ключом,
-- поэтому отдельный индекс только по project_id не создаётся.
CREATE INDEX IF NOT EXISTS idx_project_members_user_id
    ON project_members(user_id);

CREATE INDEX IF NOT EXISTS idx_tasks_project_id
    ON tasks(project_id);

CREATE INDEX IF NOT EXISTS idx_tasks_assignee_id
    ON tasks(assignee_id);

CREATE INDEX IF NOT EXISTS idx_tasks_status
    ON tasks(status);

CREATE INDEX IF NOT EXISTS idx_tasks_priority
    ON tasks(priority);

CREATE INDEX IF NOT EXISTS idx_tasks_due_date
    ON tasks(due_date);

CREATE INDEX IF NOT EXISTS idx_tasks_project_status
    ON tasks(project_id, status);
