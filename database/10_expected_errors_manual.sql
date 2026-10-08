-- ResearchHub. Лабораторная работа №1
-- 10_expected_errors_manual.sql
-- Перед выполнением запустить 08_seed.sql.

-- Ошибка 1. Повторное добавление владельца в проект.
SELECT * FROM add_project_member(1, 1, 'Member');
-- Ожидается: User is already a member of the project

-- Ошибка 2. Пользователь 5 не является участником проекта 1.
SELECT * FROM create_task(
    1,
    'Недопустимое назначение',
    'Попытка назначить неучастника проекта',
    5,
    'New',
    'Normal',
    CURRENT_DATE + 3
);
-- Ожидается: Assignee must be a member of the project

-- Ошибка 3. Несуществующий владелец проекта.
SELECT * FROM create_project(
    'Проект без владельца',
    'Проверка бизнес-правила',
    999999
);
-- Ожидается: Project owner does not exist

-- Ошибка 4. Запрещённый переход Cancelled -> Done.
-- В seed задача id=15 имеет статус Cancelled.
SELECT * FROM change_task_status(15, 'Done');
-- Ожидается: Cancelled task cannot be moved to Done

-- Ошибка 5. Создание задачи для несуществующего проекта.
SELECT * FROM create_task(
    999999,
    'Задача без проекта',
    NULL,
    NULL,
    'New',
    'Normal',
    NULL
);
-- Ожидается: Task project does not exist
