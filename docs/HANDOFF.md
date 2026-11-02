# Handoff

Aggiornato il **2026-11-03**, checkpoint **v41 / M5.3a/b implementata — da verificare su Windows**. [Rapporto M5.3a](validation/M5_3A_BARRACKS.md), [ADR 0017](adr/0017-barracks-recruitment-and-v9-save-upgrade.md), [storico](HANDOFF_HISTORY.md), [roadmap](ROADMAP.md).

## Primo passo obbligatorio

Eseguire `.\validate.cmd` su Windows. Il codice v41 è stato scritto **senza SDK .NET disponibile**: nessuna build né test eseguiti. Attesi **308 casi xUnit** (Core 77, Application 125, Infrastructure 43, Desktop 63), zero avvisi/errori, **24 cronologie** più crescita accelerata (nuova `military-8-hours`), sei benchmark, **35 PNG** (33 richiesti, inclusi `barracks-buildings*.png`, più due dettagli) e smoke dei due scenari. Correggere eventuali errori di compilazione o test prima di qualsiasi nuovo contenuto, poi registrare run, conteggi e misure nel rapporto.

Ultimo gate effettivamente verificato: **v40 / M5.2c, 260/260**, run `1c74597068ab4f96b4f6fd6d30518b45`.

## Contratto M5.3 scelto dall'utente il 03/11

- Modello **C — posti per Caserma**: ogni Caserma addestra **1 recluta alla volta**, **30 secondi validi** ciascuna; più Caserme in parallelo.
- Prezzi **A**: Caserma **260 Legno lavorato / 200 Pietra**, **0 addetti**, prerequisito **Forgia**; **5 Spade per recluta**, spese all'arruolamento.
- Reclute e soldati **restano residenti**: occupano case, consumano Cibo e Pane come ogni abitante.
- **Annullamento sì**: persona e Spade restituite; prima la coda (ultime arruolate), poi i posti con meno progresso, il cui tempo si perde. Rifiutato se le Spade non entrano nel deposito.
- **Congedo rinviato** alle missioni; perdite permanenti (D0.4e) **non decise**. Non riaprire le altre scelte.

## Implementato in v41

- **Core:** `MilitaryRules`, `MilitaryState` (coda, progressi decrescenti, soldati, uguaglianza per valore), `GameSession.Enlist`/`CancelRecruits`, addestramento a ogni secondo valido dopo produzione e crescita, `AvailableWorkers` che esclude reclute e soldati, `TrainingPlaces`, validazione dei checkpoint normalizzati. Errori nuovi `MilitaryUnavailable`, `InsufficientRecruits`, `InsufficientStorage`.
- **Application:** regole **`civil-growth-10`**, 20 edifici/16 risorse; conversione v9→v10 concatenata (esercito vuoto, Spade preservate, rifiuto di vecchi checkpoint con reclute/soldati); `CreateMintForBenchmarks()` fissa v9. Comandi `EnlistRecruits`/`CancelRecruits`; `MilitaryView` con conteggi, posti, tempi residui, massimi arruolabili/annullabili e previsione del completamento.
- **SQLite v2 invariato:** tabelle complementari `military_state`/`military_training`; righe sostituite a ogni scrittura; backup dell'originale come per S1.
- **Desktop:** Caserma nell'elenco Edifici con stato `posti in addestramento · soldati`, sezione RECLUTAMENTO, costo per 1/5/10 reclute, pulsanti **Annulla −N / Arruola +N** al posto dei lavoratori; icone originali `barracks`, `recruits`, `soldiers`; reclute e soldati nel tooltip della popolazione.
- **Simulatore/validatore:** strategia `military` (dopo `crafting`, Caserma appena acquistabile, fino a 20 reclute; tempi misurati), cronologia `military-8-hours`, controlli delle regole su `civil-growth-10`.

## Prossimo lavoro

1. `validate.cmd` e correzioni; poi aggiornare rapporto, README, roadmap e ADR 0017 a **Verificato**, con istanti di acquisto Caserma e ventesimo soldato da `military-8-hours.json`.
2. **M5.3c:** benchmark di impatto civile (ordini piccoli/grandi, reclutamento precoce/tardivo): lavoro civile perso, Cibo/Pane, Spade, tempi. Nessun valore nuovo senza scelta dell'utente.
3. Poi M5.4 Cantiere/flotta: servono scelte su prezzo, Monete per nave, equipaggio e capienza. M5.5–M5.7 e M6 seguono.

Restano aperti DPI fisici, altre piattaforme e accessibilità. [Guida di sviluppo](DEVELOPMENT.md).
