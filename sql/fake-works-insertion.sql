SELECT DATETIME('now', 'unixepoch') as a;
SELECT DATETIME('now');

SELECT unixepoch() * 1000;
SELECT unixepoch('now','subsec');

SELECT concat('w', 1);

WITH RECURSIVE numbers AS (
    SELECT 1 AS num
    UNION ALL
    SELECT num + 1 FROM numbers WHERE num < 50
)
INSERT INTO "work" (number, title, created_at, updated_at)
SELECT 
	  concat('WORK-', num) AS number
	, concat('WORK ', num) AS title
	, unixepoch('now','subsec') * 1000 AS created_at
	, unixepoch('now','subsec') * 1000 AS updated_at
FROM numbers
;
