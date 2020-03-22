# ActiveRadar

1. Einen Radar Deko Block (ID:289) aufstellen
1. Per 'di' dessen Position ermitteln
1. Ein LCD aufstellen und wie folgt benennen (Hinweis: Dabei sind die LCDSize und ScrollLines optional)
```
Scan=[X,Y,Z]=[LCDSize]=[ScrollLines]
```
also z.B.
wenn der Radarblock auf 1,137,0 liegt, das LCD eine Schriftgröße von 50% haben soll und ab 100 Zeilen scrollt.
```
Scan=1,137,0=50%=100
```

# EnemyScan

wie oben nur das eine Antenne (IDs:1877,1878,1879,1880,1880,1881,1882,1883,1884) benötigt wird und das LCD mit 'EnemyScan=...' beginnen muss
