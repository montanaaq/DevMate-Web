BEGIN;

DROP ROUTINE IF EXISTS public.renew_session(INTEGER, TIMESTAMPTZ);
CREATE PROCEDURE public.renew_session(
    IN session_id INTEGER,
    IN session_expires_at TIMESTAMPTZ,
    OUT id INTEGER,
    OUT user_id INTEGER,
    OUT token_hash BYTEA,
    OUT created_at TIMESTAMPTZ,
    OUT expires_at TIMESTAMPTZ,
    OUT revoked_at TIMESTAMPTZ
) LANGUAGE plpgsql AS $$
BEGIN
    UPDATE public.sessions AS s
    SET expires_at = GREATEST(s.expires_at, session_expires_at)
    WHERE s.id = session_id
        AND s.expires_at > clock_timestamp()
        AND s.revoked_at IS NULL
    RETURNING s.id, s.user_id, s.token_hash, s.created_at, s.expires_at, s.revoked_at
    INTO id, user_id, token_hash, created_at, expires_at, revoked_at;
END;
$$;

DROP ROUTINE IF EXISTS public.revoke_session(BYTEA);
CREATE PROCEDURE public.revoke_session(IN session_token_hash BYTEA)
LANGUAGE sql AS $$
UPDATE public.sessions AS s
SET revoked_at = clock_timestamp()
WHERE s.token_hash = session_token_hash
    AND s.revoked_at IS NULL;
$$;

COMMIT;
