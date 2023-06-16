<?php
    $todayDate = date("Y-m-d H:i:s", mktime(23, 59, 59, date("m"), date("d"), date("Y")));             // Today at 23:59:59, date-time formats
    $lastMonthDate = date("Y-m-d H:i:s", mktime(23, 59, 59, date("m") - 1, date("d"), date("Y")));     // Last month at 23:59:59, date-time format
?>