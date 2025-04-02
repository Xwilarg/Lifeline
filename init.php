<?php

$db = new SQLite3('lifeline.db');

$db->query("CREATE TABLE IF NOT EXISTS lifelines (key NUMBER PRIMARY KEY, name VARCHAR(255) NOT NULL, target VARCHAR(255) NOT NULL, id VARCHAR(40), token VARCHAR(255) NOT NULL, lastInsert DATETIME NOT NULL)");
foreach (json_decode(file_get_contents("config.json"), true) as $index=>$elem) {
    if (!$db->query("SELECT * FROM lifelines WHERE key = " . $index)->fetchArray()) {
        $q = $db->prepare("INSERT INTO lifelines (key, name, target, id, token, lastInsert) VALUES (?, ?, ?, ?, ?, CURRENT_TIMESTAMP)");
        $q->bindValue(1, $index);
        $q->bindValue(2, $elem["name"]);
        $q->bindValue(3, $elem["target"]);
        $q->bindValue(4, NULL);
        $q->bindValue(5, $elem["token"]);
        $q->execute();
        echo("Data inserted for " . $elem["name"]);
    }
    echo("Done");
}