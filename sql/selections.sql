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