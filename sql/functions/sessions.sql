CREATE OR REPLACE FUNCTION public.create_session(
    session_user_id INTEGER,
    session_token_hash BYTEA,
    session_expires_at TIMESTAMPTZ
)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    new_session_id INTEGER;
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

CREATE OR REPLACE FUNCTION public.find_session_by_token_hash(
      session_token_hash BYTEA
  )
  RETURNS SETOF public.sessions
  LANGUAGE sql
  STABLE
  AS $$
      SELECT s.*
      FROM public.sessions AS s
      WHERE s.token_hash = session_token_hash
        AND s.expires_at > CURRENT_TIMESTAMP
        AND s.revoked_at IS NULL;
  $$;

CREATE OR REPLACE FUNCTION public.renew_session(
    session_id INTEGER,
    session_expires_at TIMESTAMPTZ
)
RETURNS SETOF public.sessions
LANGUAGE sql
VOLATILE
AS $$
    UPDATE public.sessions AS s
    SET expires_at = GREATEST(s.expires_at, session_expires_at)
    WHERE s.id = session_id
      AND s.expires_at > clock_timestamp()
      AND s.revoked_at IS NULL
    RETURNING s.*;
$$;
