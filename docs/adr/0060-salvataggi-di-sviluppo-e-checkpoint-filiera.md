# ADR 0060 — Evoluzione libera dei salvataggi di sviluppo e checkpoint unico delle filiere

**5 novembre 2026. Stato: accettato come direttiva di sviluppo, implementazione RC49 candidata.**

## Problema

La strategia di migrazione conservativa dei primi checkpoint F1 aveva introdotto complessità e passi intermedi che rallentano la realizzazione delle nuove meccaniche. L'utente ha chiarito che siamo ancora in sviluppo e che **non è necessario mantenere validi tutti i vecchi database**.

## Decisione

- La compatibilità retroattiva dei salvataggi **non è un requisito** prima di una futura release stabile. Le versioni di sviluppo possono interromperla, purché il cambiamento sia dichiarato e un vecchio file non venga letto come se fosse nuovo.
- RC49 estende il checkpoint alimentare con `Animals`, `Transit` e `Buildings`; si usano tre tabelle dedicate in **un solo database SQLite**. Si porta `user_version`/`snapshot_version` da 4 a **5**, lasciando `rules_version=civil-growth-15` fino all'integrazione gameplay.
- Le sei strutture alimentari persistono **conteggi e lavoratori**; i posti derivano dalle strutture. In RC49 un edificio base corrisponde provvisoriamente a un posto: il bilanciamento finale sarà oggetto delle prossime fasi.
- Tutto il personale concorre al medesimo budget di popolazione con lavoratori legacy, scuola e militari. Inventari intermedi e del deposito hanno proprietà esclusiva, valori interi al millesimo e non perdono prodotti a causa della saturazione.
- Il ponte di simulazione `V15FoodCampaignTicks` è puro e non avanza l'orologio; il runtime giocabile resta separato fino a F1-D05d.3.

## Conseguenze

- Il formato sperimentale `user_version=4` di RC46–RC48 è **incompatibile** con il writer RC49. I test di sviluppo possono partire da database nuovi, non va promessa alcuna migrazione automatica.
- Le API storiche v14→v15 rimangono nel repository come supporto diagnostico/regressione, ma non sono la strategia obbligatoria per attivare il gioco. I vecchi ADR [0057](0057-sqlite-v15-separato-per-deposito-alimentare.md), [0058](0058-migrazione-v14-v15-senza-sovrascrittura.md) e [0059](0059-confini-produzione-alimentare-e-proprieta-stock.md) **sono superati esclusivamente** nei vincoli che impongono nuove migrazioni e compatibilità con versioni di sviluppo; restano valide le decisioni su inventari esclusivi, atomicità, verifiche e non duplicazione del lavoro.
- È permesso sostituire definitivamente gli strati legacy quando la nuova sessione sarà completa; **non** bisogna dichiarare giocabile RC49.

[Contratto](../design/F1_D05D2_RC49_CHECKPOINT_FILIERA_SQLITE5.md) · [Test](../validation/F1_D05D2_RC49_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
