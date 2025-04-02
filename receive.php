<?php

if ($_SERVER['REQUEST_METHOD'] === 'POST') {

    if (isset($_POST["id"]) && isset($_POST["token"])) {
        $db = new SQLite3('lifeline.db');
    
        $q = $db->prepare("UPDATE lifelines set id = ?, lastInsert = CURRENT_TIMESTAMP where token = ?");
        $q->bindValue(1, $_POST["id"]);
        $q->bindValue(2, $_POST["token"]);
        $results = $q->execute();
        echo "ERR: " . $db->lastErrorMsg();
        if (!$results) {
            header("HTTP/1.1 400");
        } else {
            if ($db->changes() > 0) {
                header("HTTP/1.1 204");
            } else {
                header("HTTP/1.1 403");
            }
        }

    } else header("HTTP/1.1 400");
}
else header("HTTP/1.1 405");