# ADR 0015 — Riallineamento del reclutamento individuale

**Stato: proposta implementata in v58-RC2, in attesa di validazione e ratifica della migrazione.** Data: 3 novembre 2026.

## Contesto

Una modifica fondamentale è stata sviluppata sul ramo v41 dopo che il ramo ufficiale era avanzato a v57. Lo schema SQLite e la stringa `civil-growth-10` hanno due interpretazioni diverse: gruppi da 2/60 s nel ramo v57 storico e addestramento individuale da 1/30 s nel ramo modificato. La sostituzione di sorgenti perderebbe Municipio, capacità e interfaccia.

## Soluzione

- La v57 rimane base. Il nuovo contratto vive in `civil-growth-13`, con 260 Legno lavorato e 200 Pietra per Caserma, 5 Spade per recluta, un posto individuale ogni 30 s, cancellazione anche durante addestramento, congedo disabilitato. Mantenute `civil-growth-10`, `-11`, `-12` e relative factory senza reinterpretazione.
- Stato di addestramento individuale come array ordinato di progressi; versione storica distinta tramite array non inizializzato. Nessuna perdita di soldati o reclute nella migrazione.
- `12→13`: i posti esistenti si riducono di metà; la quota di addestramento conservata è `floor(oldProgress/2)` per le persone rimaste nei posti. Eccedenti in coda senza avanzamento; niente nuovo premio o duplicazione di risorse.
- Fork v41: riconoscimento tramite nomi reali delle colonne SQLite e progressi individuali; upgrade diretto a v13 più Municipio. Backup S1 prima della prima scrittura. File SQLite v2 invariato, nuova tabella `military_training` per la v13.
- Preservare UI militare v57, descrizioni, capacità e tutte le aggiunte al magazzino; via di evoluzione a M5.4 aperta solo dopo gate Windows.

## Conseguenze

- Maggiore costo di 2,5× Spade per nuovo soldato, ma capacità teorica di addestramento invariata (2 soldati/min per Caserma) e primo soldato dopo 30 s.
- Vecchie partite possono riprendere, ma l'eventuale progresso degli apprendisti oltre i nuovi posti non può essere salvato individualmente senza una meccanica più complessa: conversione dichiarata e backup indispensabili.
- Rischio di conflitti schema gestito esplicitamente; conversioni v41 e v12 devono passare test su DB e successivo gate `validate.cmd`.

**Nota economica:** le reclute arruolate prima della conversione non pagano retroattivamente la differenza da 2 a 5 Spade. Le 5 Spade si applicano esclusivamente ai nuovi arruolamenti del ruleset v13; nessuna spesa nascosta durante il caricamento.
