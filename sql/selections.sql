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