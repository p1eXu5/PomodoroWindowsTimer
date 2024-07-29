CREATE TABLE IF NOT EXISTS active_time_point (
      id TEXT PRIMARY KEY
    , original_id TEXT
    , name TEXT NOT NULL
    , time_span TEXT NOT NULL
    , kind TEXT NOT NULL
    , kind_alias TEXT NOT NULL
    , created_at INTEGER NOT NULL
);
