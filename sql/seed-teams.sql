BEGIN;
INSERT INTO users (email, full_name, password_hash)
VALUES ('artyom@example.com', 'Артём', 'art123') ON CONFLICT (lower(email)) DO NOTHING;
DO $$
DECLARE captain INTEGER;
demo RECORD;
category INTEGER;
required_skills INTEGER [];
team_id INTEGER;
BEGIN
SELECT id INTO captain
FROM users
WHERE lower(email) = 'artyom@example.com';
FOR demo IN
SELECT *
FROM (
        VALUES (
                'Commit Together',
                'Платформа для разработчиков, где можно найти напарника для учёбы и вместе создавать небольшие проекты. Помогите нам разработать веб-интерфейс.',
                'Фронтенд-разработчик',
                'Клиентская разработка',
                ARRAY ['JavaScript', 'HTML', 'CSS']
            ),
            (
                'Campus API',
                'Открытый каталог учебных аудиторий, мероприятий и студенческих клубов. Мы создаём надёжный API с понятной документацией.',
                'Бэкенд-разработчик',
                'Серверная разработка',
                ARRAY ['C#', '.NET', 'PostgreSQL']
            ),
            (
                'Pixel Notes',
                'Удобное пространство для заметок, набросков и совместных идей. Ищем человека, которому нравится создавать продуманные интерфейсы.',
                'UI/UX-дизайнер',
                'UX/UI Дизайнер',
                ARRAY ['Figma', 'UI/UX']
            ),
            (
                'Data Trail',
                'Превращаем данные об общественном транспорте в полезные выводы. Присоединяйтесь, чтобы исследовать данные и создавать понятные графики.',
                'Аналитик данных',
                'Аналитик данных',
                ARRAY ['Python', 'pandas', 'SQL']
            ),
            (
                'Ship It',
                'Простая панель управления релизами студенческих проектов. Помогите нам подключить инструменты развёртывания и упростить отслеживание релизов.',
                'DevOps-инженер',
                'DevOps',
                ARRAY ['Docker', 'Linux', 'Git']
            ),
            (
                'Open Shelf',
                'Общественная библиотека для обмена книгами и рекомендациями. Ищем фронтенд-разработчика, который поможет запустить первую версию.',
                'React-разработчик',
                'Клиентская разработка',
                ARRAY ['React', 'TypeScript', 'CSS']
            )
    ) AS examples(name, description, title, category_name, skills) LOOP IF EXISTS (
        SELECT 1
        FROM teams
        WHERE captain_id = captain
            AND name = demo.name
    ) THEN CONTINUE;
END IF;
SELECT id INTO category
FROM position_categories
WHERE name = demo.category_name;
SELECT array_agg(id) INTO required_skills
FROM skills
WHERE name = ANY(demo.skills);
IF category IS NULL
OR cardinality(required_skills) IS DISTINCT
FROM cardinality(demo.skills) THEN RAISE EXCEPTION 'Run the skill catalog seed before the team seed.';
END IF;
team_id := public.create_team(
    captain,
    demo.name,
    demo.description,
    demo.title,
    category,
    required_skills
);
IF team_id IS NULL THEN RAISE EXCEPTION 'The team seed requires active skills and position categories.';
END IF;
END LOOP;
END;
$$;
COMMIT;