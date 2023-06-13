<?php
    include "../Connection.php";
    include "../validateSessionId.php";

    $userID = $_GET["user_id"];
    $oldPassword = $_GET["password"];
    $newPassword = $_GET["newPassword"];
    $newUsername = $_GET["newUsername"];

    // This scripts starts with verifying the user login data, so hackers outside of unity cannot edit other accounts.

    if ((filter_var($userID, FILTER_VALIDATE_INT)
        && $oldPassword == filter_var($oldPassword, FILTER_SANITIZE_STRING))) 
    {
        // Run Query
        $checkLoginQuery = "SELECT * FROM Players WHERE id = " .$userID;
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
                if (password_verify($oldPassword, $registered_password)) 
                {
                    editUser($mysqli, $userID, $newPassword, $newUsername);
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
        echo "0<br> Invalid Inputs";
    }

    function editUser($mysqli, $userID, $newPassword, $newUsername) {
        $updateQuery = "";
        $hashed_password = password_hash($newPassword, PASSWORD_DEFAULT);

        if (isValidEdit($newUsername) && isValidEdit($newPassword)) 
        {
            $newPassword = 
            $updateQuery = "UPDATE Players 
                                SET password = '$hashed_password', 
                                    name = '$newUsername'
                                WHERE id = " .$userID;
        }
        else if (isValidEdit($newUsername)) {
            $updateQuery = "UPDATE Players 
                                SET name = '$newUsername'
                                WHERE id = " .$userID;
        } 
        else if (isValidEdit($newPassword)) {
            $updateQuery = "UPDATE Players 
                                SET password = '$hashed_password' 
                                WHERE id = " .$userID;
        }
        else 
        {
            echo "0<br>";
        }


        $result = $mysqli->query($updateQuery);

        if ($result) {
            echo "1";
        }
        else {
            echo "0<br>";
            showerror($mysqli->errno, $mysqli->error);
        }
    }

    function isValidEdit($string) {
        return (isset($string) 
            && $string == filter_var($string, FILTER_SANITIZE_STRING));
    }
?>