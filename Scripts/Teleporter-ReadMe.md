# Teleporter zwischen zwei Strukturen für 'normale' Spieler

Man setzt einen Teleporter und benennt diesen mit einem Namen mit Präfix "Beam-" und der ID des Ziels. Danach macht man das selbe mit dem zweiten Teleporter, wobei der Name der selbe sein muss.

Beispiel:
Gegeben ist eine Basis mit der ID 42 und ein CV mit der ID 17. (Die ID kann man im 'P'-Menü im Reiter Statistik links unten entnehmen)

Auf der Basis bekommt der Teleporter nun die Bezeichnung Beam-T1@17 und auf dem CV Beam-T1@42

Der Name T1 sollte übereinstimmen denn sonst nimmt sich das Spiel irgendeinen Teleporter der auf dem Ziel verbaut ist.

Zur Information kann man ein LCD platzieren welches mit der Bezeichnung "Info:\[Bezeichnung des Teleporter\]" beginnt. Im Beispiel also auf der Basis Info:Beam-T1@17

Der Inhalt des LCD ist nicht modifizierbar und zeigt den Namen, Typ und Playfield des Ziels an. Außerdem die Uhrzeit der letzten Aktualisierung dieses Teleporters.

Hinweis: Zur Zeit muss noch per "gm iv" der Teleporter aus dem "privaten Netwerk" entfernt werden und er hat (noch) keine Daten über das Solarsystem (beides wird sich aber noch ändern)