<?php 
    include "../Connection.php";
    include "../validateSessionId.php";

    $email = $_GET["email"];
    $username = $_GET["username"];
    $password = $_GET["password"];
    
    // Filter all inputs. Email uses validation AND sanitization!
    if (!(isset($email) && isset($username) && isset($password) 
        && filter_var($email, FILTER_VALIDATE_EMAIL)
        && $email == filter_var($email, FILTER_SANITIZE_EMAIL)
        && $username == filter_var($username, FILTER_SANITIZE_STRING)
        && $password == filter_var($password, FILTER_SANITIZE_STRING))) 
    {
        echo "Invalid Inputs!<br>";
    }
    else {
        // Hash password
        $hashed_password = password_hash($password, PASSWORD_DEFAULT);

        // Run Query
        $insertQuery = "INSERT INTO Players (id, email, name, password) VALUES (NULL, '$email','$username', '$hashed_password')";
        $insertResult = $mysqli->query($insertQuery);

        // Validate result
        if (!$insertResult) {
            if (!($insertResult = $mysqli->query($query)))
                echo "0<br>";
                showerror($mysqli->errno,$mysqli->error);
        }

        // Get account data
        $selectIdQuery = "SELECT * FROM Players WHERE `email` = '" .$email. "'";
        $result = $mysqli->query($selectIdQuery);
        
        // Validate result
        if (!$result) {
            if (!($result = $mysqli->query($query)))
                echo "0<br>";
                showerror($mysqli->errno,$mysqli->error);
        }

        echo json_encode($result->fetch_assoc());
    }
?>