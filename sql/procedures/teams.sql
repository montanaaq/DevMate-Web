BEGIN;
DROP ROUTINE IF EXISTS public.apply_to_team_position(INTEGER, INTEGER, INTEGER, TEXT);
CREATE PROCEDURE public.apply_to_team_position(
    IN target_team_id INTEGER,
    IN target_position_id INTEGER,
    IN applicant_user_id INTEGER,
    IN letter TEXT,
    OUT application_id INTEGER
) LANGUAGE plpgsql AS $$ BEGIN PERFORM 1
FROM
    public.teams t
WHERE
    t.id = target_team_id
    AND t.closed_at IS NULL
    AND t.deleted_at IS NULL FOR NO KEY
UPDATE
;
IF NOT FOUND THEN RETURN;
END IF;
INSERT INTO
    public.applications (position_id, applicant_id, cover_letter)
SELECT
    p.id,
    applicant_user_id,
    NULLIF(letter, '')
FROM
    public.teams t
    JOIN public.team_positions p ON p.team_id = t.id
WHERE
    t.id = target_team_id
    AND t.closed_at IS NULL
    AND t.deleted_at IS NULL
    AND p.id = target_position_id
    AND p.is_open
    AND p.deleted_at IS NULL
    AND t.captain_id <> applicant_user_id
    AND NOT EXISTS (
        SELECT
            1
        FROM
            public.team_members m
        WHERE
            m.team_id = t.id
            AND m.user_id = applicant_user_id
            AND m.left_at IS NULL
    )
    AND NOT EXISTS (
        SELECT
            1
        FROM
            public.applications a
            JOIN public.team_positions existing ON existing.id = a.position_id
        WHERE
            existing.team_id = t.id
            AND a.applicant_id = applicant_user_id
            AND a.status = 'pending'
    ) FOR SHARE OF p ON CONFLICT (position_id, applicant_id)
WHERE
    status = 'pending' DO NOTHING RETURNING id INTO application_id;
END;
$$;
CREATE
OR REPLACE PROCEDURE public.resolve_team_application(
    IN target_team_id INTEGER,
    IN target_application_id INTEGER,
    IN captain_user_id INTEGER,
    IN accept_application BOOLEAN,
    OUT resolved BOOLEAN
) LANGUAGE plpgsql AS $$ DECLARE team_open BOOLEAN;
application RECORD;
membership INTEGER;
BEGIN
SELECT
    t.closed_at IS NULL INTO team_open
FROM
    public.teams t
WHERE
    t.id = target_team_id
    AND t.captain_id = captain_user_id
    AND t.deleted_at IS NULL FOR NO KEY
UPDATE
;
IF NOT FOUND THEN RETURN;
END IF;
SELECT
    a.applicant_id,
    a.status,
    p.id AS position_id,
    p.title,
    p.is_open
    AND p.deleted_at IS NULL AS position_open INTO application
FROM
    public.applications a
    JOIN public.team_positions p ON p.id = a.position_id
WHERE
    a.id = target_application_id
    AND p.team_id = target_team_id FOR
UPDATE
    OF p,
    a;
IF NOT FOUND THEN RETURN;
END IF;
resolved := FALSE;
IF application.status <> 'pending' THEN RETURN;
END IF;
IF accept_application THEN IF NOT team_open
OR NOT application.position_open THEN RETURN;
END IF;
INSERT INTO
    public.team_members (team_id, user_id)
VALUES
    (target_team_id, application.applicant_id) ON CONFLICT (team_id, user_id)
WHERE
    left_at IS NULL DO NOTHING RETURNING id INTO membership;
IF NOT FOUND THEN RETURN;
END IF;
INSERT INTO
    public.team_member_positions (
        membership_id,
        position_id,
        position_title_at_assignment
    )
VALUES
    (
        membership,
        application.position_id,
        application.title
    );
UPDATE
    public.team_positions
SET
    is_open = FALSE,
    updated_at = clock_timestamp()
WHERE
    id = application.position_id;
INSERT INTO
    public.team_events (
        team_id,
        event_type,
        actor_user_id,
        subject_user_id
    )
VALUES
    (
        target_team_id,
        'member_joined',
        captain_user_id,
        application.applicant_id
    );
END IF;
UPDATE
    public.applications
SET
    status = CASE
        WHEN accept_application THEN 'accepted'
        ELSE 'rejected'
    END :: application_status,
    resolved_at = clock_timestamp()
WHERE
    id = target_application_id;
IF accept_application THEN
UPDATE
    public.applications a
SET
    status = 'cancelled',
    resolved_at = clock_timestamp(),
    resolution_reason = CASE
        WHEN a.position_id = application.position_id
            THEN 'Another candidate was selected.'
    END
FROM
    public.team_positions p
WHERE
    p.id = a.position_id
    AND p.team_id = target_team_id
    AND a.status = 'pending'
    AND (
        a.position_id = application.position_id
        OR a.applicant_id = application.applicant_id
    );
END IF;
resolved := TRUE;
END;
$$;
COMMIT;
