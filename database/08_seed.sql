-- ResearchHub. Лабораторная работа №1
-- 08_seed.sql
-- Тестовые данные: 5 пользователей, 3 проекта, 8 участников, 15 задач.


TRUNCATE TABLE tasks, project_members, projects, users RESTART IDENTITY CASCADE;

-- 5 пользователей
SELECT * FROM create_user('Алексей Смирнов',  'alexey.smirnov@researchhub.local');
SELECT * FROM create_user('Мария Иванова',   'maria.ivanova@researchhub.local');
SELECT * FROM create_user('Иван Петров',     'ivan.petrov@researchhub.local');
SELECT * FROM create_user('Елена Соколова',  'elena.sokolova@researchhub.local');
SELECT * FROM create_user('Дмитрий Орлов',   'dmitry.orlov@researchhub.local');

-- 3 проекта. Владелец автоматически становится участником.
SELECT * FROM create_project('ResearchHub', 'Разработка учебной системы управления проектами', 1);
SELECT * FROM create_project('DataLab',     'Учебный проект анализа экспериментальных данных', 2);
SELECT * FROM create_project('Modeling',    'Исследовательский проект математического моделирования', 3);

-- Ещё 5 участий. Итого project_members = 3 владельца + 5 добавленных = 8.
SELECT * FROM add_project_member(1, 2, 'Member');
SELECT * FROM add_project_member(1, 3, 'Member');
SELECT * FROM add_project_member(2, 1, 'Member');
SELECT * FROM add_project_member(2, 4, 'Member');
SELECT * FROM add_project_member(3, 4, 'Member');

-- 15 задач: по 5 в каждом проекте.
SELECT * FROM create_task(1, 'Спроектировать структуру БД', 'Подготовить таблицы и связи', 1, 'Done',       'High',     CURRENT_DATE + 3);
SELECT * FROM create_task(1, 'Реализовать CRUD пользователей', 'Создать функции пользователей', 2, 'InProgress', 'High', CURRENT_DATE + 5);
SELECT * FROM create_task(1, 'Добавить бизнес-правила', 'Проверки участников и статусов', 3, 'Done',       'Critical', CURRENT_DATE + 7);
SELECT * FROM create_task(1, 'Подготовить индексы', 'Индексы для поиска и соединений', NULL, 'New',        'Normal',   CURRENT_DATE + 9);
SELECT * FROM create_task(1, 'Старый вариант API', 'Задача отменена до начала backend', 2, 'Cancelled',    'Low',      CURRENT_DATE + 10);

SELECT * FROM create_task(2, 'Импортировать набор данных', 'Подготовить исходные данные', 2, 'New',         'High',     CURRENT_DATE + 2);
SELECT * FROM create_task(2, 'Очистить данные', 'Удалить пропуски и дубликаты', 1, 'InProgress',            'Normal',   CURRENT_DATE + 4);
SELECT * FROM create_task(2, 'Рассчитать статистики', 'Средние значения и дисперсии', 4, 'Done',            'High',     CURRENT_DATE + 6);
SELECT * FROM create_task(2, 'Построить графики', 'Визуализировать результаты', NULL, 'New',                'Low',      CURRENT_DATE + 8);
SELECT * FROM create_task(2, 'Экспорт в старый формат', 'Не используется в текущем решении', 4, 'Cancelled','Critical', CURRENT_DATE + 11);

SELECT * FROM create_task(3, 'Сформулировать модель', 'Записать исходные уравнения', 3, 'New',              'Normal',   CURRENT_DATE + 3);
SELECT * FROM create_task(3, 'Подготовить параметры', 'Определить начальные условия', 4, 'InProgress',      'High',     CURRENT_DATE + 5);
SELECT * FROM create_task(3, 'Провести вычислительный эксперимент', 'Получить базовый результат', 3, 'Done','Critical', CURRENT_DATE + 7);
SELECT * FROM create_task(3, 'Сравнить сценарии', 'Сопоставить варианты модели', 4, 'New',                  'High',     CURRENT_DATE + 9);
SELECT * FROM create_task(3, 'Отменённый расчёт', 'Сценарий исключён из исследования', 3, 'Cancelled',      'Low',      CURRENT_DATE + 12);
