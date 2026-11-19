BEGIN;

DROP ROUTINE IF EXISTS public.apply_to_team_position(INTEGER, INTEGER, INTEGER, TEXT);

CREATE PROCEDURE public.apply_to_team_position(
    IN target_team_id INTEGER, IN target_position_id INTEGER, IN applicant_user_id INTEGER,
    IN letter TEXT, OUT application_id INTEGER
)
LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO public.applications (position_id, applicant_id, cover_letter)
    SELECT p.id, applicant_user_id, NULLIF(letter, '')
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
    RETURNING id INTO application_id;
END;
$$;

COMMIT;
