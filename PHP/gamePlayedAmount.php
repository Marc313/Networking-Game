<?php
    include "../Connection.php";            // Connects to MySQL Database
    include "../validateSessionId.php";     // Handles receiving and validating session id
    include "../makeMonthDates.php";        // Creates $todayDate and $lastMonthDate

    $gamesLastMonth = "SELECT Count(*) as games FROM Scores WHERE date_time BETWEEN '$lastMonthDate' AND '$todayDate'";
    $result = $mysqli->query($gamesLastMonth);

    // Verify result
    if ($result) 
    {
        $row = $result->fetch_assoc(); 
        echo json_encode($row);
    }
    else 
    {
        if (!($result = $mysqli->query($query)))
            echo "0<br>";
            showerror($mysqli->errno,$mysqli->error);
    }
?>