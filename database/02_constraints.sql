-- ResearchHub. Лабораторная работа №1
-- 02_constraints.sql
-- Ограничения целостности, внешние ключи и триггер updated_at.

ALTER TABLE users
    ADD CONSTRAINT uq_users_email UNIQUE (email),
    ADD CONSTRAINT ck_users_name_not_blank CHECK (btrim(name) <> ''),
    ADD CONSTRAINT ck_users_email_not_blank CHECK (btrim(email) <> '');

ALTER TABLE projects
    ADD CONSTRAINT fk_projects_owner
        FOREIGN KEY (owner_id) REFERENCES users(id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    ADD CONSTRAINT ck_projects_name_not_blank CHECK (btrim(name) <> '');

ALTER TABLE project_members
    ADD CONSTRAINT fk_project_members_project
        FOREIGN KEY (project_id) REFERENCES projects(id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    ADD CONSTRAINT fk_project_members_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON UPDATE CASCADE ON DELETE RESTRICT,
    ADD CONSTRAINT ck_project_members_role
        CHECK (role IN ('Owner', 'Member'));

ALTER TABLE tasks
    ADD CONSTRAINT fk_tasks_project
        FOREIGN KEY (project_id) REFERENCES projects(id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    ADD CONSTRAINT fk_tasks_assignee
        FOREIGN KEY (assignee_id) REFERENCES users(id)
        ON UPDATE CASCADE ON DELETE SET NULL,
    ADD CONSTRAINT ck_tasks_title_not_blank CHECK (btrim(title) <> ''),
    ADD CONSTRAINT ck_tasks_status
        CHECK (status IN ('New', 'InProgress', 'Done', 'Cancelled')),
    ADD CONSTRAINT ck_tasks_priority
        CHECK (priority IN ('Low', 'Normal', 'High', 'Critical'));

CREATE OR REPLACE FUNCTION trg_set_task_updated_at()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.updated_at := clock_timestamp();
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS set_task_updated_at ON tasks;
CREATE TRIGGER set_task_updated_at
BEFORE UPDATE ON tasks
FOR EACH ROW
EXECUTE FUNCTION trg_set_task_updated_at();
