-- Fix: Add origin_id to dummy atp:
--
--   INSERT INTO active_time_point (id, name, time_span, kind, kind_alias, created_at)
--   VALUES ('00000000-0000-0000-0000-000000000000', 'dummy', '0001-01-01 00:00:00.000', 'Dummy', 'd', -62135596800000);

UPDATE active_time_point
SET 
      original_id = '00000000-0000-0000-0000-000000000000'
    , time_span = '0001-01-01 00:00:00'
WHERE id = '00000000-0000-0000-0000-000000000000'
;