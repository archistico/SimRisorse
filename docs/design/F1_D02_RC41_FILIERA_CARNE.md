# F1-D02 / RC41 — Due filiere della Carne, nucleo Core

Data: 2026-11-04. **Stato: dominio isolato implementato; gameplay NON attivato.**

## Punto di partenza e obiettivo

La RC40 (552 test confermati superati dall'utente) contiene `FoodDepotModel.cs`, un piano di trasformazione del cibo separato dalla partita. Per non lasciare la Carne come un'unica risorsa prodotta magicamente, F1-D02 introduce in Core un secondo piano puro, `MeatChainModel.cs`. Il Macellaio può ricevere **Bovini** (`cattle`) o **Selvaggina** (`game`) e produrre **Carne** (`meat`) con rese diverse, senza scelte tramite combobox. Solo il deposito, in una successiva fase, potrà trasformare Carne in Cibo pronto (`food`).

| Filiera | Edificio | Ingresso per lotto | Uscita per lotto | Disponibilità |
| --- | --- | --- | --- | --- |
| Allevamento | Stalla | Grano (`grain`) + Acqua (`water`), entrambi obbligatori | Bovini (`cattle`) | Modello di dominio, non nel catalogo attivo |
| Caccia | Cacciatore | Nessuno | Selvaggina (`game`) | Modello di dominio, non nel catalogo attivo |
| Macellazione A | Macellaio | Bovini | Carne (`meat`) | Ricetta alternativa configurabile |
| Macellazione B | Macellaio | Selvaggina | Carne (`meat`) | Ricetta alternativa configurabile |
| Alimentazione | Deposito alimentare | Carne (assieme agli altri cibi finali) | Cibo pronto | Planner F1-D01, integrazione F1-D04 |

## Contratto operativo del planner

- `MeatChainPolicy`: identificativi delle **cinque** risorse, costo per lotto della Stalla (obbligatori Grano e Acqua), Bovini per lotto, Selvaggina per lotto di Caccia, due ricette obbligatorie del Macellaio. Tutti i valori sono parametri **senza impostazioni economiche definitive**.
- `MeatChainPlanner.PlanTick`: input immutabile, capienze per le tre risorse prodotte, lavoratori dedicati ai tre edifici. Esegue **lotti interi**, limitati da scorte, personale e capienza. Nessun consumo se un lotto non è eseguibile.
- Il Macellaio usa un unico budget di lavoratori per le due ricette: non può assegnare lo stesso lavoratore contemporaneamente a entrambe. Scelta ripetibile tramite `priority`, spareggio ID `Ordinal`.
- Non è possibile macellare in un tick gli animali prodotti nello **stesso** tick. I nuovi Bovini e la nuova Selvaggina diventano disponibili dal tick successivo: un passaggio produttivo ha sempre una fase di latenza.
- Gli output rimangono `cattle`, `game` e `meat`. **Carne non equivale a Cibo pronto** e non viene contabilizzata come razione della popolazione. `FoodDepotPlanner.Receive` + `Plan` è l'unico percorso di conversione previsto dopo l'integrazione F1-D04.
- Le risorse della partita non possono essere modificate eseguendo il nuovo planner: la RC41 non lo richiama da `V14CampaignSession` né tocca SQLite o il catalogo v14.

## Decisioni progettuali per l'integrazione

1. **IDs candidati:** `cattle` Bovini, `game` Selvaggina, `meat` Carne; edifici proposti `stable` Stalla, `hunter` Cacciatore e `butcher` Macellaio. Nessuno di questi viene ancora aggiunto a `GameCatalog`, alle 17 righe Risorse o ai salvataggi.
2. **Nessun prerequisito Stalla obbligatorio per Macellaio:** la via Caccia deve rimanere percorribile senza Allevamento. Per la Stalla servono produzioni effettive di Grano e Acqua, non necessariamente l'obbligo di possedere specifici edifici se la colonia dispone già degli input; la convenienza economica verrà misurata in E4a.
3. **Tier Base/Grande/Avanzato:** riutilizzare il sistema già esistente, con lavoratori 1/3/10 dove previsto; associare gli sblocchi avanzati a tecnologie realmente efficaci (es. Alimentazione I/II) dopo la revisione del catalogo. Nessuna nuova tecnologia senza effetto.
4. **Resa allevamento:** dovrà poter superare la Caccia a regime ma richiede filiere di Grano e Acqua, investimento, personale e capacità di Macellaio e Deposito. E4a misurerà **Carne pronta netta / tempo / lavoratori / input**, non solo animali per tick.
5. **Priorità Macellaio:** dipende dal catalogo/strategia futura; nell'attuale planner è parametro esplicito, senza selettore nell'interfaccia. Niente assegnazioni doppie.
6. **Migrazione:** unire le nuove risorse, stoccaggi, personale e salvataggi solo con regole versionate e migrazione SQLite testata; nessuna conversione silenziosa della v14 già giocata.

## Valori sperimentali (solo nei test)

Alcuni test illustrano `1 Grano + 1 Acqua → 2 Bovini`, `1 ciclo Cacciatore → 1 Selvaggina`, `1 Bovino → 3 Carne` e `1 Selvaggina → 1 Carne`. **Non sono valori approvati né applicati nel gioco**, sono fixture per provare l'alternanza delle ricette e il limite dei lavoratori. I coefficienti definitivi richiedono dati E4a e approvazione E4b.

## Gate

**F1-D02 è implementata a livello di dominio**, ma non è una nuova filiera giocabile. RC41 candidata: **563 test attesi (552 + 11)**; il test Windows non è stato eseguito qui. F1-D03 introduce Orto/Frutteto nel modello; F1-D04 collega il deposito e il consumo; F1-D05 realizza versionamento/migrazione e integrazione completa di GUI, tier, prerequisiti e salvataggi prima di abilitare le nuove filiere.

[Roadmap](../ROADMAP.md) · [Piano RC41](../validation/F1_D02_RC41_TEST_PLAN.md) · [ADR 0051](../adr/0051-due-origini-carne-e-priorita-macellaio.md)
