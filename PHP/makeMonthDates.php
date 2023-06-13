<?php
    $todayDate = date("Y-m-d H:i:s", mktime(0, 0, 0, date("m"), date("d"), date("Y")));             // Today at 00:00:00, date-time formats
    $lastMonthDate = date("Y-m-d H:i:s", mktime(0, 0, 0, date("m") - 1, date("d"), date("Y")));     // Last month at 00:00:00, date-time format
?>