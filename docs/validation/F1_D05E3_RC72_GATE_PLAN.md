# RC72 — Piano di validazione conclusiva F1-D05e

**Stato:** preparato, non eseguito su Windows. **Suite attesa:** 826 test = 822 RC71 + quattro nuovi test RC72 (3 Application, 1 Infrastructure). Ultima validata: RC63, 771/771.

## Nuove regressioni

1. `MixedChainReconcilesEverySecondOfTwoMinutesWithoutRestoringTheRetiredProducer`: 120 tick reali, audit fisico ad ogni secondo, proprietario Cibo solo Deposito, assenza di `food-store` attivo, pasto sempre contabilizzato.
2. `MixedChainContinuousAndFractionalSplitAdvancesPreserveTheSamePhysicalState`: clock da 120,125 s confrontato con 0,125 s + 120 tick; stock, crescita, personale, ricerca, scuola, militari e tempi.
3. `ZeroDurationTickLeavesEveryPhysicalOwnerAndResearchStateUntouched`: nessuna produzione, nessun consumo, inventari e checkpoint invariati.
4. `CombinedCheckpointAcrossThreeSqliteRestartsMatchesUninterruptedSimulation`: tre salvataggi/riprese generation 6 a 0,125 + 37 + 83 secondi, equivalenza con 120,125 secondi continui e assenza della famiglia legacy.

## Gate automatico da eseguire su Windows

`./validate-f1.cmd` (oppure `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/validate-f1-gate.ps1`).

- **Prerequisiti:** SDK .NET 10, restore bloccato disponibile, nessun altro processo che scriva `.local-cache/m4/validation.json`. Non avviare due gate in parallelo.
- Esegue la valida originale, controlla data di generazione del JSON, directory TRX e il conteggio preciso **826 pass, 0 fail, 0 skipped**, screenshot attesi e smoke nativo Windows. Rifiuta risultati di un run precedente.
- Esegue l'audit sintetico e quello combinato **120 s** dal DLL compilato. Verifica durata, `ImbalancedSeconds=0`, `LegacyProducedFoodMilliunits=0`, pasti contabilizzati per secondo e produzione di Bovini, Selvaggina, Carne, Verdura, Frutta, Pane e Cibo nel combinato.
- Genera report `f1-gate.json` più log/JSON audit in `.local-cache/m4/f1-gates/<id>/`. Anche in caso di errore scrive `status=failed` e il messaggio; non si deve interpretare la semplice esistenza del file come successo.
- `./validate-f1.cmd -Combined24Hours` aggiunge un audit combinato da 86.400 secondi; opzionale, lungo, non indispensabile per il gate minimo.

## Verifica manuale indispensabile

Avviare `./run-game.cmd` (v15) e confrontare con `./run-game.cmd --v14` a risoluzioni normali e compatte: edifici bloccati grigi ma consultabili, dettagli con icone e righe, pulsanti Costruisci/Lavoratori, barra crescita sotto sidebar, Risorse a sette colonne, Scuola/Esercito. Se emergono differenze non autorizzate, F1 resta **aperta anche con gate automatico verde**.

**Criterio di uscita:** 826/826 test, audit minimo verde, nessuna regressione GUI osservata, nessuna anomalia SQLite. Solo dopo si marca F1-D05e completata ed entra in E4a v15.
