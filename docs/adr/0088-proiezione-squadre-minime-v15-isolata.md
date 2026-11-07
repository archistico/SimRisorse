# ADR 0088 — Squadre minime: produzione su moduli, in proiezione isolata (RC80)

- Data: 06/11/2026.
- Stato: **implementato come laboratorio E4b.2b, non attivato nella campagna v15**.
- Prosegue ADR 0086/0087 e il requisito di 10 addetti minimi per Zecca; non approva i prezzi A/B/C né la rateizzazione.

## Contesto

La vecchia struttura `TieredBuildingPortfolio` assegna al tier Base/Grande/Avanzato rispettivamente 1/3/10 **posti individuali**. Una Zecca da dieci persone per modulo deve invece supportare 10/30/100 persone per 1/3/10 moduli; aumentare semplicemente il numero di posti senza cambiare il pianificatore moltiplicherebbe erroneamente il consumo e la produzione per dieci.

## Scelta di implementazione

Introdurre `CrewTeamPortfolio` per il **modello candidato**, con occupazione separata per tier e staff massimo = `edifici × moduliPerEdificio × CrewSize`. I lavoratori non sono aggregati fra tier diversi per completare una squadra; le frazioni di squadra restano prenotate e il metodo `MissingForNextModule` permette un feedback visivo futuro.

Il `ProductionPlanner` condiviso accetta **un parametro opzionale** `crewSizes` che scala il numero di *moduli* eseguibili, non la ricetta: `min(moduli, floor(assegnati / CrewSize))`. In sua assenza usa esattamente il vecchio percorso 1 lavoratore = 1 esecuzione. La nuova flag diagnostica `InsufficientCrew` distingue il personale insufficiente da `NoWorkers`, `MissingInputs` e `StorageFull`.

`CrewProductionProjection.SimulateSecond` espone questa variante in Core, usando la stessa ricetta della campagna, la stessa priorità, il medesimo algoritmo di consumo e spazio. Controlla copertura completa dell'elenco edifici, assenza di duplicazioni o sovrapposizioni, limiti per tier, scorta entro capienza e **budget di popolazione** comprensivo del personale riservato esterno. Non produce un comando di costruzione, non modifica il checkpoint e non scrive nel DB.

## Rischi e limiti

- È un **tick controfattuale** con input e capienza dichiarati, NON una sessione v15 e NON un benchmark di progressione naturale.
- Il portafoglio corrente 1/3/10 posti non viene convertito o usato implicitamente; un'eventuale integrazione richiederà percorso di persistenza e comandi coerente e scelta esplicita per il significato di Grande/Avanzato.
- UI v14 canonica, regole v15 vigenti, ricerche, prezzi e salvataggi invariati. Non basta l'esito delle proiezioni per affermare che la nuova Zecca sia giocabile.
- Nell'integrazione reale la riserva demografica dovrà contabilizzare scuola, militari, filiere F1 e pause; l'input `reservedOutsideRoster` rappresenta soltanto questo vincolo nel laboratorio.

[Specifica RC80](../design/E4B_RC80_CREW_MINIMUM_PROJECTION.md) · [Test RC80](../validation/E4B_RC80_CREW_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
