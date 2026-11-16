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
