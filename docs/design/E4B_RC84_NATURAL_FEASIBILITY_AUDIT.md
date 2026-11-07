# RC84 / E4b.2f — audit di fattibilità naturale della Zecca

## Fonti e confini

- **Fonte osservata:** `--benchmark-e4b-mint-ten --compare-legacy --output ...` di RC82; partita nuova v15, nessuna risorsa regalata, prezzi storici correnti, due regimi di personale `MintTen` e `Legacy`.
- **Fonte solo ipotetica:** nove righe `E4bMintScaleRiskStudy.Run()` RC83, curve A/B/C x tier Base/Grande/Avanzato. Non sono stati applicati prezzi o pagamenti delle proposte.
- **Analisi RC84:** legge un array JSON serializzato dei run e produce osservazioni distinte dalle ipotesi, senza eseguire `V15UnifiedCampaignClock.Plan`.

## Contratti di ingresso

1. Validazione preventiva `V15IndustrialProgressionAnalysis` e `V15IndustrialConstraintAnalysis` (ordine, provenienza, milestone osservabili, credito, deficit positivi, campioni validi).
2. Non accettare altri scenari, obiettivi, versioni `rules_version` o un insieme privo di MintTen.
3. Richiedere inventario iniziale, versione base, scenario iniziale e orizzonte dichiarato coerenti tra tutti i run. La duplicazione di scenario/politica è un errore.
4. Per dichiarare un successo richiedere eventi `build-mint`, `staff-mint-team` (`MintTen`) o `staff-mint` (`Legacy`), quindi milestone osservata dal clock. La sola costruzione non è produzione.
5. Se l'orizzonte è abbreviato, classificare `diagnostic-only` anche quando vengono osservate attività parziali. Le statistiche su capienza non dimostrano che la saturazione abbia inibito la produzione.

## Output

Tre file riproducibili: `e4b-mint-feasibility.json`, `e4b-mint-feasibility.csv`, `e4b-mint-feasibility.md`, con SHA-256 esatto del JSON importato. Il CSV contiene **una riga per esecuzione realmente osservata**. Le ipotesi A/B/C compaiono nel JSON e nel Markdown, non sono mischiate ai tempi CSV. Il report contrassegna `CompleteNaturalPolicyPairObserved=true` solo quando entrambe le politiche MintTen completano realmente un run **non diagnostico**.

## Comandi Windows

```powershell
# Ripetere solo quando possibile: validazione integrale del progetto.
.\validate-f1.cmd

# Benchmark naturale integrale; non inserire --max-seconds 0 in questo comando.
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4b-mint-ten --compare-legacy --output .local-cache/e4b-v15/rc84-natural-full

# Rianalisi offline dell'output autentico generato sopra.
dotnet run --project tools/SimRisorse.Simulation -c Release -- --analyze-e4b-mint-ten --input .local-cache/e4b-v15/rc84-natural-full/e4b-mint-ten-results.json --output .local-cache/e4b-v15/rc84-feasibility
```

Output immutabili: usare directory nuove per i replay. Gli orizzonti naturali e i tempi di produzione non sono stati misurati nell'ambiente di preparazione RC84 privo di .NET 10.

## Limitazioni intenzionali

- Nessuna prova diretta di doppia Zecca o tier Grande/Avanzato nella campagna naturale: le righe RC83 restano ipotesi.
- Nessun tempo teorico A/B/C calcolato riciclando i tempi della curva legacy.
- Nessuna modifica a prezzi, escrow, magazzini, ricerca, numero di abitanti, GUI v14 canonica o schema SQLite.
- Nessuna diagnosi automatica di softlock: timeout e `blocked` appartengono alla strategia di benchmark e all'orizzonte dichiarato.
