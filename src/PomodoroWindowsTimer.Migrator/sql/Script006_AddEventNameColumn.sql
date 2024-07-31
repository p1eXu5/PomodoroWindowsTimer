ALTER TABLE work_event
    ADD COLUMN event_name TEXT;

CREATE INDEX IF NOT EXISTS ix__work_event__event_name ON work_event (event_name);

WITH names (id, event_name) AS (
    SELECT
        id,
	    CASE 
		    WHEN
			    event_json LIKE '%"Case":_"WorkStarted"%'
			    OR
			    event_json LIKE '%"Case":"WorkStarted"%'
		    THEN
			    'WorkStarted'
		    WHEN
			    event_json LIKE '%"Case":_"BreakStarted"%'
			    OR
			    event_json LIKE '%"Case":"BreakStarted"%'
		    THEN
			    'BreakStarted'
		    WHEN
			    event_json LIKE '%"Case":_"Stopped"%'
			    OR
			    event_json LIKE '%"Case":"Stopped"%'
		    THEN
			    'Stopped'
		    WHEN
			    event_json LIKE '%"Case":_"WorkReduced"%'
			    OR
			    event_json LIKE '%"Case":"WorkReduced"%'
		    THEN
			    'WorkReduced'
		    WHEN
			    event_json LIKE '%"Case":_"WorkIncreased"%'
			    OR
			    event_json LIKE '%"Case":"WorkIncreased"%'
		    THEN
			    'WorkIncreased'
		    WHEN
			    event_json LIKE '%"Case":_"BreakReduced"%'
			    OR
			    event_json LIKE '%"Case":"BreakReduced"%'
		    THEN
			    'BreakReduced'
		    WHEN
			    event_json LIKE '%"Case":_"BreakIncreased"%'
			    OR
			    event_json LIKE '%"Case":"BreakIncreased"%'
		    THEN
			    'BreakIncreased'
	    END AS event_name
    FROM work_event
)
UPDATE work_event
SET
    event_name = (SELECT event_name FROM names WHERE names.id = work_event.id)
WHERE work_event.id IN (SELECT id FROM names)
;
