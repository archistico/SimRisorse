## RC85 — Politica naturale `housing-buffer` e controllo Legacy

`--benchmark-e4b-mint-ten --include-housing-buffer --compare-legacy` esegue tre politiche (`research-first`, `bread-first`, `housing-buffer`) per entrambi i regimi; senza il nuovo flag il default resta a due politiche. La terza tenta due Case piccole extra pagate normalmente prima della filiera Oro, senza grants. `--review-e4b-housing-buffer --input FILE --output NUOVA_CARTELLA` confronta `housing-buffer` contro `bread-first` a partire dalle evidenze originali, senza simulare e senza usare le curve A/B/C. Non esistono ancora misure integrali RC85 verificate. [Specifica](../../docs/design/E4B_RC85_MINT_HOUSING_BUFFER.md) · [ADR 0093](../../docs/adr/0093-politica-abitativa-naturale-per-squadra-zecca.md).

---

## RC83 — Studio isolato scalabilità della Zecca

`--study-e4b-mint-scale` confronta una e due Zecche sulle tre curve A/B/C e sui tre tier: budget personale, materiale marginale della filiera, capienza Magazzini e rate teoriche. [ADR 0091](../../docs/adr/0091-studio-rischi-scalabilita-seconda-zecca.md) · [Specifica](../../docs/design/E4B_RC83_MINT_SCALE_RISK.md). **Non è una misurazione naturale e non applica nuove regole.** RC82: 1002 test confermati dall'utente; RC83: 1020 attesi.

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-mint-scale --output .local-cache/e4b-v15/rc83-scale-001
```

---

## RC82 — Percorso naturale Zecca, 10 lavoratori

[Manifest](mint-natural-scenarios.csv) e [specifica](../../docs/design/E4B_RC82_MINT_TEN_NATURAL_BENCHMARK.md). `E4B-M01` in regime `MintTen` e due politiche, partenza senza grant. Le righe `not-run` NON contengono durate RC82: non copiarvi i tempi `Legacy` RC77.

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4b-mint-ten --compare-legacy --output .local-cache/e4b-v15/NUOVA_CARTELLA
```

---

# E4b / RC78 — tre curve analitiche

Questo strumento confronta proposte; non modifica il gioco e non simula i nuovi tempi di campagna. I coefficienti sono definiti in `E4bEconomicCurveStudy.Curves`; [metodo](../../docs/design/E4B_RC78_THREE_CURVES_STUDY.md), [risultati](../../docs/validation/E4B_RC78_CURVE_COMPARISON.md), [verifiche](../../docs/validation/E4B_RC78_TEST_PLAN.md).

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-curves --output .local-cache/e4b-v15/NUOVA_CARTELLA
```

Opzione `--baseline`: JSON o directory contenente `e4a-v15-industry-results.json` con i sei run naturali completi I01/I02/I03 × due politiche. Per riprodurre le misure, usare il runner indicato in [E4a v15](../e4a-v15/README.md) senza `--max-seconds`. Senza baseline nessun tempo è importato; non si fabbricano dati sostitutivi.

Cinque output nella directory nuova: `e4b-curves.json`, `e4b-quotes.csv`, `e4b-investments.csv`, `e4b-curves.md`, `baseline-source.json`. Tutte le metriche complete sono nel JSON; i CSV rappresentano le quantità per risorsa. Il totale con Magazzini di un percorso bloccato è nullo e non ha righe CSV come se fosse un prezzo zero. Lo stato del blocco resta nelle righe senza Magazzini e nel JSON. I dati generati restano nella cache.

Il gate `validate-f1.cmd` esegue lo studio senza una baseline locale obbligatoria: tre curve, 81 preventivi e 18 investimenti. Le misure naturali importate e il replay hanno evidenze separate. Coefficienti e moduli tier restano da decidere prima della RC79+.
