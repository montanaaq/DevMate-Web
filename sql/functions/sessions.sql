CREATE OR REPLACE FUNCTION public.create_session(
        session_user_id INTEGER,
        session_token_hash BYTEA,
        session_expires_at TIMESTAMPTZ
    ) RETURNS INTEGER LANGUAGE plpgsql AS $$
DECLARE new_session_id INTEGER;
BEGIN
INSERT INTO public.sessions (user_id, token_hash, expires_at)
VALUES (
        session_user_id,
        session_token_hash,
        session_expires_at
    )
RETURNING id INTO new_session_id;
RETURN new_session_id;
END;
$$;
CREATE OR REPLACE FUNCTION public.find_session_by_token_hash(session_token_hash BYTEA) RETURNS SETOF public.sessions LANGUAGE sql STABLE AS $$
SELECT s.*
FROM public.sessions AS s
WHERE s.token_hash = session_token_hash
    AND s.expires_at > CURRENT_TIMESTAMP
    AND s.revoked_at IS NULL;
$$;
