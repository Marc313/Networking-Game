<?php 
    include "../Connection.php";
    include "../validateSessionId.php";

    $player1_id = $_GET["player1_id"];
    $player2_id = $_GET["player2_id"];
    $winner_id = $_GET["winner_id"];

    // Filter all inputs
    if (!(filter_var($player1_id, FILTER_VALIDATE_INT)
        && filter_var($player2_id, FILTER_VALIDATE_INT)
        && filter_var($winner_id, FILTER_VALIDATE_INT))) {
        echo "Invalid Inputs!";
    }

    // Run Query
    $insertQuery = "INSERT INTO `Scores` (`id`, `player1_id`, `player2_id`, `winner_id`, `date_time`) VALUES (NULL, '$player1_id', '$player2_id', '$winner_id', current_timestamp())";
    $result = $mysqli->query($insertQuery);

    // Validate result
    if (!$result) {
        if (!($result = $mysqli->query($query)))
            echo "0<br>";
            showerror($mysqli->errno,$mysqli->error);
    }
    else 
    {
        echo "1<br>";
    }
?>