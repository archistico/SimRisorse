# ADR 0017 — Caserma, reclutamento per posti e conversione dei salvataggi v9

- **Data:** 2026-11-03
- **Stato:** Accettato; implementato, **da verificare su Windows**
- **Ambito:** M5.3a/b, Core, catalogo Application, SQLite, Desktop, conversioni S1

## Contesto

M5.2 è verificata (260/260). Il [dossier M5.3](../design/M5_3_ARMY_PREFEASIBILITY.md) lasciava aperte le scelte D0.4 e la quantità di Spade per recluta; D0.8 aveva già fissato il consumo delle Spade all'arruolamento. Il 03/11 l'utente ha scelto:

| Tema | Scelta |
| --- | --- |
| Modello di addestramento (D0.4c, D0.4f) | **C — posti per Caserma**: ogni Caserma posseduta addestra **1 recluta alla volta**, **30 secondi validi** ciascuna |
| Prezzi | **A**: Caserma **260 Legno lavorato / 200 Pietra**, **0 addetti**, prerequisito **Forgia** posseduta; **5 Spade per recluta** |
| Alloggio e razioni (D0.4a, D0.4b) | Reclute e soldati **restano residenti**: occupano posti casa e consumano Cibo e Pane come ogni abitante |
| Annullamento e congedo (D0.4d) | **Annullamento sì**: restituisce persona e Spade; chi è già in addestramento perde il tempo svolto. **Congedo rinviato** alle missioni (M5.5) |

Le perdite permanenti (D0.4e) restano fuori da M5.3.

## Decisione

- **Regole `civil-growth-10`**, 20 edifici / 16 risorse. La Caserma non ha ricetta né posti di lavoro; il contratto è un `MilitaryRules` del Core (edificio, risorsa equipaggiamento, Spade per recluta, secondi di addestramento, posti per Caserma).
- **Popolazione unica**: `abitanti = impiegati + liberi + reclute in coda + in addestramento + soldati`. `AvailableWorkers` esclude reclute e soldati; un lavoratore assegnato non viene mai arruolato implicitamente; un soldato non può essere assegnato.
- **Arruolamento atomico** (`Enlist`): verifica Caserma, civili liberi e Spade, poi scala le Spade e mette le persone in coda; i posti liberi vengono occupati subito. Nessuna mutazione parziale sui rifiuti.
- **Addestramento sul tempo simulato**: ogni secondo valido avanza tutti i posti occupati; a 30 s la recluta diventa soldato e il posto riceve subito la prossima in coda. In pausa e a gioco chiuso non avanza nulla. Una Caserma costruita mentre ci sono reclute in coda prende subito una recluta.
- **Annullamento** (`CancelRecruits`): prima la coda, dalle ultime arruolate; poi i posti con meno progresso. Rifiutato senza modifiche se le Spade restituite superano la capacità.
- **Stato normalizzato nello snapshot** (`MilitaryState`): coda, progressi dei posti in ordine decrescente, soldati. Il ripristino rifiuta posti liberi con coda, progressi fuori periodo, ordine errato, più posti delle Caserme e persone oltre la popolazione.
- **SQLite v2 invariato**: tabelle complementari opzionali `military_state` e `military_training`, come `population_growth_fraction`. Un file senza tabelle o senza riga rappresenta regole senza reclutamento.
- **Conversione v9→v10** concatenabile da v1: Caserma a zero ed esercito vuoto; scorte (Spade incluse), personale, persone, tempo e frazioni preservati. Un vecchio checkpoint che dichiara reclute o soldati non viene convertito. Backup dell'originale alla prima scrittura, come per S1.
- Catalogo v9 fissato con `CreateMintForBenchmarks()`. Il benchmark M5.2c resta sul catalogo corrente: la Caserma non vi partecipa e non ne cambia le misure.

## Alternative

- **B — coda unica** (consigliata nel dossier): scartata dall'utente a favore di C, che rende utile possedere più Caserme.
- **A — immediato**: nessuno stato temporale, ma Caserma ridotta a uno sblocco.
- **Alloggio in Caserma**: libererebbe posti casa e farebbe crescere la popolazione arruolando; richiedeva capienze nuove.
- **Annullamento senza rimborso o congedo immediato**: rinviati; il congedo verrà deciso con le missioni.

## Verifica e conseguenze

Nessun SDK .NET era disponibile nell'ambiente di preparazione: **build e test non sono stati eseguiti**. Previsti **308 casi xUnit** (260 + 48 nuovi), la cronologia `military-8-hours` e due render Caserma. Il gate è `validate.cmd` su Windows; [rapporto M5.3a](../validation/M5_3A_BARRACKS.md).

Restano per M5.3c le misure di impatto civile (benchmark contingenti piccoli/grandi). Navi, missioni, perdite e congedo hanno contratti propri.
