CREATE INDEX IF NOT EXISTS team_positions_open_team_idx
    ON public.team_positions (team_id) WHERE is_open AND deleted_at IS NULL;

CREATE OR REPLACE FUNCTION public.get_team_catalog()
RETURNS TABLE (kind TEXT, id INTEGER, name TEXT)
LANGUAGE sql STABLE AS $$
    SELECT 'skill', s.id, s.name::TEXT
    FROM public.skills s
    JOIN public.skill_categories c ON c.id = s.category_id
    WHERE s.is_active AND c.is_active
    UNION ALL
    SELECT 'category', c.id, c.name::TEXT
    FROM public.position_categories c WHERE c.is_active
    ORDER BY 1, 3;
$$;

BEGIN;

DROP FUNCTION IF EXISTS public.search_teams(INTEGER[], INTEGER);

CREATE FUNCTION public.search_teams(selected_skills INTEGER[], before_id INTEGER)
RETURNS TABLE (
    id INTEGER, name TEXT, description TEXT, member_count INTEGER,
    positions TEXT[], required_skills TEXT[], position_ids INTEGER[]
)
LANGUAGE sql STABLE AS $$
    SELECT t.id, t.name::TEXT, t.description::TEXT,
        (SELECT count(*)::INTEGER FROM public.team_members m
            WHERE m.team_id = t.id AND m.left_at IS NULL),
        ARRAY(SELECT p.title::TEXT FROM public.team_positions p
            WHERE p.team_id = t.id AND p.is_open AND p.deleted_at IS NULL ORDER BY p.id),
        ARRAY(SELECT DISTINCT s.name::TEXT
            FROM public.team_positions p
            JOIN public.position_required_skills r ON r.position_id = p.id
            JOIN public.skills s ON s.id = r.skill_id
            JOIN public.skill_categories c ON c.id = s.category_id
            WHERE p.team_id = t.id AND p.is_open AND p.deleted_at IS NULL
                AND s.is_active AND c.is_active ORDER BY s.name::TEXT),
        ARRAY(SELECT p.id FROM public.team_positions p
            WHERE p.team_id = t.id AND p.is_open AND p.deleted_at IS NULL ORDER BY p.id)
    FROM public.teams t
    WHERE t.closed_at IS NULL AND t.deleted_at IS NULL AND t.id < before_id
        AND EXISTS (SELECT 1 FROM public.team_positions p
            WHERE p.team_id = t.id AND p.is_open AND p.deleted_at IS NULL)
        AND NOT EXISTS (
            SELECT 1 FROM unnest(selected_skills) AS selected(skill_id)
            WHERE NOT EXISTS (
                SELECT 1 FROM public.team_positions p
                JOIN public.position_required_skills r ON r.position_id = p.id
                JOIN public.skills s ON s.id = r.skill_id
                JOIN public.skill_categories c ON c.id = s.category_id
                WHERE p.team_id = t.id AND p.is_open AND p.deleted_at IS NULL
                    AND s.is_active AND c.is_active AND r.skill_id = selected.skill_id
            )
        )
    ORDER BY t.id DESC LIMIT 13;
$$;

CREATE OR REPLACE FUNCTION public.apply_to_team_position(
    target_team_id INTEGER, target_position_id INTEGER, applicant_user_id INTEGER, letter TEXT
)
RETURNS INTEGER
LANGUAGE sql VOLATILE AS $$
    INSERT INTO public.applications (position_id, applicant_id, cover_letter)
    SELECT p.id, applicant_user_id, letter
    FROM public.teams t
    JOIN public.team_positions p ON p.team_id = t.id
    WHERE t.id = target_team_id AND t.closed_at IS NULL AND t.deleted_at IS NULL
        AND p.id = target_position_id AND p.is_open AND p.deleted_at IS NULL
        AND t.captain_id <> applicant_user_id
        AND NOT EXISTS (
            SELECT 1 FROM public.team_members m
            WHERE m.team_id = t.id AND m.user_id = applicant_user_id AND m.left_at IS NULL
        )
    FOR SHARE OF t, p
    ON CONFLICT (position_id, applicant_id) WHERE status = 'pending' DO NOTHING
    RETURNING id;
