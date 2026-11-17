BEGIN;

ALTER TABLE public.user_social_links
    ALTER COLUMN network TYPE VARCHAR(80) USING network::TEXT;

DROP FUNCTION IF EXISTS public.save_user_social_links(INTEGER, TEXT, TEXT);
DROP TYPE IF EXISTS public.social_network;

COMMIT;
