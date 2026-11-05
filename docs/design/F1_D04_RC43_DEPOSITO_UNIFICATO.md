# F1-D04 / RC43 — Deposito alimentare unificato: ciclo e razioni

**Data:** 2026-11-04. **Baseline:** RC42, validata dall'utente con 577 test e Salva funzionante. **Candidata:** RC43, 590 test attesi; xUnit e compilazione .NET 10 non eseguiti in questo ambiente.

## Risultato tecnico

La RC43 aggiunge al Core `src/SimRisorse.Core/Food/UnifiedFoodSupply.cs`. Il modello riunisce esplicitamente le cinque risorse finali alimentari:

| Input | Provenienza | Destinazione |
| --- | --- | --- |
| `fish` | Pescatore | Deposito alimentare → `food` |
| `bread` | Forno | Deposito alimentare → `food` **solo eccedenza rispetto alla riserva** |
| `meat` | Stalla/Cacciatore → Macellaio | Deposito alimentare → `food` |
| `fruit` | Frutteto | Deposito alimentare → `food` |
| `vegetables` | Orto | Deposito alimentare → `food` |

**Bovini e Selvaggina non sono Cibo:** il Macellaio deve prima produrre `meat`. Gli input sono un insieme **chiuso**: ricette mancanti, sconosciute o duplicate vengono respinte. Le priorità e rese per lotto sono configurabili e non rappresentano bilanciamento approvato.

### Un tick deterministico

Il nuovo `UnifiedFoodSupply.PlanTick` esegue un ciclo puro, senza scrivere la campagna o un DB:

1. **Ingresso:** da inventario cittadino temporaneo, trasferisce i cinque alimenti grezzi ai contenitori del deposito fino alla capacità disponibile per ciascuna risorsa. Il trasferimento è registrato e sottratto dallo stock di origine, senza duplicazione; materiali non alimentari rimangono nell'inventario di origine.
2. **Lavorazione:** usa `FoodDepotPlanner.Plan` e il budget di lotti per lavoratori, rispettando priorità, ricette e capacità del Cibo pronto. Nessun lavoratore significa nessuna lavorazione. Le scorte eventualmente già oltre capienza per migrazione non vengono eliminate e non accettano nuove consegne finché non si libera spazio.
3. **Razioni:** usa `FoodDepotMeals.Serve` per richiedere insieme il Cibo ordinario e la razione aggiuntiva di Pane. Se manca uno dei due, il consumo della razione è rifiutato in modo atomico; le scorte non vengono decurtate parzialmente. Le unità di Pane lavorate in Cibo non sono più disponibili come Pane.

La riserva effettiva per la conversione è almeno il maggiore fra la scorta minima protetta impostata e la razione Pane corrente. Si evitano quindi doppi consumi. **Nota:** il controllo attuale di crescita della popolazione rimane invariato; F1-D05 dovrà usare questo esito per aggiornare la progressione in maniera coerente con l'ordine dei tick.

### Contratto a prova di perdita dati

- `food` non può comparire contemporaneamente come scorta spendibile cittadina e come `FoodDepotStock.ReadyFood`. Il coordinatore rifiuta questo stato, imponendo una migrazione esplicita.
- `ResourceBundle` e `FoodDepotStock` sono trattati in modo immutabile: il risultato contiene l'inventario cittadino residuo, la scorta finale del deposito, quantità trasferite, lavorazioni e razioni.
- Consegne parziali per una risorsa sono ammesse solo fino alla sua capacità, mantenendo nello stock di origine la quantità non accettata. Nessuna risorsa viene scartata.
- Gli stock reali di RC42 non vengono convertiti automaticamente. I numeri delle ricette nei test sono **fixture illustrative**, non parametri economici definitivi.

## Copertura aggiunta

`tests/SimRisorse.Core.Tests/UnifiedFoodSupplyTests.cs` contiene 13 prove: catalogo completo cinque alimenti, flusso di ciascuno, pane riservato, mancanza lavoratori, rifiuto delle razioni incomplete, saturazione Cibo pronto, capienza ingredienti, stock storico sovraccarico, prevenzione doppio inventario, determinismo, filiera Orto/Frutteto con deposito e filiera Stalla/Caccia/Macellaio con deposito.

**RC43 attesi: 590 test (577 + 13).** In mancanza dell'SDK .NET 10 non esistono risultati di esecuzione locali da dichiarare.

## Cosa NON è stato attivato

- **Non** è stato modificato `V14CampaignClock`, `V14CampaignSession`, `PopulationPlanner` o `ProductionPlanner`: la partita attuale continua a usare il modello RC42.
- **Non** sono cambiati catalogo edifici attivi, risorse della GUI, schermate pixel art, salvataggi, versione regole né database SQLite.
- Stalla, Cacciatore, Macellaio, Orto e Frutteto non sono ancora costruibili nella partita attuale.
- Manca l'aggancio dei lavoratori reali del deposito ai livelli Base/Grande/Avanzato, la diagnostica UI e il consumo della popolazione nella sessione. Questi sono compiti **bloccanti** di F1-D05, non funzionalità già completate.

## Prossimo gate F1-D05

1. Progettare nuova versione delle regole e schema SQLite con backup **prima** di qualsiasi migrazione; importare una sola volta le scorte storiche `fish`, `bread`, `food` e conservare eventuali eccedenze in sovracapacità senza scartarne alcuna.
2. Ampliare catalogo risorse/edifici/tier/tecnologie e collegare le due origini di Carne, Orto/Frutteto e il deposito al tick della sessione v14. Tenere la v13 legacy e i suoi salvataggi separati.
3. Fare derivare capacità e throughput dai depositi costruiti e dai lavoratori realmente assegnati; preservare la logica di riserva del Pane e l'ordine deterministico dei tick.
4. Mostrare nella **stessa GUI v14** scorte grezze, Cibo pronto, capienze, lavoratori e colli di bottiglia senza modificare le sette colonne della scheda Risorse né introdurre combobox di livello.
5. Aggiungere test integrati di salvataggio/ripresa, casi storici, capienze, razioni e benchmark naturali; confrontare prima/dopo soltanto con metriche misurate.

**F1-D04 è una preparazione verificabile del nuovo ciclo, non una release di gameplay alimentare attivo.** Non avviare E4b prima di F1-D05 e nuove misurazioni E4a.
