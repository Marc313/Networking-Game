<?php
    $session_id = $_GET["session_id"];

    // Start session
    if (isset($session_id)) { // Verify if a session id is given.
    
        $session_id = htmlspecialchars($session_id); // Sanitize session id
        session_id($session_id); // Configure session id
        session_start();

        if (isset($_SESSION["server_id"]) && $_SESSION["server_id"] != 0) {

        }
        else {
            exitPage();
        }
    }
    else {
        exitPage();
    }

    function exitPage() {
        echo "0<br>";
        exit();
    }
?>