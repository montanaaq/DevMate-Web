CREATE
OR REPLACE FUNCTION public.create_user(
    user_email TEXT,
    user_full_name TEXT,
    user_password_hash TEXT
) RETURNS INTEGER LANGUAGE sql AS $$
INSERT INTO
    public.users (email, full_name, password_hash)
VALUES
    (user_email, user_full_name, user_password_hash) ON CONFLICT (lower(email)) DO NOTHING RETURNING id;
$$;
CREATE
OR REPLACE FUNCTION public.find_user_by_email(user_email TEXT) RETURNS SETOF public.users LANGUAGE sql STABLE AS $$
SELECT
    u.*
FROM
    public.users AS u
WHERE
    lower(u.email) = lower(user_email);
$$;
CREATE
OR REPLACE FUNCTION public.find_user_by_id(user_id INTEGER) RETURNS SETOF public.users LANGUAGE sql STABLE AS $$
SELECT
    u.*
FROM
    public.users AS u
WHERE
    u.id = user_id;
$$;
CREATE INDEX IF NOT EXISTS team_members_current_user_idx ON public.team_members (user_id, team_id)
WHERE
    left_at IS NULL;
CREATE INDEX IF NOT EXISTS applications_applicant_idx ON public.applications (applicant_id, created_at DESC);
CREATE INDEX IF NOT EXISTS applications_position_idx ON public.applications (position_id);
CREATE
OR REPLACE FUNCTION public.get_user_profile(profile_user_id INTEGER) RETURNS JSONB LANGUAGE plpgsql STABLE AS $$ DECLARE social_links JSONB;
user_skills TEXT [];
user_projects JSONB;
user_applications JSONB;
BEGIN
SELECT
    jsonb_agg(
        jsonb_build_object('network', network, 'url', url)
        ORDER BY
            id
    ) INTO social_links
FROM
    public.user_social_links
WHERE
    user_id = profile_user_id;
SELECT
    array_agg(
        s.name
        ORDER BY
            s.name
    ) INTO user_skills
FROM
    public.user_skills us
    JOIN public.skills s ON s.id = us.skill_id
WHERE
    us.user_id = profile_user_id;
WITH projects AS (
    SELECT
        t.id,
        t.name,
        t.description,
        CASE
            WHEN t.captain_id = profile_user_id THEN 'Captain'
            ELSE 'Member'
        END AS role,
        CASE
            WHEN t.closed_at IS NULL THEN 'Active'
            ELSE 'Closed'
        END AS status,
        (
            SELECT
                count(*)
            FROM
                public.team_members m
            WHERE
                m.team_id = t.id
                AND m.left_at IS NULL
        ) AS "memberCount",
        ARRAY(
            SELECT
                mp.position_title_at_assignment
            FROM
                public.team_member_positions mp
            WHERE
                mp.membership_id = membership.id
                AND mp.ended_at IS NULL
            ORDER BY
                mp.id
        ) AS positions,
        CASE
            WHEN t.captain_id = profile_user_id THEN COALESCE(
                (
                    SELECT
                        jsonb_agg(
                            jsonb_build_object(
                                'id',
                                a.id,
                                'applicantName',
                                applicant.full_name,
                                'applicantEmail',
                                applicant.email,
                                'position',
                                p.title,
                                'coverLetter',
                                a.cover_letter,
                                'status',
                                a.status,
                                'resolutionReason',
                                a.resolution_reason,
                                'createdAt',
                                a.created_at,
                                'canAccept',
                                t.closed_at IS NULL
                                AND p.is_open
                                AND p.deleted_at IS NULL
                            )
                            ORDER BY
                                (a.status = 'pending') DESC,
                                a.created_at DESC,
                                a.id DESC
                        )
                    FROM
                        public.team_positions p
                        JOIN public.applications a ON a.position_id = p.id
                        JOIN public.users applicant ON applicant.id = a.applicant_id
                    WHERE
                        p.team_id = t.id
                ),
                '[]' :: JSONB
            )
        END AS "incomingApplications"
    FROM
        public.team_members membership
        JOIN public.teams t ON t.id = membership.team_id
    WHERE
        membership.user_id = profile_user_id
        AND membership.left_at IS NULL
        AND t.deleted_at IS NULL
)
SELECT
    jsonb_agg(
        to_jsonb(projects)
        ORDER BY
            name,
            id
    ) INTO user_projects
FROM
    projects;
WITH sent_applications AS (
    SELECT
        a.id,
        t.name AS "teamName",
        t.description,
        p.title AS position,
        a.status,
        a.created_at AS "createdAt",
        a.cover_letter AS "coverLetter",
        a.resolution_reason AS "resolutionReason"
    FROM
        public.applications a
        JOIN public.team_positions p ON p.id = a.position_id
        JOIN public.teams t ON t.id = p.team_id
    WHERE
        a.applicant_id = profile_user_id
)
SELECT
    jsonb_agg(
        to_jsonb(sent_applications)
        ORDER BY
            "createdAt" DESC,
            id DESC
    ) INTO user_applications
FROM
    sent_applications;
RETURN jsonb_build_object(
    'links',
    COALESCE(social_links, '[]' :: JSONB),
    'skills',
    COALESCE(user_skills, ARRAY [] :: TEXT []),
    'teams',
    COALESCE(user_projects, '[]' :: JSONB),
    'applications',
    COALESCE(user_applications, '[]' :: JSONB)
);
END;
$$;
CREATE
OR REPLACE FUNCTION public.save_user_social_links(profile_user_id INTEGER, links JSONB) RETURNS VOID LANGUAGE plpgsql AS $$ BEGIN -- Serialize edits from multiple tabs before replacing this user's links.
PERFORM 1
FROM
    public.users
WHERE
    id = profile_user_id FOR
UPDATE
;
DELETE FROM
    public.user_social_links
WHERE
    user_id = profile_user_id;
INSERT INTO
    public.user_social_links (user_id, network, url)
SELECT
    profile_user_id,
    link.network,
    link.url
FROM
    jsonb_to_recordset(links) AS link(network TEXT, url TEXT);
END;
$$;
