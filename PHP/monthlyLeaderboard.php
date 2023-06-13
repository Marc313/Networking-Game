<?php
    include "../Connection.php";
    include "../validateSessionId.php";
    include "../makeMonthDates.php";        // Creates $todayDate and $lastMonthDate

    $getPlayerNameSubquery = "SELECT p.name FROM Players p WHERE p.id = s.winner_id";
    $bestRankings = "SELECT s.winner_id as winner_id, COUNT(*) as wins, ($getPlayerNameSubquery) as winner_name FROM Scores s
                        WHERE date_time BETWEEN '$lastMonthDate' AND '$todayDate'
                        GROUP BY s.winner_id
                        ORDER BY COUNT(*) desc 
                        LIMIT 5";
    $result = $mysqli->query($bestRankings);

    // Verify result
    if ($result) 
    {
        $my_json = "{\"Scores\":["; 
        $row = $result->fetch_assoc(); 
    
        do { 
            $my_json .= json_encode($row); 
        } while ($row = $result->fetch_assoc());

        echo $my_json;
    }
    else 
    {
        if (!($result = $mysqli->query($query)))
            echo "0<br>";
            showerror($mysqli->errno,$mysqli->error);
    }
?>