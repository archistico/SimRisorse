# RC82 / E4b.2d — Progressione naturale Zecca con dieci lavoratori

## Obiettivo e perimetro

**Dimostrare, e non presumere**, la percorribilità da una nuova partita v15 con squadre complete della Zecca. L'economia resta ai prezzi legacy finché non sono decisi curva e deposito. La RC82 non tocca nessuna schermata Desktop, XAML, salvataggio, catalogo, ricetta né il default delle campagne.

## Percorso `E4B-M01`

- Base: `V15BenchmarkFixture.Create()`, stessi materiali e popolazione iniziali dei run naturali RC77; nessun grant.
- Regime: `MintTen`, espresso dal campo `RulesVersion = civil-growth-15-mint-crew10`.
- Politiche: `research-first` e `bread-first`, identico insieme di costruzioni/prerequisiti e differente ordine Scuola/Pane.
- Azioni: Pescatore, Deposito, Casa piccola, catena Pane, Casa grande, Carbone, Scuola, Matite, Ricercatore, Estrazione I, Industria I, miniera/fonderia Oro, Zecca e assegnazione **atomica di dieci lavoratori**.
- Se non ci sono dieci lavoratori liberi, `InsufficientPopulation` registra un'attesa; il runner non ne inventa nove e non bypassa la crescita né gli altri addetti. Se la capacità di alloggio, l'alimentazione o la ricerca impediscono il completamento entro l'orizzonte, il risultato è `timeout` e la causa resta *da analizzare*.
- Successo: il clock deve misurare **effettiva produzione positiva di Monete** con Industria I sbloccata. Completamento/tempi solo dal risultato del run, mai dalle stime.

## Comandi

Da Windows, con .NET 10:

```powershell
# Solo diagnostica, veloce e senza risultati di progressione dichiarati:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4b-mint-ten --max-seconds 0 --compare-legacy --output .local-cache/e4b-v15/mint-diagnostic-001

# Misura completa delle due strategie, senza confronti storici se non richiesti:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4b-mint-ten --output .local-cache/e4b-v15/mint-full-001

# Confronto completo ad identica partenza con Legacy:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4b-mint-ten --compare-legacy --output .local-cache/e4b-v15/mint-compare-001
```

Output prefisso `e4b-mint-ten-`: `results.json`, `events.csv`, `samples.csv`, `analysis.json/csv/md`, `constraints.json/csv/md`. I risultati contengono eventi di acquisto solo se pagati davvero, motivi di attesa, spese in millesimi, popolazione libera e saturazioni misurate. Non confrontare tempi se una prova non ha milestone reale, o confondere confronto tra politiche e confronto tra regimi.

## Limiti

L'esperimento comprende **una** Zecca Base e usa il contratto RC81 di 10 lavoratori. Non prova ancora due Zecche/20 addetti, tier Grande/Avanzato 30/100, costo equivalente a 100–1.000 Tagliapietre, escrow né sostenibilità economica di lungo periodo. La capacità iniziale di alloggio può essere un vincolo dello script, non un'impossibilità strutturale del gioco. I benchmark RC77 devono restare riproducibili e `Legacy` resta il default, incluso nella GUI.

[ADR](../adr/0090-benchmark-naturale-zecca-squadre-complete-v15.md) · [Piano e risultati attuali](../validation/E4B_RC82_TEST_PLAN.md) · [Manifest](../../benchmarks/e4b/mint-natural-scenarios.csv).
