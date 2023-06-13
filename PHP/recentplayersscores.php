<?php
    include "../Connection.php";
    include "../validateSessionId.php";
    include "../makeMonthDates.php";        // Creates $todayDate and $lastMonthDate

    $player_id = $_GET["player_id"];

    // Validate username
    if (!isset($player_id) || !filter_var($player_id)) 
    {
        echo "0<br>";
        showerror("Invalid input", -5);
    }

    $recentScores = "SELECT s.player1_id, s.player2_id, p1.name as p1_name, p2.name as p2_name, s.winner_id, s.date_time FROM Scores s 
                    LEFT JOIN Players p1 ON (s.player1_id = p1.id) 
                    LEFT JOIN Players p2 ON (s.player2_id = p2.id) 
                    WHERE p1.id = '$player_id' OR p2.id = '$player_id'
                    AND date_time BETWEEN '$lastMonthDate' AND '$todayDate' 
                    ORDER BY s.date_time desc 
                    LIMIT 6";
    $result = $mysqli->query($recentScores);

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
            showerror($mysqli->errno,$mysqli->error);
    }
?>