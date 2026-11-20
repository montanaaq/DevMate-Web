#!/usr/bin/env bash
set +x
set -euo pipefail

sql_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
env_file="$sql_dir/../.env"

if [[ ! -f "$env_file" ]]; then
    printf 'Database configuration file not found: %s\n' "$env_file" >&2
    exit 1
fi

if ! command -v psql >/dev/null; then
    printf 'psql is required to update the database.\n' >&2
    exit 1
fi

unset POSTGRES_HOST POSTGRES_PORT POSTGRES_DB POSTGRES_USER POSTGRES_PASSWORD
source "$env_file"

: "${POSTGRES_HOST:?Set POSTGRES_HOST in .env}"
: "${POSTGRES_PORT:?Set POSTGRES_PORT in .env}"
: "${POSTGRES_DB:?Set POSTGRES_DB in .env}"
: "${POSTGRES_USER:?Set POSTGRES_USER in .env}"
: "${POSTGRES_PASSWORD:?Set POSTGRES_PASSWORD in .env}"

export LC_ALL=C
shopt -s nullglob
set --
for sql_file in "$sql_dir"/migrations/*.sql "$sql_dir"/functions/*.sql "$sql_dir"/procedures/*.sql; do
    set -- "$@" --file "$sql_file"
done

if [[ $# -eq 0 ]]; then
    printf 'No SQL update files found in %s.\n' "$sql_dir" >&2
    exit 1
fi

PGHOST="$POSTGRES_HOST" PGPORT="$POSTGRES_PORT" PGDATABASE="$POSTGRES_DB" \
    PGUSER="$POSTGRES_USER" PGPASSWORD="$POSTGRES_PASSWORD" \
    psql --no-psqlrc --no-password --set=ON_ERROR_STOP=1 \
    --command 'DO $$ BEGIN PERFORM pg_advisory_lock(hashtext($name$skillissue.sql.update$name$)); END; $$;' \
    "$@"
