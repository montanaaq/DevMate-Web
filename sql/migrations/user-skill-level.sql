DO $$ BEGIN IF EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = 'public'
        AND table_name = 'user_skills'
        AND column_name = 'level'
) THEN
ALTER TABLE public.user_skills
    RENAME COLUMN level TO skill_level;
END IF;
END;
$$;