<?php

if ($_SERVER['REQUEST_METHOD'] === 'POST') {

    if (isset($_POST["id"]) && isset($_POST["token"])) {
        $db = new SQLite3('lifeline.db');
    
        $results = $db->query("SELECT target, token from lifelines");
        $data = json_encode($results->fetchArray(SQLITE3_ASSOC)[0]);

    } else header("HTTP/1.1 400");
}
else header("HTTP/1.1 405");