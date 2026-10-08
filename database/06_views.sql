-- ResearchHub. Лабораторная работа №1
-- 06_views.sql
-- Представление с расширенной информацией о задачах.

CREATE OR REPLACE VIEW task_details AS
SELECT
    t.id AS task_id,
    t.title AS task_title,
    t.description,
    p.id AS project_id,
    p.name AS project_name,
    t.assignee_id,
    u.name AS assignee_name,
    t.status,
    t.priority,
    t.due_date,
    t.created_at,
    t.updated_at
FROM tasks t
JOIN projects p ON p.id = t.project_id
LEFT JOIN users u ON u.id = t.assignee_id;
