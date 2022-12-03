<?php

$db = new SQLite3("locks.db");
$db->busyTimeout(5000);


if (isset($_GET['new'])) {

	$db->exec("INSERT INTO maglocks(locked) VALUES(0)");
	echo $db->lastInsertRowId();
}

if (isset($_GET['isAlive'])) {
	echo "OK";
}

if (isset($_GET['id'])) {

	$rec = $_GET['id'];
	$stm = $db->prepare("SELECT locked FROM maglocks where id=?");
	$stm->bindValue(1, intval($rec), SQLITE3_INTEGER);
	$res = $stm->execute();
	$row = $res->fetchArray(SQLITE3_NUM);
	echo $row[0];

}

if (isset($_GET['change'])) {
        $rec = $_GET['change'];
        $stm = $db->prepare("SELECT locked FROM maglocks where id=?");
        $stm->bindValue(1, intval($rec), SQLITE3_INTEGER);
        $res = $stm->execute();
        $row = $res->fetchArray(SQLITE3_NUM);
        if ($row[0]) {
                $stm = $db->prepare("UPDATE maglocks SET locked=0 where id=?");
                $stm->bindValue(1, intval($rec), SQLITE3_INTEGER);
                $res = $stm->execute();
	} else {
	        $stm = $db->prepare("UPDATE maglocks SET locked=1 where id=?");
        	$stm->bindValue(1, intval($rec), SQLITE3_INTEGER);
	        $res = $stm->execute();
	}
}

$db->close();
unset($db);

?>
