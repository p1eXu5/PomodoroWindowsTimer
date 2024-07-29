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
