<?php
    include "../Connection.php";

    $server_id = $_GET["server_id"];
    $password = $_GET["password"];

    if ((filter_var($server_id, FILTER_VALIDATE_INT)
        && $password == filter_var($password, FILTER_SANITIZE_STRING))) 
    {
        // Run Query
        $checkLoginQuery = "SELECT * FROM Server WHERE `id` = " .$server_id. " AND `password` = '" .$password. "'";
        $result = $mysqli->query($checkLoginQuery);

        // Validate result
        if (!$result) 
        {
            if (!($result = $mysqli->query($query))) 
            {
                showerror($mysqli->errno,$mysqli->error);
            }
        }
        else 
        {
            if ($result->num_rows == 0) 
            {
                echo "result: 0<br>";
            }
            else if ($result->num_rows == 1) {
                // Start Session
                session_start();
                $session_id = session_id();
                $_SESSION["server_id"] = $server_id;

                echo $session_id;
            }
            else 
            {
                echo $result->num_rows;
            }
        }
    }
    else 
    {
        echo "0";
    }
?>
