# SimRisorse — P1-D50 / RC36: consolidamento Scuola e Ricerca

**Data:** 2026-11-04  
**Stato:** intervento implementato; **gate Windows NON ancora superato**.  
**Ultima suite confermata dall'utente:** RC30 / 511 test. **RC31–RC35:** candidati non ancora validati.  
**RC36:** 538 test cumulativi **attesi** (536 della RC35 + due test SQLite); non compilati né eseguiti in questo ambiente.

## Obiettivo

Consolidare i contratti della fase P1 senza cambiare il gameplay. La GUI v14 è il percorso ordinario (`run-game.cmd`); la v13 è legacy (`--legacy-v13`) con database separato. Le 44 prove storiche P0 non sono un requisito nuovamente imposto dopo l'approvazione esplicita della v14; la validazione cumulativa dei test resta invece richiesta per chiudere P1.

## Audit delle invarianti

| Area | Contratto attuale | Evidenza / residuo |
| --- | --- | --- |
| Scuola | Una formazione per posto; coda e progressi individuali; il diplomato rimane qualificato ma non è assegnato automaticamente | RC32, RC35; test cumulativi da eseguire |
| Iscrizione | Una persona viene riservata; consumo iniziale 10 Cibo + 2 Matite | RC33 e RC35; quantità economiche **candidate**, requisito minimo di due Matite confermato |
| Laboratorio | Ricetta esplicita 1 Legno lavorato + 1 Carbone → 4 Matite; lotto atomico e capienza necessaria | RC33; automatizzazione/edificio dedicato **non approvati** |
| Annullamento | Prima studenti in attesa, poi ultimi in formazione; rimborsare le Matite solo a chi le ha effettivamente pagate | Test precedenti + nuova prova RC36 con **SQLite storico e studente esistente** |
| Ricerca | Punti accumulati soltanto da ricercatori assegnati, 0,1 punti/s per ricercatore; acquisti volontari e saldo in millipunti | RC35; coefficienti candidati, precisione interna preservata |
| Tecnologie | Otto acquistabili con beneficio reale Grande/Avanzato per 16 famiglie; tre voci Logistica/Navigazione non implementate | RC34, RC35; non sbloccarle implicitamente |
| Persistenza | SQLite v14 distinto dal salvataggio v13; caricamento in pausa; vecchi schemi senza colonna Matite pagate restano leggibili | RC35 + due prove RC36 (compatibilità e rifiuto checkpoint invalido) |
| Popolazione | Studenti, ricercatori, lavoratori e militari condividono il limite degli abitanti; nessuna duplicazione | RC35 + controlli Core; test Windows pendenti |

## Interventi RC36

1. `tests/SimRisorse.Infrastructure.Tests/V14SchoolPencilPersistenceTests.cs`: **due test aggiuntivi**, senza modifiche al motore.
   - Simulazione di un database precedente alle Matite con uno studente già in formazione, eliminazione fisica della colonna `pencil_paid_students`, caricamento, annullamento, verifica del solo rimborso di Cibo e nuova scrittura del file.
   - Rifiuto di `PencilPaidStudents=1` in assenza di studenti; verifica che la copia SQLite valida non sia sovrascritta.
2. Preparato un protocollo E4a riproducibile con registro dei casi, campi obbligatori, checkpoint e rischi di stallo. Non sono stati inventati tempi di progressione.
3. Aggiornati roadmap, stato, changelog e handoff.

**Non modificati:** `src/`, `.axaml`, formule, prezzi, tempi, salvataggi dell'utente, database schema, avvio v14 e modalità v13 legacy.

## Decisioni confermate e proposte

| Aspetto | Stato | Valore attuale / decisione |
| --- | --- | --- |
| Studente richiede Matite | **Confermato** | Almeno 2 per studente; oggi ne consuma 2 |
| Iscrizione usa Cibo | **Implementato, bilanciabile** | 10 Cibo |
| Durata formazione | **Implementato, bilanciabile** | 120 secondi per posto |
| Ricerca per ricercatore | **Implementato, bilanciabile** | 0,1 punti/s |
| Produzione Matite | **Implementata, bilanciabile** | 1 Legno lavorato + 1 Carbone → 4 Matite |
| Fabbricazione | **Implementata, scelta UX per ora conservata** | Manuale nella Scuola; automatica/nuovo edificio rinviati |
| Lavoratori dei tre tier | **Confermato** | Base 1 / Grande 3 / Avanzato 10 posti per struttura |
| Prezzi per acquisto successivo | **Da analizzare in E4a** | Coefficiente attuale +15% per famiglia |
| Obiettivo di durata | **Obiettivo di design, non risultato** | ~8 ore strategia efficiente; >32 ore percorsi più lenti, da verificare nel gioco completo |

## Gate di P1 — condizioni di chiusura

- `./validate.cmd` termina con **538/538 test** sullo ZIP RC36 (o risultato successivo documentato), senza errori di build e senza test ignorati non motivati.
- La v14 parte senza argomenti; non viene alterato il salvataggio legacy v13.
- Aprire una copia di salvataggio v14, verificare studente in corso, Matite, ricerca, ricerca acquistabile e checkpoint dopo Salva/Carica.
- Nessuna tecnologia acquistabile priva di beneficio; Logistica/Navigazione restano non acquistabili.
- Per ogni test fallito, correzione specifica prima di dichiarare il gate verde. **Finché l'utente non può validare, P1 resta aperta anche se la documentazione e i test sono pronti.**

## Prossima fase E4a

Fare **misurazioni, non modificare coefficienti**. Usare [protocollo benchmark E4a](E4A_PROTOCOLLO_BENCHMARK.md) e [registro scenari](../../benchmarks/e4a/scenarios.csv). Separare sempre `natural` (risorse iniziali reali) e `controlled` (fixture finanziata dichiarata) e registrare il tempo **simulato**, non il tempo di esecuzione dei test. Se un traguardo non viene raggiunto, registrarlo come `blocked`/`timeout` con una causa, non come zero secondi. La ricerca di una strategia ottima non è garantita dal test automatico.

[Roadmap](../ROADMAP.md) · [Piano test RC36](../validation/P1_D50_RC36_TEST_PLAN.md) · [ADR 0047](../adr/0047-gate-p1-e-misure-e4a.md)
