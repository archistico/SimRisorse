# M4/M5 — Valutazione della filiera della carne

Data: 2026-10-27. **Stato: analisi, nessuna nuova regola approvata o implementata.** Non modificare `civil-growth-4`, i salvataggi o il catalogo sulla base di questo documento senza una decisione esplicita.

## Evidenza del materiale di riferimento

L'[analisi delle filiere originali](../references/RESOURCE_AND_PRODUCTION_CHAINS.md) mostra due percorsi distinti:

1. **Cacciatore → Carne**.
2. **Cereali + Acqua → Allevamento di maiali → Carne**.

Carne, Pane e Pesce confluiscono graficamente verso depositi alimentari e verso Cibo. Le immagini **non chiariscono** se siano tre ricette alternative, ingredienti simultanei o risorse che i depositi si limitano a raccogliere. Non è corretto trasformare automaticamente il diagramma in una ricetta del motore.

## Problema nelle regole attuali

In `civil-growth-4` il Deposito alimentare ha una ricetta unica: **1 Pesce → 1 Cibo/s**. Due coppie Pescatore + Deposito costano **200 legno lavorato + 100 pietra**, occupano **4 lavoratori** e producono **2 Cibo/s**, sufficienti per 200 abitanti al consumo di 0,010 Cibo/s. Per i 136 abitanti del confronto M4, questa soluzione è già semplice e raggiungibile.

Il Pane ha un ruolo non intercambiabile: alimenta i residenti avanzati in aggiunta al Cibo. Introdurre Carne come semplice duplicato del Pesce non crea automaticamente una scelta più interessante. Potrebbe aggiungere edifici che il giocatore costruisce una volta sola per completezza e poi non utilizza.

## Alternative architetturali

| Scelta | Modifica necessaria | Vantaggi | Rischi |
| --- | --- | --- | --- |
| **A. Cacciatore + nuovo edificio di lavorazione** (Carne → Cibo) | Due strutture e una risorsa, senza cambiare il motore | Filiera autonoma e semplice da testare | Se costi, personale e resa sono peggiori del Pesce, la strategia è dominata |
| **B. Carne come ricetta alternativa del Deposito alimentare** | Gestione di più ricette e scelta di quale eseguire per edificio | Riutilizza il Deposito e rende visibili le alternative | Nuova complessità in priorità, automazione, assegnazioni, UI e salvataggi se si persiste la selezione |
| **C. Allevamento alimentato da Grano + Acqua** | Nuove strutture e contesa con la filiera Pane | Introduce un compromesso fra consumi delle filiere | Senza rendimenti ben distinti può essere una catena costosa ma inutile |
| **D. Carne come vettovagliamento per spedizioni/soldati** | Requisiti separati dalle razioni civili; da progettare con M5 | Ruolo differenziato, legato agli obiettivi futuri | Prematuro senza regole di reclutamento, missione e consumo |

La scelta B è più invasiva di quanto sembri. L'attuale `BuildingDefinition` possiede al massimo una `Recipe` e il pianificatore non ha uno stato di ricetta selezionata. Non si deve introdurre un elenco di ricette senza definire cosa accade quando due input sono disponibili, quando le scorte cambiano o dopo il caricamento della partita.

## Quando diventerebbe una scelta reale?

Per una filiera civile che produce Cibo, serve almeno un trade-off verificabile:

- **Materiali diversi:** il Cacciatore potrebbe richiedere più legno lavorato ma meno pietra del Pescatore, a parità di rendimento e lavoratori. Con una cava limitata, la scelta avrebbe un significato.
- **Costo iniziale contro personale:** un Allevamento potrebbe costare molto all'inizio ma rendere più Cibo per lavoratore; servono però misure sul periodo di recupero dell'investimento.
- **Risorse condivise:** Grano e Acqua potrebbero andare al Forno oppure all'Allevamento. La contesa è interessante solo se non provoca blocchi incomprensibili e se è spiegata dalla UI.
- **Utilità non alimentare:** la Carne potrebbe soddisfare una richiesta militare o di navigazione, distinta dal Cibo ordinario. È un'opzione per M5, non una regola già approvata.

Non fissare ora prezzi esatti o moltiplicatori di produzione: manca la funzione che questi valori dovrebbero bilanciare. Una valutazione rigorosa confronterà almeno Cibo/secondo per lavoratore, legno e pietra spesi per 1 Cibo/secondo, scorte piene, concorrenza con il Pane e tempi per 136 abitanti.

## Raccomandazione prima del prossimo incremento

**Non aggiungere Carne al catalogo `civil-growth-4` durante l'attesa della validazione M4.8.** L'utente ha riferito superato il gate **v17**, inclusi diagnostica quantitativa e quattro strategie Pane (163 test xUnit attesi; report grezzi non allegati). Rimane da verificare il nuovo stress **v18** da 320 abitanti. Il dato importante è che Cibo da Pesce e Pane per la crescita avanzata coprono già gli obiettivi civili studiati; il ruolo della Carne va valutato nel [dossier D0 pre-M5](../design/PRE_M5_DECISION_GATE.md), insieme al vettovagliamento militare.

La decisione successiva dovrebbe essere una delle seguenti:

1. **M4, alternativa economica:** approvare una seconda filiera Carne → Cibo con un trade-off sui materiali, mantenendo ricette semplici e una nuova versione delle regole.
2. **M5, funzione distinta:** rinviare Carne fino alla progettazione di soldati, navigazione e vettovagliamento, evitando una risorsa priva di uno scopo specifico.

La seconda è la raccomandazione tecnica attuale, **non una decisione presa al posto dell'utente**. Nessuna di queste alternative deve essere considerata parte della milestone finché non sarà selezionata e dimensionata.
