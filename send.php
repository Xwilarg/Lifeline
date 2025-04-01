<?php

$db = new SQLite3('lifeline.db');

$results = $db->query("SELECT target, token from lifelines");
while ($row = $results->fetchArray(SQLITE3_ASSOC)) {
    $id = uniqid(rand(), true);

    $json = json_encode($row);
    $post = [
        'token' => $json["token"],
        'id' => $id
    ];
    
    $ch = curl_init($json["target"]);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_POSTFIELDS, $post);
    
    $response = curl_exec($ch);
    
    curl_close($ch);

    $respJson = json_encode($response);
}