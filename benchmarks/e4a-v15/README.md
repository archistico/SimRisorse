# E4a v15 — benchmark naturali e recupero controllato (RC73–RC77)

I manifest naturali `scenarios.csv` e `industrial-scenarios.csv` riportano le misure integrali **RC77-M01: 9/9 pass**, senza grants e con replay verificato. Non sostituirli con dati v14 o controllati. Il manifest separato [recovery-scenarios.csv](recovery-scenarios.csv) riporta gli esperimenti finanziati RC77. [Rapporto naturale con provenienza, tempi, costi e limiti](../../docs/validation/E4A_V15_RC77_M01_NATURAL_MEASUREMENTS.md).

## RC77 — Produzione non realizzata e recupero controllato

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-recovery --output DIRECTORY_NUOVA
```

Tre scenari, due politiche ciascuno e 120s dichiarati: Spade sature, Monete sature e contesa del Carbone. Il checkpoint iniziale è finanziato e dichiarato; Magazzino e assegnazioni usano comandi reali. Ogni secondo è riconciliato e distingue deficit per solo spazio, soli input o vincoli misti. Output JSON, campioni CSV, azioni CSV e Markdown; nessuna sovrascrittura. I run sono stati eseguiti e ripetuti con quattro file identici. [Specifica](../../docs/design/E4A_V15_RC77_PRODUZIONE_PERSA_E_RECUPERO.md) · [Risultati](../../docs/validation/E4A_V15_RC77_TEST_PLAN.md).

## Runner naturali RC73–RC76

La CLI dedicata usa `V15NaturalProgressionBenchmark` nel progetto Application e **non** scrive sui database personali. Parte da `GameSession.Create(CivilGrowthScenario.Create())`, usa la stessa conversione iniziale in memoria della GUI v15, acquista realmente tramite la sessione v15 e non aggiunge inventario, popolazione, lavoratori, tecnologie o edifici fittizi.

```powershell
# Diagnostica veloce, non costituisce un benchmark ufficiale:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-natural --scenario V15-N01 --max-seconds 120 --output .local-cache/e4a-v15/diagnostica-001

# Esecuzione degli orizzonti completi (dopo gate F1 riuscito):
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-natural --output .local-cache/e4a-v15/misura-001
```

L'output è costituito da un JSON di riepilogo, un CSV di campioni (scorte fisiche, popolazione, addetti, edifici) e un CSV di eventi (comandi, attese e motivi). Una directory con file risultati già presenti viene rifiutata, senza sovrascriverli. Una durata ridotta è etichettata `Diagnostic=true`. `CompletionSeconds` resta nullo per `timeout` o `blocked`: non riempire la colonna del manifest con numeri inventati.

La politica è uno **script di riferimento**, non un algoritmo ottimale: agisce ogni 60 secondi finché deve eseguire acquisti; dopo il completamento delle costruzioni osserva la produzione ogni secondo. Non interpretare i timeout come prova assoluta che la partita sia impossibile: possono indicare una strategia insufficiente o un orizzonte breve.

Documentazione: [specifica](../../docs/design/E4A_V15_RC73_NATURAL_RUNNER.md) · [piano test](../../docs/validation/E4A_V15_RC73_TEST_PLAN.md) · [roadmap](../../docs/ROADMAP.md).

## Analisi RC74 (senza riavviare il benchmark)

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --analyze-e4a-v15-natural --input .local-cache/e4a-v15/misura-001 --output .local-cache/e4a-v15/analisi-001
```

L'analizzatore legge `e4a-v15-natural-results.json` (oppure accetta il percorso del file) e scrive `e4a-v15-analysis.json`, `.csv` e `.md`. Distingue prove brevi, esiti e limiti dello script. **Un timeout non è la dimostrazione di un softlock**. I costi RC73 coprono solo `planks` e `stone`, non l'intera filiera. I valori attuali del manifest derivano dalle misure validate RC77-M01. [Specifiche](../../docs/design/E4A_V15_RC74_ANALISI_PROGRESSI.md).

## RC75 — Percorsi industriali e ricerca

Il [manifest degli scenari industriali](industrial-scenarios.csv) contiene V15-I01/I02/I03 per `research-first` e `bread-first`, **tutti misurati con esito pass nella RC77-M01**. La CLI `--benchmark-e4a-v15-industry` misura Scuola/tecnologia, prime Spade e Monete senza aggiungere scorte, popolazione o tecnologie. Registra pagamenti effettivi e punti ricerca separati; non confonde consumi continui e costi delle azioni. I run abbreviati sono diagnostici. L'analisi `--analyze-e4a-v15-industry` rilegge un JSON esistente. [Specifica RC75](../../docs/design/E4A_V15_RC75_INDUSTRIA_RICERCA.md) · [Piano test](../../docs/validation/E4A_V15_RC75_TEST_PLAN.md) · [ADR 0083](../../docs/adr/0083-benchmark-industria-ricerca-v15-senza-grants.md).


## RC76 — Diagnosi dei vincoli industriali

I report `e4a-v15-industry-constraints.json/csv/md` sono emessi dal runner industriale e dall'analizzatore. Misurano ammanchi quotabili e pressione della capienza cittadina di sette risorse; non diagnosticano un deadlock dal solo timeout o dalla saturazione. Rianalisi dei sei run RC77-M01 verificata. [Specifiche](../../docs/design/E4A_V15_RC76_ANALISI_VINCOLI.md) · [Test](../../docs/validation/E4A_V15_RC76_TEST_PLAN.md) · [ADR 0084](../../docs/adr/0084-telemetria-vincoli-benchmark-v15-senza-diagnosi-automatica-stallo.md).
