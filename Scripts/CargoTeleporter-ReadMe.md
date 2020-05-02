# Cargo Teleporter zwischen zwei Strukturen für 'normale' Spieler

1. Beide Strukturen müssen in der selben Fraktion sein
1. Eine Box/Container mit der Id "CargoOut@\[ZielID\]" beim Versender
1. An der Zielstruktur mit der Id ID eine Boc/Container mit dem Namen "CargoIn"

Der Transfer erfolgt immer von "CargoOut@..." nach "CargoIn"

Infoformationen zum Transfer erhält man über LCDs welche mit den Namen "CargoOutInfo" bzw. "CargoInInfo" beginnen.

