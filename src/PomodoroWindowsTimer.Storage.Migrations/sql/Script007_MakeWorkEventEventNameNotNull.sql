CREATE TEMPORARY TABLE work_event_backup(id, work_id, event_json, created_at, active_time_point_id, event_name);
INSERT INTO work_event_backup SELECT id, work_id, event_json, created_at, active_time_point_id, event_name FROM work_event;
DROP TABLE work_event;

CREATE TABLE IF NOT EXISTS work_event (
      id INTEGER PRIMARY KEY AUTOINCREMENT
    , work_id INTEGER NOT NULL
    , event_json TEXT NOT NULL
    , created_at INTEGER NOT NULL
    , active_time_point_id TEXT
    , event_name TEXT NOT NULL

    , FOREIGN KEY (work_id)
        REFERENCES work (id)
        ON DELETE CASCADE 
        ON UPDATE NO ACTION

    , FOREIGN KEY (active_time_point_id)
        REFERENCES active_time_point (id)
        ON DELETE SET NULL 
        ON UPDATE NO ACTION
    );

WITH wevt (id, work_id, event_json, created_at, active_time_point_id, event_name) AS (
    SELECT id, work_id, event_json, created_at, active_time_point_id, event_name
    FROM work_event_backup
    ORDER BY id
)
INSERT INTO work_event (work_id, event_json, created_at, active_time_point_id, event_name) SELECT work_id, event_json, created_at, active_time_point_id, event_name FROM wevt;
DROP TABLE work_event_backup;

