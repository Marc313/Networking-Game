<?php
    include "../Connection.php";
    include "../validateSessionId.php";

    $username = $_GET["usermail"];
    $password = $_GET["password"];

    if (($username == filter_var($username, FILTER_SANITIZE_EMAIL)
        && $password == filter_var($password, FILTER_SANITIZE_STRING))) 
    {
        // Run Query
        $checkLoginQuery = "SELECT * FROM Players WHERE `email` = '" .$username. "'";
        $result = $mysqli->query($checkLoginQuery);

        // Validate result
        if (!$result) 
        {
            if (!($result = $mysqli->query($query))) 
            {
                showerror($mysqli->errno, $mysqli->error);
            }
        }
        else 
        {
            if ($result->num_rows == 0) 
            {
                // Email address not found, return 0
                echo "0<br>";
            }
            else if ($result->num_rows == 1) 
            {
                // Email found, verify password with hash
                $row = $result->fetch_assoc();
                $registered_password = $row["password"];
                if (password_verify($password, $registered_password)) 
                {
                    echo json_encode($row);
                }
                else 
                {
                    echo "0<br>";
                }
            }
            else 
            {
                // Multiple emails found, this should not be possible!
                echo "0<br>";
            }
        }
    }
    else 
    {
        echo "0";
    }
?>