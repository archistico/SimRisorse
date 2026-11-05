# E4a v15 — benchmark naturali e recupero controllato (RC73–RC77)

I manifest naturali `scenarios.csv` e `industrial-scenarios.csv` restano **not-run**: non sostituirli con dati v14 o controllati. Il manifest separato [recovery-scenarios.csv](recovery-scenarios.csv) riporta invece le misure controllate RC77.

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

L'analizzatore legge `e4a-v15-natural-results.json` (oppure accetta il percorso del file) e scrive `e4a-v15-analysis.json`, `.csv` e `.md`. Distingue prove brevi, esiti e limiti dello script. **Un timeout non è la dimostrazione di un softlock**. I costi raccolti da RC73 coprono solo `planks` e `stone`, non il costo complessivo della filiera. Conservare tutti i valori del manifest `scenarios.csv` in `not-run` fino a misure validate. [Specifiche](../../docs/design/E4A_V15_RC74_ANALISI_PROGRESSI.md).

## RC75 — Percorsi industriali e ricerca

Il nuovo [manifest degli scenari industriali](industrial-scenarios.csv) contiene V15-I01/I02/I03 per le politiche `research-first` e `bread-first`, tutti con stato **`not-run`**. La CLI `--benchmark-e4a-v15-industry` misura Scuola/tecnologia, prima Forgia/Spade e prima Zecca/Monete senza aggiungere scorte, popolazione o tecnologie. Registra ricevute complete dei materiali effettivamente spesi e punti ricerca separati; non confonde i consumi delle ricette con i costi di costruzione. I run abbreviati sono diagnostici. L'analisi `--analyze-e4a-v15-industry` permette di interpretare un JSON già prodotto. Leggere [la specifica RC75](../../docs/design/E4A_V15_RC75_INDUSTRIA_RICERCA.md), il [piano test](../../docs/validation/E4A_V15_RC75_TEST_PLAN.md) e [ADR 0083](../../docs/adr/0083-benchmark-industria-ricerca-v15-senza-grants.md).


## RC76 — Diagnosi dei vincoli industriali

I nuovi report `e4a-v15-industry-constraints.json/csv/md` vengono emessi dal runner industriale e dall'analizzatore (`--analyze-e4a-v15-industry`). Misurano gli ammanchi quotabili al momento del tentativo e campionano la pressione della capienza cittadina di sette risorse. Non diagnosticano un deadlock sulla sola base di timeout o saturazione, né attribuiscono quantità inventate ai report RC75 privi di nuove proprietà. Leggere [specifiche](../../docs/design/E4A_V15_RC76_ANALISI_VINCOLI.md), [test](../../docs/validation/E4A_V15_RC76_TEST_PLAN.md) e [ADR 0084](../../docs/adr/0084-telemetria-vincoli-benchmark-v15-senza-diagnosi-automatica-stallo.md). Manifest industriale ancora `not-run`.
