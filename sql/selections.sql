SELECT
	 w."number" 
	,w.title 
	,we.*
FROM work_event we 
INNER JOIN "work" w ON w.id = we.work_id 
WHERE w."number" LIKE '%-161'
	AND DATE(we.created_at / 1000, 'unixepoch') = '2024-05-31'
ORDER BY we.created_at DESC
;

SELECT
	 w."number" 
	,w.title 
	,we.*
FROM work_event we 
INNER JOIN "work" w ON w.id = we.work_id 
WHERE DATE(we.created_at / 1000, 'unixepoch') = '2024-07-17'
ORDER BY we.created_at DESC
;

SELECT
	 *
FROM work_event we 
WHERE DATE(we.created_at / 1000, 'unixepoch') = '2024-07-17'
ORDER BY we.created_at DESC
;

SELECT
      work_id
    , MAX(created_at) AS last_event_created_at
FROM work_event
WHERE work_id IN (1, 2, 3)
GROUP BY work_id
;

SELECT
      work_id
    , MAX(created_at) AS last_event_created_at
FROM work_event
GROUP BY work_id
HAVING work_id IN (1, 2, 3)
;

WITH
    work_last_event(work_id, last_event_created_at) AS (
        SELECT
              work_id
            , MAX(created_at) AS last_event_created_at
        FROM work_event
        GROUP BY work_id
    )
SELECT
      w.id
    , w.number
    , w.title
    , w.created_at
    , w.updated_at
    , IFNULL(e.last_event_created_at, -1) AS last_event_created_at
FROM work AS w
LEFT JOIN work_last_event AS e ON e.work_id = w.id
;

WITH
    work_last_event(work_id, last_event_created_at) AS (
        SELECT
              work_id
            , MAX(created_at) AS last_event_created_at
        FROM work_event
        GROUP BY work_id
    )
SELECT
      w.*
    , IFNULL(e.last_event_created_at, -1) AS last_event_created_at
FROM work AS w
LEFT JOIN work_last_event AS e ON e.work_id = w.id
WHERE w.number LIKE 'Text'
    OR w.title LIKE 'Text'
;

SELECT *
FROM work_event
WHERE event_json LIKE '%"Case":_"WorkStarted"%' OR event_json LIKE '%"Case":"WorkStarted"%'
ORDER BY created_at ASC;

SELECT COUNT(*) from work_event we; 


SELECT
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
;




CREATE TEMPORARY TABLE work_event_backup(id, work_id, event_json, created_at, active_time_point_id, event_name);
INSERT INTO work_event_backup SELECT id, work_id, event_json, created_at, active_time_point_id, event_name FROM work_event;
SELECT id, work_id, event_json, created_at, active_time_point_id, event_name
FROM work_event_backup
ORDER BY id;
DROP TABLE work_event_backup;


WITH first_event_created_at (created_at) AS (
    SELECT e1.created_at
    FROM work_event e1
    WHERE e1.active_time_point_id = '19e45107-d6f2-4588-ab71-d50943d37d7f'
    ORDER BY e1.created_at
    LIMIT 1
)
SELECT e.*, '' AS split, w.*
FROM work_event e
    INNER JOIN work w ON w.id = e.work_id
WHERE
    e.created_at >= (SELECT created_at FROM first_event_created_at LIMIT 1)
    AND e.created_at <= 1715530226320
    AND (e.active_time_point_id ='19e45107-d6f2-4588-ab71-d50943d37d7f' OR e.active_time_point_id IS NULL)
ORDER BY
      e.work_id
    , e.created_at ASC
;






