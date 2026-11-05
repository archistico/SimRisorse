# ADR 0073 — Comandi civici nativi per la campagna v15

**Stato:** accettata come decisione architetturale per RC65, implementazione candidata da validare.

## Contesto

La GUI v15 (ripristinata in RC63) richiama comandi di acquisto, upgrade, personale, Scuola, laboratorio e ricerca. In RC64 ogni invocazione ricostruiva una `V14CampaignSession` con `food-store` temporaneo, poi rimuoveva lo slot. Questo modello nascondeva due responsabilità: adattamento al ruleset e validazione della nuova partita.

## Decisione

Le operazioni v15 compongono **direttamente** un nuovo `V15FoodCampaignCheckpoint` e usano un unico commit atomico. Non si crea più `V14CampaignSession` per le azioni civiche. Le regole pure già verificate (`TieredConstruction`, prezzi progressivi, Scuola, Matite, tecnologie, capacità) rimangono riutilizzate; nessun duplicato di coefficienti o ricette. Le 15 famiglie vengono indicizzate una sola volta nel costruttore della sessione. `food-store` resta non acquistabile né assegnabile.

## Invarianti

- Rifiuto di un'azione: stesso oggetto `Snapshot`, costi non addebitati, nessun worker creato o perso.
- Le spese hanno un'unica sorgente: le risorse civiche. I rimborsi di Cibo restano nel Deposito.
- La cronologia delle nuove costruzioni cresce; gli upgrade non aumentano gli acquisti storici.
- Il budget lavoratori include altre industrie, filiere, Scuola ed esercito.
- Lo schema SQLite generation 6 e i file Desktop (GUI validata) rimangono identici a RC64.

## Limiti e prossima fase

`CityCampaign` usa ancora `V14CampaignSnapshot`; `V15FoodCampaignCheckpoints.Validate` e il planner ricorrono allo slot storico **solo in memoria**. Il contatore storico resta, in attesa di una nuova struttura/validazione v15 nativa e del relativo aggiornamento della persistenza; questo ADR **non** pretende di aver eliminato l'intera eredità v14. Criterio di accettazione: tutti i 784 test RC65 su .NET 10 e la parità visiva già approvata.

## Alternative escluse

Riderefinire costi/ricette durante il refactoring, rimuovere subito tutte le dipendenze legacy senza uno scenario di confronto, o ridisegnare la GUI: espongono a regressioni non pertinenti.
