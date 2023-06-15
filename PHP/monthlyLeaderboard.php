<?php
    include "../Connection.php";
    include "../validateSessionId.php";
    include "../makeMonthDates.php";        // Creates $todayDate and $lastMonthDate

    $getPlayerNameSubquery = "SELECT p.name FROM Players p WHERE p.id = s.winner_id";
    $getTotalGames = "SELECT Count(*) FROM Scores s 
                        WHERE s.player1_id = win_id OR s.player2_id = win_id
                        AND date_time BETWEEN '$lastMonthDate' AND '$todayDate' 
                        ORDER BY s.date_time desc 
                        LIMIT 6";

    $bestRankings = "SELECT s.winner_id as win_id, 
                            COUNT(*) as wins, 
                            ($getPlayerNameSubquery) as winner_name,
                            COUNT(*)/($getTotalGames) as winRate,
                            ($getTotalGames) as totalGames
                        FROM Scores s
                        WHERE date_time BETWEEN '$lastMonthDate' AND '$todayDate'
                        GROUP BY s.winner_id
                        ORDER BY COUNT(*) desc, COUNT(*)/($getTotalGames) desc
                        LIMIT 6";
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