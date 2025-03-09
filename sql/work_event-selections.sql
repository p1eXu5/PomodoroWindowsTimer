select
  *
  , strftime('%d', date(created_at / 1000, 'unixepoch')) as created_at_month
from work_event we
where created_at_month = '07'
;