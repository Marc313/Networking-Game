<?php
   $db_user = 'marcneeleman';
   $db_pass = 'Ahw7shahTung';
   $db_host = 'localhost';
   $db_name = 'marcneeleman';

    /* Open a connection */
    $mysqli = new mysqli("$db_host","$db_user","$db_pass","$db_name");

    /* check connection */
    if ($mysqli->connect_errno) {
       echo "0: Failed to connect to MySQL: (" . $mysqli->connect_errno() . ") " . $mysqli->connect_error();
       exit();
    }

    function showerror($error,$errornr) {
        die("Error (" . $errornr . ") " . $error);
    }
?>