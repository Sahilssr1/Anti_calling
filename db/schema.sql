-- ============================================================
-- Sanjivani auto-call : local PostgreSQL schema
--
-- Create the database first, then run this file:
--   createdb -U postgres sanjivani_calls
--   psql -U postgres -d sanjivani_calls -f schema.sql
--
-- The web app (App_Code/PgDb.cs) logs every call started from
-- btnsipcall and marks whether the Sanjivani audio message
-- was played to the customer.
-- ============================================================

CREATE TABLE IF NOT EXISTS call_log (
    id              SERIAL PRIMARY KEY,
    call_time       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    mobile_no       VARCHAR(20)  NOT NULL,
    caller_name     VARCHAR(100),
    greeting_used   VARCHAR(20),
    audio_played    BOOLEAN NOT NULL DEFAULT FALSE,
    audio_played_at TIMESTAMPTZ,
    duration_sec    INTEGER,
    remarks         TEXT
);

CREATE INDEX IF NOT EXISTS idx_call_log_mobile ON call_log (mobile_no);
CREATE INDEX IF NOT EXISTS idx_call_log_time   ON call_log (call_time DESC);
