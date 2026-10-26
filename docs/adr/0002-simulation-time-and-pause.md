# 0002 — Tempo di simulazione, pausa e riproducibilità

- Stato: Accettato
- Data: 2026-10-26
- Ambito: avanzamento della partita e risultati
- Origine: pausa e assenza di progressione a gioco chiuso confermate dall'utente; riproducibilità adottata come requisito tecnico

## Contesto

Il tempo di completamento determina la classifica. Nella bozza ogni edificio legge il tempo del calendario e ricalcola quantità dalla propria creazione: questo non rappresenta correttamente consumi, acquisti e pause.

## Decisione

Usare un tempo di gioco esplicito, avanzato dal motore e conservato nel salvataggio. La pausa e l'assenza di gioco non generano produzione o avanzamento di costruzioni, addestramenti e missioni. Al caricamento non si recupera il tempo trascorso dal salvataggio.

Il Core riceve l'avanzamento temporale e non legge direttamente l'orologio del sistema. A parità di stato iniziale, versione delle regole e cronologia delle azioni, la simulazione deve produrre lo stesso risultato.

L'obiettivo di otto ore riguarda tempo di gioco. Non sono ancora decisi velocità 1:1, disponibilità degli ordini in pausa, comportamento alla minimizzazione e formato del passo di simulazione; vedere Q-04 nel game design.

## Alternative

- Tempo del calendario come misura della partita: conterebbe anche le interruzioni.
- Produzione offline: contrasta con il requisito di fermare il gioco quando non si gioca.
- Avanzamento direttamente dal timer grafico: lega le regole alla frequenza di aggiornamento della UI.

## Conseguenze

Servono una cronologia coerente delle azioni e politiche esplicite per ordine delle operazioni e arrotondamenti. Un passo fisso può aiutare, ma da solo non assicura risultati riproducibili.

È possibile eseguire molte ore simulate senza attendere ore reali negli strumenti di verifica. Questa capacità tecnica non decide se offrire al giocatore velocità accelerate.

## Verifica

Controllare pausa e ripresa, caricamento dopo una lunga chiusura, sospensione del computer e avanzamento senza UI. Confrontare esecuzioni con identici input, comprese le frontiere tra un passo e il successivo.

## Collegamenti

- [Game design](../GAME_DESIGN.md).
- [Architettura](../ARCHITECTURE.md).
- [Game Loop, Game Programming Patterns](https://gameprogrammingpatterns.com/game-loop.html).