$$;

COMMIT;

CREATE OR REPLACE FUNCTION public.create_team(
    captain INTEGER, team_name TEXT, team_description TEXT, positions JSONB
)
RETURNS INTEGER
LANGUAGE plpgsql VOLATILE AS $$
DECLARE
    new_team_id INTEGER;
    new_position_id INTEGER;
    membership INTEGER;
    position RECORD;
    slot INTEGER;
BEGIN
    IF jsonb_typeof(positions) IS DISTINCT FROM 'array' THEN
        RETURN NULL;
    END IF;
    IF jsonb_array_length(positions) NOT BETWEEN 1 AND 20 THEN
        RETURN NULL;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM jsonb_to_recordset(positions) AS p(
            title TEXT, "categoryId" INTEGER, count INTEGER,
            "includeCaptain" BOOLEAN, "skillIds" INTEGER[]
        )
        WHERE p.title IS NULL OR length(trim(p.title)) NOT BETWEEN 1 AND 120
            OR p.count IS NULL OR p.count NOT BETWEEN 1 AND 20
            OR p."includeCaptain" IS NULL
            OR NOT EXISTS (SELECT 1 FROM public.position_categories c
                WHERE c.id = p."categoryId" AND c.is_active)
            OR p."skillIds" IS NULL OR cardinality(p."skillIds") NOT BETWEEN 1 AND 20
            OR (SELECT count(*) FROM public.skills s
                JOIN public.skill_categories c ON c.id = s.category_id
                WHERE s.id = ANY(p."skillIds") AND s.is_active AND c.is_active)
                <> cardinality(p."skillIds")
    ) OR (
        SELECT sum(p.count) > 20 OR count(*) FILTER (WHERE p."includeCaptain") > 1
        FROM jsonb_to_recordset(positions) AS p(count INTEGER, "includeCaptain" BOOLEAN)
    ) THEN
        RETURN NULL;
    END IF;

    INSERT INTO public.teams (captain_id, name, description)
    VALUES (captain, trim(team_name), trim(team_description)) RETURNING id INTO new_team_id;

    INSERT INTO public.team_members (team_id, user_id)
    VALUES (new_team_id, captain) RETURNING id INTO membership;

    FOR position IN SELECT * FROM jsonb_to_recordset(positions) AS p(
        title TEXT, "categoryId" INTEGER, count INTEGER,
        "includeCaptain" BOOLEAN, "skillIds" INTEGER[]
    )
    LOOP
        FOR slot IN 1..position.count
        LOOP
            INSERT INTO public.team_positions (team_id, category_id, title, is_open)
            VALUES (new_team_id, position."categoryId", trim(position.title),
                NOT (position."includeCaptain" AND slot = 1))
            RETURNING id INTO new_position_id;

            INSERT INTO public.position_required_skills (position_id, skill_id, required_level)
            SELECT new_position_id, skill_id, 1
            FROM unnest(position."skillIds") AS selected(skill_id);

            IF position."includeCaptain" AND slot = 1 THEN
                INSERT INTO public.team_member_positions (
                    membership_id, position_id, position_title_at_assignment
                ) VALUES (membership, new_position_id, trim(position.title));
            END IF;
        END LOOP;
    END LOOP;

    INSERT INTO public.team_events (team_id, event_type, actor_user_id)
    VALUES (new_team_id, 'team_created', captain);

    RETURN new_team_id;
END;
$$;

-- Keep the single-position entry point used by the demo seed.
CREATE OR REPLACE FUNCTION public.create_team(
    captain INTEGER, team_name TEXT, team_description TEXT,
    position_title TEXT, position_category INTEGER, skill_ids INTEGER[]
)
RETURNS INTEGER
LANGUAGE sql VOLATILE AS $$
    SELECT public.create_team(captain, team_name, team_description,
        jsonb_build_array(jsonb_build_object(
            'title', position_title, 'categoryId', position_category,
            'count', 1, 'includeCaptain', false, 'skillIds', skill_ids
        )));
$$;
