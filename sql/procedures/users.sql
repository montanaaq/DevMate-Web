BEGIN;
DROP ROUTINE IF EXISTS public.save_user_social_links(INTEGER, JSONB);
CREATE PROCEDURE public.save_user_social_links(IN profile_user_id INTEGER, IN links JSONB) LANGUAGE plpgsql AS $$ BEGIN PERFORM 1
FROM public.users
WHERE id = profile_user_id FOR
UPDATE;
DELETE FROM public.user_social_links
WHERE user_id = profile_user_id;
INSERT INTO public.user_social_links (user_id, network, url)
SELECT profile_user_id,
    link.network,
    link.url
FROM jsonb_to_recordset(links) AS link(network TEXT, url TEXT);
END;
$$;
DROP ROUTINE IF EXISTS public.save_user_skills(INTEGER, INTEGER []);
CREATE PROCEDURE public.save_user_skills(
    IN profile_user_id INTEGER,
    IN skill_ids INTEGER [],
    OUT saved BOOLEAN
) LANGUAGE plpgsql AS $$
BEGIN
    IF (
        SELECT count(*)
        FROM public.skills
        WHERE is_active AND id = ANY(skill_ids)
    ) <> cardinality(skill_ids) THEN
        saved := FALSE;
        RETURN;
    END IF;

    PERFORM 1
    FROM public.users
    WHERE id = profile_user_id
    FOR UPDATE;

    DELETE FROM public.user_skills AS us
    USING public.skills AS s
    WHERE us.user_id = profile_user_id
        AND us.skill_id = s.id
        AND s.is_active
        AND NOT (s.id = ANY(skill_ids));

    INSERT INTO public.user_skills (user_id, skill_id, skill_level)
    SELECT profile_user_id, requested.skill_id, 1
    FROM unnest(skill_ids) AS requested(skill_id)
    ON CONFLICT (user_id, skill_id) DO NOTHING;

    saved := TRUE;
END;
$$;

COMMIT;
