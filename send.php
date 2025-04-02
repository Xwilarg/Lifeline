<?php
$db = new SQLite3('lifeline.db');

$results = $db->query("SELECT target, token, name from lifelines");

$rows = [];
while ($row = $results->fetchArray(SQLITE3_ASSOC)) {
    array_push($rows, [
        'token' => $row["token"],
        'target' => $row["target"],
        'name' => $row["name"]
    ]);
}

$finalRes = array();
foreach ($rows as $row) {
    $id = uniqid(rand(), true);

    $post = [
        'token' => $row["token"],
        'id' => $id
    ];
    
    $ch = curl_init($row["target"]);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_POST, true);
    curl_setopt($ch, CURLOPT_POSTFIELDS, $post);
    curl_setopt($ch, CURLOPT_HEADER, true);  
    
    curl_exec($ch);
    $httpcode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    
    curl_close($ch);
    
    $answerId = $httpcode === 204 ? $id : NULL;
    $q = $db->prepare("UPDATE lifelines set id = ?, lastInsert = CURRENT_TIMESTAMP where token = ?");
    $q->bindValue(1, $answerId);
    $q->bindValue(2, $row["token"]);
    $results = $q->execute();

    $finalRes[$row["name"]] = $answerId;
}

header('Content-type: application/json');
echo json_encode($finalRes);