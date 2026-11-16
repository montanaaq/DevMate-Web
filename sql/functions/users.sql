CREATE OR REPLACE FUNCTION public.create_user(
    user_email TEXT,
    user_full_name TEXT,
    user_password_hash TEXT
)
RETURNS INTEGER
LANGUAGE sql
VOLATILE
AS $$
    INSERT INTO public.users (email, full_name, password_hash)
    VALUES (user_email, user_full_name, user_password_hash)
    ON CONFLICT (lower(email)) DO NOTHING
    RETURNING id;
$$;

CREATE OR REPLACE FUNCTION public.find_user_by_email(user_email TEXT)
RETURNS SETOF public.users
LANGUAGE sql
STABLE
AS $$
    SELECT u.*
    FROM public.users AS u
    WHERE lower(u.email) = lower(user_email);
$$;


CREATE OR REPLACE FUNCTION public.find_user_by_id(user_id INTEGER)
RETURNS SETOF public.users
LANGUAGE sql
STABLE
AS $$
    SELECT u.*
    FROM public.users AS u
    WHERE u.id = user_id;
$$;
